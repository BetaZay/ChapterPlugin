using System;
using System.Collections.Generic;
using System.IO;
using ChapterInjector.Api;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ChapterInjector.Tests
{
    public class ExternalChaptersControllerTests
    {
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<ILogger<ExternalChaptersController>> _loggerMock;
        private readonly ExternalChaptersController _controller;

        public ExternalChaptersControllerTests()
        {
            _libraryManagerMock = new Mock<ILibraryManager>();
            _loggerMock = new Mock<ILogger<ExternalChaptersController>>();
            _controller = new ExternalChaptersController(_libraryManagerMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void Get_ItemNotFound_ReturnsNotFound()
        {
            var itemId = Guid.NewGuid();
            _libraryManagerMock.Setup(lm => lm.GetItemById(itemId)).Returns((BaseItem)null);

            var result = _controller.Get(itemId);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public void Get_ValidItemWithChapters_ReturnsChapters()
        {
            var itemId = Guid.NewGuid();
            var tempFile = Path.GetTempFileName();
            var chapterFile = Path.ChangeExtension(tempFile, ".chapters.txt");
            
            // Create dummy video file path (doesn't need to exist, just the string path)
            var videoPath = Path.ChangeExtension(tempFile, ".mkv");

            // Create chapter file
            File.WriteAllText(chapterFile, "CHAPTER01=00:00:00.000\nCHAPTER01NAME=Test Chapter");

            try
            {
                var itemMock = new Mock<BaseItem>();  
                // BaseItem path is not virtual usually, but BaseItem is a class. 
                // We can't easily mock concrete BaseItem properties unless they are virtual or we use a subclass.
                // However, LibraryManager returns BaseItem. 
                // Let's assume we can create a concrete Video item.
                var video = new Video { Path = videoPath };
                
                _libraryManagerMock.Setup(lm => lm.GetItemById(itemId)).Returns(video);

                var result = _controller.Get(itemId);

                var actionResult = Assert.IsType<OkObjectResult>(result.Result);
                var chapters = Assert.IsAssignableFrom<List<ChapterInfo>>(actionResult.Value);
                
                Assert.Single(chapters);
                Assert.Equal("Test Chapter", chapters[0].Name);
            }
            finally
            {
                if (File.Exists(chapterFile)) File.Delete(chapterFile);
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
        [Fact]
        public void Get_EpisodeWithIndexUsage_ReturnsChapters()
        {
            var itemId = Guid.NewGuid();
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            
            var episodePath = Path.Combine(tempDir, "S01E05.mkv");
            var chapterFile = Path.Combine(tempDir, "5_chapters.txt");

            // Create chapter file
            File.WriteAllText(chapterFile, "CHAPTER01=00:00:00.000\nCHAPTER01NAME=Episode Chapter use");

            try
            {
                var episode = new MediaBrowser.Controller.Entities.TV.Episode 
                { 
                    Path = episodePath,
                    IndexNumber = 5
                };
                
                _libraryManagerMock.Setup(lm => lm.GetItemById(itemId)).Returns(episode);

                var result = _controller.Get(itemId);

                var actionResult = Assert.IsType<OkObjectResult>(result.Result);
                var chapters = Assert.IsAssignableFrom<List<ChapterInfo>>(actionResult.Value);
                
                Assert.Single(chapters);
                Assert.Equal("Episode Chapter use", chapters[0].Name);
            }
            finally
            {
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            }
        }
    }
}
