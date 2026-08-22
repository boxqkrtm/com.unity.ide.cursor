using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

using NUnit.Framework;
using UnityEditor.Compilation;

namespace Microsoft.Unity.VisualStudio.Editor.Tests
{
    public class SlnxGenerationTests
    {
        [Test]
        public void SdkStyleSolutionFileUsesSlnxExtension()
        {
            var generator = new SdkStyleProjectGeneration();

            Assert.AreEqual(".slnx", Path.GetExtension(generator.SolutionFile()));
        }

        [Test]
        public void SolutionParserReadsSlnxProjectPaths()
        {
            const string solutionFile = "CursorProject.slnx";
            const string solutionText =
                "<Solution>\r\n" +
                "  <Project Path=\"Assembly-CSharp.csproj\" />\r\n" +
                "  <Project Path=\"Packages/Tools/Tools.csproj\" />\r\n" +
                "</Solution>\r\n";
            var fileIo = new InMemoryFileIo();
            fileIo.WriteAllText(solutionFile, solutionText);

            var solution = SolutionParser.ParseSolutionFile(solutionFile, fileIo);

            Assert.AreEqual(2, solution.Projects.Length);
            Assert.AreEqual("Assembly-CSharp.csproj", solution.Projects[0].FileName);
            Assert.AreEqual("Packages/Tools/Tools.csproj", solution.Projects[1].FileName);
            Assert.AreEqual(0, solution.Properties.Length);
        }

        [Test]
        public void SdkStyleSyncDeletesLegacySolution()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), $"CursorSlnxTests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDirectory);

            try
            {
                var generator = new SdkStyleProjectGeneration();
                SetProjectDirectory(generator, tempDirectory);
                var solutionFile = generator.SolutionFile();
                var legacySolutionFile = Path.ChangeExtension(solutionFile, ".sln");
                File.WriteAllText(legacySolutionFile, "legacy solution");

                InvokeSyncSolution(generator);

                Assert.IsFalse(File.Exists(legacySolutionFile));
            }
            finally
            {
                Directory.Delete(tempDirectory, true);
            }
        }

        [Test]
        public void SdkStyleSyncWritesXmlSolutionWithProjectPaths()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), $"CursorSlnxTests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDirectory);

            try
            {
                var generator = new SdkStyleProjectGeneration();
                SetProjectDirectory(generator, tempDirectory);

                InvokeSyncSolution(generator, new[] { CreateAssembly() });

                var solutionText = File.ReadAllText(generator.SolutionFile());
                StringAssert.StartsWith("<Solution>", solutionText);
                var solution = XDocument.Parse(solutionText);
                Assert.AreEqual("Solution", solution.Root.Name.LocalName);
                Assert.AreEqual("Test.csproj", solution.Root.Element("Project").Attribute("Path").Value);
            }
            finally
            {
                Directory.Delete(tempDirectory, true);
            }
        }

        [Test]
        public void SdkStyleSolutionEscapesProjectPaths()
        {
            var generator = new SdkStyleProjectGeneration();

            var solutionText = generator.SolutionText(new[] { CreateAssembly("Test & Tools") });
            var solution = XDocument.Parse(solutionText);

            Assert.AreEqual("Test & Tools.csproj", solution.Root.Element("Project").Attribute("Path").Value);
            StringAssert.Contains("Test &amp; Tools.csproj", solutionText);
        }

        [Test]
        public void SdkStyleMigrationPreservesExternalProjects()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), $"CursorSlnxTests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDirectory);

            try
            {
                var generator = new SdkStyleProjectGeneration();
                SetProjectDirectory(generator, tempDirectory);
                var legacySolutionFile = Path.ChangeExtension(generator.SolutionFile(), ".sln");
                File.WriteAllText(
                    legacySolutionFile,
                    "Microsoft Visual Studio Solution File, Format Version 12.00\r\n" +
                    "# Visual Studio 15\r\n" +
                    "Project(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"ExternalTool\", " +
                    "\"..\\External\\ExternalTool.csproj\", \"{11111111-1111-1111-1111-111111111111}\"\r\n" +
                    "EndProject\r\n" +
                    "Project(\"{2150E333-8FDC-42A3-9474-1A3956D46DE8}\") = \"Tools\", \"Tools\", " +
                    "\"{22222222-2222-2222-2222-222222222222}\"\r\n" +
                    "EndProject\r\n" +
                    "Global\r\nEndGlobal\r\n");

                InvokeSyncSolution(generator, new[] { CreateAssembly() });

                var solution = XDocument.Load(generator.SolutionFile());
                var projectPaths = new List<string>();
                foreach (var project in solution.Root.Elements("Project"))
                {
                    projectPaths.Add(project.Attribute("Path").Value);
                }

                CollectionAssert.Contains(projectPaths, "Test.csproj");
                CollectionAssert.Contains(projectPaths, "..\\External\\ExternalTool.csproj");
                CollectionAssert.DoesNotContain(projectPaths, "Tools");
                Assert.IsFalse(File.Exists(legacySolutionFile));
            }
            finally
            {
                Directory.Delete(tempDirectory, true);
            }
        }

        [Test]
        public void SdkStyleMigrationKeepsLegacySolutionWhenSlnxWriteFails()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), $"CursorSlnxTests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDirectory);

            try
            {
                var generator = new SdkStyleProjectGeneration();
                SetProjectDirectory(generator, tempDirectory);
                var legacySolutionFile = Path.ChangeExtension(generator.SolutionFile(), ".sln");
                File.WriteAllText(
                    legacySolutionFile,
                    "Microsoft Visual Studio Solution File, Format Version 12.00\r\n" +
                    "# Visual Studio 15\r\nGlobal\r\nEndGlobal\r\n");
                Directory.CreateDirectory(generator.SolutionFile());

                Assert.Throws<TargetInvocationException>(() => InvokeSyncSolution(generator));

                Assert.IsTrue(File.Exists(legacySolutionFile));
            }
            finally
            {
                Directory.Delete(tempDirectory, true);
            }
        }

        [Test]
        public void SdkStyleSyncStillWritesSlnxWhenLegacySolutionIsReadOnly()
        {
            if (Path.DirectorySeparatorChar != '\\')
                Assert.Ignore("Read-only files can be deleted on this platform.");

            var tempDirectory = Path.Combine(Path.GetTempPath(), $"CursorSlnxTests-{Guid.NewGuid():N}");
            string legacySolutionFile = null;
            Directory.CreateDirectory(tempDirectory);

            try
            {
                var generator = new SdkStyleProjectGeneration();
                SetProjectDirectory(generator, tempDirectory);
                legacySolutionFile = Path.ChangeExtension(generator.SolutionFile(), ".sln");
                File.WriteAllText(
                    legacySolutionFile,
                    "Microsoft Visual Studio Solution File, Format Version 12.00\r\n" +
                    "# Visual Studio 15\r\nGlobal\r\nEndGlobal\r\n");
                File.SetAttributes(legacySolutionFile, FileAttributes.ReadOnly);

                Assert.DoesNotThrow(() => InvokeSyncSolution(generator));

                Assert.IsTrue(File.Exists(generator.SolutionFile()));
                Assert.IsTrue(File.Exists(legacySolutionFile));
            }
            finally
            {
                if (!string.IsNullOrEmpty(legacySolutionFile) && File.Exists(legacySolutionFile))
                    File.SetAttributes(legacySolutionFile, FileAttributes.Normal);
                Directory.Delete(tempDirectory, true);
            }
        }

        private static UnityEditor.Compilation.Assembly CreateAssembly(string name = "Test")
        {
            var options = new ScriptCompilerOptions();
            return new UnityEditor.Compilation.Assembly(
                name,
                $"Temp/{name}.dll",
                new[] { $"Assets/{name}.cs" },
                new string[0],
                new UnityEditor.Compilation.Assembly[0],
                new string[0],
                AssemblyFlags.None,
#if UNITY_2020_2_OR_NEWER
                options,
                string.Empty);
#else
                options);
#endif
        }

        private static void SetProjectDirectory(ProjectGeneration generator, string projectDirectory)
        {
            var field = typeof(ProjectGeneration).GetField(
                "<ProjectDirectory>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field);
            field.SetValue(generator, projectDirectory);
        }

        private static void InvokeSyncSolution(
            ProjectGeneration generator,
            UnityEditor.Compilation.Assembly[] assemblies = null)
        {
            MethodInfo method = null;
            var type = generator.GetType();
            while (type != null && method == null)
            {
                method = type.GetMethod(
                    "SyncSolution",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                type = type.BaseType;
            }

            Assert.IsNotNull(method);
            method.Invoke(
                generator,
                new object[] { assemblies ?? Array.Empty<UnityEditor.Compilation.Assembly>() });
        }

        private sealed class InMemoryFileIo : IFileIO
        {
            private readonly Dictionary<string, string> files = new Dictionary<string, string>();

            public bool Exists(string fileName)
            {
                return files.ContainsKey(fileName);
            }

            public string ReadAllText(string fileName)
            {
                return files[fileName];
            }

            public void WriteAllText(string fileName, string content)
            {
                files[fileName] = content;
            }
        }
    }
}
