Sub CreateISpanShopPresentation()
    Dim pptApp As Object
    Dim pptPres As Object
    Dim slideIndex As Integer
    
    Set pptApp = Application
    Set pptPres = pptApp.ActivePresentation
    
    ' Slide 1: Title
    slideIndex = 1
    Dim sld As slide
    Set sld = pptPres.Slides.Add(slideIndex, 1) ' ppLayoutTitle
    sld.Shapes.Title.TextFrame.TextRange.Text = "ISpanShop 金流與點數管理系統概覽"
    sld.Shapes(2).TextFrame.TextRange.Text = "從支付整合到會員點數稽核" & vbCrLf & "報告人：開發團隊"
    sld.NotesPage.Shapes.Placeholders(2).TextFrame.TextRange.Text = "大家好，我是負責 ISpanShop 核心模組的開發工程師。今天我將在五分鐘內，向各位介紹我們如何建構安全且具備高度追蹤性的金流與點數管理系統。"
    
    ' Slide 2: Payment Architecture
    slideIndex = slideIndex + 1
    Set sld = pptPres.Slides.Add(slideIndex, 2) ' ppLayoutText
    sld.Shapes.Title.TextFrame.TextRange.Text = "雙引擎金流整合 (Payment Gateway)"
    sld.Shapes(2).TextFrame.TextRange.Text = "• 綠界 (ECPay)：信用卡、ATM、超商代碼多元支付" & vbCrLf & _
                                            "• 藍新 (NewebPay)：備援路徑，確保營運不中斷" & vbCrLf & _
                                            "• 抽象化介面：未來可快速擴展 LINE Pay / Apple Pay"
    sld.NotesPage.Shapes.Placeholders(2).TextFrame.TextRange.Text = "在金流方面，我們同時整合了綠界與藍新兩大平台。透過抽象化設計，未來擴展新路徑時無需大幅改動程式碼。"

    ' Slide 3: Payment Audit Log
    slideIndex = slideIndex + 1
    Set sld = pptPres.Slides.Add(slideIndex, 2)
    sld.Shapes.Title.TextFrame.TextRange.Text = "嚴謹的金流稽核軌跡 (Audit Log)"
    sld.Shapes(2).TextFrame.TextRange.Text = "• PaymentLog：記錄商戶訂單號、支付方式、回傳時間" & vbCrLf & _
                                            "• 完整生命週期：待付款 -> 付款成功 -> 付款失敗" & vbCrLf & _
                                            "• Callback 驗證：數位簽章 (CheckValue) 防止偽造"
    sld.NotesPage.Shapes.Placeholders(2).TextFrame.TextRange.Text = "每一筆交易背後都有完整的 PaymentLog，並透過數位簽章驗證合法性，確保帳務與資料庫狀態一致。"

    ' Slide 4: Point Management
    slideIndex = slideIndex + 1
    Set sld = pptPres.Slides.Add(slideIndex, 2)
    sld.Shapes.Title.TextFrame.TextRange.Text = "點數管理系統 (Point Management)"
    sld.Shapes(2).TextFrame.TextRange.Text = "• 靈活發放：訂單贈點、行銷補發" & vbCrLf & _
                                            "• 負值鎖定：資料庫交易層級嚴禁點數負值" & vbCrLf & _
                                            "• 餘額同步：PointHistory 與 MemberProfile 即時連動"
    sld.NotesPage.Shapes.Placeholders(2).TextFrame.TextRange.Text = "點數系統在交易層級加入了餘額檢查，確保不會發生點數超支的情況。"

    ' Slide 5: Bulk Operations
    slideIndex = slideIndex + 1
    Set sld = pptPres.Slides.Add(slideIndex, 2)
    sld.Shapes.Title.TextFrame.TextRange.Text = "稽核軌跡與批次操作"
    sld.Shapes(2).TextFrame.TextRange.Text = "• 全站批次發放：節慶贈點、維護補償一鍵完成" & vbCrLf & _
                                            "• 稽核必填：調整原因、關聯單號、備註全面稽核" & vbCrLf & _
                                            "• 責任透明：自動記錄操作管理員帳號"
    sld.NotesPage.Shapes.Placeholders(2).TextFrame.TextRange.Text = "所有的手動調整都必須填寫原因與單號。我們也實作了全站批次發放功能，大幅提升管理效率。"

    ' Slide 6: Summary
    slideIndex = slideIndex + 1
    Set sld = pptPres.Slides.Add(slideIndex, 2)
    sld.Shapes.Title.TextFrame.TextRange.Text = "結語：安全、穩定、透明"
    sld.Shapes(2).TextFrame.TextRange.Text = "• 確保每一塊錢、每一點點數皆有跡可循" & vbCrLf & _
                                            "• 完善的後台查詢介面提升營運效率" & vbCrLf & _
                                            "• 未來計畫：導入異常點數異動自動監控"
    sld.NotesPage.Shapes.Placeholders(2).TextFrame.TextRange.Text = "ISpanShop 的目標是資料透明化。未來我們將朝向自動化監控邁進。謝謝各位！"

    MsgBox "PPT 生成完成！每一頁的備忘錄 (Notes) 已包含演講稿內容。"
End Sub
