# PDF to JPG Converter

Windows用のPDF→JPG変換アプリケーションです。.NET 8で開発され、Macでビルド可能です。

## 機能

- **単一ファイル変換**: 選択したPDFファイルをJPG画像に変換
- **バッチ変換**: フォルダ内のすべてのPDFファイルを一括変換
- **品質調整**: 画質を10%～100%で調整可能（デフォルト85%）
- **ページごとの出力**: PDFの各ページを`[ファイル名]_page_001.jpg`形式で保存
- **変換後にフォルダを開く**: オプションで自動的に出力先フォルダを開く

## システム要件

- **Windows 10/11**（64ビット）
- Google Chrome（PuppeteerSharp使用のため、システムのChromeが必要）

## ビルド方法

### Macでビルドする場合

```bash
# プロジェクトディレクトリに移動
cd PdfToJpgConverter/PdfToJpgConverter

# パッケージの復元
dotnet restore

# 自己完結型EXEを作成（Windows用）
dotnet publish -c Release -r win-x64 --self-contained true
```

ビルド完了後、`bin/Release/net8.0-windows/win-x64/publish/` フォルダに配布用ファイルが生成されます。

### Windowsでビルドする場合

Visual Studio 2022または.NET SDK 8.0が必要です。

```bash
cd PdfToJpgConverter/PdfToJpgConverter
dotnet publish -c Release -r win-x64 --self-contained true
```

## 使用方法

### 単一ファイル変換

1. 「PDF選択」ボタンをクリックしてPDFファイルを選択
2. 「出力先」ボタンをクリックして保存先フォルダを選択（PDFと同じ場所がデフォルト）
3. 画質スライダーで品質を調整（オプション）
4. 「単一ファイル変換」ボタンをクリック

### バッチ変換

1. 「フォルダ内のPDFを一括変換」ボタンをクリック
2. 出力先フォルダを選択
3. PDFファイルが含まれるフォルダを選択
4. 自動的にすべてのPDFが変換されます

## 配布方法

### 配布用ファイル

`bin/Release/net8.0-windows/win-x64/publish/` フォルダ内の全ファイル（約164MB）を配布します。

- `PdfToJpgConverter.exe` - メインの実行ファイル
- すべてのDLLファイル - .NETランタイムと依存ライブラリ
- 言語フォルダ（cs、de、es、fr、it、ja、koなど）

これらを1つのフォルダに配置するだけで、.NETランタイムがインストールされていないWindows PCでも動作します。

### Google Chromeの要件

**重要**: このアプリケーションはPuppeteerSharpを使用しており、システムにGoogle Chromeがインストールされている必要があります。

企業PCでChromeがインストールされていない場合は、以下のいずれかが必要です：
- Google Chromeのインストール
- または、Chromiumベースのブラウザ（Microsoft Edgeなど）

## トラブルシューティング

### アプリケーションが起動しない
- Windows 10/11（64ビット）であることを確認してください
- Google Chromeがインストールされていることを確認してください

### 変換中にエラーが発生する
- PDFファイルが破損していないか確認してください
- 出力先フォルダに書き込み権限があるか確認してください
- 十分なディスク容量があるか確認してください

### Chromeが見つからないエラー
- Google Chromeをインストールしてください
- または、環境変数`CHROME_EXECUTABLE_PATH`を設定してください

## 開発環境

- **.NET 8.0**
- **WPF** (Windows Presentation Foundation)
- **PuppeteerSharp** - PDFレンダリング用

## ライセンス

このアプリケーションは以下のライブラリを使用しています：
- **PuppeteerSharp** - Apache License 2.0

## 注意事項

- このアプリケーションはMacでビルドできますが、Windowsでのみ動作します
- Google Chromeがインストールされている必要があります
- 変換にはPDFのサイズとページ数に応じて時間がかかります
- 大きなPDFファイルを変換する場合は、十分なメモリとディスク容量が必要です
- PuppeteerSharpはPDFの最初のページをスクリーンショットとして保存します（マルチページ対応には追加開発が必要）

## 制限事項

- 現在の実装では、PDFの最初のページのみを変換します
- マルチページPDFの完全な変換には追加の開発が必要です
- 企業PCでChromeが制限されている場合、動作しない可能性があります

## 今後の改善予定

- マルチページPDF対応
- ページ範囲選択機能
- 変換速度の最適化
- Chrome不要のレンダリングエンジンへの移行
