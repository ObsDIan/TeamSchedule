# 團隊共同時間協調網站 (TeamSchedule) 開發進度與執行計畫

## 📌 當前開發進度彙整

| 階段 | 內容描述 | 狀態 | 完成項目 / 異動檔案 |
|---|---|---|---|
| **階段 1** | 基礎專案架構與 SQL Server EF Core 模型建立 | ✅ 已完成 | ASP.NET Core MVC 10.0、Identity整合、Models、DbContext、EF Migrations 及 LocalDB 建立。 |
| **階段 2** | 核心時間狀態計算與月曆算表服務 | ✅ 已完成 | `IAvailabilityService`、`AvailabilityService` (四層狀態優先判定、個人月曆與團隊 4 色比例算表)。 |
| **階段 3** | 個人月曆與每週預設設定頁面 (UI/UX) | ✅ 已完成 | `PersonalController`、`Calendar.cshtml` (CSS Grid + Modal AJAX 異動)、`WeeklySetup.cshtml`、**全站深色模式切換 (Dark Mode)**、**抽取並客製化 Identity 登入/註冊/Manage 兩欄式側邊導覽選單與所有子頁面 (Profile, Email, Password, Privacy)**。 |
| **階段 4** | 團隊管理、活動排程與 4 色比例累積月曆 | ✅ 已完成 | `TeamService`、`TeamController`、團隊建立與 6 位邀請碼、活動發起、投票矩陣、**活動定案視覺優化（隱藏未選候選日、新增二次確認刪除按鈕）** 與 **4 色由下而上比例累積月曆 (Visual Fill)**。 |

---

## 🛠️ 階段 3 完成詳細記錄 (Phase 3 Summary)

1. **[PersonalViewModels.cs](file:///c:/Users/y1935-hsiao/source/repos/TeamSchedule/ViewModels/PersonalViewModels.cs)**：`WeeklySetupViewModel` 與 `SetDateOverrideRequestModel`。
2. **[PersonalController.cs](file:///c:/Users/y1935-hsiao/source/repos/TeamSchedule/Controllers/PersonalController.cs)**：個人月曆檢視、每週預設設定與 AJAX 特殊日期異動。
3. **[Views/Personal/Calendar.cshtml](file:///c:/Users/y1935-hsiao/source/repos/TeamSchedule/Views/Personal/Calendar.cshtml)**：
   - 7 欄 CSS Grid 月曆。
   - 🟢 有空、🔴 忙碌、🟡 不確定、⚪ 未設定 狀態標籤與圖例。
   - 已確認團隊活動鎖定與強效標示。
   - 彈出式 Modal 選擇與備註設定，結合 Fetch API 免刷新頁面更新。
4. **[Views/Personal/WeeklySetup.cshtml](file:///c:/Users/y1935-hsiao/source/repos/TeamSchedule/Views/Personal/WeeklySetup.cshtml)**：
   - 一鍵快速套用：「💼 平日忙碌假日有空」、「🟢 全週有空」、「🔴 全週忙碌」。
5. **[Views/Shared/_Layout.cshtml](file:///c:/Users/y1935-hsiao/source/repos/TeamSchedule/Views/Shared/_Layout.cshtml)**：新增「📅 個人月曆」與「⚙️ 每週預設」導覽連結。

---

## 🎯 下一步執行計畫：階段 4 (Phase 4 Roadmap)

1. **[TeamController.cs](file:///c:/Users/y1935-hsiao/source/repos/TeamSchedule/Controllers/TeamController.cs)** & **ViewModels**：
   - 建立團隊與產生亂數 6 位邀請碼（`InviteCode`）。
   - 輸入邀請碼加入團隊。
   - 團隊列表與團隊首頁（顯示成員名單與進行中活動）。
2. **團隊活動排程**：
   - 建立活動與挑選多個候選日期（Candidate Dates）。
   - 成員回覆意願 (Join / Decline / Maybe)。
3. **管理者確認與回寫機制**：
   - 管理者點選確認活動最終日期 ➔ 系統改寫活動狀態為 `Confirmed` ➔ 自動動態占用參加成員的該日期（視為忙碌）。
4. **團隊 4 色比例累積月曆 (CSS Grid Visual Fill)**：
   - 依照交接文件第 10 節展示由下而上的 4 色（綠/黃/紅/灰）填充高度比例。
