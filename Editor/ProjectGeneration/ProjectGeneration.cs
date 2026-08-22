/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Unity Technologies.
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SR = System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Unity.CodeEditor;
using Unity.Profiling;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Microsoft.Unity.VisualStudio.Editor
{
	public enum ScriptingLanguage
	{
		None,
		CSharp
	}

	public interface IGenerator
	{
		bool SyncIfNeeded(IEnumerable<string> affectedFiles, IEnumerable<string> reimportedFiles);
		void Sync();
		bool HasSolutionBeenGenerated();
		bool IsSupportedFile(string path);
		string SolutionFile();
		string ProjectDirectory { get; }
		IAssemblyNameProvider AssemblyNameProvider { get; }
	}

	public class ProjectGeneration : IGenerator
	{
		// do not remove because of the Validation API, used in LegacyStyleProjectGeneration
		public static readonly string MSBuildNamespaceUri = "http://schemas.microsoft.com/developer/msbuild/2003";

		public IAssemblyNameProvider AssemblyNameProvider => m_AssemblyNameProvider;
		public string ProjectDirectory { get; }

		// Use this to have the same newline ending on all platforms for consistency.
		internal const string k_WindowsNewline = "\r\n";

		const string m_SolutionProjectEntryTemplate = @"Project(""{{{0}}}"") = ""{1}"", ""{2}"", ""{{{3}}}""{4}EndProject";

		readonly string m_SolutionProjectConfigurationTemplate = string.Join(k_WindowsNewline,
			@"        {{{0}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU",
			@"        {{{0}}}.Debug|Any CPU.Build.0 = Debug|Any CPU",
			@"        {{{0}}}.Release|Any CPU.ActiveCfg = Release|Any CPU",
			@"        {{{0}}}.Release|Any CPU.Build.0 = Release|Any CPU").Replace("    ", "\t");

		static readonly string[] k_ReimportSyncExtensions = { ".dll", ".asmdef" };

		HashSet<string> m_ProjectSupportedExtensions = new HashSet<string>();
		HashSet<string> m_BuiltinSupportedExtensions = new HashSet<string>();

		readonly string m_ProjectName;
		internal readonly IAssemblyNameProvider m_AssemblyNameProvider;
		readonly IFileIO m_FileIOProvider;
		readonly IGUIDGenerator m_GUIDGenerator;
		bool m_ShouldGenerateAll;
		IVisualStudioInstallation m_CurrentInstallation;

		public ProjectGeneration() : this(Directory.GetParent(Application.dataPath).FullName)
		{
		}

		public ProjectGeneration(string tempDirectory) : this(tempDirectory, new AssemblyNameProvider(), new FileIOProvider(), new GUIDProvider())
		{
		}

		public ProjectGeneration(string tempDirectory, IAssemblyNameProvider assemblyNameProvider, IFileIO fileIoProvider, IGUIDGenerator guidGenerator)
		{
			ProjectDirectory = FileUtility.NormalizeWindowsToUnix(tempDirectory);
			m_ProjectName = Path.GetFileName(ProjectDirectory);
			m_AssemblyNameProvider = assemblyNameProvider;
			m_FileIOProvider = fileIoProvider;
			m_GUIDGenerator = guidGenerator;

			SetupProjectSupportedExtensions();
		}

		internal virtual string StyleName => "";

		/// <summary>
		/// Syncs the scripting solution if any affected files are relevant.
		/// </summary>
		/// <returns>
		/// Whether the solution was synced.
		/// </returns>
		/// <param name='affectedFiles'>
		/// A set of files whose status has changed
		/// </param>
		/// <param name="reimportedFiles">
		/// A set of files that got reimported
		/// </param>
		public bool SyncIfNeeded(IEnumerable<string> affectedFiles, IEnumerable<string> reimportedFiles)
		{
			using (solutionSyncMarker.Auto())
			{
				// We need the exact VS version/capabilities to tweak project generation (analyzers/langversion)
				RefreshCurrentInstallation();

				SetupProjectSupportedExtensions();

				CreateExtraFiles(m_CurrentInstallation);

				// Don't sync if we haven't synced before
				var affected = affectedFiles as ICollection<string> ?? affectedFiles.ToArray();
				var reimported = reimportedFiles as ICollection<string> ?? reimportedFiles.ToArray();
				if (!HasFilesBeenModified(affected, reimported))
				{
					return false;
				}

				var assemblies = m_AssemblyNameProvider.GetAssemblies(ShouldFileBePartOfSolution);
				var allProjectAssemblies = RelevantAssembliesForMode(assemblies).ToList();
				SyncSolution(allProjectAssemblies);

				var allAssetProjectParts = GenerateAllAssetProjectParts();

				var affectedNames = affected
					.Select(asset => m_AssemblyNameProvider.GetAssemblyNameFromScriptPath(asset))
					.Where(name => !string.IsNullOrWhiteSpace(name)).Select(name =>
						name.Split(new[] { ".dll" }, StringSplitOptions.RemoveEmptyEntries)[0]);
				var reimportedNames = reimported
					.Select(asset => m_AssemblyNameProvider.GetAssemblyNameFromScriptPath(asset))
					.Where(name => !string.IsNullOrWhiteSpace(name)).Select(name =>
						name.Split(new[] { ".dll" }, StringSplitOptions.RemoveEmptyEntries)[0]);
				var affectedAndReimported = new HashSet<string>(affectedNames.Concat(reimportedNames));

				foreach (var assembly in allProjectAssemblies)
				{
					if (!affectedAndReimported.Contains(assembly.name))
						continue;

					SyncProject(assembly,
						allAssetProjectParts,
						responseFilesData: ParseResponseFileData(assembly).ToArray());
				}

				return true;
			}
		}

		private void CreateExtraFiles(IVisualStudioInstallation installation)
		{
			installation?.CreateExtraFiles(ProjectDirectory);
		}

		private bool HasFilesBeenModified(IEnumerable<string> affectedFiles, IEnumerable<string> reimportedFiles)
		{
			return affectedFiles.Any(ShouldFileBePartOfSolution) || reimportedFiles.Any(ShouldSyncOnReimportedAsset);
		}

		private static bool ShouldSyncOnReimportedAsset(string asset)
		{
			return k_ReimportSyncExtensions.Contains(new FileInfo(asset).Extension);
		}

		private void RefreshCurrentInstallation()
		{
			var editor = CodeEditor.CurrentEditor as VisualStudioEditor;
			editor?.TryGetVisualStudioInstallationForPath(CodeEditor.CurrentEditorInstallation, lookupDiscoveredInstallations: true, out m_CurrentInstallation);
		}

		static ProfilerMarker solutionSyncMarker = new ProfilerMarker("SolutionSynchronizerSync");

		public void Sync()
		{
			// We need the exact VS version/capabilities to tweak project generation (analyzers/langversion)
			RefreshCurrentInstallation();

			SetupProjectSupportedExtensions();

			(m_AssemblyNameProvider as AssemblyNameProvider)?.ResetPackageInfoCache();

			// See https://devblogs.microsoft.com/setup/configure-visual-studio-across-your-organization-with-vsconfig/
			// We create a .vsconfig file to make sure our ManagedGame workload is installed
			CreateExtraFiles(m_CurrentInstallation);

			var externalCodeAlreadyGeneratedProjects = OnPreGeneratingCSProjectFiles();

			if (!externalCodeAlreadyGeneratedProjects)
			{
				GenerateAndWriteSolutionAndProjects();
			}

			OnGeneratedCSProjectFiles();
		}

		public bool HasSolutionBeenGenerated()
		{
			return m_FileIOProvider.Exists(SolutionFile());
		}

		private void SetupProjectSupportedExtensions()
		{
			m_ProjectSupportedExtensions = new HashSet<string>(m_AssemblyNameProvider.ProjectSupportedExtensions);
			m_BuiltinSupportedExtensions = new HashSet<string>(EditorSettings.projectGenerationBuiltinExtensions);
		}

		private bool ShouldFileBePartOfSolution(string file)
		{
			// Exclude files coming from packages except if they are internalized.
			if (m_AssemblyNameProvider.IsInternalizedPackagePath(file))
			{
				return false;
			}

			return IsSupportedFile(file);
		}

		private static string GetExtensionWithoutDot(string path)
		{
			// Prevent re-processing and information loss
			if (!Path.HasExtension(path))
				return path;

			return Path
				.GetExtension(path)
				.TrimStart('.')
				.ToLower();
		}

		public bool IsSupportedFile(string path)
		{
			return IsSupportedFile(path, out _);
		}

		private bool IsSupportedFile(string path, out string extensionWithoutDot)
		{
			extensionWithoutDot = GetExtensionWithoutDot(path);

			// Dll's are not scripts but still need to be included
			if (extensionWithoutDot == "dll")
				return true;

			if (extensionWithoutDot == "asmdef")
				return true;

			if (m_BuiltinSupportedExtensions.Contains(extensionWithoutDot))
				return true;

			if (m_ProjectSupportedExtensions.Contains(extensionWithoutDot))
				return true;

			return false;
		}


		private static ScriptingLanguage ScriptingLanguageFor(Assembly assembly)
		{
			var files = assembly.sourceFiles;

			if (files.Length == 0)
				return ScriptingLanguage.None;

			return ScriptingLanguageForFile(files[0]);
		}

		internal static ScriptingLanguage ScriptingLanguageForExtension(string extensionWithoutDot)
		{
			return extensionWithoutDot == "cs" ? ScriptingLanguage.CSharp : ScriptingLanguage.None;
		}

		internal static ScriptingLanguage ScriptingLanguageForFile(string path)
		{
			return ScriptingLanguageForExtension(GetExtensionWithoutDot(path));
		}

		public void GenerateAndWriteSolutionAndProjects()
		{
			// Only synchronize assemblies that have associated source files and ones that we actually want in the project.
			// This also filters out DLLs coming from .asmdef files in packages.
			var assemblies = m_AssemblyNameProvider.GetAssemblies(ShouldFileBePartOfSolution).ToList();

			var allAssetProjectParts = GenerateAllAssetProjectParts();

			SyncSolution(assemblies);

			var allProjectAssemblies = RelevantAssembliesForMode(assemblies);

			foreach (var assembly in allProjectAssemblies)
			{
				SyncProject(assembly,
					allAssetProjectParts,
					responseFilesData: ParseResponseFileData(assembly).ToArray());
			}
		}

		private IEnumerable<ResponseFileData> ParseResponseFileData(Assembly assembly)
		{
			var systemReferenceDirectories = CompilationPipeline.GetSystemAssemblyDirectories(assembly.compilerOptions.ApiCompatibilityLevel);

			Dictionary<string, ResponseFileData> responseFilesData = assembly.compilerOptions.ResponseFiles.ToDictionary(x => x, x => m_AssemblyNameProvider.ParseResponseFile(
				x,
				ProjectDirectory,
				systemReferenceDirectories
			));

			Dictionary<string, ResponseFileData> responseFilesWithErrors = responseFilesData.Where(x => x.Value.Errors.Any())
				.ToDictionary(x => x.Key, x => x.Value);

			if (responseFilesWithErrors.Any())
			{
				foreach (var error in responseFilesWithErrors)
					foreach (var valueError in error.Value.Errors)
					{
						Debug.LogError($"{error.Key} Parse Error : {valueError}");
					}
			}

			return responseFilesData.Select(x => x.Value);
		}

		private Dictionary<string, string> GenerateAllAssetProjectParts()
		{
			Dictionary<string, StringBuilder> stringBuilders = new Dictionary<string, StringBuilder>();

			foreach (string asset in m_AssemblyNameProvider.GetAllAssetPaths())
			{
				// Exclude files coming from packages except if they are internalized.
				if (m_AssemblyNameProvider.IsInternalizedPackagePath(asset))
				{
					continue;
				}

				if (IsSupportedFile(asset, out var extensionWithoutDot) && ScriptingLanguage.None == ScriptingLanguageForExtension(extensionWithoutDot))
				{
					// Find assembly the asset belongs to by adding script extension and using compilation pipeline.
					var assemblyName = m_AssemblyNameProvider.GetAssemblyNameFromScriptPath(asset);

					if (string.IsNullOrEmpty(assemblyName))
					{
						continue;
					}

					assemblyName = Path.GetFileNameWithoutExtension(assemblyName);

					if (!stringBuilders.TryGetValue(assemblyName, out var projectBuilder))
					{
						projectBuilder = new StringBuilder();
						stringBuilders[assemblyName] = projectBuilder;
					}

					IncludeAsset(projectBuilder, IncludeAssetTag.None, asset);
				}
			}

			var result = new Dictionary<string, string>();

			foreach (var entry in stringBuilders)
				result[entry.Key] = entry.Value.ToString();

			return result;
		}

		internal enum IncludeAssetTag
		{
			Compile,
			None
		}

		internal virtual void IncludeAsset(StringBuilder builder, IncludeAssetTag tag, string asset)
		{
			var filename = EscapedRelativePathFor(asset, out var packageInfo);

			builder.Append("    <").Append(tag).Append(@" Include=""").Append(filename);
			if (Path.IsPathRooted(filename) && packageInfo != null)
			{
				// We are outside the Unity project and using a package context
				var linkPath = SkipPathPrefix(asset.NormalizePathSeparators(), packageInfo.assetPath.NormalizePathSeparators());

				builder.Append(@""">").Append(k_WindowsNewline);
				builder.Append("      <Link>").Append(linkPath).Append("</Link>").Append(k_WindowsNewline);
				builder.Append($"    </{tag}>").Append(k_WindowsNewline);
			}
			else
			{
				builder.Append(@""" />").Append(k_WindowsNewline);
			}
		}

		private void SyncProject(
			Assembly assembly,
			Dictionary<string, string> allAssetsProjectParts,
			ResponseFileData[] responseFilesData)
		{
			SyncProjectFileIfNotChanged(
				ProjectFile(assembly),
				ProjectText(assembly, allAssetsProjectParts, responseFilesData));
		}

		private void SyncProjectFileIfNotChanged(string path, string newContents)
		{
			if (Path.GetExtension(path) == ".csproj")
			{
				newContents = OnGeneratedCSProject(path, newContents);
			}

			SyncFileIfNotChanged(path, newContents);
		}

		private void SyncSolutionFileIfNotChanged(string path, string newContents)
		{
			newContents = OnGeneratedSlnSolution(path, newContents);

			SyncFileIfNotChanged(path, newContents);
		}

		private static IEnumerable<SR.MethodInfo> GetPostProcessorCallbacks(string name)
		{
			return TypeCache
				.GetTypesDerivedFrom<AssetPostprocessor>()
				.Where(t => t.Assembly.GetName().Name != KnownAssemblies.Bridge) // never call into the bridge if loaded with the package
				.Select(t => t.GetMethod(name, SR.BindingFlags.Public | SR.BindingFlags.NonPublic | SR.BindingFlags.Static))
				.Where(m => m != null);
		}

		static void OnGeneratedCSProjectFiles()
		{
			foreach (var method in GetPostProcessorCallbacks(nameof(OnGeneratedCSProjectFiles)))
			{
				method.Invoke(null, Array.Empty<object>());
			}
		}

		private static bool OnPreGeneratingCSProjectFiles()
		{
			bool result = false;

			foreach (var method in GetPostProcessorCallbacks(nameof(OnPreGeneratingCSProjectFiles)))
			{
				var retValue = method.Invoke(null, Array.Empty<object>());
				if (method.ReturnType == typeof(bool))
				{
					result |= (bool)retValue;
				}
			}

			return result;
		}

		private static string InvokeAssetPostProcessorGenerationCallbacks(string name, string path, string content)
		{
			foreach (var method in GetPostProcessorCallbacks(name))
			{
				var args = new[] { path, content };
				var returnValue = method.Invoke(null, args);
				if (method.ReturnType == typeof(string))
				{
					// We want to chain content update between invocations
					content = (string)returnValue;
				}
			}

			return content;
		}

		private static string OnGeneratedCSProject(string path, string content)
		{
			return InvokeAssetPostProcessorGenerationCallbacks(nameof(OnGeneratedCSProject), path, content);
		}

		private static string OnGeneratedSlnSolution(string path, string content)
		{
			return InvokeAssetPostProcessorGenerationCallbacks(nameof(OnGeneratedSlnSolution), path, content);
		}

		private void SyncFileIfNotChanged(string filename, string newContents)
		{
			try
			{
				if (m_FileIOProvider.Exists(filename) && newContents == m_FileIOProvider.ReadAllText(filename))
				{
					return;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}

			m_FileIOProvider.WriteAllText(filename, newContents);
		}

		private string ProjectText(Assembly assembly,
			Dictionary<string, string> allAssetsProjectParts,
			ResponseFileData[] responseFilesData)
		{
			ProjectHeader(assembly, responseFilesData, out StringBuilder projectBuilder);

			var references = new List<string>();

			projectBuilder.Append(@"  <ItemGroup>").Append(k_WindowsNewline);
			foreach (string file in assembly.sourceFiles)
			{
				if (!IsSupportedFile(file, out var extensionWithoutDot))
					continue;

				if ("dll" != extensionWithoutDot)
				{
					IncludeAsset(projectBuilder, IncludeAssetTag.Compile, file);
				}
				else
				{
					var fullFile = EscapedRelativePathFor(file, out _);
					references.Add(fullFile);
				}
			}
			projectBuilder.Append(@"  </ItemGroup>").Append(k_WindowsNewline);

			// Append additional non-script files that should be included in project generation.
			if (allAssetsProjectParts.TryGetValue(assembly.name, out var additionalAssetsForProject))
			{
				projectBuilder.Append(@"  <ItemGroup>").Append(k_WindowsNewline);

				projectBuilder.Append(additionalAssetsForProject);

				projectBuilder.Append(@"  </ItemGroup>").Append(k_WindowsNewline);

			}

			projectBuilder.Append(@"  <ItemGroup>").Append(k_WindowsNewline);

			var responseRefs = responseFilesData.SelectMany(x => x.FullPathReferences.Select(r => r));
			var internalAssemblyReferences = assembly.assemblyReferences
				.Where(i => !i.sourceFiles.Any(ShouldFileBePartOfSolution)).Select(i => i.outputPath);
			var allReferences =
				assembly.compiledAssemblyReferences
					.Union(responseRefs)
					.Union(references)
					.Union(internalAssemblyReferences);

			foreach (var reference in allReferences)
			{
				string fullReference = Path.IsPathRooted(reference) ? reference : Path.Combine(ProjectDirectory, reference);
				AppendReference(fullReference, projectBuilder);
			}

			projectBuilder.Append(@"  </ItemGroup>").Append(k_WindowsNewline);

			if (0 < assembly.assemblyReferences.Length)
			{
				projectBuilder.Append("  <ItemGroup>").Append(k_WindowsNewline);
				foreach (var reference in assembly.assemblyReferences.Where(i => i.sourceFiles.Any(ShouldFileBePartOfSolution)))
				{
					AppendProjectReference(assembly, reference, projectBuilder);
				}

				projectBuilder.Append(@"  </ItemGroup>").Append(k_WindowsNewline);
			}

			GetProjectFooter(projectBuilder);
			return projectBuilder.ToString();
		}

		private static string XmlFilename(string path)
		{
			if (string.IsNullOrEmpty(path))
				return path;

			path = path.Replace(@"%", "%25");
			path = path.Replace(@";", "%3b");

			return XmlEscape(path);
		}

		internal static string XmlEscape(string s)
		{
			return SecurityElement.Escape(s);
		}

		internal virtual void AppendProjectReference(Assembly assembly, Assembly reference, StringBuilder projectBuilder)
		{
		}

		private void AppendReference(string fullReference, StringBuilder projectBuilder)
		{
			var escapedFullPath = EscapedRelativePathFor(fullReference, out _);
			projectBuilder.Append(@"    <Reference Include=""").Append(Path.GetFileNameWithoutExtension(escapedFullPath)).Append(@""">").Append(k_WindowsNewline);
			projectBuilder.Append("      <HintPath>").Append(escapedFullPath).Append("</HintPath>").Append(k_WindowsNewline);
			projectBuilder.Append("      <Private>False</Private>").Append(k_WindowsNewline);
			projectBuilder.Append("    </Reference>").Append(k_WindowsNewline);
		}

		public string ProjectFile(Assembly assembly)
		{
			return Path.Combine(ProjectDirectory, $"{m_AssemblyNameProvider.GetAssemblyName(assembly.outputPath, assembly.name)}.csproj");
		}

#if UNITY_EDITOR_WIN
		private static readonly Regex InvalidCharactersRegexPattern = new Regex(@"\?|&|\*|""|<|>|\||#|%|\^|;", RegexOptions.Compiled);
#else
		private static readonly Regex InvalidCharactersRegexPattern = new Regex(@"\?|&|\*|""|<|>|\||#|%|\^|;|:", RegexOptions.Compiled);
#endif

		public string SolutionFile()
		{
			return SolutionFileImpl();
		}

		internal virtual string SolutionFileImpl()
		{
			return Path.Combine(ProjectDirectory.NormalizePathSeparators(), $"{InvalidCharactersRegexPattern.Replace(m_ProjectName, "_")}.sln");
		}

		internal virtual string FallbackSolutionFile()
		{
			return null;
		}

		internal string GetLangVersion(Assembly assembly)
		{
			var targetLanguageVersion = "latest"; // danger: latest is not the same absolute value depending on the VS version.
			if (m_CurrentInstallation != null)
			{
				var vsLanguageSupport = m_CurrentInstallation.LatestLanguageVersionSupported;
				var unityLanguageSupport = UnityInstallation.LatestLanguageVersionSupported(assembly);

				// Use the minimal supported version between VS and Unity, so that compilation will work in both
				targetLanguageVersion = (vsLanguageSupport <= unityLanguageSupport ? vsLanguageSupport : unityLanguageSupport).ToString(2); // (major, minor) only
			}

			return targetLanguageVersion;
		}

		private static IEnumerable<string> GetOtherArguments(ResponseFileData[] responseFilesData, HashSet<string> names)
		{
			var lines = responseFilesData
				.SelectMany(x => x.OtherArguments)
				.Where(l => !string.IsNullOrEmpty(l))
				.Select(l => l.Trim())
				.Where(l => l.StartsWith("/") || l.StartsWith("-"));

			foreach (var argument in lines)
			{
				var index = argument.IndexOf(":", StringComparison.Ordinal);
				if (index == -1)
					continue;

				var key = argument
					.Substring(1, index - 1)
					.Trim();
				
				if (!names.Contains(key))
					continue;

				if (argument.Length <= index)
					continue;

				yield return argument
					.Substring(index + 1)
					.Trim();
			}
		}

		private void SetAnalyzerAndSourceGeneratorProperties(Assembly assembly, ResponseFileData[] responseFilesData, ProjectProperties properties)
		{
			if (m_CurrentInstallation == null || !m_CurrentInstallation.SupportsAnalyzers)
				return;

			// Analyzers provided by VisualStudio
			var analyzers = new List<string>(m_CurrentInstallation.GetAnalyzers());
			var additionalFilePaths = new List<string>();
			var rulesetPath = string.Empty;
			var analyzerConfigPath = string.Empty;
			var compilerOptions = assembly.compilerOptions;

#if UNITY_2020_2_OR_NEWER
			// Analyzers + ruleset provided by Unity
			analyzers.AddRange(compilerOptions.RoslynAnalyzerDllPaths);
			rulesetPath = compilerOptions.RoslynAnalyzerRulesetPath;
#endif

			// We have support in 2021.3, 2022.2 but without a backport in 2022.1
#if UNITY_2021_3
			// Unfortunately those properties were introduced in a patch release of 2021.3, so not found in 2021.3.2f1 for example
			var scoType = compilerOptions.GetType();
			var afpProperty = scoType.GetProperty("RoslynAdditionalFilePaths");
			var acpProperty = scoType.GetProperty("AnalyzerConfigPath");
			additionalFilePaths.AddRange(afpProperty?.GetValue(compilerOptions) as string[] ?? Array.Empty<string>());
			analyzerConfigPath = acpProperty?.GetValue(compilerOptions) as string ?? analyzerConfigPath;
#elif UNITY_2022_2_OR_NEWER
			additionalFilePaths.AddRange(compilerOptions.RoslynAdditionalFilePaths);
			analyzerConfigPath = compilerOptions.AnalyzerConfigPath;
#endif

			// Analyzers and additional files provided by csc.rsp
			analyzers.AddRange(GetOtherArguments(responseFilesData, new HashSet<string>(new[] { "analyzer", "a" })));
			additionalFilePaths.AddRange(GetOtherArguments(responseFilesData, new HashSet<string>(new[] { "additionalfile" })));

			properties.RulesetPath = ToNormalizedPath(rulesetPath);
			properties.Analyzers = ToNormalizedPaths(analyzers);
			properties.AnalyzerConfigPath = ToNormalizedPath(analyzerConfigPath);
			properties.AdditionalFilePaths = ToNormalizedPaths(additionalFilePaths);
		}

		private string ToNormalizedPath(string path)
		{
			return path
				.MakeAbsolutePath()
				.NormalizePathSeparators();
		}

		private string[] ToNormalizedPaths(IEnumerable<string> values)
		{
			return values
				.Where(a => !string.IsNullOrEmpty(a))
				.Select(a => ToNormalizedPath(a))
				.Distinct()
				.ToArray();
		}

		private void ProjectHeader(
			Assembly assembly,
			ResponseFileData[] responseFilesData,
			out StringBuilder headerBuilder
		)
		{
			var projectType = ProjectTypeOf(assembly.name);

			var projectProperties = new ProjectProperties
			{
				ProjectGuid = ProjectGuid(assembly),
				LangVersion = GetLangVersion(assembly),
				AssemblyName = assembly.name,
				RootNamespace = GetRootNamespace(assembly),
				OutputPath = assembly.outputPath,
				// RSP alterable
				Defines = assembly.defines.Concat(responseFilesData.SelectMany(x => x.Defines)).Distinct().ToArray(),
				Unsafe = assembly.compilerOptions.AllowUnsafeCode | responseFilesData.Any(x => x.Unsafe),
				// VSTU Flavoring
				FlavoringProjectType = projectType + ":" + (int)projectType,
				FlavoringBuildTarget = EditorUserBuildSettings.activeBuildTarget + ":" + (int)EditorUserBuildSettings.activeBuildTarget,
				FlavoringUnityVersion = Application.unityVersion,
				FlavoringPackageVersion = VisualStudioIntegration.PackageVersion(),
			};

			SetAnalyzerAndSourceGeneratorProperties(assembly, responseFilesData, projectProperties);

			GetProjectHeader(projectProperties, out headerBuilder);
		}

		private enum ProjectType
		{
			GamePlugins = 3,
			Game = 1,
			EditorPlugins = 7,
			Editor = 5,
		}

		private static ProjectType ProjectTypeOf(string fileName)
		{
			var plugins = fileName.Contains("firstpass");
			var editor = fileName.Contains("Editor");

			if (plugins && editor)
				return ProjectType.EditorPlugins;
			if (plugins)
				return ProjectType.GamePlugins;
			if (editor)
				return ProjectType.Editor;

			return ProjectType.Game;
		}

		internal virtual void GetProjectHeader(ProjectProperties properties, out StringBuilder headerBuilder)
		{
			headerBuilder = default;
		}

		internal static void GetProjectHeaderConfigurations(ProjectProperties properties, StringBuilder headerBuilder)
		{
			const string NoWarn = "0169;USG0001";

			headerBuilder.Append(@"  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "">").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <DebugSymbols>true</DebugSymbols>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <DebugType>full</DebugType>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <Optimize>false</Optimize>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <OutputPath>").Append(properties.OutputPath).Append(@"</OutputPath>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <DefineConstants>").Append(string.Join(";", properties.Defines)).Append(@"</DefineConstants>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <ErrorReport>prompt</ErrorReport>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <WarningLevel>4</WarningLevel>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <NoWarn>").Append(NoWarn).Append("</NoWarn>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <AllowUnsafeBlocks>").Append(properties.Unsafe).Append(@"</AllowUnsafeBlocks>").Append(k_WindowsNewline);
			headerBuilder.Append(@"  </PropertyGroup>").Append(k_WindowsNewline);
			headerBuilder.Append(@"  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' "">").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <DebugType>pdbonly</DebugType>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <Optimize>true</Optimize>").Append(k_WindowsNewline);
			headerBuilder.Append($"    <OutputPath>{@"Temp\bin\Release\".NormalizePathSeparators()}</OutputPath>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <ErrorReport>prompt</ErrorReport>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <WarningLevel>4</WarningLevel>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <NoWarn>").Append(NoWarn).Append("</NoWarn>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <AllowUnsafeBlocks>").Append(properties.Unsafe).Append(@"</AllowUnsafeBlocks>").Append(k_WindowsNewline);
			headerBuilder.Append(@"  </PropertyGroup>").Append(k_WindowsNewline);
		}

		internal static void GetProjectHeaderAnalyzers(ProjectProperties properties, StringBuilder headerBuilder)
		{
			if (!string.IsNullOrEmpty(properties.RulesetPath))
			{
				headerBuilder.Append(@"  <PropertyGroup>").Append(k_WindowsNewline);
				headerBuilder.Append(@"    <CodeAnalysisRuleSet>").Append(properties.RulesetPath).Append(@"</CodeAnalysisRuleSet>").Append(k_WindowsNewline);
				headerBuilder.Append(@"  </PropertyGroup>").Append(k_WindowsNewline);
			}

			if (properties.Analyzers.Any())
			{
				headerBuilder.Append(@"  <ItemGroup>").Append(k_WindowsNewline);
				foreach (var analyzer in properties.Analyzers)
				{
					headerBuilder.Append(@"    <Analyzer Include=""").Append(analyzer).Append(@""" />").Append(k_WindowsNewline);
				}
				headerBuilder.Append(@"  </ItemGroup>").Append(k_WindowsNewline);
			}

			if (!string.IsNullOrEmpty(properties.AnalyzerConfigPath))
			{
				headerBuilder.Append(@"  <ItemGroup>").Append(k_WindowsNewline);
				headerBuilder.Append(@"    <EditorConfigFiles Include=""").Append(properties.AnalyzerConfigPath).Append(@""" />").Append(k_WindowsNewline);
				headerBuilder.Append(@"  </ItemGroup>").Append(k_WindowsNewline);
			}

			if (properties.AdditionalFilePaths.Any())
			{
				headerBuilder.Append(@"  <ItemGroup>").Append(k_WindowsNewline);
				foreach (var additionalFile in properties.AdditionalFilePaths)
				{
					headerBuilder.Append(@"    <AdditionalFiles Include=""").Append(additionalFile).Append(@""" />").Append(k_WindowsNewline);
				}
				headerBuilder.Append(@"  </ItemGroup>").Append(k_WindowsNewline);
			}
		}

		internal void GetProjectHeaderVstuFlavoring(ProjectProperties properties, StringBuilder headerBuilder, bool includeProjectTypeGuids = true)
		{
			// Flavoring
			headerBuilder.Append(@"  <PropertyGroup>").Append(k_WindowsNewline);

			if (includeProjectTypeGuids)
			{
				headerBuilder.Append(@"    <ProjectTypeGuids>{E097FAD1-6243-4DAD-9C02-E9B9EFC3FFC1};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</ProjectTypeGuids>").Append(k_WindowsNewline);
			}

			headerBuilder.Append(@"    <UnityProjectGenerator>Package</UnityProjectGenerator>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <UnityProjectGeneratorVersion>").Append(properties.FlavoringPackageVersion).Append(@"</UnityProjectGeneratorVersion>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <UnityProjectGeneratorStyle>").Append(StyleName).Append("</UnityProjectGeneratorStyle>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <UnityProjectType>").Append(properties.FlavoringProjectType).Append(@"</UnityProjectType>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <UnityBuildTarget>").Append(properties.FlavoringBuildTarget).Append(@"</UnityBuildTarget>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <UnityVersion>").Append(properties.FlavoringUnityVersion).Append(@"</UnityVersion>").Append(k_WindowsNewline);
			headerBuilder.Append(@"  </PropertyGroup>").Append(k_WindowsNewline);
		}

		internal virtual void GetProjectFooter(StringBuilder footerBuilder)
		{
		}

		private static string GetSolutionText()
		{
			return string.Join(k_WindowsNewline,
			@"",
			@"Microsoft Visual Studio Solution File, Format Version {0}",
			@"# Visual Studio {1}",
			@"{2}",
			@"Global",
			@"    GlobalSection(SolutionConfigurationPlatforms) = preSolution",
			@"        Debug|Any CPU = Debug|Any CPU",
			@"        Release|Any CPU = Release|Any CPU",
			@"    EndGlobalSection",
			@"    GlobalSection(ProjectConfigurationPlatforms) = postSolution",
			@"{3}",
			@"    EndGlobalSection",
			@"{4}",
			@"EndGlobal",
			@"").Replace("    ", "\t");
		}

		internal virtual void SyncSolution(IEnumerable<Assembly> assemblies)
		{
			if (InvalidCharactersRegexPattern.IsMatch(ProjectDirectory))
				Debug.LogWarning("Project path contains special characters, which can be an issue when opening Visual Studio");

			var solutionFile = SolutionFile();
			var previousSolutionFile = m_FileIOProvider.Exists(solutionFile) ? solutionFile : FallbackSolutionFile();
			var previousSolution = !string.IsNullOrEmpty(previousSolutionFile) && m_FileIOProvider.Exists(previousSolutionFile)
				? SolutionParser.ParseSolutionFile(previousSolutionFile, m_FileIOProvider)
				: null;
			SyncSolutionFileIfNotChanged(solutionFile, SolutionText(assemblies, previousSolution));
		}

		internal virtual string SolutionText(IEnumerable<Assembly> assemblies, Solution previousSolution = null)
		{
			const string fileversion = "12.00";
			const string vsversion = "15";

			var projects = GetSolutionProjects(assemblies, previousSolution);
			var properties = previousSolution != null ? previousSolution.Properties : null;

			string propertiesText = GetPropertiesText(properties);
			string projectEntriesText = GetProjectEntriesText(projects);

			// do not generate configurations for SolutionFolders
			var configurableProjects = projects.Where(p => !p.IsSolutionFolderProjectFactory());
			string projectConfigurationsText = string.Join(k_WindowsNewline, configurableProjects.Select(p => GetProjectActiveConfigurations(p.ProjectGuid)).ToArray());

			return string.Format(GetSolutionText(), fileversion, vsversion, projectEntriesText, projectConfigurationsText, propertiesText);
		}

		internal List<SolutionProjectEntry> GetSolutionProjects(IEnumerable<Assembly> assemblies, Solution previousSolution = null)
		{
			var relevantAssemblies = RelevantAssembliesForMode(assemblies);
			var generatedProjects = ToProjectEntries(relevantAssemblies).ToList();

			var projects = new List<SolutionProjectEntry>();
			projects.AddRange(generatedProjects);

			if (previousSolution != null)
			{
				var externalProjects = previousSolution.Projects
					.Where(p => p.IsSolutionFolderProjectFactory() || !FileUtility.IsFileInProjectRootDirectory(p.FileName))
					.Where(p => generatedProjects.All(gp => gp.FileName != p.FileName));

				projects.AddRange(externalProjects);
			}

			return projects;
		}

		private static IEnumerable<Assembly> RelevantAssembliesForMode(IEnumerable<Assembly> assemblies)
		{
			return assemblies.Where(i => ScriptingLanguage.CSharp == ScriptingLanguageFor(i));
		}

		private static string GetPropertiesText(SolutionProperties[] array)
		{
			if (array == null || array.Length == 0)
			{
				// HideSolution by default
				array = new[] {
					new SolutionProperties() {
						Name = "SolutionProperties",
						Type = "preSolution",
						Entries = new List<KeyValuePair<string,string>>() { new KeyValuePair<string, string> ("HideSolutionNode", "FALSE") }
					}
				};
			}
			var result = new StringBuilder();

			for (var i = 0; i < array.Length; i++)
			{
				if (i > 0)
					result.Append(k_WindowsNewline);

				var properties = array[i];

				result.Append($"\tGlobalSection({properties.Name}) = {properties.Type}");
				result.Append(k_WindowsNewline);

				foreach (var entry in properties.Entries)
				{
					result.Append($"\t\t{entry.Key} = {entry.Value}");
					result.Append(k_WindowsNewline);
				}

				result.Append("\tEndGlobalSection");
			}

			return result.ToString();
		}

		/// <summary>
		/// Get a Project("{guid}") = "MyProject", "MyProject.unityproj", "{projectguid}"
		/// entry for each relevant language
		/// </summary>
		private string GetProjectEntriesText(IEnumerable<SolutionProjectEntry> entries)
		{
			var projectEntries = entries.Select(entry => string.Format(
				m_SolutionProjectEntryTemplate,
				entry.ProjectFactoryGuid, entry.Name, entry.FileName, entry.ProjectGuid, entry.Metadata
			));

			return string.Join(k_WindowsNewline, projectEntries.ToArray());
		}

		private IEnumerable<SolutionProjectEntry> ToProjectEntries(IEnumerable<Assembly> assemblies)
		{
			foreach (var assembly in assemblies)
				yield return new SolutionProjectEntry()
				{
					ProjectFactoryGuid = SolutionGuid(assembly),
					Name = assembly.name,
					FileName = Path.GetFileName(ProjectFile(assembly)),
					ProjectGuid = ProjectGuid(assembly),
					Metadata = k_WindowsNewline
				};
		}

		/// <summary>
		/// Generate the active configuration string for a given project guid
		/// </summary>
		private string GetProjectActiveConfigurations(string projectGuid)
		{
			return string.Format(
				m_SolutionProjectConfigurationTemplate,
				projectGuid);
		}

		internal string EscapedRelativePathFor(string file, out UnityEditor.PackageManager.PackageInfo packageInfo)
		{
			var projectDir = ProjectDirectory.NormalizePathSeparators();
			file = file.NormalizePathSeparators();
			var path = SkipPathPrefix(file, projectDir);

			packageInfo = m_AssemblyNameProvider.FindForAssetPath(path.NormalizeWindowsToUnix());
			if (packageInfo != null)
			{
				// We have to normalize the path, because the PackageManagerRemapper assumes
				// dir seperators will be os specific.
				var absolutePath = Path.GetFullPath(path.NormalizePathSeparators());
				path = SkipPathPrefix(absolutePath, projectDir);
			}

			return XmlFilename(path);
		}

		internal static string SkipPathPrefix(string path, string prefix)
		{
			if (path.StartsWith($"{prefix}{Path.DirectorySeparatorChar}") && (path.Length > prefix.Length))
				return path.Substring(prefix.Length + 1);
			return path;
		}

		internal static string GetProjectExtension()
		{
			return ".csproj";
		}

		internal string ProjectGuid(string assemblyName)
		{
			return m_GUIDGenerator.ProjectGuid(m_ProjectName, assemblyName);
		}

		internal string ProjectGuid(Assembly assembly)
		{
			return ProjectGuid(m_AssemblyNameProvider.GetAssemblyName(assembly.outputPath, assembly.name));
		}

		private string SolutionGuid(Assembly assembly)
		{
			return m_GUIDGenerator.SolutionGuid(m_ProjectName, ScriptingLanguageFor(assembly));
		}

		private static string GetRootNamespace(Assembly assembly)
		{
#if UNITY_2020_2_OR_NEWER
			return assembly.rootNamespace;
#else
			return EditorSettings.projectGenerationRootNamespace;
#endif
		}
	}

	public static class SolutionGuidGenerator
	{
		public static string GuidForProject(string projectName)
		{
			return ComputeGuidHashFor(projectName + "salt");
		}

		public static string GuidForSolution(string projectName, ScriptingLanguage language)
		{
			if (language == ScriptingLanguage.CSharp)
			{
				// GUID for a C# class library: http://www.codeproject.com/Reference/720512/List-of-Visual-Studio-Project-Type-GUIDs
				return "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC";
			}

			return ComputeGuidHashFor(projectName);
		}

		private static string ComputeGuidHashFor(string input)
		{
			var hash = MD5.Create().ComputeHash(Encoding.Default.GetBytes(input));
			return HashAsGuid(HashToString(hash));
		}

		private static string HashAsGuid(string hash)
		{
			var guid = hash.Substring(0, 8) + "-" + hash.Substring(8, 4) + "-" + hash.Substring(12, 4) + "-" + hash.Substring(16, 4) + "-" + hash.Substring(20, 12);
			return guid.ToUpper();
		}

		private static string HashToString(byte[] bs)
		{
			var sb = new StringBuilder();
			foreach (byte b in bs)
				sb.Append(b.ToString("x2"));
			return sb.ToString();
		}
	}
}
