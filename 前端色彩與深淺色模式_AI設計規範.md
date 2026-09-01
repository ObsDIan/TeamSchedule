# 前端色彩與深淺色模式設計規範｜AI 開發交接文件

## 1. 文件目的

本文件用於指導 AI 或開發人員設計本專案的前端頁面，特別是：

- 明亮模式與深色模式切換
- 文字與背景的可讀性
- 表單、卡片、導覽列、按鈕及 Modal 的顏色一致性
- 行事曆日期格與狀態色彩
- 無障礙對比與鍵盤焦點
- 避免 AI 只改背景色，卻留下無法辨識的文字或元件

本文件是設計與實作限制，不是單純的色票建議。

AI 修改前端時，必須遵守本文件，不可自行大量加入未定義顏色。

---

## 2. 核心設計原則

### 2.1 深色模式不是顏色反轉

禁止使用下列方式製作深色模式：

```css
filter: invert(1);
```

也禁止單純將：

```text
白色 → 黑色
黑色 → 白色
```

深色模式必須重新定義：

- 頁面背景
- 區塊背景
- 卡片背景
- 文字階層
- 邊框
- 輸入框
- Hover
- Active
- Focus
- Disabled
- 陰影
- 語意狀態色
- 圖表與行事曆填充色

### 2.2 使用語意 Token，不使用散落色碼

錯誤：

```css
.card {
    background: #202124;
}

.modal {
    background: #1f1f1f;
}
```

正確：

```css
.card {
    background: var(--surface-1);
}

.modal-content {
    background: var(--surface-2);
}
```

元件只描述用途，不直接決定顏色。

色彩應集中定義在 Theme Token。

### 2.3 色彩不能是唯一資訊來源

不可只使用：

- 綠色代表成功
- 紅色代表錯誤
- 黃色代表警告

同時必須搭配至少一項：

- 圖示
- 文字
- 標籤
- 邊框樣式
- 形狀
- 數字

例如：

```html
<span class="status-badge status-available">
    <i class="bi bi-check-circle" aria-hidden="true"></i>
    有空
</span>
```

不要只顯示一個綠色圓點。

---

## 3. 色彩基本概念

### 3.1 色相 Hue

色相代表顏色種類，例如紅、黃、綠、藍與紫。

色相本身不代表亮度。深藍可能比淺黃色暗很多，因此不能只看顏色名稱判斷可讀性。

### 3.2 飽和度 Saturation

飽和度代表顏色鮮豔程度。

高飽和度：

- 醒目
- 適合少量強調
- 大面積使用容易疲勞
- 深色背景上可能產生發光感

低飽和度：

- 平穩
- 適合背景和大區塊
- 過低時可能與灰色難以區分

深色模式的大面積背景應以低飽和度中性色為主。

### 3.3 明度 Lightness

明度是深色模式最重要的概念之一。

同一色相可以有：

- 很暗的背景色
- 中等亮度的邊框色
- 高亮度的文字色

深色模式的資訊階層主要應依靠：

- 明度差
- 邊框
- 間距
- 字重

不要依靠大量不同色相。

### 3.4 RGB 與 HSL

RGB 適合電腦顯示，但人工調整較難理解。

HSL 較適合設計 Token：

```css
hsl(220 20% 14%)
```

依序代表：

```text
色相 220
飽和度 20%
明度 14%
```

但 HSL 的 Lightness 不等於人眼感知亮度，因此最終仍必須使用對比工具驗證。

### 3.5 對比要求

建議遵守以下最低標準：

| 使用情境 | 最低對比 |
|---|---:|
| 一般文字 | 4.5:1 |
| 大型文字 | 3:1 |
| 圖示、輸入框邊界及重要 UI 元件 | 3:1 |
| 建議的一般內文目標 | 約 7:1 |

注意：

> `opacity: 0.5` 的白色文字在深色背景上，不一定符合 4.5:1。

必須實際計算合成後的顏色。

---

## 4. 深色模式背景階層

深色模式不要使用純黑 `#000000` 作為所有背景。

建議背景分為：

| Token | 用途 |
|---|---|
| `--canvas` | 整體頁面背景 |
| `--surface-1` | 導覽列、側欄、一般區塊 |
| `--surface-2` | 卡片、Modal、Dropdown |
| `--surface-3` | Hover、選取區、凸顯區 |
| `--surface-raised` | 浮層或高層級區塊 |

建議深色背景使用藍灰色階：

```css
--canvas: #0f141b;
--surface-1: #151b23;
--surface-2: #1b232d;
--surface-3: #24303d;
```

---

## 5. 文字階層

建議至少定義：

| Token | 用途 |
|---|---|
| `--text-primary` | 標題、主要資料、表單內容 |
| `--text-secondary` | 輔助說明 |
| `--text-muted` | 時間、次要註記 |
| `--text-disabled` | 不可操作內容 |
| `--text-on-accent` | 位於主要按鈕或色塊上的文字 |

深色模式不要使用純白作為所有文字。

範例：

```css
--text-primary: #f4f7fb;
--text-secondary: #c6d0dc;
--text-muted: #9eabb9;
--text-disabled: #707c89;
```

禁止使用模糊的：

```css
color: gray;
```

或：

```css
color: #777;
```

這些顏色在不同背景上通常無法確保對比。

---

## 6. Theme Token 建議

### 6.1 明亮模式

```css
:root,
[data-bs-theme="light"] {
    color-scheme: light;

    --canvas: #f5f7fa;
    --surface-1: #ffffff;
    --surface-2: #ffffff;
    --surface-3: #eef2f6;
    --surface-raised: #ffffff;

    --text-primary: #17202a;
    --text-secondary: #3f4b59;
    --text-muted: #667382;
    --text-disabled: #929ca7;
    --text-on-accent: #ffffff;

    --border-subtle: #d9e0e7;
    --border-strong: #aeb9c5;

    --accent: #0b62d6;
    --accent-hover: #094fae;
    --accent-active: #073f8b;
    --accent-soft: #e7f0ff;

    --success: #18794e;
    --success-soft: #e5f6ed;
    --warning: #8a5700;
    --warning-soft: #fff3d6;
    --danger: #b4232c;
    --danger-soft: #fdebec;
    --info: #096b83;
    --info-soft: #e4f5fa;

    --focus-ring: #2563eb;

    --shadow-color: 16 24 40;
    --shadow-sm: 0 1px 3px rgb(var(--shadow-color) / 0.12);
    --shadow-md: 0 8px 24px rgb(var(--shadow-color) / 0.14);
}
```

### 6.2 深色模式

```css
[data-bs-theme="dark"] {
    color-scheme: dark;

    --canvas: #0f141b;
    --surface-1: #151b23;
    --surface-2: #1b232d;
    --surface-3: #24303d;
    --surface-raised: #293645;

    --text-primary: #f4f7fb;
    --text-secondary: #c7d0db;
    --text-muted: #9eabb9;
    --text-disabled: #707d8b;
    --text-on-accent: #ffffff;

    --border-subtle: #344150;
    --border-strong: #526274;

    --accent: #74a9ff;
    --accent-hover: #91bbff;
    --accent-active: #a9c9ff;
    --accent-soft: #19385f;

    --success: #63d69b;
    --success-soft: #143b2b;
    --warning: #f2c464;
    --warning-soft: #493814;
    --danger: #ff8b93;
    --danger-soft: #4b2025;
    --info: #67d2e8;
    --info-soft: #133d48;

    --focus-ring: #8ab4ff;

    --shadow-color: 0 0 0;
    --shadow-sm: 0 1px 3px rgb(var(--shadow-color) / 0.35);
    --shadow-md: 0 12px 30px rgb(var(--shadow-color) / 0.45);
}
```

---

## 7. Bootstrap 5.3 整合

本專案統一使用：

```html
<html lang="zh-Hant" data-bs-theme="light">
```

切換深色模式：

```javascript
document.documentElement.setAttribute("data-bs-theme", "dark");
```

切換明亮模式：

```javascript
document.documentElement.setAttribute("data-bs-theme", "light");
```

不要同時混用：

```text
dark-mode
theme-dark
data-theme
data-bs-theme
```

本專案統一使用 `data-bs-theme`。

---

## 8. Theme 切換與保存

建議支援：

- `light`
- `dark`
- `auto`

初始化應在主要畫面顯示前完成，避免白色閃爍：

```html
<script>
(() => {
    const savedTheme = localStorage.getItem("theme") || "auto";

    const systemTheme = window.matchMedia(
        "(prefers-color-scheme: dark)"
    ).matches
        ? "dark"
        : "light";

    const resolvedTheme =
        savedTheme === "auto"
            ? systemTheme
            : savedTheme;

    document.documentElement.setAttribute(
        "data-bs-theme",
        resolvedTheme
    );
})();
</script>
```

完整切換程式：

```javascript
const THEME_KEY = "theme";

function getSystemTheme() {
    return window.matchMedia("(prefers-color-scheme: dark)").matches
        ? "dark"
        : "light";
}

function getSavedTheme() {
    return localStorage.getItem(THEME_KEY) || "auto";
}

function resolveTheme(theme) {
    return theme === "auto"
        ? getSystemTheme()
        : theme;
}

function applyTheme(theme) {
    const resolved = resolveTheme(theme);

    document.documentElement.setAttribute(
        "data-bs-theme",
        resolved
    );
}

function setTheme(theme) {
    if (!["light", "dark", "auto"].includes(theme)) {
        throw new Error("Unsupported theme.");
    }

    localStorage.setItem(THEME_KEY, theme);
    applyTheme(theme);
}

const systemThemeQuery =
    window.matchMedia("(prefers-color-scheme: dark)");

systemThemeQuery.addEventListener("change", () => {
    if (getSavedTheme() === "auto") {
        applyTheme("auto");
    }
});

applyTheme(getSavedTheme());
```

---

## 9. 元件設計規範

### 9.1 頁面

```css
body {
    background: var(--canvas);
    color: var(--text-primary);
}
```

### 9.2 卡片

```css
.app-card {
    background: var(--surface-2);
    color: var(--text-primary);
    border: 1px solid var(--border-subtle);
    box-shadow: var(--shadow-sm);
    border-radius: 0.75rem;
}
```

深色模式中，卡片不要只靠陰影區分，必須搭配背景明度差或邊框。

### 9.3 表單

```css
.form-control,
.form-select {
    background-color: var(--surface-1);
    color: var(--text-primary);
    border-color: var(--border-strong);
}

.form-control::placeholder {
    color: var(--text-muted);
    opacity: 1;
}

.form-control:focus,
.form-select:focus {
    color: var(--text-primary);
    background-color: var(--surface-1);
    border-color: var(--focus-ring);
}

.form-control:disabled,
.form-select:disabled {
    background-color: var(--surface-3);
    color: var(--text-disabled);
    cursor: not-allowed;
}
```

### 9.4 按鈕

按鈕至少需要：

- Default
- Hover
- Active
- Focus
- Disabled

```css
.btn-app-primary {
    background: var(--accent);
    color: var(--text-on-accent);
    border: 1px solid transparent;
}

.btn-app-primary:hover {
    background: var(--accent-hover);
}

.btn-app-primary:active {
    background: var(--accent-active);
}

.btn-app-primary:focus-visible {
    outline: 3px solid var(--focus-ring);
    outline-offset: 2px;
}
```

### 9.5 次要按鈕

```css
.btn-app-secondary {
    background: var(--surface-2);
    color: var(--text-primary);
    border: 1px solid var(--border-strong);
}

.btn-app-secondary:hover {
    background: var(--surface-3);
}
```

### 9.6 連結

```css
a {
    color: var(--accent);
    text-underline-offset: 0.18em;
}

a:hover {
    color: var(--accent-hover);
}

.prose a {
    text-decoration: underline;
}
```

### 9.7 Focus

禁止直接移除：

```css
outline: none;
```

建議：

```css
:focus-visible {
    outline: 3px solid var(--focus-ring);
    outline-offset: 2px;
}
```

---

## 10. 行事曆專用色彩規範

### 10.1 狀態 Token

```css
:root,
[data-bs-theme="light"] {
    --calendar-available-fill: rgb(24 121 78 / 0.34);
    --calendar-available-line: #18794e;

    --calendar-busy-fill: rgb(180 35 44 / 0.30);
    --calendar-busy-line: #b4232c;

    --calendar-maybe-fill: rgb(138 87 0 / 0.30);
    --calendar-maybe-line: #8a5700;

    --calendar-unset-fill: rgb(102 115 130 / 0.18);
    --calendar-unset-line: #667382;
}

[data-bs-theme="dark"] {
    --calendar-available-fill: rgb(99 214 155 / 0.27);
    --calendar-available-line: #63d69b;

    --calendar-busy-fill: rgb(255 139 147 / 0.25);
    --calendar-busy-line: #ff8b93;

    --calendar-maybe-fill: rgb(242 196 100 / 0.25);
    --calendar-maybe-line: #f2c464;

    --calendar-unset-fill: rgb(158 171 185 / 0.16);
    --calendar-unset-line: #9eabb9;
}
```

### 10.2 日期格文字與填充圖分離

日期格內的文字不建議直接壓在比例會變動的彩色背景上。

建議結構：

```text
┌──────────────┐
│ 8/16   推薦   │
│ 有空 6 / 10  │
├──────────────┤
│ 灰 10%       │
│ 紅 20%       │
│ 黃 10%       │
│ 綠 60%       │
└──────────────┘
```

上方是固定資訊區，下方是比例填充區。

### 10.3 狀態不能只靠顏色

每種狀態至少顯示：

- 狀態名稱
- 人數
- 圖例
- Tooltip 或明細

### 10.4 日期狀態的區分方式

- 今天：外框
- 已選取：較粗的 Accent 外框
- 推薦日期：星號圖示與文字
- 最終日期：勾選圖示與標籤
- 自己忙碌：日期角落顯示小型狀態標籤

---

## 11. 常見失敗模式

### 11.1 只改 Body 背景

症狀：

- 頁面變黑
- 卡片仍為白色
- 文字仍為深灰
- Modal 與 Dropdown 不一致

修正：所有元件必須使用 Token。

### 11.2 使用固定亮色工具類

避免：

```html
<div class="bg-white text-dark border-light">
```

應改為自訂語意類別：

```html
<div class="app-card">
```

### 11.3 次要文字過暗

避免在深色背景直接使用：

```css
color: #6c757d;
```

次要文字仍需符合可讀性。

### 11.4 邊框消失

必須使用：

```css
var(--border-subtle)
var(--border-strong)
```

### 11.5 Hover 不明顯

Hover 必須有可感知的背景或邊框變化。

同時不能只做 Hover，也必須處理鍵盤 Focus。

### 11.6 Placeholder 看不到

Placeholder 必須使用專用 Token，不能過暗，也不能與輸入文字完全相同。

### 11.7 濫用 opacity

避免：

```css
opacity: 0.6;
```

因為會同時影響文字、圖示與子元素。

### 11.8 語意色過度刺眼

深色模式應區分：

- Soft Background
- Clear Foreground
- Border 或 Icon

不要直接套用大面積高飽和色。

### 11.9 SVG 顏色寫死

SVG 優先使用：

```css
fill: currentColor;
stroke: currentColor;
```

不要寫死：

```text
fill="#000000"
```

---

## 12. AI 修改前端的硬性指令

以下內容可直接加入 AI Prompt：

```text
修改前端時，必須遵守以下規則：

1. 使用 Bootstrap 5.3 的 data-bs-theme 控制 light/dark。
2. 所有自訂顏色必須使用 CSS 語意變數，不得在元件中散落 Hex 色碼。
3. 同時定義頁面背景、表面、文字、邊框、主要色、語意色與 Focus Ring。
4. 一般文字與背景對比至少 4.5:1。
5. 大型文字至少 3:1。
6. 輸入框、按鈕、圖示、邊框與 Focus 等重要非文字元素至少 3:1。
7. 不得只用顏色表達狀態；狀態必須同時有文字、圖示或數字。
8. 不得使用 filter: invert() 製作深色模式。
9. 不得只修改 body 背景。
10. 不得在深色模式沿用 bg-white、text-dark、border-light 等固定亮色類別。
11. 不得移除鍵盤 Focus，必須使用清楚的 :focus-visible 樣式。
12. 表單必須處理 Default、Hover、Focus、Disabled、Readonly、Invalid。
13. 按鈕必須處理 Default、Hover、Active、Focus、Disabled。
14. Modal、Dropdown、Navbar、Offcanvas、Table 與 Tooltip 都必須檢查深色模式。
15. 行事曆狀態除了顏色，必須顯示狀態名稱與人數。
16. 文字不得直接覆蓋在會依比例變化的彩色填充背景上，除非有固定資訊底板。
17. 優先修改共用 Token 與元件類別，不得逐頁硬改顏色。
18. 完成後列出 light/dark 兩種模式下已檢查的元件。
19. 不得自行改變既有產品功能與版面流程。
20. 若無法確認對比是否合格，需明確標記待檢查，不可宣稱已符合 WCAG。
```

---

## 13. 建議 CSS 架構

```text
/wwwroot/css
    tokens.css
    base.css
    components.css
    calendar.css
    utilities.css
```

### tokens.css

只放：

- 明亮 Theme Token
- 深色 Theme Token
- 字體
- 間距
- 圓角
- 陰影

### base.css

只放：

- Body
- Heading
- Paragraph
- Link
- Focus
- Selection

### components.css

只放：

- Card
- Button
- Form
- Navbar
- Modal
- Dropdown
- Table
- Badge
- Alert

### calendar.css

只放：

- 月曆網格
- 日期格
- 狀態填充
- 日期標籤
- 推薦與最終日期

---

## 14. 測試清單

### 14.1 Theme

- 首次進入是否跟隨系統
- 手動切換是否正常
- 重新整理後是否記住
- Auto 模式下是否跟著系統切換
- 是否出現白色閃爍

### 14.2 文字

- 標題
- 內文
- 次要文字
- Placeholder
- Disabled
- Link
- Validation Message

### 14.3 元件

- Navbar
- Sidebar
- Card
- Form Control
- Select
- Checkbox
- Radio
- Button
- Table
- Modal
- Dropdown
- Tooltip
- Alert
- Badge
- Pagination

### 14.4 狀態

- Default
- Hover
- Active
- Focus
- Disabled
- Invalid
- Loading
- Empty State

### 14.5 行事曆

- 今天
- 其他月份日期
- 有空
- 忙碌
- 不確定
- 未設定
- 推薦日期
- 最終日期
- 點擊選取
- 手機寬度

---

## 15. 建議驗證工具

至少使用一種：

- Chrome DevTools Accessibility
- Firefox Accessibility Inspector
- WebAIM Contrast Checker
- axe DevTools
- Lighthouse Accessibility

自動化工具不能取代人工檢查。

人工仍需確認：

- 資訊階層是否清楚
- Hover 是否看得出來
- Focus 是否容易追蹤
- 狀態是否只靠顏色
- 日期填充圖上的文字是否容易閱讀
- 深色頁面是否有大片過亮區塊

---

## 16. 參考資料

- W3C WCAG 2.2  
  https://www.w3.org/TR/WCAG22/

- W3C Contrast Minimum  
  https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum

- W3C Non-text Contrast  
  https://www.w3.org/WAI/WCAG22/Understanding/non-text-contrast

- W3C Use of Color  
  https://www.w3.org/WAI/WCAG22/Understanding/use-of-color

- MDN prefers-color-scheme  
  https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/At-rules/@media/prefers-color-scheme

- MDN color-scheme  
  https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Properties/color-scheme

- Bootstrap 5.3 Color Modes  
  https://getbootstrap.com/docs/5.3/customize/color-modes/

- Bootstrap 5.3 CSS Variables  
  https://getbootstrap.com/docs/5.3/customize/css-variables/

- Material Design 3 Color Roles  
  https://m3.material.io/styles/color/roles

- WebAIM Contrast Checker  
  https://webaim.org/resources/contrastchecker/

---

## 17. 最終原則

本專案的深色模式品質，不以「畫面有變黑」判定完成。

必須同時達到：

```text
可閱讀
可辨識
可操作
可維護
語意一致
明暗模式資訊等價
```

AI 應優先建立完整 Token 系統，再修改元件。

不得以逐頁修補色碼的方式完成 Theme。
