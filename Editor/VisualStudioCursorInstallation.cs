/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using SimpleJSON;
using IOPath = System.IO.Path;
using Debug = UnityEngine.Debug;

namespace Microsoft.Unity.VisualStudio.Editor
{
	internal class VisualStudioCursorInstallation : VisualStudioInstallation
	{
		private static readonly IGenerator _generator = new SdkStyleProjectGeneration();
		internal const string ReuseExistingWindowKey = "cursor_reuse_existing_window";

		public override bool SupportsAnalyzers
		{
			get
			{
				return true;
			}
		}

		public override Version LatestLanguageVersionSupported
		{
			get
			{
				return new Version(11, 0);
			}
		}

		private string GetExtensionPath()
		{
			var vscode = IsPrerelease ? ".vscode-insiders" : ".vscode";
			var extensionsPath = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), vscode, "extensions");
			if (!Directory.Exists(extensionsPath))
				return null;

			return Directory
				.EnumerateDirectories(extensionsPath, $"{MicrosoftUnityExtensionId}*") // publisherid.extensionid
				.OrderByDescending(n => n)
				.FirstOrDefault();
		}

		public override string[] GetAnalyzers()
		{
			var vstuPath = GetExtensionPath();
			if (string.IsNullOrEmpty(vstuPath))
				return Array.Empty<string>();

			return GetAnalyzers(vstuPath);
		}

		public override IGenerator ProjectGenerator
		{
			get
			{
				return _generator;
			}
		}

		private static bool IsCandidateForDiscovery(string path)
		{
#if UNITY_EDITOR_OSX
			return Directory.Exists(path) && Regex.IsMatch(path, ".*Cursor.*.app$", RegexOptions.IgnoreCase);
#elif UNITY_EDITOR_WIN
			return File.Exists(path) && Regex.IsMatch(path, ".*Cursor.*.exe$", RegexOptions.IgnoreCase);
#else
			return File.Exists(path) && path.EndsWith("cursor", StringComparison.OrdinalIgnoreCase);
#endif
		}

		[Serializable]
		internal class VisualStudioCodeManifest
		{
			public string name;
			public string version;
		}

		public static bool TryDiscoverInstallation(string editorPath, out IVisualStudioInstallation installation)
		{
			installation = null;

			if (string.IsNullOrEmpty(editorPath))
				return false;

			if (!IsCandidateForDiscovery(editorPath))
				return false;

			Version version = null;
			var isPrerelease = false;

			try
			{
				var manifestBase = GetRealPath(editorPath);

#if UNITY_EDITOR_WIN
				// on Windows, editorPath is a file, resources as subdirectory
				manifestBase = IOPath.GetDirectoryName(manifestBase);
#elif UNITY_EDITOR_OSX
				// on Mac, editorPath is a directory
				manifestBase = IOPath.Combine(manifestBase, "Contents");
#else
				// on Linux, editorPath is a file, in a bin sub-directory
				var parent = Directory.GetParent(manifestBase);
				// but we can link to [vscode]/code or [vscode]/bin/code
				manifestBase = parent?.Name == "bin" ? parent.Parent?.FullName : parent?.FullName;
#endif

				if (manifestBase == null)
					return false;

				var manifestFullPath = IOPath.Combine(manifestBase, "resources", "app", "package.json");
				if (File.Exists(manifestFullPath))
				{
					var manifest = JsonUtility.FromJson<VisualStudioCodeManifest>(File.ReadAllText(manifestFullPath));
					Version.TryParse(manifest.version.Split('-').First(), out version);
					isPrerelease = manifest.version.ToLower().Contains("insider");
				}
			}
			catch (Exception)
			{
				// do not fail if we are not able to retrieve the exact version number
			}

			isPrerelease = isPrerelease || editorPath.ToLower().Contains("insider");
			installation = new VisualStudioCursorInstallation()
			{
				IsPrerelease = isPrerelease,
				Name = "Cursor" + (isPrerelease ? " - Insider" : string.Empty) + (version != null ? $" [{version.ToString(3)}]" : string.Empty),
				Path = editorPath,
				Version = version ?? new Version()
			};

			return true;
		}

		public static IEnumerable<IVisualStudioInstallation> GetVisualStudioInstallations()
		{
			var candidates = new List<string>();

#if UNITY_EDITOR_WIN
			var localAppPath = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs");
			var programFiles = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));

			foreach (var basePath in new[] { localAppPath, programFiles }) {
				candidates.Add(IOPath.Combine(basePath, "cursor", "cursor.exe"));
			}
#elif UNITY_EDITOR_OSX
			var appPath = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
			candidates.AddRange(Directory.EnumerateDirectories(appPath, "Cursor*.app"));
#elif UNITY_EDITOR_LINUX
			// Well known locations
			candidates.Add("/usr/bin/cursor");
			candidates.Add("/bin/cursor");
			candidates.Add("/usr/local/bin/cursor");

			// Preference ordered base directories relative to which desktop files should be searched
			candidates.AddRange(GetXdgCandidates());
#endif

			foreach (var candidate in candidates.Distinct())
			{
				if (TryDiscoverInstallation(candidate, out var installation))
					yield return installation;
			}
		}

#if UNITY_EDITOR_LINUX
		private static readonly Regex DesktopFileExecEntry = new Regex(@"Exec=(\S+)", RegexOptions.Singleline | RegexOptions.Compiled);

		private static IEnumerable<string> GetXdgCandidates()
		{
			var envdirs = Environment.GetEnvironmentVariable("XDG_DATA_DIRS");
			if (string.IsNullOrEmpty(envdirs))
				yield break;

			var dirs = envdirs.Split(':');
			foreach(var dir in dirs)
			{
				Match match = null;

				try
				{
					var desktopFile = IOPath.Combine(dir, "applications/code.desktop");
					if (!File.Exists(desktopFile))
						continue;

					var content = File.ReadAllText(desktopFile);
					match = DesktopFileExecEntry.Match(content);
				}
				catch
				{
					// do not fail if we cannot read desktop file
				}

				if (match == null || !match.Success)
					continue;

				yield return match.Groups[1].Value;
				break;
			}
		}

		[System.Runtime.InteropServices.DllImport ("libc")]
		private static extern int readlink(string path, byte[] buffer, int buflen);

		internal static string GetRealPath(string path)
		{
			byte[] buf = new byte[512];
			int ret = readlink(path, buf, buf.Length);
			if (ret == -1) return path;
			char[] cbuf = new char[512];
			int chars = System.Text.Encoding.Default.GetChars(buf, 0, ret, cbuf, 0);
			return new String(cbuf, 0, chars);
		}
#else
		internal static string GetRealPath(string path)
		{
			return path;
		}
#endif

		public override void CreateExtraFiles(string projectDirectory)
		{
			try
			{
				var vscodeDirectory = IOPath.Combine(projectDirectory.NormalizePathSeparators(), ".vscode");
				Directory.CreateDirectory(vscodeDirectory);

				var enablePatch = !File.Exists(IOPath.Combine(vscodeDirectory, ".vstupatchdisable"));

				CreateRecommendedExtensionsFile(vscodeDirectory, enablePatch);
				CreateSettingsFile(vscodeDirectory, enablePatch);
				CreateLaunchFile(vscodeDirectory, enablePatch);
			}
			catch (IOException)
			{
			}
		}

		private const string DefaultLaunchFileContent = @"{
    ""version"": ""0.2.0"",
    ""configurations"": [
        {
            ""name"": ""Attach to Unity"",
            ""type"": ""vstuc"",
            ""request"": ""attach""
        }
     ]
}";

		private static void CreateLaunchFile(string vscodeDirectory, bool enablePatch)
		{
			var launchFile = IOPath.Combine(vscodeDirectory, "launch.json");
			if (File.Exists(launchFile))
			{
				if (enablePatch)
					PatchLaunchFile(launchFile);

				return;
			}

			File.WriteAllText(launchFile, DefaultLaunchFileContent);
		}

		private static void PatchLaunchFile(string launchFile)
		{
			try
			{
				const string configurationsKey = "configurations";
				const string typeKey = "type";

				var content = File.ReadAllText(launchFile);
				var launch = JSONNode.Parse(content);

				var configurations = launch[configurationsKey] as JSONArray;
				if (configurations == null)
				{
					configurations = new JSONArray();
					launch.Add(configurationsKey, configurations);
				}

				if (configurations.Linq.Any(entry => entry.Value[typeKey].Value == "vstuc"))
					return;

				var defaultContent = JSONNode.Parse(DefaultLaunchFileContent);
				configurations.Add(defaultContent[configurationsKey][0]);

				WriteAllTextFromJObject(launchFile, launch);
			}
			catch (Exception)
			{
				// do not fail if we cannot patch the launch.json file
			}
		}

		private void CreateSettingsFile(string vscodeDirectory, bool enablePatch)
		{
			var settingsFile = IOPath.Combine(vscodeDirectory, "settings.json");
			if (File.Exists(settingsFile))
			{
				if (enablePatch)
					PatchSettingsFile(settingsFile);

				return;
			}

			const string excludes = @"    ""files.exclude"": {
        ""**/.DS_Store"": true,
        ""**/.git"": true,
        ""**/.vs"": true,
        ""**/.gitmodules"": true,
        ""**/.vsconfig"": true,
        ""**/*.booproj"": true,
        ""**/*.pidb"": true,
        ""**/*.suo"": true,
        ""**/*.user"": true,
        ""**/*.userprefs"": true,
        ""**/*.unityproj"": true,
        ""**/*.dll"": true,
        ""**/*.exe"": true,
        ""**/*.pdf"": true,
        ""**/*.mid"": true,
        ""**/*.midi"": true,
        ""**/*.wav"": true,
        ""**/*.gif"": true,
        ""**/*.ico"": true,
        ""**/*.jpg"": true,
        ""**/*.jpeg"": true,
        ""**/*.png"": true,
        ""**/*.psd"": true,
        ""**/*.tga"": true,
        ""**/*.tif"": true,
        ""**/*.tiff"": true,
        ""**/*.3ds"": true,
        ""**/*.3DS"": true,
        ""**/*.fbx"": true,
        ""**/*.FBX"": true,
        ""**/*.lxo"": true,
        ""**/*.LXO"": true,
        ""**/*.ma"": true,
        ""**/*.MA"": true,
        ""**/*.obj"": true,
        ""**/*.OBJ"": true,
        ""**/*.asset"": true,
        ""**/*.cubemap"": true,
        ""**/*.flare"": true,
        ""**/*.mat"": true,
        ""**/*.meta"": true,
        ""**/*.prefab"": true,
        ""**/*.unity"": true,
        ""build/"": true,
        ""Build/"": true,
        ""Library/"": true,
        ""library/"": true,
        ""obj/"": true,
        ""Obj/"": true,
        ""Logs/"": true,
        ""logs/"": true,
        ""ProjectSettings/"": true,
        ""UserSettings/"": true,
        ""temp/"": true,
        ""Temp/"": true
    }";

			var content = @"{
" + excludes + @",
    ""dotnet.defaultSolution"": """ + IOPath.GetFileName(ProjectGenerator.SolutionFile()) + @"""
}";

			File.WriteAllText(settingsFile, content);
		}

		private void PatchSettingsFile(string settingsFile)
		{
			try
			{
				const string excludesKey = "files.exclude";
				const string solutionKey = "dotnet.defaultSolution";

				var content = File.ReadAllText(settingsFile);
				var settings = JSONNode.Parse(content);

				var excludes = settings[excludesKey] as JSONObject;
				if (excludes == null)
					return;

				var patchList = new List<string>();
				var patched = false;

				// Remove files.exclude for solution+project files in the project root
				foreach (var exclude in excludes)
				{
					if (!bool.TryParse(exclude.Value, out var exc) || !exc)
						continue;

					var key = exclude.Key;

					if (!key.EndsWith(".sln") && !key.EndsWith(".csproj"))
						continue;

					if (!Regex.IsMatch(key, "^(\\*\\*[\\\\\\/])?\\*\\.(sln|csproj)$"))
						continue;

					patchList.Add(key);
					patched = true;
				}

				// Check default solution
				var defaultSolution = settings[solutionKey];
				var solutionFile = IOPath.GetFileName(ProjectGenerator.SolutionFile());
				if (defaultSolution == null || defaultSolution.Value != solutionFile)
				{
					settings[solutionKey] = solutionFile;
					patched = true;
				}

				if (!patched)
					return;

				foreach (var patch in patchList)
					excludes.Remove(patch);

				WriteAllTextFromJObject(settingsFile, settings);
			}
			catch (Exception)
			{
				// do not fail if we cannot patch the settings.json file
			}
		}

		private const string MicrosoftUnityExtensionId = "visualstudiotoolsforunity.vstuc";
		private const string DefaultRecommendedExtensionsContent = @"{
    ""recommendations"": [
      """ + MicrosoftUnityExtensionId + @"""
    ]
}
";

		private static void CreateRecommendedExtensionsFile(string vscodeDirectory, bool enablePatch)
		{
			// see https://tattoocoder.com/recommending-vscode-extensions-within-your-open-source-projects/
			var extensionFile = IOPath.Combine(vscodeDirectory, "extensions.json");
			if (File.Exists(extensionFile))
			{
				if (enablePatch)
					PatchRecommendedExtensionsFile(extensionFile);

				return;
			}

			File.WriteAllText(extensionFile, DefaultRecommendedExtensionsContent);
		}

		private static void PatchRecommendedExtensionsFile(string extensionFile)
		{
			try
			{
				const string recommendationsKey = "recommendations";

				var content = File.ReadAllText(extensionFile);
				var extensions = JSONNode.Parse(content);

				var recommendations = extensions[recommendationsKey] as JSONArray;
				if (recommendations == null)
				{
					recommendations = new JSONArray();
					extensions.Add(recommendationsKey, recommendations);
				}

				if (recommendations.Linq.Any(entry => entry.Value.Value == MicrosoftUnityExtensionId))
					return;

				recommendations.Add(MicrosoftUnityExtensionId);
				WriteAllTextFromJObject(extensionFile, extensions);
			}
			catch (Exception)
			{
				// do not fail if we cannot patch the extensions.json file
			}
		}

		private static void WriteAllTextFromJObject(string file, JSONNode node)
		{
			using (var fs = File.Open(file, FileMode.Create))
			using (var sw = new StreamWriter(fs))
			{
				// Keep formatting/indent in sync with default contents
				sw.Write(node.ToString(aIndent: 4));
			}
		}

		// Always pass the project folder so Cursor loads Project Rules (.cursor/rules).
		// Prefer solution directory over auto-discovered *.code-workspace (Open Folder semantics).
		private static string BuildCursorArgs(string directory, string path, int line, int column, bool reuse)
		{
			var flag = reuse ? "--reuse-window" : "--new-window";
			if (string.IsNullOrEmpty(path))
				return $"{flag} \"{directory}\"";

			return $"{flag} \"{directory}\" -g \"{path}\":{line}:{column}";
		}

		public override bool Open(string path, int line, int column, string solution)
		{
			line = Math.Max(1, line);
			column = Math.Max(0, column);

			if (string.IsNullOrEmpty(solution))
			{
				Debug.LogWarning("[Cursor] Cannot open editor: solution path is empty.");
				return false;
			}

			var directory = IOPath.GetDirectoryName(solution);
			if (string.IsNullOrEmpty(directory))
			{
				Debug.LogWarning($"[Cursor] Cannot open editor: unable to resolve project directory from solution '{solution}'.");
				return false;
			}

			var reuse = EditorPrefs.GetBool(ReuseExistingWindowKey, false);
			var args = BuildCursorArgs(directory, path, line, column, reuse);
			var editorPath = Path;
			// Grant foreground before launch (Windows) while Unity still owns focus (issue #3).
			BringCursorToForeground(editorPath);
			ProcessRunner.Start(ProcessStartInfoFor(editorPath, args));
			BringCursorToForeground(editorPath);
			return true;
		}

		// Do not use open -n: a new instance confuses Mission Control Spaces while IPC still
		// delivers goto to the existing Cursor window (issue #16).
		private static ProcessStartInfo ProcessStartInfoFor(string application, string arguments)
		{
#if UNITY_EDITOR_OSX
			arguments = $"\"{application}\" --args {arguments}";
			application = "open";
			return ProcessRunner.ProcessStartInfoFor(application, arguments, redirect: false, shell: true);
#else
			return ProcessRunner.ProcessStartInfoFor(application, arguments, redirect: false);
#endif
		}

		private static void BringCursorToForeground(string editorAppPath)
		{
			try
			{
#if UNITY_EDITOR_WIN
				BringCursorToForegroundWindows(editorAppPath);
#elif UNITY_EDITOR_OSX
				BringCursorToForegroundMacOS(editorAppPath);
#elif UNITY_EDITOR_LINUX
				BringCursorToForegroundLinux(editorAppPath);
#endif
			}
			catch (Exception)
			{
				// Never fail Open() because activation failed
			}
		}

#if UNITY_EDITOR_WIN
		private const int SwRestore = 9;

		private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern bool AllowSetForegroundWindow(int dwProcessId);

		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll")]
		private static extern bool IsIconic(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("kernel32.dll")]
		private static extern uint GetCurrentThreadId();

		[DllImport("user32.dll")]
		private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

		// Keep delegate alive for EnumWindows (avoid GC during native callback).
		private static readonly EnumWindowsProc EnumWindowsCallback = OnEnumWindows;
		private static HashSet<uint> _enumTargetPids;
		private static List<IntPtr> _enumFoundWindows;

		private static void BringCursorToForegroundWindows(string editorAppPath)
		{
			var processes = GetCursorProcesses(editorAppPath).ToList();
			if (processes.Count == 0)
				return;

			foreach (var process in processes)
			{
				try
				{
					AllowSetForegroundWindow(process.Id);
				}
				catch (Exception)
				{
					/* ignore per-process ASFW failures */
				}
			}

			var hwnd = FindCursorMainWindow(processes);
			if (hwnd == IntPtr.Zero)
				return;

			if (IsIconic(hwnd))
				ShowWindow(hwnd, SwRestore);

			if (SetForegroundWindow(hwnd))
				return;

			// Fallback: temporarily attach to the foreground thread (same idea as stubborn Electron focus).
			var foreground = GetForegroundWindow();
			if (foreground == IntPtr.Zero)
				return;

			var foreThread = GetWindowThreadProcessId(foreground, out _);
			var appThread = GetWindowThreadProcessId(hwnd, out _);
			var curThread = GetCurrentThreadId();

			try
			{
				AttachThreadInput(curThread, foreThread, true);
				AttachThreadInput(curThread, appThread, true);
				if (IsIconic(hwnd))
					ShowWindow(hwnd, SwRestore);
				SetForegroundWindow(hwnd);
			}
			finally
			{
				AttachThreadInput(curThread, appThread, false);
				AttachThreadInput(curThread, foreThread, false);
			}
		}

		private static IntPtr FindCursorMainWindow(IEnumerable<Process> processes)
		{
			_enumTargetPids = new HashSet<uint>();
			foreach (var process in processes)
			{
				try
				{
					_enumTargetPids.Add((uint)process.Id);
				}
				catch (Exception)
				{
					/* process may have exited */
				}
			}

			if (_enumTargetPids.Count == 0)
				return IntPtr.Zero;

			_enumFoundWindows = new List<IntPtr>();
			EnumWindows(EnumWindowsCallback, IntPtr.Zero);

			return _enumFoundWindows.Count > 0 ? _enumFoundWindows[0] : IntPtr.Zero;
		}

		private static bool OnEnumWindows(IntPtr hWnd, IntPtr lParam)
		{
			if (!IsWindowVisible(hWnd) || GetWindowTextLength(hWnd) <= 0)
				return true;

			GetWindowThreadProcessId(hWnd, out var pid);
			if (_enumTargetPids != null && _enumTargetPids.Contains(pid))
				_enumFoundWindows.Add(hWnd);

			return true;
		}

		private static IEnumerable<Process> GetCursorProcesses(string editorAppPath)
		{
			var baseName = IOPath.GetFileNameWithoutExtension(editorAppPath);
			if (string.IsNullOrEmpty(baseName))
				baseName = "Cursor";

			var candidates = new[] { baseName, baseName.ToLowerInvariant(), "Cursor", "cursor" }
				.Distinct(StringComparer.OrdinalIgnoreCase);

			var seen = new HashSet<int>();
			foreach (var name in candidates)
			{
				Process[] processes;
				try
				{
					processes = Process.GetProcessesByName(name);
				}
				catch (Exception)
				{
					continue;
				}

				foreach (var process in processes)
				{
					if (seen.Add(process.Id))
						yield return process;
				}
			}
		}
#endif

#if UNITY_EDITOR_OSX
		private static void BringCursorToForegroundMacOS(string editorAppPath)
		{
			if (string.IsNullOrEmpty(editorAppPath))
				return;

			var appName = IOPath.GetFileNameWithoutExtension(editorAppPath);
			if (string.IsNullOrEmpty(appName))
				return;

			// Single-quoted -e payload for the shell; escape any single quotes in the app name
			appName = appName.Replace("'", "'\\''");
			var arguments = $"-e 'tell application \"{appName}\" to activate'";
			ProcessRunner.Start(ProcessRunner.ProcessStartInfoFor("osascript", arguments, redirect: false, shell: true));
		}
#endif

#if UNITY_EDITOR_LINUX
		private static void BringCursorToForegroundLinux(string editorAppPath)
		{
			var appName = IOPath.GetFileNameWithoutExtension(editorAppPath);
			if (string.IsNullOrEmpty(appName))
				appName = "Cursor";

			// Best-effort; wmctrl may be missing on Wayland or minimal installs.
			ProcessRunner.Start(ProcessRunner.ProcessStartInfoFor("wmctrl", $"-xa \"{appName}\"", redirect: false, shell: true));
		}
#endif

		public static void Initialize()
		{
		}
	}
}
