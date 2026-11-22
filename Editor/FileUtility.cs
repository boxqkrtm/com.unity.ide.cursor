/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Unity Technologies.
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.IO;
using UnityEngine;

namespace Microsoft.Unity.VisualStudio.Editor
{
	internal static class FileUtility
	{
		public const char WinSeparator = '\\';
		public const char UnixSeparator = '/';

		public static string GetPackageAssetFullPath(params string[] components)
		{
			// Unity has special IO handling of Packages and will resolve those path to the right package location
			return Path.GetFullPath(Path.Combine("Packages", "com.unity.ide.visualstudio", Path.Combine(components)));
		}

		public static string GetAssetFullPath(string asset)
		{
			var basePath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
			return Path.GetFullPath(Path.Combine(basePath, NormalizePathSeparators(asset)));
		}

		public static string NormalizePathSeparators(this string path)
		{
			if (string.IsNullOrEmpty(path))
				return path;

			if (Path.DirectorySeparatorChar == WinSeparator)
				path = path.Replace(UnixSeparator, WinSeparator);
			if (Path.DirectorySeparatorChar == UnixSeparator)
				path = path.Replace(WinSeparator, UnixSeparator);

			return path.Replace(string.Concat(WinSeparator, WinSeparator), WinSeparator.ToString());
		}

		public static string NormalizeWindowsToUnix(this string path)
		{
			if (string.IsNullOrEmpty(path))
				return path;

			return path.Replace(WinSeparator, UnixSeparator);
		}

		internal static bool IsFileInProjectRootDirectory(string fileName)
		{
			var relative = MakeRelativeToProjectPath(fileName);
			if (string.IsNullOrEmpty(relative))
				return false;

			return relative == Path.GetFileName(relative);
		}

		public static string MakeAbsolutePath(this string path)
		{
			if (string.IsNullOrEmpty(path)) { return string.Empty; }
			return Path.IsPathRooted(path) ? path : Path.GetFullPath(path);
		}
		
		// returns null if outside of the project scope
		internal static string MakeRelativeToProjectPath(string fileName)
		{
			var basePath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
			fileName = NormalizePathSeparators(fileName);

			if (!Path.IsPathRooted(fileName))
				fileName = Path.Combine(basePath, fileName);

			if (!fileName.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
				return null;

			return fileName
				.Substring(basePath.Length)
				.Trim(Path.DirectorySeparatorChar);
		}

		internal static void SafeDelete(string file)
		{
			try
			{
				if (File.Exists(file))
					File.Delete(file);
			}
			catch (IOException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
		}
	}
}
