# 團隊共同時間協調網站 (TeamSchedule) 開發進度與執行計畫

## 📌 當前開發進度彙整

| 階段 | 內容描述 | 狀態 | 完成項目 / 異動檔案 |
|---|---|---|---|
| **階段 1** | 基礎專案架構與 SQL Server EF Core 模型建立 | ✅ 已完成 | ASP.NET Core MVC 10.0、Identity整合、Models、DbContext、EF Migrations 及 LocalDB 建立。 |
| **階段 2** | 核心時間狀態計算與月曆算表服務 | ✅ 已完成 | `IAvailabilityService`、`AvailabilityService` (四層狀態優先判定、個人月曆與團隊 4 色比例算表)。 |
| **階段 3** | 個人月曆與每週預設設定頁面 (UI/UX) | ✅ 已完成 | `PersonalController`、`Calendar.cshtml` (CSS Grid + Modal AJAX 異動)、`WeeklySetup.cshtml`、**全站深色模式切換 (Dark Mode)**、**抽取並客製化 Identity 登入/註冊/Manage 兩欄式側邊導覽選單與所有子頁面 (Profile, Email, Password, Privacy)**。 |
| **階段 4** | 團隊管理、活動排程與 4 色比例累積月曆 | ✅ 已完成 | `TeamService`、`TeamController`、團隊建立與 6 位邀請碼、活動發起、投票矩陣、**活動定案視覺優化（隱藏未選候選日、新增二次確認刪除按鈕）**、**4 色由下而上比例累積月曆 (Visual Fill)**、**首頁真實數據端點接合** 與 **完整的 AI 開發交接文件**。 |
| **階段 5** | 活動定案/取消 Email 與 LINE 通知 | ✅ 已完成 | `INotificationService` / `NotificationService` (SMTP + LINE Notify，設定走 User Secrets，未設定僅記 log)、`TeamService` 定案/取消後自動通知全體成員、`Program.cs` 註冊與 `IHttpClientFactory`。 |
| **階段 6** | ICS / iCal 行事曆匯出 | ✅ 已完成 | `TeamController.ExportIcs` 產生 RFC 5545 VEVENT、`ActivityDetail.cshtml` 定案後「匯出 ICS 行事曆」按鈕、`ITeamService.GetActivityForExportAsync` 權限驗證。 |
| **階段 7** | 小時/分鐘細粒度排程 | ✅ 已完成 | `ActivityCandidateDate.StartTime/EndTime`、`TeamActivity.FinalStartTime/FinalEndTime` (可空 `TimeSpan?`)、Migration `AddTimeSlotsToActivities`、唯一索引放寬 `ActivityId+CandidateDate+StartTime+EndTime`、`CreateActivity` 時間輸入 (`YYYY-MM-DD HH:mm~HH:mm`)、月曆/活動詳細頁時間顯示、雙層時間驗證。 |
| **程式碼檢視** | 全面檢視並修正現有問題 | ✅ 已完成 | 每週設定載入邏輯修正（直查資料表）、AJAX 端點補防偽驗證、Calendar 備註 XSS 修正、候選日期驗證（過去日期/上限 30 個/至少 1 個）、未授權改回 `Forbid()`、已取消活動禁止重新定案、補齊 `site.css` 4 色填充條與缺失樣式、移除 Home 註解展示資料。 |

---

## 🎯 產出文件與系統資產

1. **[團隊共同時間協調網站_AI開發交接文件.md](file:///c:/Users/y1935-hsiao/source/repos/TeamSchedule/%E5%9C%98%E9%9A%8A%E5%85%B1%E5%90%8C%E6%99%82%E9%96%93%E5%8D%94%E8%AA%BF%E7%B6%B2%E7%AB%99_AI%E9%96%8B%E7%99%BC%E4%BA%A4%E6%8E%A5%E6%96%87%E4%BB%B6.md)**：包含全專案架構、資料庫模型、四層狀態判定演算法、4 色比例累積演算法、User Secrets 安全指南（含 SMTP/LINE 通知設定）與後續 Roadmap。
2. **[TeamSchedule 系統與 UI/UX 設計文件 (design_document.md)](file:///C:/Users/y1935-hsiao/.gemini/antigravity-ide/brain/75586192-e3f2-4492-834b-58bdda16d97c/design_document.md)**：3-Tiered Semantic Token 系統、無障礙 WCAG 對比規範與 Controller / ViewModel 結構。

---

## 💡 通過編譯驗證
- `dotnet build` 0 警告 0 錯誤（2026-08-13 全功能合併驗證）。

## 🔒 通知設定（可選，未設定不影響功能）
```powershell
dotnet user-secrets set "Smtp:Host" "smtp.gmail.com"
dotnet user-secrets set "Smtp:Port" "587"
dotnet user-secrets set "Smtp:UserName" "your-account@example.com"
dotnet user-secrets set "Smtp:Password" "APP_PASSWORD"
dotnet user-secrets set "Smtp:From" "your-account@example.com"
dotnet user-secrets set "LineNotify:Token" "你的LINE_NOTIFY_TOKEN"
```
