using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using TeamSchedule.Models;

namespace TeamSchedule.Services;

public class NotificationService(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    ILogger<NotificationService> logger) : INotificationService
{
    public async Task SendActivityConfirmedAsync(TeamActivity activity, IReadOnlyList<ApplicationUser> recipients)
    {
        var teamName = activity.Team?.TeamName ?? "";
        var finalDate = activity.FinalDate?.ToString("yyyy-MM-dd (ddd)") ?? "";
        var subject = $"【TeamSchedule】活動「{activity.Title}」已定案";
        var lineMessage = $"【TeamSchedule】活動「{activity.Title}」已定案！\n團隊：{teamName}\n定案日期：{finalDate}\n{GetSiteUrl()}";
        var emailBody = BuildEmailHtml(
            title: "🎉 活動已定案通知",
            detailItems: new Dictionary<string, string>
            {
                ["活動名稱"] = activity.Title,
                ["團隊"] = teamName,
                ["定案日期"] = finalDate
            },
            actionText: "前往 TeamSchedule 查看活動詳情",
            siteUrl: GetSiteUrl());

        await SendEmailAsync(subject, emailBody, recipients);
        await SendLineAsync(lineMessage, activity.Title);
    }

    public async Task SendActivityCancelledAsync(TeamActivity activity, IReadOnlyList<ApplicationUser> recipients)
    {
        var teamName = activity.Team?.TeamName ?? "";
        var subject = $"【TeamSchedule】活動「{activity.Title}」已取消";
        var lineMessage = $"【TeamSchedule】活動「{activity.Title}」已取消。\n團隊：{teamName}\n{GetSiteUrl()}";
        var emailBody = BuildEmailHtml(
            title: "⚠️ 活動取消通知",
            detailItems: new Dictionary<string, string>
            {
                ["活動名稱"] = activity.Title,
                ["團隊"] = teamName,
                ["狀態"] = "已取消"
            },
            actionText: "前往 TeamSchedule 查看其他活動",
            siteUrl: GetSiteUrl());

        await SendEmailAsync(subject, emailBody, recipients);
        await SendLineAsync(lineMessage, activity.Title);
    }

    private async Task SendEmailAsync(string subject, string htmlBody, IReadOnlyList<ApplicationUser> recipients)
    {
        var host = configuration["Smtp:Host"];
        var port = configuration.GetValue<int?>("Smtp:Port");
        var from = configuration["Smtp:From"];

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from) || port is null or <= 0)
        {
            logger.LogWarning("SMTP 設定不完整（缺少 Smtp:Host、Smtp:Port 或 Smtp:From），略過 Email 通知。");
            return;
        }

        var emailRecipients = recipients.Where(r => !string.IsNullOrWhiteSpace(r.Email)).ToList();
        if (emailRecipients.Count == 0)
        {
            logger.LogWarning("通知對象均無 Email 資料，略過 Email 通知。");
            return;
        }

        var userName = configuration["Smtp:UserName"];
        var password = configuration["Smtp:Password"];
        var enableSsl = configuration.GetValue<bool?>("Smtp:EnableSsl") ?? true;

        try
        {
#pragma warning disable SYSLIB0014
            using var smtpClient = new SmtpClient(host, port.Value)
            {
                EnableSsl = enableSsl,
                Timeout = 15000,
                Credentials = string.IsNullOrWhiteSpace(userName) ? null : new NetworkCredential(userName, password)
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(from),
                Subject = subject,
                SubjectEncoding = Encoding.UTF8,
                Body = htmlBody,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true
            };

            foreach (var recipient in emailRecipients)
            {
                mailMessage.To.Add(new MailAddress(recipient.Email!, recipient.DisplayName));
            }

            await smtpClient.SendMailAsync(mailMessage);
#pragma warning restore SYSLIB0014
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "寄送 Email 通知失敗（Subject：{Subject}）。", subject);
        }
    }

    private async Task SendLineAsync(string message, string activityTitle)
    {
        var token = configuration["LineNotify:Token"];
        if (string.IsNullOrWhiteSpace(token))
        {
            logger.LogWarning("未設定 LineNotify:Token，略過 LINE 通知。");
            return;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://notify-api.line.me/api/notify");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["message"] = message
            });

            var client = httpClientFactory.CreateClient();
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                logger.LogWarning(
                    "LINE Notify 傳送失敗（活動：{Title}），回傳狀態碼 {StatusCode}：{Body}",
                    activityTitle, (int)response.StatusCode, responseBody);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "發送 LINE 通知失敗（活動：{Title}）。", activityTitle);
        }
    }

    private string GetSiteUrl()
    {
        return configuration["SiteUrl"] ?? "https://teamschedule.local";
    }

    private static string BuildEmailHtml(string title, Dictionary<string, string> detailItems, string actionText, string siteUrl)
    {
        var items = string.Join("", detailItems.Select(kv =>
            $"<li><strong>{WebUtility.HtmlEncode(kv.Key)}：</strong>{WebUtility.HtmlEncode(kv.Value)}</li>"));

        return $"""
            <html>
            <body style="font-family:微軟正黑體, Microsoft JhengHei, sans-serif; color:#212529;">
                <h2 style="color:#0d6efd;">{WebUtility.HtmlEncode(title)}</h2>
                <p>您好，您所屬團隊的活動狀態已更新，詳情如下：</p>
                <ul>
                    {items}
                </ul>
                <p><a href="{WebUtility.HtmlEncode(siteUrl)}" style="color:#0d6efd;">{WebUtility.HtmlEncode(actionText)}</a></p>
            </body>
            </html>
            """;
    }
}
