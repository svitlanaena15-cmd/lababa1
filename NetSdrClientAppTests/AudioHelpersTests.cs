using System;
using Xunit;
using labora1.UI;
using System.IO;

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
            Assert.Equal("song.mp3", Path.GetFileName(output));
            Assert.Equal("out", Path.GetFileName(Path.GetDirectoryName(output)));
        }

        [Fact]
        public void GetOutputFilePath_NoOutputFolder_ChangesExtension()
        {
            var input = "/home/user/song.wav";
            var output = AudioHelpers.GetOutputFilePath(input, "ogg", null);
            Assert.Equal("song.ogg", Path.GetFileName(output));
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
            Assert.Contains("out.mp3", args);
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

        [Fact]
        public void GetCodecByFormat_UnknownFormat_ReturnsSameFormat()
        {
            var result = AudioHelpers.GetCodecByFormat("flac");
            Assert.Equal("flac", result);
        }

        [Fact]
        public void BuildFfmpegArguments_NoBitrate_NoSpeed()
        {
            var args = AudioHelpers.BuildFfmpegArguments(
                "in.wav",
                "out.wav",
                "wav",
                startTime: null,
                duration: null,
                bitrate: null,
                speed: "1.0"
            );

            Assert.DoesNotContain("-b:a", args);
            Assert.DoesNotContain("atempo=", args);
        }

    }
}
