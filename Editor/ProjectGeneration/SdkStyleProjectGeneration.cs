/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Unity Technologies.
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.Compilation;
using UnityEngine;

namespace Microsoft.Unity.VisualStudio.Editor
{
	internal class SdkStyleProjectGeneration : ProjectGeneration
	{
		internal override string StyleName => "SDK";

		internal class SdkStyleAssemblyNameProvider : AssemblyNameProvider
		{
			// disable PlayerGeneration with SdkStyle projects
			internal override ProjectGenerationFlag ProjectGenerationFlagImpl => base.ProjectGenerationFlagImpl & ~ProjectGenerationFlag.PlayerAssemblies;
		}

		public SdkStyleProjectGeneration() : base(
			Directory.GetParent(Application.dataPath)?.FullName,
			new SdkStyleAssemblyNameProvider(),
			new FileIOProvider(),
			new GUIDProvider())
		{
		}

		internal static readonly string[] SupportedCapabilities = new string[]
		{
			"Unity",
		};

		internal static readonly string[] UnsupportedCapabilities = new string[]
		{
			"LaunchProfiles",
			"SharedProjectReferences",
			"ReferenceManagerSharedProjects",
			"ProjectReferences",
			"ReferenceManagerProjects",
			"COMReferences",
			"ReferenceManagerCOM",
			"AssemblyReferences",
			"ReferenceManagerAssemblies",
		};

		internal override void GetProjectHeader(ProjectProperties properties, out StringBuilder headerBuilder)
		{
			headerBuilder = new StringBuilder();

			headerBuilder.Append(@"<Project ToolsVersion=""Current"">").Append(k_WindowsNewline);
			headerBuilder.Append(@"  <!-- Generated file, do not modify, your changes will be overwritten (use AssetPostprocessor.OnGeneratedCSProject) -->").Append(k_WindowsNewline);

			// Prevent circular dependency issues see https://github.com/microsoft/vscode-dotnettools/issues/401
			// We need a dedicated subfolder for each project in obj, else depending on the build order, nuget cache files could be overwritten
			// We need to do this before common.props, else we'll have a MSB3539 The value of the property "BaseIntermediateOutputPath" was modified after it was used by MSBuild
			headerBuilder.Append(@"  <PropertyGroup>").Append(k_WindowsNewline);
			headerBuilder.Append($"    <BaseIntermediateOutputPath>{@"Temp\obj\$(Configuration)\$(MSBuildProjectName)".NormalizePathSeparators()}</BaseIntermediateOutputPath>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <IntermediateOutputPath>$(BaseIntermediateOutputPath)</IntermediateOutputPath>").Append(k_WindowsNewline);
			headerBuilder.Append(@"  </PropertyGroup>").Append(k_WindowsNewline);

			// Supported capabilities
			GetCapabilityBlock(headerBuilder, "Sdk.props", "Include", SupportedCapabilities);
		
			headerBuilder.Append(@"  <PropertyGroup>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <EnableDefaultItems>false</EnableDefaultItems>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <LangVersion>").Append(properties.LangVersion).Append(@"</LangVersion>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <Configurations>Debug;Release</Configurations>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <RootNamespace>").Append(properties.RootNamespace).Append(@"</RootNamespace>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <OutputType>Library</OutputType>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <AppDesignerFolder>Properties</AppDesignerFolder>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <AssemblyName>").Append(properties.AssemblyName).Append(@"</AssemblyName>").Append(k_WindowsNewline);
			// In the end, given we use NoConfig/NoStdLib (see below), hardcoding the target framework version will have no impact, even when targeting netstandard/net48 from Unity.
			// But with SDK style we use netstandard2.1 (net471 for legacy), so 3rd party tools will not fail to work when .NETFW reference assemblies are not installed.
			// Unity already selected proper API surface through referenced DLLs for us.
			headerBuilder.Append(@"    <TargetFramework>netstandard2.1</TargetFramework>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <BaseDirectory>.</BaseDirectory>").Append(k_WindowsNewline);
			headerBuilder.Append(@"  </PropertyGroup>").Append(k_WindowsNewline);

			GetProjectHeaderConfigurations(properties, headerBuilder);

			// Explicit references
			headerBuilder.Append(@"  <PropertyGroup>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <NoStandardLibraries>true</NoStandardLibraries>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <NoStdLib>true</NoStdLib>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <NoConfig>true</NoConfig>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <DisableImplicitFrameworkReferences>true</DisableImplicitFrameworkReferences>").Append(k_WindowsNewline);
			headerBuilder.Append(@"    <MSBuildWarningsAsMessages>MSB3277</MSBuildWarningsAsMessages>").Append(k_WindowsNewline);
			headerBuilder.Append(@"  </PropertyGroup>").Append(k_WindowsNewline);

			GetProjectHeaderVstuFlavoring(properties, headerBuilder, false);
			GetProjectHeaderAnalyzers(properties, headerBuilder);
		}

		internal override void AppendProjectReference(Assembly assembly, Assembly reference, StringBuilder projectBuilder)
		{
			// If the current assembly is a Player project, we want to project-reference the corresponding Player project
			var referenceName = m_AssemblyNameProvider.GetAssemblyName(assembly.outputPath, reference.name);
			projectBuilder.Append(@"    <ProjectReference Include=""").Append(referenceName).Append(GetProjectExtension()).Append(@""" />").Append(k_WindowsNewline);
		}

		internal override void GetProjectFooter(StringBuilder footerBuilder)
		{
			// Unsupported capabilities
			GetCapabilityBlock(footerBuilder, "Sdk.targets", "Remove", UnsupportedCapabilities);

			footerBuilder.Append("</Project>").Append(k_WindowsNewline);
		}

		internal static void GetCapabilityBlock(StringBuilder footerBuilder, string import, string attribute, string[] capabilities)
		{
			footerBuilder.Append($@"  <Import Project=""{import}"" Sdk=""Microsoft.NET.Sdk"" />").Append(k_WindowsNewline);
			footerBuilder.Append(@"  <ItemGroup>").Append(k_WindowsNewline);
			foreach (var capability in capabilities)
			{
				footerBuilder.Append($@"    <ProjectCapability {attribute}=""{capability}"" />").Append(k_WindowsNewline);
			}
			footerBuilder.Append(@"  </ItemGroup>").Append(k_WindowsNewline);
		}

		internal override string SolutionFileImpl()
		{
			return base.SolutionFileImpl() + "x";
		}

		internal override string FallbackSolutionFile()
		{
			return base.SolutionFileImpl();
		}

		internal override string SolutionText(IEnumerable<Assembly> assemblies, Solution previousSolution = null)
		{
			var projects = GetSolutionProjects(assemblies, previousSolution);

			var content = new StringBuilder();
			content.Append("<Solution>").Append(k_WindowsNewline);

			foreach (var project in projects)
			{
				if (project.IsSolutionFolderProjectFactory())
					continue;

				content.Append("  ").Append("<Project Path=\"").Append(XmlEscape(project.FileName)).Append("\" />").Append(k_WindowsNewline);
			}

			content.Append("</Solution>").Append(k_WindowsNewline);

			return content.ToString();
		}

		internal override void SyncSolution(IEnumerable<Assembly> assemblies)
		{
			base.SyncSolution(assemblies);

			FileUtility.SafeDelete(FallbackSolutionFile());
		}
	}
}
