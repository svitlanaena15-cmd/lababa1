using System;
using System.IO;

namespace labora1
{
    public static class AudioHelpers //дозволяє писати тести без запуску Авалонії
    {
        public static string GetCodecByFormat(string format)
        {
            if (format == "mp3")
                return "libmp3lame";
            if (format == "ogg")
                return "libvorbis";
            if (format == "aac")
                return "aac";
            return format;
        }

        public static string GetOutputFilePath(string inputFile, string format, string? outputFolder)
        {
            if (!string.IsNullOrEmpty(outputFolder))
            {
                var fileName = Path.GetFileNameWithoutExtension(inputFile);
                return Path.Combine(outputFolder, $"{fileName}.{format}");
            }
            return Path.ChangeExtension(inputFile, format);
        }

        public static string BuildFfmpegArguments(
            string inputFile,
            string outputFile,
            string codec,
            string? startTime,
            string? duration,
            string? bitrate,
            string? speed)
        {
            var arguments = $"-y -i \"{inputFile}\"";

            if (!string.IsNullOrWhiteSpace(startTime))
                arguments += $" -ss {startTime}";

            if (!string.IsNullOrWhiteSpace(duration))
                arguments += $" -t {duration}";

            arguments += $" -c:a {codec}";

            if (!string.IsNullOrWhiteSpace(bitrate))
                arguments += $" -b:a {bitrate}k";

            if (!string.IsNullOrWhiteSpace(speed) && speed != "1.0")
                arguments += $" -filter:a \"atempo={speed}\"";

            arguments += $" \"{outputFile}\"";
            return arguments;
        }
    }
}
