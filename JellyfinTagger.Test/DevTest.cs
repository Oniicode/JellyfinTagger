using Castle.Core.Logging;
using Emby.Naming.TV;
using JellyfinTagger.Services;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;
using System.Text.Json;

namespace JellyfinTagger.Test;

[TestClass]
public class DevTest
{
    [TestMethod]
    [DataRow("I:\\FreeFlix\\Ruby Gloom\\s01e01.mkv")]
    public async Task PerformTest(string testFileName)
    {
        if(!File.Exists(testFileName)) 
            Assert.Inconclusive("Test file not found.");

        GetMockFileModels(testFileName, true, out var itemInfo, out var directoryService);

        var provider = new TaggerEpisodeProvider(new Mock<ILogger<TaggerEpisodeProvider>>().Object);
        
        var result = await provider.GetMetadata(itemInfo, directoryService, CancellationToken.None);

        Assert.IsTrue(!string.IsNullOrWhiteSpace(result.Item.Name) ^ !result.HasMetadata);

        if(result.HasMetadata)
            Debug.WriteLine($"E.{result.Item.IndexNumber}-{result.Item.IndexNumberEnd} S.{result.Item.ParentIndexNumber} Name: {result.Item.Name}, Desc: {result.Item.Overview}");
        
        Debug.WriteLine(JsonSerializer.Serialize(result.Item));
    }

    [TestMethod]
    [DataRow("s01e01.mkv", true)]
    [DataRow("s1e1.mkv", true)]
    [DataRow("s01e01-02.mkv", true)]
    [DataRow("s01e02-02.mkv", true)]
    [DataRow("s01e02-06.mkv", true)]
    [DataRow("s01e05.mkv", true)]
    [DataRow("s02e07.mkv", true)]
    [DataRow("s00e07.mkv", true)]
    [DataRow("s00OP.mkv", false)]
    [DataRow("s01OP.mkv", false)]
    [DataRow("s1ep1.mkv", false)]
    [DataRow("se1ep1.mkv", false)]
    [DataRow("se01ep01.mkv", false)]
    [DataRow("s01 - Extra test.mkv", false)]
    public void TestJellyfinEpisodeResolver(string fileName, bool isSupported)
    {
        var resolver = new EpisodeResolver(new());
        var result = resolver.Resolve(fileName, false);

        Debug.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions() { WriteIndented = true }));

        if(isSupported)
            Assert.IsNotNull(result);
        else
            Assert.IsNull(result);
    }

    private void GetMockFileModels(string fileName, bool forcetagsSet, out ItemInfo itemInfo, out IDirectoryService directoryService)
    {
        {
            var mockItem = new Mock<BaseItem>();
            mockItem.SetupGet(x => x.Path)
                .Returns(fileName);

            itemInfo = new ItemInfo(mockItem.Object);
        }
        {
            var mockDirectoryService = new Mock<IDirectoryService>();
            mockDirectoryService.Setup(x => x.GetFiles(Path.GetDirectoryName(fileName)!))
                .Returns(Enumerable.Repeat(
                    new FileSystemMetadata { Name = TaggerEpisodeProvider.ForcetagsFileName }, 
                    forcetagsSet ? 1 : 0).ToList());

            directoryService = mockDirectoryService.Object;
        }
    }
}