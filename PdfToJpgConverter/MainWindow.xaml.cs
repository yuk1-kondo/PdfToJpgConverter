using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using PuppeteerSharp;

namespace PdfToJpgConverter;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private int _quality = 85;
    private const string PdfPlaceholder = "PDFファイルを選択してください";
    private const string OutputPlaceholder = "出力先フォルダを選択してください";

    public MainWindow()
    {
        InitializeComponent();
        SetupPlaceholders();
    }

    private void SetupPlaceholders()
    {
        TxtPdfPath.Text = PdfPlaceholder;
        TxtPdfPath.Foreground = System.Windows.Media.Brushes.Gray;
        TxtPdfPath.GotFocus += (s, e) =>
        {
            if (TxtPdfPath.Text == PdfPlaceholder)
            {
                TxtPdfPath.Text = "";
                TxtPdfPath.Foreground = System.Windows.Media.Brushes.Black;
            }
        };
        TxtPdfPath.LostFocus += (s, e) =>
        {
            if (string.IsNullOrEmpty(TxtPdfPath.Text))
            {
                TxtPdfPath.Text = PdfPlaceholder;
                TxtPdfPath.Foreground = System.Windows.Media.Brushes.Gray;
            }
        };

        TxtOutputPath.Text = OutputPlaceholder;
        TxtOutputPath.Foreground = System.Windows.Media.Brushes.Gray;
        TxtOutputPath.GotFocus += (s, e) =>
        {
            if (TxtOutputPath.Text == OutputPlaceholder)
            {
                TxtOutputPath.Text = "";
                TxtOutputPath.Foreground = System.Windows.Media.Brushes.Black;
            }
        };
        TxtOutputPath.LostFocus += (s, e) =>
        {
            if (string.IsNullOrEmpty(TxtOutputPath.Text))
            {
                TxtOutputPath.Text = OutputPlaceholder;
                TxtOutputPath.Foreground = System.Windows.Media.Brushes.Gray;
            }
        };
    }

    // ドラッグ＆ドロップイベント
    private void Window_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
            this.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 240, 255));
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
    }

    private void Window_DragLeave(object sender, DragEventArgs e)
    {
        this.Background = System.Windows.Media.Brushes.White;
    }

    private async void Window_Drop(object sender, DragEventArgs e)
    {
        this.Background = System.Windows.Media.Brushes.White;

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            // PDFファイルのみをフィルタリング
            var pdfFiles = Array.FindAll(files, f => f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));

            if (pdfFiles.Length > 0)
            {
                TxtPdfPath.Text = pdfFiles[0];
                TxtPdfPath.Foreground = System.Windows.Media.Brushes.Black;

                // 出力先が未設定の場合はPDFと同じフォルダをデフォルトに
                if (string.IsNullOrEmpty(TxtOutputPath.Text) || TxtOutputPath.Text == OutputPlaceholder)
                {
                    string? pdfDirectory = Path.GetDirectoryName(pdfFiles[0]);
                    if (!string.IsNullOrEmpty(pdfDirectory))
                    {
                        TxtOutputPath.Text = pdfDirectory;
                        TxtOutputPath.Foreground = System.Windows.Media.Brushes.Black;
                    }
                }

                // 複数のPDFがドロップされた場合は自動的にバッチ変換
                if (pdfFiles.Length > 1)
                {
                    await BatchConvertAsync(pdfFiles);
                }
            }
        }
    }

    private void BtnSelectPdf_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
            FilterIndex = 1,
            RestoreDirectory = true
        };

        if (openFileDialog.ShowDialog() == true)
        {
            TxtPdfPath.Text = openFileDialog.FileName;
            TxtPdfPath.Foreground = System.Windows.Media.Brushes.Black;

            if (string.IsNullOrEmpty(TxtOutputPath.Text) || TxtOutputPath.Text == OutputPlaceholder)
            {
                string? pdfDirectory = Path.GetDirectoryName(openFileDialog.FileName);
                if (!string.IsNullOrEmpty(pdfDirectory))
                {
                    TxtOutputPath.Text = pdfDirectory;
                    TxtOutputPath.Foreground = System.Windows.Media.Brushes.Black;
                }
            }
        }
    }

    private void BtnSelectOutput_Click(object sender, RoutedEventArgs e)
    {
        var openFolderDialog = new OpenFileDialog
        {
            Title = "出力先フォルダを選択してください",
            Filter = "Folders|*.none",
            FileName = "Select Folder",
            CheckFileExists = false,
            CheckPathExists = true
        };

        if (openFolderDialog.ShowDialog() == true)
        {
            string? folder = Path.GetDirectoryName(openFolderDialog.FileName);
            if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
            {
                TxtOutputPath.Text = folder;
                TxtOutputPath.Foreground = System.Windows.Media.Brushes.Black;
            }
        }
    }

    private void SliderQuality_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _quality = (int)SliderQuality.Value;
        LblQuality.Text = _quality + "%";
    }

    private async void BtnConvert_Click(object sender, RoutedEventArgs e)
    {
        string pdfPath = TxtPdfPath.Text;
        string outputPath = TxtOutputPath.Text;

        if (string.IsNullOrEmpty(pdfPath) || pdfPath == PdfPlaceholder || !File.Exists(pdfPath))
        {
            MessageBox.Show("PDFファイルを選択してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (string.IsNullOrEmpty(outputPath) || outputPath == OutputPlaceholder || !Directory.Exists(outputPath))
        {
            MessageBox.Show("出力先フォルダを選択してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        BtnConvert.IsEnabled = false;
        BtnConvertAll.IsEnabled = false;

        try
        {
            await ConvertPdfToJpgAsync(pdfPath, outputPath);
            LblStatus.Text = "完了！";

            if (ChkOpenFolder.IsChecked == true)
            {
                Process.Start("explorer.exe", outputPath);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"変換中にエラーが発生しました:\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            LblStatus.Text = "エラー発生";
        }
        finally
        {
            BtnConvert.IsEnabled = true;
            BtnConvertAll.IsEnabled = true;
        }
    }

    private async void BtnConvertAll_Click(object sender, RoutedEventArgs e)
    {
        // 出力先フォルダを選択
        var outputFolderDialog = new OpenFileDialog
        {
            Title = "出力先フォルダを選択してください",
            Filter = "Folders|*.none",
            FileName = "Select Folder",
            CheckFileExists = false,
            CheckPathExists = true
        };

        if (outputFolderDialog.ShowDialog() != true)
        {
            return;
        }

        string? outputPath = Path.GetDirectoryName(outputFolderDialog.FileName);
        if (string.IsNullOrEmpty(outputPath) || !Directory.Exists(outputPath))
        {
            MessageBox.Show("出力先フォルダを選択してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        TxtOutputPath.Text = outputPath;
        TxtOutputPath.Foreground = System.Windows.Media.Brushes.Black;

        // PDFファイルが含まれるフォルダを選択
        var sourceFolderDialog = new OpenFileDialog
        {
            Title = "PDFファイルが含まれるフォルダを選択してください",
            Filter = "Folders|*.none",
            FileName = "Select Folder",
            CheckFileExists = false,
            CheckPathExists = true
        };

        if (sourceFolderDialog.ShowDialog() != true)
        {
            return;
        }

        string? sourceFolder = Path.GetDirectoryName(sourceFolderDialog.FileName);
        if (string.IsNullOrEmpty(sourceFolder))
        {
            return;
        }

        string[] pdfFiles = Directory.GetFiles(sourceFolder, "*.pdf", SearchOption.TopDirectoryOnly);
        await BatchConvertAsync(pdfFiles);
    }

    private async Task BatchConvertAsync(string[] pdfFiles)
    {
        string outputPath = TxtOutputPath.Text;

        if (string.IsNullOrEmpty(outputPath) || outputPath == OutputPlaceholder || !Directory.Exists(outputPath))
        {
            MessageBox.Show("出力先フォルダを選択してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        BtnConvert.IsEnabled = false;
        BtnConvertAll.IsEnabled = false;

        try
        {
            if (pdfFiles.Length == 0)
            {
                MessageBox.Show("PDFファイルが見つかりませんでした。", "情報", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int successCount = 0;
            int failCount = 0;

            foreach (string pdfFile in pdfFiles)
            {
                try
                {
                    LblStatus.Text = $"変換中: {Path.GetFileName(pdfFile)}";
                    await ConvertPdfToJpgAsync(pdfFile, outputPath);
                    successCount++;
                }
                catch
                {
                    failCount++;
                }
            }

            LblStatus.Text = $"完了！成功: {successCount}, 失敗: {failCount}";

            MessageBox.Show(
                $"バッチ変換が完了しました。\n成功: {successCount}ファイル\n失敗: {failCount}ファイル",
                "完了",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            if (ChkOpenFolder.IsChecked == true && successCount > 0)
            {
                Process.Start("explorer.exe", outputPath);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"バッチ変換中にエラーが発生しました:\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            LblStatus.Text = "エラー発生";
        }
        finally
        {
            BtnConvert.IsEnabled = true;
            BtnConvertAll.IsEnabled = true;
        }
    }

    private async Task ConvertPdfToJpgAsync(string pdfPath, string outputPath)
    {
        await Task.Run(async () =>
        {
            string pdfName = Path.GetFileNameWithoutExtension(pdfPath);

            // PuppeteerSharpでブラウザを起動
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = null
            });

            await using var page = await browser.NewPageAsync();

            // PDFのデータを読み込み
            byte[] pdfBytes = File.ReadAllBytes(pdfPath);
            string base64Pdf = Convert.ToBase64String(pdfBytes);

            // PDFをページに読み込むためのHTMLを作成
            string html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ margin: 0; padding: 0; }}
        iframe {{ width: 100%; height: 100vh; border: none; }}
    </style>
</head>
<body>
    <embed id='pdf' src='data:application/pdf;base64,{base64Pdf}' type='application/pdf' width='100%' height='100%' />
    <script>
        // PDFのページ数を取得して親ウィンドウに通知
        window.onload = function() {{
            const pdf = document.getElementById('pdf');
            // ChromeのPDFビューアではページ数を直接取得できないため、
            // 固定値を返す（実際には各ページをスクロールしてキャプチャ）
        }};
    </script>
</body>
</html>";

            // 解像度を画質に応じて調整
            int viewportWidth = 1200 + (int)((_quality - 50) * 10);
            int viewportHeight = 1600 + (int)((_quality - 50) * 10);

            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = viewportWidth,
                Height = viewportHeight,
                DeviceScaleFactor = 1
            });

            // 注: PuppeteerSharp/Chromeの制約により、複数ページPDFの全ページ変換は複雑です
            // 現在の実装では1ページ目をキャプチャします
            // 完全な複数ページ対応には、追加のライブラリ（PdfiumViewerなど）が必要です

            await page.GoToAsync($"data:text/html;base64,{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(html))}");

            // ページが読み込まれるのを待つ
            await Task.Delay(2000);

            // スクリーンショットを撮る
            byte[] screenshotBytes = await page.ScreenshotDataAsync(new ScreenshotOptions
            {
                Type = ScreenshotType.Jpeg,
                Quality = _quality
            });

            string outputFile = Path.Combine(outputPath, $"{pdfName}_page_001.jpg");
            File.WriteAllBytes(outputFile, screenshotBytes);

            Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = 1;
                LblStatus.Text = "変換完了";
            });

            await browser.CloseAsync();
        });
    }
}
