using System;
using Xunit;
using labora1;

namespace NetSdrClientAppTests
{
    public class AudioHelpersTests
    {
        [Theory]
        [InlineData("mp3", "libmp3lame")]
        [InlineData("ogg", "libvorbis")]
        [InlineData("aac", "aac")]
        [InlineData("wav", "wav")]
        public void GetCodecByFormat_ReturnsExpectedCodec(string format, string expected)
        {
            var actual = AudioHelpers.GetCodecByFormat(format);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetOutputFilePath_WithOutputFolder_ReturnsPathInFolder()
        {
            var input = "/home/user/song.wav";
            var output = AudioHelpers.GetOutputFilePath(input, "mp3", "/tmp/out");
            Assert.EndsWith("/tmp/out/song.mp3".Replace('/', System.IO.Path.DirectorySeparatorChar), output);
        }

        [Fact]
        public void GetOutputFilePath_NoOutputFolder_ChangesExtension()
        {
            var input = "/home/user/song.wav";
            var output = AudioHelpers.GetOutputFilePath(input, "ogg", null);
            Assert.EndsWith(System.IO.Path.DirectorySeparatorChar + "song.ogg".TrimStart(System.IO.Path.DirectorySeparatorChar), output, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void BuildFfmpegArguments_IncludesAllOptions()
        {
            var args = AudioHelpers.BuildFfmpegArguments(
                inputFile: "in.wav",
                outputFile: "out.mp3",
                codec: "libmp3lame",
                startTime: "10",
                duration: "5",
                bitrate: "192",
                speed: "1.5");

            Assert.Contains("-ss 10", args);
            Assert.Contains("-t 5", args);
            Assert.Contains("-c:a libmp3lame", args);
            Assert.Contains("-b:a 192k", args);
            Assert.Contains("atempo=1.5", args);
            Assert.EndsWith("\"out.mp3\"", args);
        }

        [Fact]
        public void BuildFfmpegArguments_NoOptionalOptions_Minimal()
        {
            var args = AudioHelpers.BuildFfmpegArguments("in.wav", "out.wav", "wav", null, null, null, "1.0");
            Assert.DoesNotContain("-ss", args);
            Assert.DoesNotContain("-t ", args);
            Assert.DoesNotContain("-b:a", args);
            Assert.DoesNotContain("atempo", args);
        }
    }
}
