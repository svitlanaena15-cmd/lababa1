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

    //запуск конвертації!!!!
    private void OnConvert(object? sender, RoutedEventArgs e)
    {
        if (selectedFiles.Count == 0)
        {
            StatusText.Text = "Файли не вибрані";
            return;
        }

        var format = (FormatBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        var codec = (CodecBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

        if (format == null || codec == null)
        {
            StatusText.Text = "Оберіть формат і кодек";
            return;
        }

        if (format == "mp3")
        {
            codec = "libmp3lame";
        }
        else if (format == "ogg")
        {
            codec = "libvorbis";
        }
        else if (format == "aac")
        {
            codec = "aac";
        }


        StatusText.Text = "Конвертація триває...";

        foreach (var file in selectedFiles)
        {
            string outputFile;

            if (outputFolder != null)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                outputFile = Path.Combine(outputFolder, $"{fileName}.{format}");
            }
            else
            {
    // якщо папку не вибрати то буде збережено файли біля оригіналу
             outputFile = Path.ChangeExtension(file, format);
            }


            var process = new Process();
            process.StartInfo.FileName = "ffmpeg";
            var arguments = $"-y -i \"{file}\"";

            // обрізання
            if (!string.IsNullOrWhiteSpace(StartTimeBox.Text))
            {
                arguments += $" -ss {StartTimeBox.Text}";
            }

            if (!string.IsNullOrWhiteSpace(DurationBox.Text))
            {
                arguments += $" -t {DurationBox.Text}";
            }

// кодек
            arguments += $" -c:a {codec}";

    // бітрейт
            var bitrate = (BitrateBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (!string.IsNullOrWhiteSpace(bitrate))
            {
                arguments += $" -b:a {bitrate}k";
            }

        // швидкість
            var speed = (SpeedBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (!string.IsNullOrWhiteSpace(speed) && speed != "1.0")
            {
                arguments += $" -filter:a \"atempo={speed}\"";
            }

            arguments += $" \"{outputFile}\"";

            process.StartInfo.Arguments = arguments;

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            process.WaitForExit();
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