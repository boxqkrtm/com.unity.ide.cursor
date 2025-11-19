/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Unity Technologies.
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using Unity.CodeEditor;

[assembly: InternalsVisibleTo("Unity.VisualStudio.EditorTests")]
[assembly: InternalsVisibleTo("Unity.VisualStudio.Standalone.EditorTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Microsoft.Unity.VisualStudio.Editor
{
	[InitializeOnLoad]
	public class VisualStudioEditor : IExternalCodeEditor
	{
		CodeEditor.Installation[] IExternalCodeEditor.Installations => _discoverInstallations
			.Result
			.Values
			.Select(v => v.ToCodeEditorInstallation())
			.ToArray();

		private static readonly AsyncOperation<Dictionary<string, IVisualStudioInstallation>> _discoverInstallations;

		static VisualStudioEditor()
		{
			if (!UnityInstallation.IsMainUnityEditorProcess)
				return;

			Discovery.Initialize();
			CodeEditor.Register(new VisualStudioEditor());

			_discoverInstallations = AsyncOperation<Dictionary<string, IVisualStudioInstallation>>.Run(DiscoverInstallations);
		}

#if UNITY_2019_4_OR_NEWER && !UNITY_2020
		[InitializeOnLoadMethod]
		static void LegacyVisualStudioCodePackageDisabler()
		{
			// disable legacy Visual Studio Code packages
			var editor = CodeEditor.Editor.GetCodeEditorForPath("code.cmd");
			if (editor == null)
				return;

			if (editor is VisualStudioEditor)
				return;

			// only disable the com.unity.ide.vscode package
			var assembly = editor.GetType().Assembly;
			var assemblyName = assembly.GetName().Name;
			if (assemblyName != "Unity.VSCode.Editor")
				return;

			CodeEditor.Unregister(editor);
		}
#endif

		private static Dictionary<string, IVisualStudioInstallation> DiscoverInstallations()
		{
			try
			{
				return Discovery
					.GetVisualStudioInstallations()
					.ToDictionary(i => Path.GetFullPath(i.Path), i => i);
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error detecting Visual Studio installations: {ex}");
				return new Dictionary<string, IVisualStudioInstallation>();
			}
		}

		internal static bool IsEnabled => CodeEditor.CurrentEditor is VisualStudioEditor && UnityInstallation.IsMainUnityEditorProcess;

		// this one seems legacy and not used anymore
		// keeping it for now given it is public, so we need a major bump to remove it 
		public void CreateIfDoesntExist()
		{
			if (!TryGetVisualStudioInstallationForPath(CodeEditor.CurrentEditorInstallation, true, out var installation)) 
				return;

			var generator = installation.ProjectGenerator;
			if (!generator.HasSolutionBeenGenerated())
				generator.Sync();
		}

		public void Initialize(string editorInstallationPath)
		{
		}

		internal virtual bool TryGetVisualStudioInstallationForPath(string editorPath, bool lookupDiscoveredInstallations, out IVisualStudioInstallation installation)
		{
			editorPath = Path.GetFullPath(editorPath);

			// lookup for well known installations
			if (lookupDiscoveredInstallations && _discoverInstallations.Result.TryGetValue(editorPath, out installation))
				return true;

			return Discovery.TryDiscoverInstallation(editorPath, out installation);
		}

		public virtual bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
		{
			var result = TryGetVisualStudioInstallationForPath(editorPath, lookupDiscoveredInstallations: false, out var vsi);
			installation = vsi?.ToCodeEditorInstallation() ?? default;
			return result;
		}

		public void OnGUI()
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (!TryGetVisualStudioInstallationForPath(CodeEditor.CurrentEditorInstallation, true, out var installation))
				return;

			var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(GetType().Assembly);

			var style = new GUIStyle
			{
				richText = true,
				margin = new RectOffset(0, 4, 0, 0)
			};

			GUILayout.Label($"<size=10><color=grey>{package.displayName} v{package.version} enabled</color></size>", style);
			GUILayout.EndHorizontal();

			if (installation is VisualStudioAntigravityInstallation)
			{
				var reuseWindow = EditorPrefs.GetBool(VisualStudioAntigravityInstallation.ReuseExistingWindowKey, false);
				var newReuseWindow = EditorGUILayout.Toggle(new GUIContent("Reuse existing Antigravity window", "When enabled, opens files in an existing Antigravity window if found. When disabled, always opens a new window."), reuseWindow);
				if (newReuseWindow != reuseWindow)
					EditorPrefs.SetBool(VisualStudioAntigravityInstallation.ReuseExistingWindowKey, newReuseWindow);

				EditorGUILayout.Space();
			}

			EditorGUILayout.LabelField("Generate .csproj files for:");
			EditorGUI.indentLevel++;
			SettingsButton(ProjectGenerationFlag.Embedded, "Embedded packages", "", installation);
			SettingsButton(ProjectGenerationFlag.Local, "Local packages", "", installation);
			SettingsButton(ProjectGenerationFlag.Registry, "Registry packages", "", installation);
			SettingsButton(ProjectGenerationFlag.Git, "Git packages", "", installation);
			SettingsButton(ProjectGenerationFlag.BuiltIn, "Built-in packages", "", installation);
			SettingsButton(ProjectGenerationFlag.LocalTarBall, "Local tarball", "", installation);
			SettingsButton(ProjectGenerationFlag.Unknown, "Packages from unknown sources", "", installation);
			SettingsButton(ProjectGenerationFlag.PlayerAssemblies, "Player projects", "For each player project generate an additional csproj with the name 'project-player.csproj'", installation);
			RegenerateProjectFiles(installation);
			EditorGUI.indentLevel--;
		}

		private static void RegenerateProjectFiles(IVisualStudioInstallation installation)
		{
			var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
			rect.width = 252;
			if (GUI.Button(rect, "Regenerate project files"))
			{
				installation.ProjectGenerator.Sync();
			}
		}

		private static void SettingsButton(ProjectGenerationFlag preference, string guiMessage, string toolTip, IVisualStudioInstallation installation)
		{
			var generator = installation.ProjectGenerator;
			var prevValue = generator.AssemblyNameProvider.ProjectGenerationFlag.HasFlag(preference);

			var newValue = EditorGUILayout.Toggle(new GUIContent(guiMessage, toolTip), prevValue);
			if (newValue != prevValue)
				generator.AssemblyNameProvider.ToggleProjectGeneration(preference);
		}

		public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles)
		{
			if (TryGetVisualStudioInstallationForPath(CodeEditor.CurrentEditorInstallation, true, out var installation))
			{
				installation.ProjectGenerator.SyncIfNeeded(addedFiles.Union(deletedFiles).Union(movedFiles).Union(movedFromFiles), importedFiles);
			}

			foreach (var file in importedFiles.Where(a => Path.GetExtension(a) == ".pdb"))
			{
				var pdbFile = FileUtility.GetAssetFullPath(file);

				// skip Unity packages like com.unity.ext.nunit
				if (pdbFile.IndexOf($"{Path.DirectorySeparatorChar}com.unity.", StringComparison.OrdinalIgnoreCase) > 0)
					continue;

				var asmFile = Path.ChangeExtension(pdbFile, ".dll");
				if (!File.Exists(asmFile) || !Image.IsAssembly(asmFile))
					continue;

				if (Symbols.IsPortableSymbolFile(pdbFile))
					continue;

				Debug.LogWarning($"Unity is only able to load mdb or portable-pdb symbols. {file} is using a legacy pdb format.");
			}
		}

		public void SyncAll()
		{
			if (TryGetVisualStudioInstallationForPath(CodeEditor.CurrentEditorInstallation, true, out var installation))
			{
				installation.ProjectGenerator.Sync();
			}
		}

		private static bool IsSupportedPath(string path, IGenerator generator)
		{
			// Path is empty with "Open C# Project", as we only want to open the solution without specific files
			if (string.IsNullOrEmpty(path))
				return true;

			// cs, uxml, uss, shader, compute, cginc, hlsl, glslinc, template are part of Unity builtin extensions
			// txt, xml, fnt, cd are -often- par of Unity user extensions
			// asdmdef is mandatory included
			return generator.IsSupportedFile(path);
		}

		public bool OpenProject(string path, int line, int column)
		{
			var editorPath = CodeEditor.CurrentEditorInstallation;

			if (!Discovery.TryDiscoverInstallation(editorPath, out var installation)) {
				Debug.LogWarning($"Visual Studio executable {editorPath} is not found. Please change your settings in Edit > Preferences > External Tools.");
				return false;
			}

			var generator = installation.ProjectGenerator;
			if (!IsSupportedPath(path, generator))
				return false;

			if (!IsProjectGeneratedFor(path, generator, out var missingFlag))
				Debug.LogWarning($"You are trying to open {path} outside a generated project. This might cause problems with IntelliSense and debugging. To avoid this, you can change your .csproj preferences in Edit > Preferences > External Tools and enable {GetProjectGenerationFlagDescription(missingFlag)} generation.");

			var solution = GetOrGenerateSolutionFile(generator);
			return installation.Open(path, line, column, solution);
		}

		private static string GetProjectGenerationFlagDescription(ProjectGenerationFlag flag)
		{
			switch (flag)
			{
				case ProjectGenerationFlag.BuiltIn:
					return "Built-in packages";
				case ProjectGenerationFlag.Embedded:
					return "Embedded packages";
				case ProjectGenerationFlag.Git:
					return "Git packages";
				case ProjectGenerationFlag.Local:
					return "Local packages";
				case ProjectGenerationFlag.LocalTarBall:
					return "Local tarball";
				case ProjectGenerationFlag.PlayerAssemblies:
					return "Player projects";
				case ProjectGenerationFlag.Registry:
					return "Registry packages";
				case ProjectGenerationFlag.Unknown:
					return "Packages from unknown sources";
				default:
					return string.Empty;
			}
		}

		private static bool IsProjectGeneratedFor(string path, IGenerator generator, out ProjectGenerationFlag missingFlag)
		{
			missingFlag = ProjectGenerationFlag.None;

			// No need to check when opening the whole solution
			if (string.IsNullOrEmpty(path))
				return true;

			// We only want to check for cs scripts
			if (ProjectGeneration.ScriptingLanguageForFile(path) != ScriptingLanguage.CSharp)
				return true;

			// Even on windows, the package manager requires relative path + unix style separators for queries
			var basePath = generator.ProjectDirectory;
			var relativePath = path
				.NormalizeWindowsToUnix()
				.Replace(basePath, string.Empty)
				.Trim(FileUtility.UnixSeparator);

			var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(relativePath);
			if (packageInfo == null)
				return true;

			var source = packageInfo.source;
			if (!Enum.TryParse<ProjectGenerationFlag>(source.ToString(), out var flag))
				return true;

			if (generator.AssemblyNameProvider.ProjectGenerationFlag.HasFlag(flag))
				return true;

			// Return false if we found a source not flagged for generation
			missingFlag = flag;
			return false;
		}

		private static string GetOrGenerateSolutionFile(IGenerator generator)
		{
			generator.Sync();
			return generator.SolutionFile();
		}
	}
}
