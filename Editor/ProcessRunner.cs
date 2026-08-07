/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Unity.VisualStudio.Editor
{
	internal class ProcessRunnerResult
	{
		public bool Success { get; set; }
		public string Output { get; set; }
		public string Error { get; set; }
	}

	internal static class ProcessRunner
	{
		public const int DefaultTimeoutInMilliseconds = 300000;

		public static ProcessStartInfo ProcessStartInfoFor(string filename, string arguments, bool redirect = true, bool shell = false)
		{
			return new ProcessStartInfo
			{
				UseShellExecute = shell,
				CreateNoWindow = true, 
				RedirectStandardOutput = redirect,
				RedirectStandardError = redirect,
				FileName = filename,
				Arguments = arguments
			};
		}

		public static void Start(string filename, string arguments)
		{
			Start(ProcessStartInfoFor(filename, arguments, false));
		}

		public static void Start(ProcessStartInfo processStartInfo)
		{
			var process = new Process { StartInfo = processStartInfo };

			using (process)
			{
				process.Start();
			}
		}

		public static ProcessRunnerResult StartAndWaitForExit(string filename, string arguments, int timeoutms = DefaultTimeoutInMilliseconds, Action<string> onOutputReceived = null)
		{
			return StartAndWaitForExit(ProcessStartInfoFor(filename, arguments), timeoutms, onOutputReceived);
		}

		public static ProcessRunnerResult StartAndWaitForExit(ProcessStartInfo processStartInfo, int timeoutms = DefaultTimeoutInMilliseconds, Action<string> onOutputReceived = null)
		{
			var process = new Process { StartInfo = processStartInfo };

			using (process)
			{
				var sbOutput = new StringBuilder();
				var sbError = new StringBuilder();

				var outputSource = new TaskCompletionSource<bool>();
				var errorSource = new TaskCompletionSource<bool>();
				
				process.OutputDataReceived += (_, e) =>
				{
					Append(sbOutput, e.Data, outputSource);
					if (onOutputReceived != null && e.Data != null)
						onOutputReceived(e.Data);
				};
				process.ErrorDataReceived += (_, e) => Append(sbError, e.Data, errorSource);

				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				
				var run = Task.Run(() => process.WaitForExit(timeoutms));
				var processTask = Task.WhenAll(run, outputSource.Task, errorSource.Task);

				if (Task.WhenAny(Task.Delay(timeoutms), processTask).Result == processTask && run.Result)
					return new ProcessRunnerResult {Success = true, Error = sbError.ToString(), Output = sbOutput.ToString()};

				try
				{
					process.Kill();
				}
				catch
				{
					/* ignore */
				}
				
				return new ProcessRunnerResult {Success = false, Error = sbError.ToString(), Output = sbOutput.ToString()};
			}
		}

		private static void Append(StringBuilder sb, string data, TaskCompletionSource<bool> taskSource)
		{
			if (data == null)
			{
				taskSource.SetResult(true);
				return;
			}

			sb?.Append(data);
		}
	}
}
