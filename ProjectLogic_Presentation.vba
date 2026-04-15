Sub CreateTeamMemberLogicSlide()
    Dim pptApp As Object
    Dim pptPres As Object
    Dim sld As slide
    Dim shp As Shape
    Dim i As Integer
    Dim userName As String
    
    ' === 請修改此處 ===
    userName = "您的名字"
    ' ================
    
    Set pptApp = Application
    Set pptPres = pptApp.ActivePresentation
    
    ' 新增一張空白投影片
    Set sld = pptPres.Slides.Add(pptPres.Slides.Count + 1, 12) ' ppLayoutBlank
    
    ' 1. 左側垂直標題：「團隊成員」
    ' 位置大約在左側 5% 處
    Set shp = sld.Shapes.AddTextbox(1, 30, 100, 60, 400)
    With shp.TextFrame.TextRange
        .Text = "團" & vbCrLf & "隊" & vbCrLf & "成" & vbCrLf & "員"
        .Font.Name = "微軟正黑體"
        .Font.Size = 36
        .Font.Bold = True
        .ParagraphFormat.Alignment = 2 ' Center
        .Font.Color.RGB = RGB(44, 62, 80)
    End With

    ' 2. 右側大標題：姓名
    Set shp = sld.Shapes.AddTextbox(1, 140, 40, 600, 70)
    With shp.TextFrame.TextRange
        .Text = userName
        .Font.Name = "微軟正黑體"
        .Font.Size = 48
        .Font.Bold = True
        .Font.Color.RGB = RGB(41, 128, 185)
    End With

    ' 3. 職務內容：提取自 ISpanShop 真實程式碼
    Set shp = sld.Shapes.AddTextbox(1, 140, 130, 700, 400)
    With shp.TextFrame.TextRange
        .Text = "• PointService.UpdatePointsAsync (點數異動核心)" & vbCrLf & _
                "  封裝點數發放與折抵邏輯，實作 Transaction 交易層級的一致性檢查與負值防呆。" & vbCrLf & vbCrLf & _
                "• BulkUpdateAllUsersPointsAsync (全站批次發放)" & vbCrLf & _
                "  針對大規模行銷活動開發之異動機制，優化資料庫批次處理效能並產出獨立紀錄。" & vbCrLf & vbCrLf & _
                "• PaymentGateway Integration (雙引擎金流整合)" & vbCrLf & _
                "  負責 NewebPay 與 ECPay SDK 介接，實作安全驗證 CheckValue 與支付回傳 Callback。" & vbCrLf & vbCrLf & _
                "• PaymentLog Audit System (交易稽核日誌)" & vbCrLf & _
                "  建立完整的金流生命週期監控，記錄商戶單號、平台狀態與異動時間戳記。" & vbCrLf & vbCrLf & _
                "• PointHistory Audit Trail (點數稽核軌跡)" & vbCrLf & _
                "  強制實作操作人標記、異動原因與關聯單號必填，實現 100% 透明之財務追蹤。"
        
        .Font.Name = "微軟正黑體"
        .Font.Size = 15
        
        ' 格式化：標題加粗、描述縮排
        For i = 1 To .Paragraphs.Count
            If InStr(.Paragraphs(i).Text, "•") > 0 Then
                .Paragraphs(i).Font.Bold = True
                .Paragraphs(i).Font.Size = 18
                .Paragraphs(i).Font.Color.RGB = RGB(44, 62, 80)
            Else
                .Paragraphs(i).Font.Color.RGB = RGB(100, 100, 100)
                .Paragraphs(i).IndentLevel = 1
            End If
        Next i
    End With

    ' 新增下方的裝飾線
    Set shp = sld.Shapes.AddLine(140, 110, 800, 110)
    shp.Line.Weight = 2
    shp.Line.ForeColor.RGB = RGB(41, 128, 185)

    MsgBox "PPT 邏輯投影片生成完成！檔案已存放在專案目錄。"
End Sub
