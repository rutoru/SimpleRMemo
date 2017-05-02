Imports System.Drawing.Printing
''' <summary>
''' SimpleRMemoアプリケーションクラス
''' </summary>
''' <author>Rutoru</author>
''' <create>2017/5/1</create>
Public Class Form1

    'アプリケーション名
    Const ApplicationName = "SimpleRMemo"
    Const ApplicationJName = "るとるのメモ帳"

    'フォルダ名保存用レジストリ
    Const RegistryKey As String = "RutoruSoftware\" & ApplicationName
    '作業フォルダ名変数
    Private FilePath As String

    '最小ウィンドウサイズ
    Const initialWidth = 400
    Const initialHeight = 100

    'ウィンドウ場所
    Const initialLeft As Integer = 100
    Const initialTop As Integer = 100

    '印刷用文字列
    Private PrintString As String

    ''' <summary>
    ''' ファイル名プロパティ
    ''' </summary>
    Private FileNameValue As String
    Private Property FileName As String

        Get
            Return FileNameValue
        End Get

        'ファイル名を設定し、編集状態をFalseに設定
        Set(value As String)
            FileNameValue = value

            'フォルダ名取得
            If value <> "" Then
                FilePath = System.IO.Path.GetDirectoryName(value)
            End If

            Edited = False

        End Set

    End Property

    ''' <summary>
    ''' 編集状態プロパティ
    ''' </summary>
    Private EditedValue As Boolean
    Private Property Edited As Boolean

        Get
            Return EditedValue
        End Get

        '編集状態を設定し、ステータス変更を実施
        Set(value As Boolean)
            EditedValue = value
            UpdateStatus()
        End Set
    End Property

    ''' <summary>
    ''' 文字コードプロパティ
    ''' </summary>
    Private EncordingValue As String
    Private Property Encording As String

        Get
            Return EncordingValue
        End Get

        '文字コードセット
        Set(value As String)
            If value = "" Then
                EncordingValue = "shift_jis"
            Else
                EncordingValue = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' ステータス変更サブプロシージャ
    ''' タイトル設定とメニュー設定処理を実施
    ''' </summary>
    Private Sub UpdateStatus()

        'タイトル設定処理
        Dim s = ApplicationName

        'ファイル名がある場合はファイルパスを表示
        If FileName <> "" Then s &= " - " & FileName

        'ファイルが変更されていれば変更ありとタイトルに表示
        If Edited Then s &= "（変更あり）"

        'タイトル設定
        Me.Text = s

        'メニュー設定処理
        'ファイル名が無いあるいは編集されていない場合あるいはデータがない場合は上書き保存メニューを表示しない
        If FileName = "" OrElse Not Edited OrElse TextBoxMain.TextLength = 0 Then
            MenuItemFileSave.Enabled = False
        Else
            MenuItemFileSave.Enabled = True
        End If

        'ファイルが編集されていない場合あるいはデータがない場合は名前を付けて保存メニューを表示しない
        If Not Edited OrElse TextBoxMain.TextLength = 0 Then
            MenuItemFileSaveAs.Enabled = False
        Else
            MenuItemFileSaveAs.Enabled = True
        End If

        '印刷有効/無効設定
        Dim b = TextBoxMain.TextLength = 0
        MenuItemFilePrint.Enabled = Not b
        MenuItemFilePrintPreview.Enabled = Not b

    End Sub

    ''' <summary>
    ''' フォームロードイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        'アプリ名の設定
        FileName = ""

        'テキストボックスエリア設定
        TextBoxMain.Multiline = True
        TextBoxMain.ScrollBars = ScrollBars.Vertical
        TextBoxMain.Dock = DockStyle.Fill

        '保存ファイルダイアログ設定
        SaveFileDialog1.Filter = "テキスト文書|*.txt|すべてのファイル|*.*"

        'フォントダイアログ設定
        FontDialog1.ShowEffects = False
        FontDialog1.AllowScriptChange = False

        '最小ウィンドウサイズ指定
        Me.MinimumSize = New Size(initialWidth, initialHeight)

        '文字コードメニュー
        MenuItemSettingEncodingSelection.Items.Add("shift_jis")
        MenuItemSettingEncodingSelection.Items.Add("utf-8")
        MenuItemSettingEncodingSelection.Items.Add("EUC-JP")

        '印刷機能
        PrintPreviewDialog1.Document = PrintDocument1

        'コマンドライン起動で、引数にファイル名が指定された場合の処理（第一引数をファイル名として開く）
        If 1 < Environment.GetCommandLineArgs.Length Then
            LoadFile(Environment.GetCommandLineArgs(1))
        End If

        'HKEY_CURRENT_USER配下にレジストリを作る/すでに存在すれば開く
        Dim regKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(RegistryKey)
        'レジストリにキー"FilePath"が存在しない場合はMyDocumentsを設定
        FilePath = regKey.GetValue("FilePath", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))
        'フォント設定
        Dim name As String = regKey.GetValue("FontName", "ＭＳ ゴシック")
        Dim size As Integer = regKey.GetValue("FontSize", 12)
        Dim bold As Boolean = regKey.GetValue("FontBold", False)
        Dim italic As Boolean = regKey.GetValue("FontItalic", False)
        Dim style As FontStyle
        If bold Then style = FontStyle.Bold
        If italic Then style = style Or FontStyle.Italic
        TextBoxMain.Font = New Font(name, size, style)
        'ウィンドウ場所設定
        Dim l As Integer = regKey.GetValue("Left", initialLeft)
        Dim t As Integer = regKey.GetValue("Top", initialTop)
        Dim w As Integer = regKey.GetValue("Width", initialWidth)
        Dim h As Integer = regKey.GetValue("Height", initialHeight)
        If l < Screen.GetWorkingArea(Me).Left OrElse
           l >= Screen.GetWorkingArea(Me).Right Then
            l = initialLeft
        End If
        If t < Screen.GetWorkingArea(Me).Top OrElse
           t >= Screen.GetWorkingArea(Me).Bottom Then
            t = initialTop
        End If
        Me.SetDesktopBounds(l, t, w, h)
        'エンコーディング
        Encording = regKey.GetValue("Encording", "shift_jis")

    End Sub

    ''' <summary>
    ''' 「ファイル」-「終了」(MenuItemFileExit)クリックイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MenuItemFileExit_Click(sender As Object, e As EventArgs) Handles MenuItemFileExit.Click

        Me.Close()

    End Sub

    ''' <summary>
    ''' 「ファイル」-「開く」(MenuItemFileOpen)クリックイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MenuItemFileOpen_Click(sender As Object, e As EventArgs) Handles MenuItemFileOpen.Click

        '編集内容破棄確認しOKであれば開く
        If AskGiveUpText() Then

            '「開く」コモンダイアログ設定
            OpenFileDialog1.InitialDirectory = FilePath
            OpenFileDialog1.FileName = ""

            '「開く」が押下された場合はファイルを開く
            If DialogResult.OK = OpenFileDialog1.ShowDialog() Then
                LoadFile(OpenFileDialog1.FileName)
            End If

        End If

    End Sub

    ''' <summary>
    ''' 「ファイル」-「名前を付けて保存」(MenuItemFileSaveAs)クリックイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MenuItemFileSaveAs_Click(sender As Object, e As EventArgs) Handles MenuItemFileSaveAs.Click

        'デフォルトのディレクトリとファイル名を設定
        SaveFileDialog1.InitialDirectory = FilePath
        SaveFileDialog1.FileName = System.IO.Path.GetFileName(FileName)

        '「保存」が押下された場合はファイルを保存する
        If DialogResult.OK = SaveFileDialog1.ShowDialog() Then
            SaveFile(SaveFileDialog1.FileName)
        End If

    End Sub

    ''' <summary>
    ''' 「ファイル」-「上書き保存」(MenuItemFileSave)クリックイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MenuItemFileSave_Click(sender As Object, e As EventArgs) Handles MenuItemFileSave.Click

        'ファイルを保存
        SaveFile(FileName)

    End Sub

    ''' <summary>
    ''' 「テキストエリア」(TextBoxMain)テキスト変更イベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TextBoxMain_TextChanged(sender As Object, e As EventArgs) Handles TextBoxMain.TextChanged

        '編集状態
        Edited = True

    End Sub

    ''' <summary>
    ''' 「ファイル」-「新規」(MenuItemFileNew)クリックイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MenuItemFileNew_Click(sender As Object, e As EventArgs) Handles MenuItemFileNew.Click

        '編集内容破棄確認しOKであればクリア
        If AskGiveUpText() Then
            TextBoxMain.Clear()
            FileName = ""
        End If

    End Sub

    ''' <summary>
    ''' 「編集」-「元に戻す」(MenuItemEditUndo)クリックイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MenuItemEditUndo_Click(sender As Object, e As EventArgs) Handles MenuItemEditUndo.Click

        TextBoxMain.Undo()

    End Sub

    ''' <summary>
    ''' 「編集」-「切り取り」(MenuItemEditCut)クリックイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MenuItemEditCut_Click(sender As Object, e As EventArgs) Handles MenuItemEditCut.Click

        TextBoxMain.Cut()

    End Sub

    ''' <summary>
    ''' 「編集」-「コピー」(MenuItemEditCopy)クリックイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MenuItemEditCopy_Click(sender As Object, e As EventArgs) Handles MenuItemEditCopy.Click

        TextBoxMain.Copy()

    End Sub

    ''' <summary>
    ''' 「編集」-「貼り付け」(MenuItemEditPaste)クリックイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MenuItemEditPaste_Click(sender As Object, e As EventArgs) Handles MenuItemEditPaste.Click

        TextBoxMain.Paste()

    End Sub

    ''' <summary>
    ''' 「編集」-「削除」(MenuItemEditDelete)クリックイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MenuItemEditDelete_Click(sender As Object, e As EventArgs) Handles MenuItemEditDelete.Click

        TextBoxMain.SelectedText = ""

    End Sub

    ''' <summary>
    ''' 「編集」-「すべて選択」(MenuItemEditSelectAll)クリックイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MenuItemEditSelectAll_Click(sender As Object, e As EventArgs) Handles MenuItemEditSelectAll.Click

        TextBoxMain.SelectAll()

    End Sub

    ''' <summary>
    '''  「編集」(MenuItemEdit)ドロップダウンオープニングイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MenuItemEdit_DropDownOpening(sender As Object, e As EventArgs) Handles MenuItemEdit.DropDownOpening

        'クリップボードにテキストがある場合はペーストはOnとする
        MenuItemEditPaste.Enabled = Clipboard.ContainsText
        'テキストを選択していない場合は切り取り、コピー、削除はOffとする
        Dim b = TextBoxMain.SelectionLength = 0
        MenuItemEditCut.Enabled = Not b
        MenuItemEditCopy.Enabled = Not b
        MenuItemEditDelete.Enabled = Not b

    End Sub

    ''' <summary>
    ''' 「編集」(MenuItemEdit)ドロップダウンクローズドイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MenuItemEdit_DropDownClosed(sender As Object, e As EventArgs) Handles MenuItemEdit.DropDownClosed

        MenuItemEditDelete.Enabled = False

    End Sub

    ''' <summary>
    ''' 「設定」-「フォント」(MenuItemSettingFont)クリックイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MenuItemSettingFont_Click(sender As Object, e As EventArgs) Handles MenuItemSettingFont.Click

        '現在の設定をダイアログに反映
        FontDialog1.Font = TextBoxMain.Font

        'フォント設定
        If DialogResult.OK = FontDialog1.ShowDialog() Then
            TextBoxMain.Font = FontDialog1.Font
            'Edited = True
        End If

    End Sub

    ''' <summary>
    ''' 「設定」-「文字コード」が変更された時のイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MenuItemSettingEncodingSelection_SelectedIndexChanged(sender As Object, e As EventArgs) Handles MenuItemSettingEncodingSelection.SelectedIndexChanged

        '文字コード設定
        Encording = MenuItemSettingEncodingSelection.Text

        '入力されているorファイルが開かれているときは編集されているとする
        If TextBoxMain.Text.Length <> 0 Then
            Edited = True
        End If


    End Sub

    ''' <summary>
    ''' 「設定」ドロップダウが開いた時のイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MenuItemSetting_DropDownOpening(sender As Object, e As EventArgs) Handles MenuItemSetting.DropDownOpening

        '設定されている文字コードをメニューに反映
        MenuItemSettingEncodingSelection.Text = Encording

    End Sub

    ''' <summary>
    ''' 「ヘルプ」-「README.TXTの表示」(MenuItemHelpReadMe)クリックイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MenuItemHelpReadMe_Click(sender As Object, e As EventArgs) Handles MenuItemHelpReadMe.Click

        'README.TXT表示
        Dim s As String = System.IO.Path.GetDirectoryName(Application.ExecutablePath)
        s = System.IO.Path.Combine(s, "README.TXT")
        If System.IO.File.Exists(s) Then
            Process.Start(s)
        Else
            MessageBox.Show(s & "が見つかりません", ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If

    End Sub

    ''' <summary>
    ''' 「ヘルプ」-「Webサイトの表示」(MenuItemHelpVersion)クリックイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MenuItemHelpWeb_Click(sender As Object, e As EventArgs) Handles MenuItemHelpWeb.Click

        Process.Start("http://rutoru.com/")

    End Sub

    ''' <summary>
    ''' 「ヘルプ」-「バージョン情報」クリックイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MenuItemHelpVersion_Click(sender As Object, e As EventArgs) Handles MenuItemHelpVersion.Click

        MessageBox.Show(ApplicationJName & "0.01" & vbCrLf &
                        "(c) 2017 Rutoru", "バージョン情報")

    End Sub

    ''' <summary>
    ''' 「ファイル」-「印刷プレビュー」(MenuItemFilePrintPreview)クリックイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MenuItemFilePrintPreview_Click(sender As Object, e As EventArgs) Handles MenuItemFilePrintPreview.Click

        SetPrintDocument1()
        PrintPreviewDialog1.ShowDialog()

    End Sub

    ''' <summary>
    ''' 「ファイル」-「印刷」(MenuItemFilePrint)クリックイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MenuItemFilePrint_Click(sender As Object, e As EventArgs) Handles MenuItemFilePrint.Click

        If DialogResult.OK = PrintDialog1.ShowDialog() Then
            SetPrintDocument1()
            PrintDocument1.Print()
        End If

    End Sub

    ''' <summary>
    ''' フォームクロージングイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing

        '編集内容破棄確認
        If Not AskGiveUpText() Then e.Cancel = True

    End Sub

    ''' <summary>
    ''' フォームクローズイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Form1_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed

        'HKEY_CURRENT_USER配下にレジストリを作る/すでに存在すれば開く
        Dim regKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(RegistryKey)
        'レジストリに作業フォルダを保存
        regKey.SetValue("FilePath", FilePath)
        'レジストリにフォント設定を保存
        regKey.SetValue("FontName", TextBoxMain.Font.Name)
        regKey.SetValue("FontSize", TextBoxMain.Font.Size)
        regKey.SetValue("FontBold", TextBoxMain.Font.Bold)
        regKey.SetValue("FontItalic", TextBoxMain.Font.Italic)
        'レジストリにウィンドウ位置を保存
        regKey.SetValue("Left", DesktopBounds.Left)
        regKey.SetValue("Top", DesktopBounds.Top)
        regKey.SetValue("Width", DesktopBounds.Width)
        regKey.SetValue("Height", DesktopBounds.Height)
        '文字コード
        regKey.SetValue("Encording", Encording)

    End Sub

    ''' <summary>
    ''' ファイルを開くSubプロシージャ
    ''' </summary>
    ''' <param name="value"></param>
    Private Sub LoadFile(value As String)

        If System.IO.File.Exists(value) Then

            'テキストボックスの中身を置き換える
            TextBoxMain.Text = System.IO.File.ReadAllText(
                                            value,
                                            System.Text.Encoding.GetEncoding(Encording))

            FileName = value

        Else

            MessageBox.Show(value & "が見つかりません", ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Warning)

        End If

    End Sub

    ''' <summary>
    ''' ファイル保存Subプロシージャ
    ''' </summary>
    ''' <param name="value">ファイルパス</param>
    Private Sub SaveFile(value As String)

        'ファイル書き込み
        System.IO.File.WriteAllText(
                            value,
                            TextBoxMain.Text,
                            System.Text.Encoding.GetEncoding(Encording))

        FileName = value

    End Sub

    ''' <summary>
    ''' 編集内容破棄確認
    ''' </summary>
    ''' <returns>Trueの場合は破棄する</returns>
    Private Function AskGiveUpText() As Boolean

        '未編集かデータがない場合はクローズ
        If Not Edited OrElse TextBoxMain.TextLength = 0 Then Return True

        '編集内容破棄確認ダイアログ
        If DialogResult.Yes =
            MessageBox.Show("編集内容を破棄しますか？", ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) Then
            Return True
        Else
            Return False
        End If

    End Function

    ''' <summary>
    ''' 印刷準備Subプロシージャ
    ''' </summary>
    Private Sub SetPrintDocument1()

        PrintString = TextBoxMain.Text
        PrintDocument1.DefaultPageSettings.Margins =
            New Printing.Margins(20, 60, 20, 60)
        PrintDocument1.DocumentName = FileName

    End Sub

    ''' <summary>
    ''' 1ページごとに呼ばれる印刷Subプロシージャ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub PrintDocument1_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintDocument1.PrintPage

        Dim charactersOnPage As Integer = 0
        Dim linesPerPage As Integer = 0
        e.Graphics.MeasureString(PrintString, TextBoxMain.Font,
                                 e.MarginBounds.Size, StringFormat.GenericTypographic,
                                 charactersOnPage, linesPerPage
                                 )
        e.Graphics.DrawString(PrintString, TextBoxMain.Font,
                              Brushes.Black, e.MarginBounds,
                              StringFormat.GenericTypographic)
        PrintString = PrintString.Substring(charactersOnPage)
        If PrintString.Length > 0 Then
            e.HasMorePages = True
        Else
            e.HasMorePages = False
            PrintString = TextBoxMain.Text
        End If

    End Sub


End Class
