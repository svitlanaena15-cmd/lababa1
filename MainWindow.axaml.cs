using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace labora1;

public partial class MainWindow : Window
{
    private List<string> selectedFiles = new(); // список вибраних файлів

    private string? outputFolder = null; // папка для збереження результатів(можна буде вибирати)


    public MainWindow()
    {
        InitializeComponent();
    }

    // вибір аудіофайлів
    private async void OnSelectFiles(object? sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            AllowMultiple = true
        };

        var result = await dialog.ShowAsync(this);

        if (result != null)
        {
            selectedFiles = new List<string>(result);
            FilesInfo.Text = $"Обрано файлів: {selectedFiles.Count}";
        }
    }

//ДОПОМІЖНІ ФУНКЦІЇ ДЛЯ КОНВЕрТАЦІЇ
    private bool AreFilesSelected()
    {
        if (selectedFiles.Count == 0)
        {
            StatusText.Text = "Файли не вибрані";
            return false;
        }
        return true;
    }

    private bool TryGetFormatAndCodec(out string format, out string codec)
    {
        format = (FormatBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        codec = (CodecBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

        if (format == null || codec == null)
        {
            StatusText.Text = "Оберіть формат і кодек";
            return false;
        }

        codec = GetCodecByFormat(format);
        return true;
    }

    private string GetCodecByFormat(string format)
    {
        if (format == "mp3")
            return "libmp3lame";
        if (format == "ogg")
            return "libvorbis";
        if (format == "aac")
            return "aac";

        return format;
    }

    private string GetOutputFilePath(string inputFile, string format)
    {
        if (outputFolder != null)
        {
            var fileName = Path.GetFileNameWithoutExtension(inputFile);
            return Path.Combine(outputFolder, $"{fileName}.{format}");
        }

        return Path.ChangeExtension(inputFile, format);
    }

    private string BuildFfmpegArguments(string inputFile, string outputFile, string codec)
    {
        var arguments = $"-y -i \"{inputFile}\"";

        if (!string.IsNullOrWhiteSpace(StartTimeBox.Text))
            arguments += $" -ss {StartTimeBox.Text}";

        if (!string.IsNullOrWhiteSpace(DurationBox.Text))
            arguments += $" -t {DurationBox.Text}";

        arguments += $" -c:a {codec}";

        var bitrate = (BitrateBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        if (!string.IsNullOrWhiteSpace(bitrate))
            arguments += $" -b:a {bitrate}k";

        var speed = (SpeedBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        if (!string.IsNullOrWhiteSpace(speed) && speed != "1.0")
            arguments += $" -filter:a \"atempo={speed}\"";

        arguments += $" \"{outputFile}\"";

        return arguments;
    }   

    private void RunFfmpeg(string arguments)
    {
        var process = new Process();

        process.StartInfo.FileName = "ffmpeg";
        process.StartInfo.Arguments = arguments;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        process.Start();
        process.WaitForExit();
    }


    //запуск конвертації!!!!
    private void OnConvert(object? sender, RoutedEventArgs e)
    {
        if (!AreFilesSelected())
            return;

        if (!TryGetFormatAndCodec(out var format, out var codec))
            return;

        StatusText.Text = "Конвертація триває...";

        foreach (var file in selectedFiles)
        {
            var outputFile = GetOutputFilePath(file, format);
            var arguments = BuildFfmpegArguments(file, outputFile, codec);

            RunFfmpeg(arguments);
        }

        StatusText.Text = "Конвертація завершена";
    }

// вибір папки для збереження файлів!!!
    private async void OnSelectOutputFolder(object? sender, RoutedEventArgs e)
    {
       var dialog = new OpenFolderDialog();

     var result = await dialog.ShowAsync(this);

        if (result != null)
        {
            outputFolder = result;
            OutputFolderText.Text = $"Папка: {outputFolder}";
        }
    }
}

//тест