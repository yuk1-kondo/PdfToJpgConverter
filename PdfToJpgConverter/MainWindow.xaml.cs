using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Docnet.Core;
using Docnet.Core.Models;
using Microsoft.Win32;
using WinForms = System.Windows.Forms;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfColor = System.Windows.Media.Color;
using WpfMessageBox = System.Windows.MessageBox;

namespace PdfToJpgConverter;

public partial class MainWindow : Window
{
    private int _quality = 85;
    private int _dpi = 150;

    private const string PdfPlaceholder = "Select or drop a PDF file";
    private const string OutputPlaceholder = "Select an output folder";

    public MainWindow()
    {
        InitializeComponent();
    }

    private void DropZone_DragEnter(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
        {
            e.Effects = System.Windows.DragDropEffects.Copy;
            DropZone.BorderBrush = new SolidColorBrush(WpfColor.FromRgb(25, 118, 210));
            DropZone.Background = new SolidColorBrush(WpfColor.FromRgb(227, 242, 253));
        }
        else
        {
            e.Effects = System.Windows.DragDropEffects.None;
        }

        e.Handled = true;
    }

    private void DropZone_DragLeave(object sender, System.Windows.DragEventArgs e)
    {
        ResetDropZone();
    }

    private void DropZone_DragOver(object sender, System.Windows.DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop)
            ? System.Windows.DragDropEffects.Copy
            : System.Windows.DragDropEffects.None;
        e.Handled = true;
    }

    private async void DropZone_Drop(object sender, System.Windows.DragEventArgs e)
    {
        ResetDropZone();

        if (!e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
        {
            return;
        }

        var dropped = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
        var pdfFiles = dropped
            .Where(path => path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (pdfFiles.Length == 0)
        {
            WpfMessageBox.Show("Please drop one or more PDF files.", "No PDF files", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(TxtOutputPath.Text) || TxtOutputPath.Text == OutputPlaceholder)
        {
            var firstDir = Path.GetDirectoryName(pdfFiles[0]);
            if (!string.IsNullOrWhiteSpace(firstDir))
            {
                SetOutputPath(firstDir);
            }
        }

        if (pdfFiles.Length == 1)
        {
            SetPdfPath(pdfFiles[0]);
            return;
        }

        TxtPdfPath.Text = $"{pdfFiles.Length} PDF files selected";
        TxtPdfPath.Foreground = new SolidColorBrush(WpfColor.FromRgb(33, 150, 243));

        if (!IsOutputPathValid(TxtOutputPath.Text))
        {
            WpfMessageBox.Show("Please select a valid output folder.", "Output folder required", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        await BatchConvertAsync(pdfFiles);
    }

    private void ResetDropZone()
    {
        DropZone.BorderBrush = new SolidColorBrush(WpfColor.FromRgb(144, 202, 249));
        DropZone.Background = WpfBrushes.White;
    }

    private void BtnSelectPdf_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
            FilterIndex = 1,
            RestoreDirectory = true,
            Multiselect = false
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        SetPdfPath(dialog.FileName);

        if (string.IsNullOrWhiteSpace(TxtOutputPath.Text) || TxtOutputPath.Text == OutputPlaceholder)
        {
            var dir = Path.GetDirectoryName(dialog.FileName);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                SetOutputPath(dir);
            }
        }
    }

    private void BtnSelectOutput_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new WinForms.FolderBrowserDialog
        {
            Description = "Select output folder",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() == WinForms.DialogResult.OK)
        {
            SetOutputPath(dialog.SelectedPath);
        }
    }

    private void SliderQuality_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _quality = (int)SliderQuality.Value;
        UpdateQualityLabel();
    }

    private void SliderDpi_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _dpi = (int)SliderDpi.Value;
        UpdateQualityLabel();
    }

    private void UpdateQualityLabel()
    {
        if (LblQuality != null)
        {
            LblQuality.Text = $"Quality: {_quality}%   DPI: {_dpi}";
        }
    }

    private async void BtnConvert_Click(object sender, RoutedEventArgs e)
    {
        var pdfPath = TxtPdfPath.Text;
        var outputPath = TxtOutputPath.Text;

        if (string.IsNullOrWhiteSpace(pdfPath) || pdfPath == PdfPlaceholder || !File.Exists(pdfPath))
        {
            WpfMessageBox.Show("Please select a valid PDF file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (!IsOutputPathValid(outputPath))
        {
            WpfMessageBox.Show("Please select a valid output folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        SetButtonsEnabled(false);
        ProgressBar.Maximum = 100;
        ProgressBar.Value = 0;

        try
        {
            await ConvertPdfToJpgAsync(pdfPath, outputPath);
            LblStatus.Text = "Conversion completed.";

            if (ChkOpenFolder.IsChecked == true)
            {
                Process.Start("explorer.exe", outputPath);
            }
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show($"An error occurred during conversion:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            LblStatus.Text = "Conversion failed.";
        }
        finally
        {
            SetButtonsEnabled(true);
        }
    }

    private async void BtnConvertAll_Click(object sender, RoutedEventArgs e)
    {
        string sourceFolder;
        using (var srcDialog = new WinForms.FolderBrowserDialog
        {
            Description = "Select source folder containing PDF files",
            UseDescriptionForTitle = true
        })
        {
            if (srcDialog.ShowDialog() != WinForms.DialogResult.OK)
            {
                return;
            }

            sourceFolder = srcDialog.SelectedPath;
        }

        string outputFolder;
        using (var outDialog = new WinForms.FolderBrowserDialog
        {
            Description = "Select output folder for JPG files",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        })
        {
            if (outDialog.ShowDialog() != WinForms.DialogResult.OK)
            {
                return;
            }

            outputFolder = outDialog.SelectedPath;
        }

        SetOutputPath(outputFolder);

        var pdfFiles = Directory.GetFiles(sourceFolder, "*.pdf", SearchOption.TopDirectoryOnly);
        if (pdfFiles.Length == 0)
        {
            WpfMessageBox.Show("No PDF files found in the selected folder.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        await BatchConvertAsync(pdfFiles);
    }

    private async Task BatchConvertAsync(string[] pdfFiles)
    {
        var outputPath = TxtOutputPath.Text;
        if (!IsOutputPathValid(outputPath))
        {
            WpfMessageBox.Show("Please select a valid output folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        SetButtonsEnabled(false);
        ProgressBar.Maximum = pdfFiles.Length;
        ProgressBar.Value = 0;

        var successCount = 0;
        var failCount = 0;
        var errors = new List<string>();

        try
        {
            for (var i = 0; i < pdfFiles.Length; i++)
            {
                var pdfFile = pdfFiles[i];
                LblStatus.Text = $"Converting ({i + 1}/{pdfFiles.Length}): {Path.GetFileName(pdfFile)}";

                try
                {
                    await ConvertPdfToJpgAsync(pdfFile, outputPath);
                    successCount++;
                }
                catch (Exception ex)
                {
                    failCount++;
                    errors.Add($"{Path.GetFileName(pdfFile)}: {ex.Message}");
                }

                ProgressBar.Value = i + 1;
            }

            LblStatus.Text = $"Done - Success: {successCount}, Failed: {failCount}";

            var summary = $"Batch conversion completed.\nSuccess: {successCount}\nFailed: {failCount}";
            if (errors.Count > 0)
            {
                summary += "\n\nError details:\n" + string.Join("\n", errors.Take(5));
            }

            WpfMessageBox.Show(summary, "Result", MessageBoxButton.OK, MessageBoxImage.Information);

            if (ChkOpenFolder.IsChecked == true && successCount > 0)
            {
                Process.Start("explorer.exe", outputPath);
            }
        }
        finally
        {
            SetButtonsEnabled(true);
        }
    }

    private async Task ConvertPdfToJpgAsync(string pdfPath, string outputFolder)
    {
        await Task.Run(() =>
        {
            var pdfName = Path.GetFileNameWithoutExtension(pdfPath);
            var quality = _quality;
            var dpi = _dpi;

            using var docReader = DocLib.Instance.GetDocReader(pdfPath, new PageDimensions(dpi / 72.0));
            var pageCount = docReader.GetPageCount();

            for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
            {
                using var pageReader = docReader.GetPageReader(pageIndex);

                var width = pageReader.GetPageWidth();
                var height = pageReader.GetPageHeight();
                var rawBytes = pageReader.GetImage();

                using var bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                var bitmapData = bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, width, height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                Marshal.Copy(rawBytes, 0, bitmapData.Scan0, rawBytes.Length);
                bitmap.UnlockBits(bitmapData);

                using var whiteBackground = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                using var graphics = System.Drawing.Graphics.FromImage(whiteBackground);
                graphics.Clear(System.Drawing.Color.White);
                graphics.DrawImage(bitmap, 0, 0, width, height);

                var jpegEncoder = GetJpegEncoder();
                using var encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
                encoderParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(
                    System.Drawing.Imaging.Encoder.Quality,
                    (long)quality);

                var pageNum = (pageIndex + 1).ToString("D3");
                var outputFile = Path.Combine(outputFolder, $"{pdfName}_page_{pageNum}.jpg");
                whiteBackground.Save(outputFile, jpegEncoder, encoderParams);
            }

            Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = ProgressBar.Maximum > 1 ? ProgressBar.Value : 100;
                LblStatus.Text = $"Completed: {pdfName} ({pageCount} pages)";
            });
        });
    }

    private static System.Drawing.Imaging.ImageCodecInfo GetJpegEncoder()
    {
        foreach (var codec in System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders())
        {
            if (codec.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid)
            {
                return codec;
            }
        }

        throw new InvalidOperationException("JPEG encoder not found.");
    }

    private void SetPdfPath(string path)
    {
        TxtPdfPath.Text = path;
        TxtPdfPath.Foreground = new SolidColorBrush(WpfColor.FromRgb(33, 33, 33));
    }

    private void SetOutputPath(string path)
    {
        TxtOutputPath.Text = path;
        TxtOutputPath.Foreground = new SolidColorBrush(WpfColor.FromRgb(33, 33, 33));
    }

    private bool IsOutputPathValid(string path)
    {
        return !string.IsNullOrWhiteSpace(path) && path != OutputPlaceholder && Directory.Exists(path);
    }

    private void SetButtonsEnabled(bool enabled)
    {
        BtnConvert.IsEnabled = enabled;
        BtnConvertAll.IsEnabled = enabled;
        BtnSelectPdf.IsEnabled = enabled;
        BtnSelectOutput.IsEnabled = enabled;
    }
}
