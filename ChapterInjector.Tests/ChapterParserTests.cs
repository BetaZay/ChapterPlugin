using System;
using System.IO;
using System.Linq;
using Xunit;

namespace ChapterInjector.Tests
{
    public class ChapterParserTests
    {
        [Fact]
        public void Parse_TxtFile_ReturnsCorrectChapters()
        {
            var content = @"CHAPTER01=00:00:00.000
CHAPTER01NAME=Shell Cottage
CHAPTER02=00:02:58.033
CHAPTER02NAME=Griphook's Price";
            
            var filePath = Path.GetTempFileName() + ".txt";
            File.WriteAllText(filePath, content);

            try
            {
                var chapters = ChapterParser.Parse(filePath);

                Assert.Equal(2, chapters.Count);
                Assert.Equal("Shell Cottage", chapters[0].Name);
                Assert.Equal(0, chapters[0].StartPositionTicks); // 00:00:00.000

                // 00:02:58.033 = 2*60 + 58.033 = 178.033 seconds
                // 178.033 * 10,000,000 = 1,780,330,000 ticks
                Assert.Equal(TimeSpan.Parse("00:02:58.033").Ticks, chapters[1].StartPositionTicks);
                Assert.Equal("Griphook's Price", chapters[1].Name);
            }
            finally
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
        }

        [Fact]
        public void Parse_XmlFile_ReturnsCorrectChapters()
        {
             var content = @"<?xml version=""1.0""?>
<Chapters>
  <EditionEntry>
    <ChapterAtom>
      <ChapterTimeStart>00:00:00.000000000</ChapterTimeStart>
      <ChapterDisplay>
        <ChapterString>Shell Cottage</ChapterString>
      </ChapterDisplay>
    </ChapterAtom>
    <ChapterAtom>
      <ChapterTimeStart>00:02:58.033000000</ChapterTimeStart>
      <ChapterDisplay>
        <ChapterString>Griphook's Price</ChapterString>
      </ChapterDisplay>
    </ChapterAtom>
  </EditionEntry>
</Chapters>";

            var filePath = Path.GetTempFileName() + ".xml";
            File.WriteAllText(filePath, content);

            try
            {
                var chapters = ChapterParser.Parse(filePath);

                Assert.Equal(2, chapters.Count);
                Assert.Equal("Shell Cottage", chapters[0].Name);
                Assert.Equal(0, chapters[0].StartPositionTicks);
                Assert.Equal(TimeSpan.Parse("00:02:58.033").Ticks, chapters[1].StartPositionTicks);
                Assert.Equal("Griphook's Price", chapters[1].Name);
            }
            finally
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
        }
    }
}
