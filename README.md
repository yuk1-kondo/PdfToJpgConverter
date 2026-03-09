# 📄 PDF to JPG Converter

Windows用のPDF→JPG変換アプリケーションです。.NET 8で開発され、Macでビルド可能な自己完結型EXEとして配布します。

![.NET](https://img.shields.io/badge/.NET-8-purple)
![Platform](https://img.shields.io/badge/platform-Windows-blue)
![License](https://img.shields.io/badge/license-MIT-green)

## ✨ 機能

- **単一ファイル変換** - 選択したPDFファイルをJPG画像に変換
- **ドラッグ&ドロップ** - PDFをウィンドウにドロップするだけで変換
- **バッチ変換** - 複数のPDFファイルを一括変換
  - フォルダを指定して一括変換
  - 複数ファイルをドラッグ&ドロップで一括変換
- **画質調整** - 10%～100%で画質を調整可能（デフォルト85%）
- **自動フォルダオープン** - 変換後にエクスプローラーを自動で開く

## 🎯 使い方

### 方法1: ドラッグ&ドロップ（推奨）

1. アプリを起動
2. PDFファイルをウィンドウに**ドラッグ&ドロップ**
3. 自動で変換開始！

複数のPDFをドロップすると、一括変換されます。

### 方法2: ボタン操作

1. **「PDF選択」**ボタンでファイル選択
2. **「出力先」**ボタンで保存先フォルダ選択
3. 画質スライダーで調整（オプション）
4. **「単一ファイル変換」**ボタンをクリック

### 方法3: フォルダ一括変換

1. **「フォルダ内のPDFを一括変換」**ボタンをクリック
2. 出力先フォルダを選択
3. PDFが含まれるフォルダを選択
4. 自動的にすべてのPDFが変換されます

## 📥 ダウンロード

### 最新版リリース

[Releases](https://github.com/yuk1-kondo/PdfToJpgConverter/releases) からダウンロードしてください。

**含まれるファイル:**
- `PdfToJpgConverter.exe` - 実行ファイル
- 依存DLLファイル（約164MB）
- すべてのファイルを1つのフォルダに配置して使用してください

### システム要件

| 項目 | 要件 |
|------|------|
| OS | Windows 10/11 (64-bit) |
| ブラウザ | Google Chrome または Microsoft Edge |
| .NET | 不要（自己完結型） |

## 🛠️ 開発環境

### ビルド方法

**Macでビルド:**
```bash
cd PdfToJpgConverter/PdfToJpgConverter
dotnet restore
dotnet publish -c Release -r win-x64 --self-contained true
```

**Windowsでビルド:**
```bash
cd PdfToJpgConverter/PdfToJpgConverter
dotnet publish -c Release -r win-x64 --self-contained true
```

ビルド後、`bin/Release/net8.0-windows/win-x64/publish/` に配布用ファイルが生成されます。

### 開発スタック

- **.NET 8.0** - 最新の.NETプラットフォーム
- **WPF** - Windows Presentation Foundation
- **PuppeteerSharp** - PDFレンダリング（Chromeを使用）

## 📸 UIプレビュー

[UI-Preview.html](UI-Preview.html) をブラウザで開くと、UIのプレビューが見られます。

## ⚠️ 制限事項

| 項目 | 説明 |
|------|------|
| **複数ページPDF** | 現在は**1ページ目のみ**変換されます |
| **Chrome必須** | Google ChromeまたはEdgeが必要です |
| **Windows限定** | Windows 10/11 (64-bit) のみで動作 |

## 🔧 トラブルシューティング

### アプリケーションが起動しない
- Windows 10/11 (64-bit) であることを確認してください
- Google ChromeまたはEdgeがインストールされていることを確認してください

### 変換中にエラーが発生する
- PDFファイルが破損していないか確認してください
- 出力先フォルダに書き込み権限があるか確認してください
- 十分なディスク容量があるか確認してください

### Chromeが見つからないエラー
- Google ChromeまたはEdgeをインストールしてください
- 環境変数`CHROME_EXECUTABLE_PATH`を設定してください

## 🗺️ ロードマップ

- [ ] 複数ページPDFの完全対応
- [ ] ページ範囲選択機能
- [ ] 変換速度の最適化
- [ ] PNG形式での保存対応
- [ ] 画像サイズ指定機能

## 📝 ライセンス

MIT License - 詳しくは [LICENSE](LICENSE) ファイルを参照してください。

### 使用ライブラリ

- **PuppeteerSharp** - Apache License 2.0
- **.NET 8** - MIT License

## 🤝 コントリビューション

バグ報告や機能リクエストは [Issues](https://github.com/yuk1-kondo/PdfToJpgConverter/issues) までお願いします。

PRも常に受け付けています！

## 👨‍💻 作者

作成: yuki-kondo

---

**もし役に立った場合は、⭐️をつけてください！**
