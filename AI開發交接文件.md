# 團隊共同時間協調網站 TeamSchedule｜AI 開發與交接文件

> **文件狀態**：✅ 第一至第四階段完全開發完成，通過 0 警告 0 錯誤建置驗證。  
> **最後更新時間**：2026-08-04  
> **技術棧**：ASP.NET Core 10.0 MVC + EF Core + SQL Server (LocalDB / Production) + Identity + Bootstrap 5.3 + Vanilla CSS (3-Tiered Semantic Tokens)

---

## 1. 專案背景與核心定位

本專案是一個以「**個人可用狀態**」為核心的跨團隊活動協調網站（TeamSchedule）。

### 核心解法
> 使用者只需設定一次自己的平日、假日與特殊日期狀態；加入團隊後，團隊直接共用這份個人狀態。當使用者確認參加某個團隊活動後，該日期自動視為忙碌，其他團隊也會立即看到更新後的結果。

---

## 2. 系統技術架構與專案結構

```
TeamSchedule/
├── Areas/
│   └── Identity/
│       └── Pages/
│           ├── Account/
│           │   ├── Login.cshtml (.cs)          # 客製化登入頁面
│           │   ├── Register.cshtml (.cs)       # 客製化註冊頁面 (含暱稱欄位)
│           │   └── Manage/
│           │       ├── _Layout.cshtml          # 帳號管理中心 2 欄式 Layout
│           │       ├── _ManageNav.cshtml       # 側邊導覽列與 ManageNavPages 輔助類別
│           │       ├── Index.cshtml (.cs)       # 個人暱稱與基本資料修改
│           │       ├── Email.cshtml (.cs)       # Email 與登入帳號變更
│           │       ├── ChangePassword.cshtml    # 變更密碼
│           │       └── PersonalData.cshtml     # 隱私與帳號刪除
│           └── _ViewImports.cshtml             # Identity 命名空間與 TagHelpers 匯入
├── Controllers/
│   ├── HomeController.cs                       # 首頁 Dashboard (接合真實 SQL 統計端點)
│   ├── PersonalController.cs                   # 個人月曆與每週預設控制器
│   └── TeamController.cs                       # 團隊、成員管理與活動排程控制器
├── Data/
│   ├── ApplicationDbContext.cs                 # EF Core DbContext (唯一索引與 Cascade 關聯)
│   └── Migrations/                             # EF Core 資料庫移轉紀錄
├── Models/                                     # EF Core Entity Models
├── Services/
│   ├── IAvailabilityService.cs / AvailabilityService.cs # 時間狀態判定與 4 色比例累積計算
│   └── ITeamService.cs / TeamService.cs                 # 團隊、邀請碼與活動投票運算
├── ViewModels/                                 # 強型別 ViewModels (Calendar, Team, Home)
├── Views/
│   ├── Home/Index.cshtml                       # 首頁 Hero、Preview Card 與活動態勢
│   ├── Personal/
│   │   ├── Calendar.cshtml                     # 個人月曆 (CSS Grid + Modal AJAX)
│   │   └── WeeklySetup.cshtml                  # 每週預設一鍵套用頁面
│   ├── Team/
│   │   ├── Index.cshtml                        # 我的團隊列表
│   │   ├── Detail.cshtml                       # 團隊 Dashboard (4色比例累積月曆 + 投票矩陣)
│   │   └── ActivityDetail.cshtml               # 活動詳細頁 (定案日期過濾 + 刪除活動機制)
│   └── Shared/
│       ├── _Layout.cshtml                      # 全站 Layout (Sticky Navbar + 深色模式切換)
│       └── _LoginPartial.cshtml                # 導覽列使用者名稱與登入狀態
└── wwwroot/css/site.css                        # 3-Tiered Semantic Token CSS 設計系統
```

---

## 3. 資料庫結構與模型設計

### 3.1 實體關聯 (Entity Relationship)

* **ApplicationUser**: 繼承 `IdentityUser`，新增 `DisplayName` (暱稱) 與 `CreatedAt`。
* **UserWeeklyAvailability**: 記錄一至日預設空檔 (唯一索引：`UserId + DayOfWeek`)。
* **UserDateOverride**: 記錄特殊日期覆蓋 (唯一索引：`UserId + TargetDate`)。
* **Team**: 團隊主檔 (唯一索引：`InviteCode` 6 位大寫英數)。
* **TeamMember**: 團隊成員關係檔 (唯一索引：`TeamId + UserId`)。
* **TeamActivity**: 團隊活動 (狀態：`Open`, `Confirmed`, `Cancelled`)。
* **ActivityCandidateDate**: 活動候選日期 (唯一索引：`ActivityId + CandidateDate`)。
* **ActivityResponse**: 成員對候選日期的投票 (唯一索引：`ActivityId + CandidateDateId + UserId`)。
* **ActivityParticipant**: 活動定案後的實際參加者 (唯一索引：`ActivityId + UserId`)。

---

## 4. 核心演算法與服務邏輯

### 4.1 四層時間狀態優先判定邏輯 (`AvailabilityService`)

當查詢某位使用者在特定日期的可用狀態時，系統依據以下**優先順序**進行計算：

1. **第一優先 (Priority 1)**：**活動占用 (`Confirmed Activity Participant`)**  
   若該使用者已確認參加任何一個團隊的定案活動，該日期直接強制判定為 **忙碌 (`Busy`)**。
2. **第二優先 (Priority 2)**：**個人特殊日期覆蓋 (`UserDateOverride`)**  
   若使用者在該日期設定了單日覆蓋，套用覆蓋狀態（`Available` / `Busy` / `Maybe`）。若覆蓋為 `null` 則代表刪除覆蓋。
3. **第三優先 (Priority 3)**：**個人每週預設 (`UserWeeklyAvailability`)**  
   若無單日覆蓋，查詢該日期為星期幾（1~7），套用每週預設狀態。
4. **第四優先 (Priority 4)**：**未設定 (`Unset`)**  
   若以上皆無設定，回傳 `Unset`（灰色）。

### 4.2 團隊 4 色比例由下而上累積填滿月曆 (Visual Fill)

在團隊月曆 `Detail.cshtml` 與首頁 `Index.cshtml` 預覽卡片中：
- 每個日期區塊的高度固定（`height: 7rem` 或 `100px`），內部包含四層 `.fill-*` `<div>`：
  - **🟩 有空 (Available)**: `height: {AvailablePercent}%` (位於最下方)
  - **🟨 不確定 (Maybe)**: `height: {MaybePercent}%`
  - **🔴 忙碌 (Busy)**: `height: {BusyPercent}%`
  - **⬜ 未設定 (Unset)**: `height: {UnsetPercent}%` (位於最上方)
- 當天最多人有空（且有空人數 > 0）的日期自動加上外框與推薦星號 (`recommended`)。

---

## 5. 前端色彩與深淺色模式系統 (Design System)

專案 100% 依據 `前端色彩與深淺色模式_AI設計規範.md` 規範實作：

1. **3-Tiered Semantic Token 體系**：
   - `:root` 與 `[data-bs-theme="dark"]` 定義 `--canvas`, `--surface-1/2/3`, `--text-primary/secondary/muted`, `--accent`。
2. **防閃爍 (FOUC Free) 主題腳本**：
   - 在 `_Layout.cshtml` 的 `<head>` 頂部加入同步執行 JavaScript，避免切換頁面時出現白光閃爍。
3. **無障礙規範 (WCAG 2.1 & W3C)**：
   - 內文對比度均小於 4.5:1，標題小於 7:1。
   - 保留完整 `:focus-visible` 外框，不使用 `outline: none;`。

---

## 6. 安全性與環境設定

* **資料庫連線與 User Secrets**：
  連線字串統一集中在 `.NET User Secrets` 管理。`appsettings.json` 與 `appsettings.Production.json` 僅保留 `YOUR_PASSWORD` 預設預留位置。
  ```bash
  dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=localhost;Database=TeamScheduleDB;User Id=TeamScheduleDB;Password=your_password;TrustServerCertificate=True;"
  ```
* **自動 Migration 機制**：
  `Program.cs` 包含自動 `dbContext.Database.Migrate()` 邏輯，應用程式啟動時會自動建立並升級 SQL Server 資料庫。

---

## 7. AI 接手的後續建議 (Roadmap for Next AI Agent)

若後續有額外擴充需求，建議發展順序如下：

1. **Phase 5：簡訊/Email/LINE 通知整合**
   * 當活動定案 (`ConfirmActivity`) 時，使用 Background Worker 發送 LINE Notify 或 Email 通知團隊成員。
2. **Phase 6：ICS / iCal 日期匯出**
   * 提供 `.ics` 檔下載，讓使用者能一鍵將已確認的團隊活動匯入 iPhone 行事曆或 Google Calendar。
3. **Phase 7：小時與分鐘細粒度排程**
   * 在目前「日期」層級的基礎上，擴充單日內的時間區段選項（如：14:00~17:00）。

---

## 8. 建置與驗證指令

```bash
# 建置專案 (確保 0 警告 0 錯誤)
dotnet build

# 執行移轉 (如有修改 Models)
dotnet ef migrations add <MigrationName>

# 啟動本機開發伺服器
dotnet run
```
