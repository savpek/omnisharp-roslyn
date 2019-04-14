﻿using System.Linq;
using System.Threading.Tasks;
using OmniSharp.MSBuild.ProjectFile;
using OmniSharp.MSBuild.Tests;
using TestUtility;
using Xunit;

namespace OmniSharp.Tests
{
    public class MSBuildProjectSystemMiscTests: AbstractMSBuildTestFixture
    {
        public MSBuildProjectSystemMiscTests(Xunit.Abstractions.ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Project_path_is_case_insensitive()
        {
            var projectPath = @"c:\projects\project1\project.csproj";
            var searchProjectPath =  @"c:\Projects\Project1\Project.csproj";

            var collection = new ProjectFileInfoCollection();
            collection.Add(ProjectFileInfo.CreateEmpty(projectPath));

            Assert.True(collection.TryGetValue(searchProjectPath, out var outInfo));
            Assert.NotNull(outInfo);
        }

        [Fact]
        public async Task ProjectFileInfoCollection()
        {
            using (var testProject = await TestAssets.Instance.GetTestProjectAsync("ProjectWithRulesets"))
            using (var host = CreateMSBuildTestHost(testProject.Directory))
            {
                var project = host.Workspace.CurrentSolution.Projects.Single();

                Assert.Contains(project.CompilationOptions.SpecificDiagnosticOptions, x => x.Key == "CA1021");
            }
        }
    }
}