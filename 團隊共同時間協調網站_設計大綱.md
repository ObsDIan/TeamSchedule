# 團隊共同時間協調網站｜AI 開發交接文件

## 1. 專案背景

本專案是一個以「個人可用狀態」為核心的團隊活動協調網站。

主要使用情境：

- 使用者平日通常上班忙碌。
- 假日可能有空安排聚餐、旅遊、運動或其他團體活動。
- 使用者可能同時加入多個團隊。
- 使用者不希望每加入一個團隊，就重新填寫一次自己的可用日期。
- 團隊活動確認後，希望自動回寫個人狀態，避免同一天重複答應不同活動。

本系統第一階段不打算做完整的 Google Calendar 或 Outlook Calendar，而是先建立最簡單、可用的日期同步機制。

---

## 2. 第一階段產品定位

第一階段產品定義：

> 使用者只設定一次自己的平日、假日與特殊日期狀態；加入團隊後，團隊直接共用這份個人狀態。當使用者確認參加某個團隊活動後，該日期自動視為忙碌，其他團隊也會立即看到更新後的結果。

本產品不是完整行事曆，而是：

> 以個人可用狀態為核心的跨團隊活動協調工具。

---

## 3. 第一階段開發範圍

第一階段只處理「日期」，不處理一天內的詳細時間。

### 第一階段包含

- 使用者註冊、登入、登出。
- 個人每週預設狀態。
- 個人特殊日期設定。
- 個人月曆。
- 建立團隊。
- 使用邀請碼或邀請連結加入團隊。
- 團隊成員管理。
- 團隊管理者建立活動。
- 活動可設定一個或多個候選日期。
- 團隊自動讀取成員個人狀態。
- 日期格顯示團隊有空、忙碌、不確定、未設定人數。
- 成員針對活動選擇參加、不參加或不確定。
- 管理者確認最終活動日期。
- 活動確認後，參加成員的該日期自動視為忙碌。
- 活動取消後，自動解除該活動造成的占用。

### 第一階段不包含

- 小時與分鐘。
- 日檢視與週檢視。
- 拖曳行程。
- 重複行程。
- Google Calendar 同步。
- Outlook Calendar 同步。
- ICS 匯入與匯出。
- Email 通知。
- LINE、Teams、Slack 或 Discord 通知。
- 行動 App。
- AI 排程。
- 會議室預約。
- 多組織架構。
- 複雜角色權限。
- 跨時區。
- 國定假日自動載入。

---

## 4. 核心使用流程

### 4.1 一般使用者流程

1. 註冊帳號。
2. 登入系統。
3. 設定每週預設狀態。
4. 設定特殊日期狀態。
5. 建立或加入團隊。
6. 團隊建立活動後，查看候選日期。
7. 系統顯示自己的個人狀態。
8. 使用者針對候選日期選擇參加、不參加或不確定。
9. 管理者確認最終日期。
10. 若使用者參加，系統將該日期自動視為忙碌。
11. 其他團隊查詢該日期時，也會看到該使用者為忙碌。

### 4.2 團隊管理者流程

1. 建立團隊。
2. 取得邀請碼或邀請連結。
3. 邀請其他使用者加入。
4. 建立活動。
5. 選擇一個或多個候選日期。
6. 查看每個候選日期的團隊可用狀況。
7. 查看成員活動回覆。
8. 選擇最終活動日期。
9. 系統建立活動占用。
10. 必要時取消活動。

---

## 5. 使用者角色

第一階段只保留兩種角色。

### 5.1 團隊管理者

可執行：

- 修改團隊名稱與說明。
- 產生邀請碼。
- 查看團隊成員。
- 移除團隊成員。
- 建立活動。
- 修改尚未確認的活動。
- 確認最終活動日期。
- 取消活動。

### 5.2 一般成員

可執行：

- 查看所屬團隊。
- 查看團隊活動。
- 查看候選日期。
- 回覆參加、不參加或不確定。
- 修改尚未確認活動的回覆。
- 查看最終活動結果。
- 退出活動。
- 離開團隊。

---

## 6. 個人狀態設計

### 6.1 個人日期狀態

個人日期狀態包含：

| 狀態 | 代碼建議 | 顏色 | 說明 |
|---|---|---|---|
| 有空 | `Available` | 綠色 | 當天可以安排活動 |
| 忙碌 | `Busy` | 紅色 | 當天無法安排活動 |
| 不確定 | `Maybe` | 黃色 | 尚未能確認 |
| 未設定 | `Unset` | 灰色 | 沒有任何規則或設定 |

### 6.2 每週預設狀態

使用者可設定星期一至星期日的預設狀態。

範例：

| 星期 | 預設狀態 |
|---|---|
| 星期一 | 忙碌 |
| 星期二 | 忙碌 |
| 星期三 | 忙碌 |
| 星期四 | 忙碌 |
| 星期五 | 忙碌 |
| 星期六 | 有空 |
| 星期日 | 有空 |

### 6.3 特殊日期設定

特殊日期可覆蓋每週預設。

範例：

- 星期六預設有空。
- 2026-08-15 有家庭聚會。
- 使用者將 2026-08-15 特別設定為忙碌。
- 最終結果為忙碌。

---

## 7. 個人最終狀態判斷規則

某位使用者在某一天的最終狀態，依以下優先順序判斷：

1. 是否有已確認參加的有效團隊活動。
2. 是否有特殊日期設定。
3. 是否有每週預設狀態。
4. 若皆沒有則為未設定。

判斷概念：

```text
已確認活動
    ↓
特殊日期設定
    ↓
星期預設設定
    ↓
未設定
```

建議後端建立集中方法處理，不要在多個 Controller 重複撰寫。

```csharp
private AvailabilityStatus? GetFinalStatus(
    bool hasConfirmedActivity,
    AvailabilityStatus? dateOverride,
    AvailabilityStatus? weeklyStatus)
{
    if (hasConfirmedActivity)
    {
        return AvailabilityStatus.Busy;
    }

    if (dateOverride.HasValue)
    {
        return dateOverride.Value;
    }

    if (weeklyStatus.HasValue)
    {
        return weeklyStatus.Value;
    }

    return null;
}
```

---

## 8. 活動參加意願與個人狀態的差異

必須區分：

1. 個人日期狀態。
2. 活動參加意願。

例如：

- 使用者當天個人狀態是有空。
- 但使用者不想參加該活動。
- 活動回覆可以選擇不參加。
- 個人狀態仍然可以保持有空。

因此：

> 有空不等於參加。

活動回覆建議包含：

| 狀態 | 代碼建議 | 說明 |
|---|---|---|
| 參加 | `Join` | 願意參加該候選日期 |
| 不參加 | `Decline` | 不參加該候選日期 |
| 不確定 | `Maybe` | 尚未能決定 |
| 未回覆 | `NoResponse` | 尚未操作 |

---

## 9. 活動確認與回寫規則

### 9.1 回寫時機

不要在使用者剛點選某個候選日期可以參加時，就立刻把該日期設為忙碌。

正確流程：

1. 管理者建立活動與候選日期。
2. 成員填寫參加意願。
3. 管理者選擇最終日期。
4. 活動狀態改為已確認。
5. 系統將選擇參加該日期的成員視為忙碌。

### 9.2 不要直接修改特殊日期設定

活動確認後，不建議把 `UserDateOverride` 改成忙碌。

建議透過活動資料動態判斷忙碌。

理由：

- 活動取消時可直接解除占用。
- 不會覆蓋使用者原本手動設定。
- 可保留忙碌來源。
- 可知道是哪個活動造成占用。

### 9.3 活動取消

活動取消後：

- 活動狀態改為 `Cancelled`。
- 該活動不再影響個人最終狀態。
- 不要刪除使用者原本的特殊日期設定。

---

## 10. 日期填充視覺化

### 10.1 設計概念

每個日期格像一個透明容器。

團隊總人數是容器容量。

每位成員占日期格高度的一部分。

例如團隊有 10 人：

- 6 人有空：綠色 60%。
- 2 人忙碌：紅色 20%。
- 1 人不確定：黃色 10%。
- 1 人未設定：灰色 10%。

四種顏色加總必須為 100%。

### 10.2 日期格顯示內容

每個日期格至少顯示：

- 日期。
- 有空人數。
- 忙碌人數。
- 不確定人數。
- 未設定人數。
- 團隊總人數。
- 使用者自己的狀態。
- 是否為推薦日期。

### 10.3 填充順序

建議由下往上：

1. 綠色：有空。
2. 黃色：不確定。
3. 紅色：忙碌。
4. 灰色：未設定。

### 10.4 CSS 實作概念

```css
.calendar-fill {
    position: absolute;
    inset: 0;
    display: flex;
    flex-direction: column;
    justify-content: flex-end;
}

.fill-segment {
    width: 100%;
}

.fill-available {
    background-color: rgba(25, 135, 84, 0.35);
}

.fill-maybe {
    background-color: rgba(255, 193, 7, 0.35);
}

.fill-busy {
    background-color: rgba(220, 53, 69, 0.35);
}

.fill-unset {
    background-color: rgba(108, 117, 125, 0.15);
}
```

---

## 11. 月曆畫面實作方式

第一階段建議不要使用 FullCalendar。

理由：

- 只需要月檢視。
- 日期格需要大量客製化。
- 日期格要顯示比例填充。
- 不需要拖曳與時間軸。
- 自行使用 CSS Grid 較容易掌控。

技術建議：

- ASP.NET Core MVC。
- Razor View。
- Bootstrap 5。
- CSS Grid。
- 原生 JavaScript。
- Entity Framework Core。
- SQL Server。

月曆本質上是七欄網格：

```text
星期日 星期一 星期二 星期三 星期四 星期五 星期六
```

需要計算：

- 當月第一天。
- 當月最後一天。
- 月曆顯示起始日。
- 月曆顯示結束日。
- 前後月份補位日期。

---

## 12. 建議 ViewModel

```csharp
public class CalendarMonthViewModel
{
    public int Year { get; set; }

    public int Month { get; set; }

    public string MonthTitle => $"{Year} 年 {Month} 月";

    public List<CalendarDayViewModel> Days { get; set; } = new();
}

public class CalendarDayViewModel
{
    public DateTime Date { get; set; }

    public bool IsCurrentMonth { get; set; }

    public bool IsToday { get; set; }

    public int AvailableCount { get; set; }

    public int BusyCount { get; set; }

    public int MaybeCount { get; set; }

    public int UnsetCount { get; set; }

    public int TotalMemberCount { get; set; }

    public string? MyStatus { get; set; }

    public double AvailablePercent =>
        CalculatePercent(AvailableCount);

    public double BusyPercent =>
        CalculatePercent(BusyCount);

    public double MaybePercent =>
        CalculatePercent(MaybeCount);

    public double UnsetPercent =>
        CalculatePercent(UnsetCount);

    private double CalculatePercent(int count)
    {
        if (TotalMemberCount <= 0)
        {
            return 0;
        }

        return Math.Round(
            (double)count / TotalMemberCount * 100,
            2);
    }
}
```

---

## 13. 月曆日期產生邏輯

```csharp
private CalendarMonthViewModel BuildCalendar(
    int year,
    int month)
{
    DateTime firstDayOfMonth =
        new DateTime(year, month, 1);

    DateTime lastDayOfMonth =
        firstDayOfMonth.AddMonths(1).AddDays(-1);

    int previousDays =
        (int)firstDayOfMonth.DayOfWeek;

    DateTime calendarStart =
        firstDayOfMonth.AddDays(-previousDays);

    int nextDays =
        6 - (int)lastDayOfMonth.DayOfWeek;

    DateTime calendarEnd =
        lastDayOfMonth.AddDays(nextDays);

    var model = new CalendarMonthViewModel
    {
        Year = year,
        Month = month
    };

    for (
        DateTime date = calendarStart;
        date <= calendarEnd;
        date = date.AddDays(1))
    {
        model.Days.Add(new CalendarDayViewModel
        {
            Date = date,
            IsCurrentMonth = date.Month == month,
            IsToday = date.Date == DateTime.Today
        });
    }

    return model;
}
```

---

## 14. 頁面規劃

第一階段建議控制在以下頁面。

### 14.1 登入頁

功能：

- Email。
- 密碼。
- 登入。
- 前往註冊。

### 14.2 註冊頁

功能：

- 顯示名稱。
- Email。
- 密碼。
- 確認密碼。

### 14.3 個人月曆頁

功能：

- 查看目前月份。
- 切換上個月與下個月。
- 顯示個人最終狀態。
- 點擊日期。
- 設定有空、忙碌、不確定。
- 清除特殊日期設定。
- 顯示已確認參加活動。

### 14.4 每週預設設定頁

功能：

- 設定星期一至星期日的預設狀態。
- 一次儲存七天設定。

### 14.5 我的團隊頁

功能：

- 查看加入的團隊。
- 建立團隊。
- 輸入邀請碼加入團隊。

### 14.6 團隊首頁

功能：

- 團隊名稱。
- 團隊成員。
- 邀請碼。
- 進行中活動。
- 已確認活動。
- 建立活動。

### 14.7 建立活動頁

功能：

- 活動名稱。
- 活動說明。
- 候選日期。
- 建立活動。

### 14.8 活動回覆頁

功能：

- 顯示候選日期。
- 顯示日期填充圖。
- 顯示自己的個人狀態。
- 選擇參加、不參加、不確定。
- 查看團隊回覆統計。

### 14.9 活動結果頁

功能：

- 顯示最終日期。
- 顯示參加成員。
- 顯示不參加成員。
- 取消活動。
- 退出活動。

---

## 15. 資料庫設計

## 15.1 ApplicationUser

可使用 ASP.NET Core Identity。

建議額外欄位：

| 欄位 | 型別建議 | 說明 |
|---|---|---|
| Id | `nvarchar(450)` | Identity 使用者 ID |
| DisplayName | `nvarchar(100)` | 顯示名稱 |
| CreatedAt | `datetime2` | 建立時間 |
| Status | `int` | 帳號狀態 |

---

## 15.2 UserWeeklyAvailability

| 欄位 | 型別建議 | 說明 |
|---|---|---|
| Id | `bigint` | 主鍵 |
| UserId | `nvarchar(450)` | 使用者 ID |
| DayOfWeek | `int` | 0 至 6 |
| AvailabilityStatus | `int` | 有空、忙碌、不確定 |
| UpdatedAt | `datetime2` | 修改時間 |

唯一索引：

```text
UserId + DayOfWeek
```

---

## 15.3 UserDateOverride

| 欄位 | 型別建議 | 說明 |
|---|---|---|
| Id | `bigint` | 主鍵 |
| UserId | `nvarchar(450)` | 使用者 ID |
| TargetDate | `date` | 特殊日期 |
| AvailabilityStatus | `int` | 有空、忙碌、不確定 |
| Note | `nvarchar(200)` | 備註 |
| UpdatedAt | `datetime2` | 修改時間 |

唯一索引：

```text
UserId + TargetDate
```

---

## 15.4 Team

| 欄位 | 型別建議 | 說明 |
|---|---|---|
| TeamId | `bigint` | 主鍵 |
| TeamName | `nvarchar(100)` | 團隊名稱 |
| Description | `nvarchar(500)` | 團隊說明 |
| OwnerUserId | `nvarchar(450)` | 團隊建立者 |
| InviteCode | `nvarchar(50)` | 邀請碼 |
| Status | `int` | 團隊狀態 |
| CreatedAt | `datetime2` | 建立時間 |

`InviteCode` 建議建立唯一索引。

---

## 15.5 TeamMember

| 欄位 | 型別建議 | 說明 |
|---|---|---|
| TeamMemberId | `bigint` | 主鍵 |
| TeamId | `bigint` | 團隊 ID |
| UserId | `nvarchar(450)` | 使用者 ID |
| Role | `int` | 管理者或一般成員 |
| JoinedAt | `datetime2` | 加入時間 |

唯一索引：

```text
TeamId + UserId
```

---

## 15.6 TeamActivity

| 欄位 | 型別建議 | 說明 |
|---|---|---|
| ActivityId | `bigint` | 主鍵 |
| TeamId | `bigint` | 團隊 ID |
| Title | `nvarchar(200)` | 活動名稱 |
| Description | `nvarchar(1000)` | 活動說明 |
| FinalDate | `date` nullable | 最終日期 |
| Status | `int` | 回覆中、已確認、已取消 |
| CreatedBy | `nvarchar(450)` | 建立者 |
| CreatedAt | `datetime2` | 建立時間 |
| ConfirmedAt | `datetime2` nullable | 確認時間 |

---

## 15.7 ActivityCandidateDate

| 欄位 | 型別建議 | 說明 |
|---|---|---|
| CandidateDateId | `bigint` | 主鍵 |
| ActivityId | `bigint` | 活動 ID |
| CandidateDate | `date` | 候選日期 |

唯一索引：

```text
ActivityId + CandidateDate
```

---

## 15.8 ActivityResponse

| 欄位 | 型別建議 | 說明 |
|---|---|---|
| ResponseId | `bigint` | 主鍵 |
| ActivityId | `bigint` | 活動 ID |
| CandidateDateId | `bigint` | 候選日期 ID |
| UserId | `nvarchar(450)` | 使用者 ID |
| ResponseStatus | `int` | 參加、不參加、不確定 |
| UpdatedAt | `datetime2` | 修改時間 |

唯一索引：

```text
ActivityId + CandidateDateId + UserId
```

---

## 15.9 ActivityParticipant

| 欄位 | 型別建議 | 說明 |
|---|---|---|
| ActivityParticipantId | `bigint` | 主鍵 |
| ActivityId | `bigint` | 活動 ID |
| UserId | `nvarchar(450)` | 使用者 ID |
| ParticipationStatus | `int` | 參加、退出 |
| JoinedAt | `datetime2` | 加入時間 |
| UpdatedAt | `datetime2` | 修改時間 |

唯一索引：

```text
ActivityId + UserId
```

---

## 16. 建議 Enum

```csharp
public enum AvailabilityStatus
{
    Available = 1,
    Busy = 2,
    Maybe = 3
}

public enum TeamRole
{
    Owner = 1,
    Member = 2
}

public enum ActivityStatus
{
    Open = 1,
    Confirmed = 2,
    Cancelled = 3
}

public enum ActivityResponseStatus
{
    Join = 1,
    Decline = 2,
    Maybe = 3
}

public enum ParticipationStatus
{
    Joined = 1,
    Withdrawn = 2
}
```

---

## 17. 建議服務拆分

不要把所有邏輯寫在 Controller。

建議建立以下服務。

### 17.1 CalendarService

負責：

- 產生月份日期。
- 查詢個人最終狀態。
- 查詢指定期間狀態。
- 計算活動造成的日期占用。

### 17.2 TeamService

負責：

- 建立團隊。
- 加入團隊。
- 驗證團隊成員。
- 驗證團隊管理者。
- 查詢團隊成員。

### 17.3 ActivityService

負責：

- 建立活動。
- 建立候選日期。
- 儲存活動回覆。
- 確認最終日期。
- 建立參加名單。
- 取消活動。

### 17.4 AvailabilityService

負責：

- 讀取每週預設狀態。
- 讀取特殊日期設定。
- 計算某使用者某日最終狀態。
- 計算團隊每日統計。

---

## 18. API 或 Controller Action 建議

### CalendarController

```text
GET  /Calendar
GET  /Calendar?year=2026&month=8
POST /Calendar/SaveStatus
POST /Calendar/SaveWeeklySettings
```

### TeamController

```text
GET  /Team
GET  /Team/Details/{teamId}
GET  /Team/Create
POST /Team/Create
POST /Team/Join
POST /Team/Leave
POST /Team/RemoveMember
```

### ActivityController

```text
GET  /Activity/Create/{teamId}
POST /Activity/Create
GET  /Activity/Details/{activityId}
POST /Activity/SaveResponse
POST /Activity/Confirm
POST /Activity/Cancel
POST /Activity/Withdraw
```

---

## 19. 個人日期儲存邏輯

### Request Model

```csharp
public class SaveCalendarStatusRequest
{
    public DateTime Date { get; set; }

    public AvailabilityStatus? Status { get; set; }
}
```

### 規則

- `Status` 有值：新增或更新 `UserDateOverride`。
- `Status` 無值：刪除該日期的特殊設定。
- TargetDate 儲存前必須使用 `.Date`。
- UserId 必須從登入身分取得，不可由前端傳入。

---

## 20. 團隊統計計算

團隊日期統計的分母：

```text
目前有效團隊成員總數
```

每位成員需要計算該日期最終狀態。

統計結果：

```text
AvailableCount
BusyCount
MaybeCount
UnsetCount
```

計算：

```text
UnsetCount =
TotalMemberCount
- AvailableCount
- BusyCount
- MaybeCount
```

比例：

```text
狀態人數 ÷ 團隊總人數 × 100%
```

不要建立額外統計資料表。

第一階段可在查詢時即時計算。

---

## 21. 推薦日期邏輯

第一階段不要使用 AI。

建議排序：

1. 有空人數最多。
2. 忙碌人數最少。
3. 未設定人數最少。
4. 不確定人數最少。
5. 日期較近者優先。

可以使用簡單分數：

```text
推薦分數 =
有空人數 × 3
－ 忙碌人數 × 3
－ 不確定人數
－ 未設定人數
```

推薦結果只是建議。

最終日期必須由管理者手動確認。

---

## 22. 權限規則

後端必須檢查：

- 使用者是否已登入。
- 使用者是否為團隊成員。
- 使用者是否為團隊管理者。
- 使用者只能修改自己的個人狀態。
- 使用者只能修改自己的活動回覆。
- 只有管理者可以確認活動。
- 只有管理者可以取消活動。
- 已確認或已取消活動不可再修改候選日期。
- 未加入團隊的使用者不可查看活動內容。

不要只依賴前端隱藏按鈕。

所有權限必須由後端再次驗證。

---

## 23. 建議專案結構

```text
/Controllers
    AccountController.cs
    CalendarController.cs
    TeamController.cs
    ActivityController.cs

/Models
    ApplicationUser.cs
    UserWeeklyAvailability.cs
    UserDateOverride.cs
    Team.cs
    TeamMember.cs
    TeamActivity.cs
    ActivityCandidateDate.cs
    ActivityResponse.cs
    ActivityParticipant.cs

/ViewModels
    CalendarMonthViewModel.cs
    CalendarDayViewModel.cs
    TeamDetailsViewModel.cs
    ActivityDetailsViewModel.cs
    ActivityCandidateViewModel.cs

/Services
    CalendarService.cs
    AvailabilityService.cs
    TeamService.cs
    ActivityService.cs

/Views
    /Calendar
    /Team
    /Activity
    /Account

/wwwroot
    /css
        site.css
        calendar.css
    /js
        calendar.js
        activity.js
```

---

## 24. 建議開發順序

### 階段一：專案初始化

- 建立 ASP.NET Core MVC 專案。
- 建立 SQL Server 連線。
- 加入 Entity Framework Core。
- 加入 ASP.NET Core Identity。
- 建立 Migration。
- 完成註冊、登入與登出。

### 階段二：個人月曆靜態畫面

- 建立月曆 ViewModel。
- 產生月份日期格。
- 完成上個月、下個月切換。
- 完成 CSS Grid 月曆。
- 點擊日期開啟 Bootstrap Modal。
- 統計資料先使用假資料。

### 階段三：個人狀態

- 建立每週預設資料表。
- 建立特殊日期資料表。
- 完成日期狀態新增、修改、刪除。
- 完成最終狀態判斷。
- 重新整理後狀態必須保留。

### 階段四：團隊

- 建立團隊。
- 產生唯一邀請碼。
- 透過邀請碼加入。
- 顯示團隊成員。
- 驗證團隊權限。

### 階段五：團隊狀態統計

- 取得所有有效成員。
- 計算每位成員每日最終狀態。
- 計算有空、忙碌、不確定、未設定人數。
- 顯示日期填充圖。

### 階段六：活動

- 建立活動。
- 建立候選日期。
- 成員填寫參加意願。
- 顯示活動統計。
- 管理者確認最終日期。

### 階段七：活動回寫

- 建立活動參加名單。
- 已確認活動影響個人最終狀態。
- 活動取消解除占用。
- 活動退出解除該使用者占用。

---

## 25. 第一階段驗收條件

### 帳號

- 使用者可以註冊。
- 使用者可以登入與登出。
- 未登入者不能使用系統主要功能。

### 個人狀態

- 使用者可以設定星期一至星期日的預設狀態。
- 使用者可以設定特殊日期。
- 特殊日期優先於星期預設。
- 使用者可以清除特殊日期設定。
- 個人月曆可以切換月份。

### 團隊

- 使用者可以建立團隊。
- 使用者可以透過邀請碼加入團隊。
- 同一使用者不能重複加入同一團隊。
- 未加入團隊者不能查看團隊內容。

### 團隊統計

- 系統可以統計每一天各狀態人數。
- 日期格比例以團隊總人數為分母。
- 四種顏色比例總和為 100%。
- 點擊日期可以查看各狀態成員名單。

### 活動

- 管理者可以建立活動。
- 活動可以包含多個候選日期。
- 成員可以回覆參加、不參加或不確定。
- 管理者可以確認最終日期。
- 已確認活動可以占用參加成員日期。
- 活動取消後不再占用日期。

### 權限

- 一般成員不能確認或取消活動。
- 使用者不能修改其他人的狀態。
- 使用者不能修改其他人的活動回覆。

---

## 26. AI 開發注意事項

1. 不要擴充第一階段範圍。
2. 不要自行加入 Angular。
3. 不要自行加入 FullCalendar。
4. 不要先實作 Google 或 Outlook 同步。
5. 不要將個人狀態複製到每一個團隊。
6. 團隊統計必須即時讀取個人狀態。
7. 活動占用不要覆寫使用者特殊日期。
8. 日期欄位應使用 `date` 或 C# 的 `.Date`。
9. UserId 必須從登入身分取得。
10. 所有修改操作必須做後端權限驗證。
11. Controller 只負責接收請求與回傳結果。
12. 核心判斷必須放在 Service。
13. 第一階段優先完成可用流程，不追求動畫與複雜 UI。
14. 每完成一個階段，先確保 Migration、資料庫與頁面可正常運作。
15. 所有程式碼需具備基本例外處理與驗證。

---

## 27. 第一個開發任務

AI 接手後，第一個任務應為：

> 建立 ASP.NET Core MVC 專案，加入 ASP.NET Core Identity、Entity Framework Core 與 SQL Server，完成登入註冊後，建立一個能切換月份、點擊日期並開啟狀態設定 Modal 的靜態個人月曆。

第一個任務暫時不要接團隊與活動資料。

完成標準：

- 月曆正確顯示七欄。
- 月份前後補位正確。
- 上個月與下個月切換正常。
- 今日日期有明顯標記。
- 點擊日期可開啟 Modal。
- Modal 顯示選中的日期。
- 可選擇有空、忙碌、不確定與清除設定。
- 第一版可先不寫入資料庫。

---

## 28. 最終核心原則

本專案最重要的三個原則：

### 原則一：個人狀態只有一份

團隊不能複製個人狀態。

所有團隊必須共用使用者的同一份個人狀態。

### 原則二：活動與個人設定分開保存

活動造成的忙碌不能直接覆寫使用者特殊日期。

個人最終狀態由規則即時計算。

### 原則三：第一階段只做日期

先完成：

```text
設定個人狀態
→ 加入團隊
→ 查看團隊日期狀況
→ 建立活動
→ 成員回覆
→ 確認日期
→ 回寫忙碌
```

暫時不要實作詳細時段與外部行事曆同步。
