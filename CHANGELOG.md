# Code Editor Package for Cursor

## [2.0.26] - 2025-11-02

Integration:

- Add workspace support

## [2.0.22] - 2023-10-03

Integration:

- Add support for `XDG_DATA_DIRS` and `.desktop` files on Linux for `VS Code` discovery.
- Use compile-time platform-specifics instead of using runtime conditions.

Project generation:

- Suppress `USG0001` warnings.
- Mark referenced assemblies as private (to not copy extra files to output directory when building).
- Add Unity capability to SDK-Style projects.
- Prevent circular dependency errors with SDK-Style projects.


## [2.0.21] - 2023-09-05

Integration:

- Only disable the legacy `com.unity.ide.vscode` package going forward.
- Fix json parsing issues with specific non-UTF code pages.

Project generation:

- Target `netstandard2.1` instead of `netstandard2.0`.
- Set `defaultSolution` in `settings.json`.
- Remove `files.exclude` entries for root `csproj` and `sln` files in `settings.json` when needed.
- Add `vstuc` launch configuration to `launch.json` when needed.
- Add `visualstudiotoolsforunity.vstuc` entry to `extensions.json` when needed.
- You can prevent the package from patching those configuration files by creating a `.vscode/.vstupatchdisable` file.

## [2.0.20] - 2023-06-27

Integration:

- Internal API refactoring.
- Add support for Visual Studio Code.

Project generation:

- Add support for Sdk Style project generation.
- Fix an issue related to missing properties with 2021.3.

## [2.0.18] - 2023-03-17

Integration:

- Performance improvements with `EditorApplication.update` callbacks.
 
Project generation:

- Add extra compiler options for analyzers and source generators.

## [2.0.17] - 2022-12-06

Integration:

- Fix rare deadlocks while discovering or launching Visual Studio on Windows.
- Improve launching Visual Studio on macOs.

Project generation:

- Include analyzers from response files.
- Update supported C# versions.
- Performance improvements.

## [2.0.16] - 2022-06-08

Integration:

- Prevent ADB Refresh while being in safe-mode with a URP project
- Fixed an issue keeping the progress bar visible even after opening a script with Visual Studio. 

## [2.0.15] - 2022-03-21

Integration:

- Improved project generation performance.
- Added support for keeping file/folder structure when working with external packages.
- Fixed project generation not being refreshed when selecting Visual Studio as the preferred external editor.

## [2.0.14] - 2022-01-14

Integration:

- Remove package version checking.

## [2.0.13] - 2022-01-12

Integration:

- Fixed wrong path to analyzers in generated projects when using external packages.
- Fixed selective project generation not creating Analyzer/LangVersion nodes.
- Fixed asmdef references with Player projects.

Documentation:

- Added new documentation including ToC, overview, how to use and images.

## [2.0.12] - 2021-10-20

Integration:

- Do not block asset opening when only a VS instance without a loaded solution is found.
- Only check package version once per Unity session.
- Improved support for Visual Studio For Mac 2022.

## [2.0.11] - 2021-07-01

Integration:

- Added support for Visual Studio and Visual Studio For Mac 2022.
- Fixed an issue when the package was enabled for background processes.

Project generation:

- Use absolute paths for Analyzers and rulesets.

## [2.0.10] - 2021-06-10

Project generation:

- Improved project generation performance when a file is moved, deleted or modified.

Integration:

- Improved Inner-loop performance by avoiding to call the package manager when looking up `vswhere` utility.
- Fixed a network issue preventing the communication between Visual Studio and Unity on Windows.

## [2.0.9] - 2021-05-04

Project generation:

- Added support for CLI.

Integration:

- Improved performance when discovering Visual Studio installations.
- Warn when legacy assemblies are present in the project.
- Warn when the package version is not up-to-date.

## [2.0.8] - 2021-04-09

Project generation:

- Improved generation performance (especially with DOTS enabled projects).
- Improved stability.
- Updated Analyzers lookup strategy.
- Fixed .vsconfig file not generated when using "regenerate all".

Integration:

- Improved automation plugins.

Documentation:

- Open sourced automation plugins.

## [2.0.7] - 2021-02-02

Integration:

- Remove `com.unity.nuget.newtonsoft-json` dependency in favor of the built-in JsonUtility for the VS Test Runner.

## [2.0.6] - 2021-01-20

Project generation:

- Improved language version detection.

Integration:

- Added support for the VS Test Runner.
- Added initial support for displaying asset usage.
- Fixed remaining issues with special characters in file/path.

## [2.0.5] - 2020-10-30

Integration:

- Disable legacy pdb symbol checking for Unity packages.

## [2.0.4] - 2020-10-15

Project generation:

- Added support for embedded Roslyn analyzer DLLs and ruleset files.
- Warn the user when the opened script is not part of the generation scope.
- Warn the user when the selected Visual Studio installation is not found.
- Generate a .vsconfig file to ensure Visual Studio installation is compatible.

Integration:

- Fix automation issues on MacOS, where a new Visual Studio instance is opened every time.

## [2.0.3] - 2020-09-09

Project generation:

- Added C#8 language support.
- Added `UnityProjectGeneratorVersion` property.
- Local and Embedded packages are now selected by default for generation.
- Added support for asmdef root namespace.

Integration:

- When the user disabled auto-refresh in Unity, do not try to force refresh the Asset database.
- Fix Visual Studio detection issues with languages using special characters.


## [2.0.2] - 2020-05-27

- Added support for solution folders.
- Only bind the messenger when the VS editor is selected.
- Warn when unable to create the messenger.
- Fixed an initialization issue triggering legacy code generation.
- Allow package source in assembly to be generated when referenced from asmref.


## [2.0.1] - 2020-03-19

- When Visual Studio installation is compatible with C# 8.0, setup the language version to not prompt the user with unsupported constructs. (So far Unity only supports C# 7.3).
- Use Unity's `TypeCache` to improve project generation speed.
- Properly check for a managed assembly before displaying a warning regarding legacy PDB usage.
- Add support for selective project generation (embedded, local, registry, git, builtin, player).

## [2.0.0] - 2019-11-06

- Improved Visual Studio and Visual Studio for Mac automatic discovery.
- Added support for the VSTU messaging system (start/stop features from Visual Studio).
- Added support for solution roundtrip (preserves references to external projects and solution properties).
- Added support for VSTU Analyzers (requires Visual Studio 2019 16.3, Visual Studio for Mac 8.3).
- Added a warning when using legacy pdb symbol files.
- Fixed issues while Opening Visual Studio on Windows.
- Fixed issues while Opening Visual Studio on Mac.

## [1.1.1] - 2019-05-29

- Fix Bridge assembly loading with non VS2017 editors.

## [1.1.0] - 2019-05-27

- Move internal extension handling to package.

## [1.0.11] - 2019-05-21

- Fix detection of visual studio for mac installation.

## [1.0.10] - 2019-05-04

- Fix ignored comintegration executable.

## [1.0.9] - 2019-03-05

- Updated MonoDevelop support, to pass correct arguments, and not import VSTU plugin.
- Use release build of COMIntegration for Visual Studio.

## [1.0.7] - 2019-04-30

- Ensure asset database is refreshed when generating csproj and solution files.

## [1.0.6] - 2019-04-27

- Add support for generating all csproj files.

## [1.0.5] - 2019-04-18

- Fix relative package paths.
- Fix opening editor on mac.

## [1.0.4] - 2019-04-12

- Fixing null reference issue for callbacks to `AssetPostProcessor`.
- Ensure `Path.GetFullPath` does not get an empty string.

## [1.0.3] - 2019-01-01

### This is the first release of *Unity Package visualstudio_editor*.

- Using the newly created api to integrate Visual Studio with Unity.
