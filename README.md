# TeamSchedule

[![CI](https://github.com/ObsDIan/TeamSchedule/actions/workflows/ci.yml/badge.svg)](https://github.com/ObsDIan/TeamSchedule/actions/workflows/ci.yml)

TeamSchedule 是以 ASP.NET Core MVC 建立的團隊時間協調工具。成員可以設定每週預設狀態與單日例外、建立候選活動時段、共同投票，並將定案活動匯出為 ICS 行事曆。

## 主要功能

- ASP.NET Core Identity 帳號註冊、登入與個人資料管理
- 個人每週可用狀態與日期覆寫
- 團隊建立、邀請碼與成員管理
- 活動候選日期／時段、成員投票與活動定案
- 團隊月曆與可用比例視覺化
- Email 通知與 ICS 行事曆匯出
- 深色模式與行動版介面
- `/health` 應用程式健康檢查端點

## 技術

- .NET 10 / ASP.NET Core MVC
- Entity Framework Core 10
- ASP.NET Core Identity
- SQL Server / LocalDB
- Razor、Bootstrap、JavaScript

## 本機啟動

需求：.NET 10 SDK 與 SQL Server LocalDB，或可用的 SQL Server 執行個體。

```powershell
git clone https://github.com/ObsDIan/TeamSchedule.git
cd TeamSchedule
dotnet restore
dotnet ef database update
dotnet run
```

開發環境預設使用 `appsettings.json` 中的 LocalDB。程式啟動時也會套用尚未執行的 EF Core migrations。

## Production 設定

Production 不在版本庫保存連線字串或通知憑證。請使用環境變數或部署平台的 Secret 設定：

```text
ConnectionStrings__DefaultConnection
SiteUrl
Smtp__Host
Smtp__Port
Smtp__UserName
Smtp__Password
Smtp__From
Smtp__EnableSsl
```

本機開發可使用 User Secrets：

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_CONNECTION_STRING"
dotnet user-secrets set "Smtp:Host" "smtp.example.com"
dotnet user-secrets set "Smtp:Port" "587"
dotnet user-secrets set "Smtp:UserName" "your-account@example.com"
dotnet user-secrets set "Smtp:Password" "APP_PASSWORD"
dotnet user-secrets set "Smtp:From" "your-account@example.com"
dotnet user-secrets set "Smtp:EnableSsl" "true"
dotnet user-secrets set "SiteUrl" "https://your-domain.example"
```

未設定完整 SMTP 資料時，系統會略過 Email 通知，核心排程功能仍可使用。

> LINE Notify 已於 2025 年停止服務。現有 `LineNotify` 程式碼僅供歷史參考，不應視為可用的 Production 通知管道；後續應改接 LINE Messaging API 或其他通知服務。

## 驗證

```powershell
dotnet restore
dotnet build --configuration Release --no-restore
```

Pull Request 與 `master` 更新時，GitHub Actions 會自動執行 Release build。

## 專案文件

- [開發進度](PROJECT_PROGRESS.md)
- [AI 開發交接文件](AI開發交接文件.md)
- [系統設計大綱](團隊共同時間協調網站_設計大綱.md)
- [前端設計規範](前端色彩與深淺色模式_AI設計規範.md)

## 安全提醒

- Production 必須使用 HTTPS 並透過環境變數或 Secret 管理敏感設定。
- 部署前應確認資料庫 migrations 已備份並可回復。
- 邀請碼與使用者資料屬敏感資訊，不應寫入 log 或公開文件。
