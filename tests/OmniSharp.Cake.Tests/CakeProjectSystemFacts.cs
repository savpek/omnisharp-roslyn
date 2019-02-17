﻿using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using OmniSharp.FileWatching;
using OmniSharp.Models.FilesChanged;
using OmniSharp.Models.WorkspaceInformation;
using OmniSharp.Roslyn.CSharp.Services.Files;
using TestUtility;
using Xunit;
using Xunit.Abstractions;

namespace OmniSharp.Cake.Tests
{
    public class CakeProjectSystemFacts : AbstractTestFixture
    {
        public CakeProjectSystemFacts(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact(Skip = "Testing it really is cake tests that hangs.")]
        public async Task ShouldGetProjects()
        {
            using (var testProject = await TestAssets.Instance.GetTestProjectAsync("CakeProject", shadowCopy : false))
            using (var host = CreateOmniSharpHost(testProject.Directory))
            {
                var workspaceInfo = await GetWorkspaceInfoAsync(host);

                Assert.Equal(3, workspaceInfo.Projects.Count());
                Assert.Contains("build.cake", workspaceInfo.Projects.Select(p => Path.GetFileName(p.Path)));
                Assert.Contains("foo.cake", workspaceInfo.Projects.Select(p => Path.GetFileName(p.Path)));
                Assert.Contains("error.cake", workspaceInfo.Projects.Select(p => Path.GetFileName(p.Path)));
            }
        }

        [Fact(Skip = "Testing it really is cake tests that hangs.")]
        public async Task ShouldAddAndRemoveProjects()
        {
            using (var testProject = await TestAssets.Instance.GetTestProjectAsync("CakeProject", shadowCopy : false))
            using (var host = CreateOmniSharpHost(testProject.Directory))
            {
                var tempFile = Path.Combine(testProject.Directory, "temp.cake");

                var workspaceInfo = await GetWorkspaceInfoAsync(host);
                Assert.Equal(3, workspaceInfo.Projects.Count());

                await AddFile(host, tempFile);
                workspaceInfo = await GetWorkspaceInfoAsync(host);
                Assert.Equal(4, workspaceInfo.Projects.Count());
                Assert.Contains("temp.cake", workspaceInfo.Projects.Select(p => Path.GetFileName(p.Path)));

                await RemoveFile(host, tempFile);
                workspaceInfo = await GetWorkspaceInfoAsync(host);
                Assert.Equal(3, workspaceInfo.Projects.Count());
                Assert.DoesNotContain("temp.cake", workspaceInfo.Projects.Select(p => Path.GetFileName(p.Path)));
            }
        }

        [Fact(Skip = "Testing it really is cake tests that hangs.")]
        public async Task AllProjectsShouldUseLatestLanguageVersion()
        {
            using (var testProject = await TestAssets.Instance.GetTestProjectAsync("CakeProject", shadowCopy: false))
            using (var host = CreateOmniSharpHost(testProject.Directory))
            {
                Assert.All(host.Workspace.CurrentSolution.Projects, project =>
                    Assert.Equal(
                        expected: LanguageVersion.Latest,
                        actual: ((CSharpParseOptions)project.ParseOptions).SpecifiedLanguageVersion));
            }
        }

        private static async Task<CakeContextModelCollection> GetWorkspaceInfoAsync(OmniSharpTestHost host)
        {
            var service = host.GetWorkspaceInformationService();

            var request = new WorkspaceInformationRequest
            {
                ExcludeSourceFiles = false
            };

            var response = await service.Handle(request);

            return (CakeContextModelCollection)response["Cake"];
        }

        private static async Task AddFile(OmniSharpTestHost host, string filePath)
        {
            File.Create(filePath).Dispose();
            var service = host.GetRequestHandler<OnFilesChangedService>(OmniSharpEndpoints.FilesChanged);
            await service.Handle(new[] { new FilesChangedRequest { FileName = filePath, ChangeType = FileChangeType.Create }});
        }

        private static async Task RemoveFile(OmniSharpTestHost host, string filePath)
        {
            File.Delete(filePath);
            var service = host.GetRequestHandler<OnFilesChangedService>(OmniSharpEndpoints.FilesChanged);
            await service.Handle(new[] { new FilesChangedRequest { FileName = filePath, ChangeType = FileChangeType.Delete }});
        }
    }
}
