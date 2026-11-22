/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.Unity.VisualStudio.Editor.Messaging;
using Microsoft.Unity.VisualStudio.Editor.Testing;
using UnityEditor;
using UnityEngine;
using MessageType = Microsoft.Unity.VisualStudio.Editor.Messaging.MessageType;

namespace Microsoft.Unity.VisualStudio.Editor
{
	[InitializeOnLoad]
	internal class VisualStudioIntegration
	{
		class Client
		{
			public IPEndPoint EndPoint { get; set; }
			public double LastMessage { get; set; }
		}

		private static Messager _messager;

		private static readonly Queue<Message> _incoming = new Queue<Message>();
		private static readonly Dictionary<IPEndPoint, Client> _clients = new Dictionary<IPEndPoint, Client>();
		private static readonly object _incomingLock = new object();
		private static readonly object _clientsLock = new object();

		static VisualStudioIntegration()
		{
			if (!VisualStudioEditor.IsEnabled)
				return;

			RunOnceOnUpdate(() =>
			{
				// Despite using ReuseAddress|!ExclusiveAddressUse, we can fail here:
				// - if another application is using this port with exclusive access
				// - or if the firewall is not properly configured
				var messagingPort = MessagingPort();

				try
				{
					_messager = Messager.BindTo(messagingPort);
					_messager.ReceiveMessage += ReceiveMessage;
				}
				catch (SocketException)
				{
					// We'll have a chance to try to rebind on next domain reload
					Debug.LogWarning($"Unable to use UDP port {messagingPort} for VS/Unity messaging. You should check if another process is already bound to this port or if your firewall settings are compatible.");
				}

				RunOnShutdown(Shutdown);
			});

			EditorApplication.update += OnUpdate;

			CheckLegacyAssemblies();
		}

		private static void CheckLegacyAssemblies()
		{
			var checkList = new HashSet<string>(new[] { KnownAssemblies.UnityVS, KnownAssemblies.Messaging, KnownAssemblies.Bridge });

			try
			{
				var assemblies = AppDomain
					.CurrentDomain
					.GetAssemblies()
					.Where(a => checkList.Contains(a.GetName().Name));

				foreach (var assembly in assemblies)
				{
					// for now we only want to warn against local assemblies, do not check externals.
					var relativePath = FileUtility.MakeRelativeToProjectPath(assembly.Location);
					if (relativePath == null)
						continue;

					Debug.LogWarning($"Project contains legacy assembly that could interfere with the Visual Studio Package. You should delete {relativePath}");
				}
			}
			catch (Exception)
			{
				// abandon legacy check
			}
		}

		private static void RunOnceOnUpdate(Action action)
		{
			var callback = null as EditorApplication.CallbackFunction;

			callback = () =>
			{
				EditorApplication.update -= callback;
				action();
			};

			EditorApplication.update += callback;
		}

		private static void RunOnShutdown(Action action)
		{
			// Mono on OSX has all kinds of quirks on AppDomain shutdown
#if UNITY_EDITOR_WIN
			AppDomain.CurrentDomain.DomainUnload += (_, __) => action();
#endif
		}

		private static int DebuggingPort()
		{
			return 56000 + (System.Diagnostics.Process.GetCurrentProcess().Id % 1000);
		}

		private static int MessagingPort()
		{
			return DebuggingPort() + 2;
		}

		private static void ReceiveMessage(object sender, MessageEventArgs args)
		{
			OnMessage(args.Message);
		}

		private static void OnUpdate()
		{
			lock (_incomingLock)
			{
				while (_incoming.Count > 0)
				{
					ProcessIncoming(_incoming.Dequeue());
				}
			}

			lock (_clientsLock)
			{
				foreach (var client in _clients.Values.ToArray())
				{
					// EditorApplication.timeSinceStartup: The time since the editor was started, in seconds, not reset when starting play mode.
					if (EditorApplication.timeSinceStartup - client.LastMessage > 4)
						_clients.Remove(client.EndPoint);
				}
			}
		}

		private static void AddMessage(Message message)
		{
			lock (_incomingLock)
			{
				_incoming.Enqueue(message);
			}
		}

		private static void ProcessIncoming(Message message)
		{
			lock (_clientsLock)
			{
				CheckClient(message);
			}

			switch (message.Type)
			{
				case MessageType.Ping:
					Answer(message, MessageType.Pong);
					break;
				case MessageType.Play:
					Shutdown();
					EditorApplication.isPlaying = true;
					break;
				case MessageType.Stop:
					EditorApplication.isPlaying = false;
					break;
				case MessageType.Pause:
					EditorApplication.isPaused = true;
					break;
				case MessageType.Unpause:
					EditorApplication.isPaused = false;
					break;
				case MessageType.Build:
					// Not used anymore
					break;
				case MessageType.Refresh:
					Refresh();
					break;
				case MessageType.Version:
					Answer(message, MessageType.Version, PackageVersion());
					break;
				case MessageType.UpdatePackage:
					// Not used anymore
					break;
				case MessageType.ProjectPath:
					Answer(message, MessageType.ProjectPath, Path.GetFullPath(Path.Combine(Application.dataPath, "..")));
					break;
				case MessageType.ExecuteTests:
					TestRunnerApiListener.ExecuteTests(message.Value);
					break;
				case MessageType.RetrieveTestList:
					TestRunnerApiListener.RetrieveTestList(message.Value);
					break;
				case MessageType.ShowUsage:
					UsageUtility.ShowUsage(message.Value);
					break;
			}
		}

		private static void CheckClient(Message message)
		{
			var endPoint = message.Origin;

			if (!_clients.TryGetValue(endPoint, out var client))
			{
				client = new Client
				{
					EndPoint = endPoint,
					LastMessage = EditorApplication.timeSinceStartup
				};

				_clients.Add(endPoint, client);
			}
			else
			{
				client.LastMessage = EditorApplication.timeSinceStartup;
			}
		}

		internal static string PackageVersion()
		{
			var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(VisualStudioIntegration).Assembly);
			return package.version;
		}

		private static bool IsAutoRefreshEnabled()
		{
			// Unity 2022.2+ / Unity 6: Auto Refresh is an enum stored as kAutoRefreshMode (0 = Disabled).
			// Legacy Unity used the bool kAutoRefresh.
			if (EditorPrefs.HasKey("kAutoRefreshMode"))
				return EditorPrefs.GetInt("kAutoRefreshMode") != 0;

			return EditorPrefs.GetBool("kAutoRefresh", true);
		}

		private static void Refresh()
		{
			// If the user disabled auto-refresh in Unity, do not try to force refresh the Asset database
			if (!IsAutoRefreshEnabled())
				return;

			if (UnityInstallation.IsInSafeMode)
				return;

			RunOnceOnUpdate(AssetDatabase.Refresh);
		}

		private static void OnMessage(Message message)
		{
			AddMessage(message);
		}

		private static void Answer(Client client, MessageType answerType, string answerValue)
		{
			Answer(client.EndPoint, answerType, answerValue);
		}

		private static void Answer(Message message, MessageType answerType, string answerValue = "")
		{
			var targetEndPoint = message.Origin;

			Answer(
				targetEndPoint,
				answerType,
				answerValue);
		}

		private static void Answer(IPEndPoint targetEndPoint, MessageType answerType, string answerValue)
		{
			_messager?.SendMessage(targetEndPoint, answerType, answerValue);
		}

		private static void Shutdown()
		{
			if (_messager == null)
				return;

			_messager.ReceiveMessage -= ReceiveMessage;
			_messager.Dispose();
			_messager = null;
		}

		internal static void BroadcastMessage(MessageType type, string value)
		{
			lock (_clientsLock)
			{
				foreach (var client in _clients.Values.ToArray())
				{
					Answer(client, type, value);
				}
			}
		}
	}
}
