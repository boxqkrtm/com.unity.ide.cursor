using System;
using System.IO;

using NUnit.Framework;
using SimpleJSON;

namespace Microsoft.Unity.VisualStudio.Editor.Tests
{
    public class SlnxSettingsTests
    {
        [Test]
        public void CursorSettingsIncludeSlnxNesting()
        {
            AssertSlnxSettings(new VisualStudioCursorInstallation());
        }

        [Test]
        public void CodiumSettingsIncludeSlnxNesting()
        {
            AssertSlnxSettings(new VisualStudioCodiumInstallation());
        }

        [Test]
        public void CursorSettingsPatchRemovesSlnxExclude()
        {
            AssertSlnxExcludeIsRemoved(new VisualStudioCursorInstallation());
        }

        [Test]
        public void CodiumSettingsPatchRemovesSlnxExclude()
        {
            AssertSlnxExcludeIsRemoved(new VisualStudioCodiumInstallation());
        }

        [Test]
        public void CursorSettingsPatchUpdatesDefaultSolutionWithoutExcludes()
        {
            AssertDefaultSolutionIsUpdatedWithoutExcludes(new VisualStudioCursorInstallation());
        }

        [Test]
        public void CodiumSettingsPatchUpdatesDefaultSolutionWithoutExcludes()
        {
            AssertDefaultSolutionIsUpdatedWithoutExcludes(new VisualStudioCodiumInstallation());
        }

        private static void AssertSlnxSettings(VisualStudioInstallation installation)
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), $"CursorSlnxSettingsTests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDirectory);

            try
            {
                installation.CreateExtraFiles(tempDirectory);

                var settingsFile = Path.Combine(tempDirectory, ".vscode", "settings.json");
                var settings = File.ReadAllText(settingsFile);
                var settingsJson = JSONNode.Parse(settings);

                Assert.IsTrue(settingsJson["explorer.fileNesting.enabled"].AsBool);
                Assert.AreEqual("*.csproj", settingsJson["explorer.fileNesting.patterns"]["*.slnx"].Value);
                Assert.AreEqual(
                    Path.GetFileName(installation.ProjectGenerator.SolutionFile()),
                    settingsJson["dotnet.defaultSolution"].Value);
            }
            finally
            {
                Directory.Delete(tempDirectory, true);
            }
        }

        private static void AssertSlnxExcludeIsRemoved(VisualStudioInstallation installation)
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), $"CursorSlnxSettingsTests-{Guid.NewGuid():N}");
            var vscodeDirectory = Path.Combine(tempDirectory, ".vscode");
            Directory.CreateDirectory(vscodeDirectory);

            try
            {
                var settingsFile = Path.Combine(vscodeDirectory, "settings.json");
                File.WriteAllText(
                    settingsFile,
                    "{\r\n" +
                    "  \"files.exclude\": {\r\n" +
                    "    \"**/*.slnx\": true,\r\n" +
                    "    \"**/*.asset\": true\r\n" +
                    "  },\r\n" +
                    "  \"dotnet.defaultSolution\": \"Old.sln\"\r\n" +
                    "}\r\n");

                installation.CreateExtraFiles(tempDirectory);

                var settings = File.ReadAllText(settingsFile);
                StringAssert.DoesNotContain("\"**/*.slnx\"", settings);
                StringAssert.Contains("\"**/*.asset\"", settings);
                StringAssert.Contains(
                    $"\"dotnet.defaultSolution\": \"{Path.GetFileName(installation.ProjectGenerator.SolutionFile())}\"",
                    settings);
            }
            finally
            {
                Directory.Delete(tempDirectory, true);
            }
        }

        private static void AssertDefaultSolutionIsUpdatedWithoutExcludes(VisualStudioInstallation installation)
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), $"CursorSlnxSettingsTests-{Guid.NewGuid():N}");
            var vscodeDirectory = Path.Combine(tempDirectory, ".vscode");
            Directory.CreateDirectory(vscodeDirectory);

            try
            {
                var settingsFile = Path.Combine(vscodeDirectory, "settings.json");
                File.WriteAllText(settingsFile, "{\r\n  \"dotnet.defaultSolution\": \"Old.sln\"\r\n}\r\n");

                installation.CreateExtraFiles(tempDirectory);

                var settings = File.ReadAllText(settingsFile);
                StringAssert.Contains(
                    $"\"dotnet.defaultSolution\": \"{Path.GetFileName(installation.ProjectGenerator.SolutionFile())}\"",
                    settings);
            }
            finally
            {
                Directory.Delete(tempDirectory, true);
            }
        }
    }
}
