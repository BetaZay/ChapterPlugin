using System.IO;
using System.Threading.Tasks;
using ChapterInjector.Api;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ChapterInjector.Tests
{
    public class ClientScriptControllerTests
    {
        [Fact]
        public void GetClientScript_ReturnsJsFile()
        {
            // Arrange
            var controller = new ClientScriptController();

            // Act
            var result = controller.GetClientScript();

            // Assert
            var fileResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("application/javascript", fileResult.ContentType);
            
            using var reader = new StreamReader(fileResult.FileStream);
            var content = reader.ReadToEnd();
            Assert.Contains("ChapterInjector", content);
            Assert.Contains("fetchChapters", content);
        }
    }
}
