# BasicThemer2 汉化 · 中英文切换 · fork 版权改造计划

## Context（背景与目标）

BasicThemer2 是一个 C# WinForms（net4.0 / net4.8 双目标）的 DWM 基础主题工具。当前 UI 全部为硬编码英文，`Strings.resx` 只有 `AppName` 一个资源。用户希望：

1. **汉化**：把全部用户可见 UI 文本抽到资源文件，支持中文。
2. **中英文即时切换**：界面上方加语言下拉框，选中即整窗刷新，并把语言记忆到注册表（下次启动沿用）。
3. **首次默认语言跟随系统**：系统语言为中文→默认中文，否则英文。
4. **fork 改造**：版本号改为 `1.0.0`；版本行下方新增一行作者水印；GitHub 链接**保留原作者**不变。
5. **语言设置不污染原软件配置**：语言存到**新的注册表键**，不碰原作者 `HKCU\SOFTWARE\Ingan121\BasicThemer2` 键。

现有基础可复用：`BasicThemer2/Strings.resx` + `Strings.Designer.cs`（强类型资源类）已接入 csproj 构建，只需扩充资源与新增 zh-CN 卫星资源。

---

## 方案概述

采用 .NET 标准的 **resx 卫星资源 + Culture 切换**：
- `Strings.resx` = 英文默认值（fallback，语言切换失败时兜底）
- 新增 `Strings.zh-CN.resx` = 中文值
- `Strings.Designer.cs` 扩充为全部 key 的强类型属性
- 运行时把 `Strings.Culture` 设为选中语言的 CultureInfo，再全窗 `L``ApplyLanguage()` 刷新

语言记忆读取/写入新注册表键 `HKCU\SOFTWARE\BasicThemer2\Localization`（与原作者键完全独立）。

---

## 待翻译 text key 清单（两套 resx 都必须包含全部 key）

### 静态控件文本（Designer.cs 初始化 + ApplyLanguage 覆盖）
| Key | 英文(en) | 中文(zh-CN) |
|---|---|---|
| ExclsOrInclsLabel | Exclusions | 排除列表 |
| Show | &Show | 显示(&S) |
| Exit | &Exit | 退出(&E) |
| RevertingMode | &Reverting Mode | 还原模式(&R) |
| ExclExtWnds | Exclude all windows with extended &client area | 排除所有扩展客户区窗口 |
| Pause | &Pause | 暂停(&P) |
| GitHub | &GitHub | GitHub(&G) |
| EnableLogging | Enable &Logging | 启动日志(&L) |
| OpenLogFile | &Open log file | 打开日志文件(&O) |
| Add | &Add | 添加(&A) |
| Delete | &Delete | 删除(&D) |
| TimerSpeed | Timer speed: | 计时器速度: |
| Ms | ms | 毫秒 |
| WhitelistMode | Whitelist Mode | 白名单模式 |
| AutoUpdChk | Automatically | 自动 |
| CheckForUpdates | check for updates | 检查更新 |
| AppName | BasicThemer 2 | BasicThemer 2 |
| IncLabels | Inclusions | 包含列表 |
| Err | Err! | 错误! |
| Language | Language: / 语言 | 语言 |

### 动态文本（MessageBox / dbg / 更新检查 / cmd help / 管理员提示，替换为资源调用）
- `MsgLogFileNotExist` : "Log file doesn't exist!" / "日志文件不存在!"
- `MsgNewVerAvailable` : "New version of BasicThemer 2 is available. Download it now?" / "BasicThemer 2 有可用更新，是否现在下载?"
- `MsgLatestVersion` : "You are running the latest version of BasicThemer 2." / "您已运行 BasicThemer 2 的最新版本。"
- `MsgUnreleasedVersion` : "You are running a unreleased version of BasicThemer 2." / "您正运行 BasicThemer 2 的未发布版本。"
- `MsgUpdateFailed` : "Update check failed!" / "更新检查失败!"
- `DbgInfo`(带拼接参数) : "lastHwnd: {0}, GetForegroundWindow(): {1}, isMainLoopRunning: {2}"
- `CmdHelp`(Program.cs 多行) + 标题 `CmdHelpTitle`
- `CmdVersion` : "Version {0}" / "版本 {0}"
- `AdminPrompt`, `AdminAborted`, `AdminFailedNote`(Program.cs 管理员三级提示)
- `VersionLine` : "BasicThemer 2 v{ver} by Ingan121"（`{ver}` 占位符原逻辑保留）
- `Watermark` : "Fork & 汉化版 by YOUR_NAME"（**占位署名，用户后续替换**）／中文显示中文，英文可英文

> 聚焦用户可见文本。内部 `log(...)` 的调试日志、更新检查的 coarse 标签 `[New window detected!]` 等保留英文不改（开发者观感日志，避免过度改动）。

---

## 具体改动文件

### 1. `BasicThemer2/Strings.resx`
在现有 `AppName` 后追加全部 key（英文值，含上述清单）。

### 2. `BasicThemer2/Strings.zh-CN.resx`（新建）
含与 en 完全一致的 key 集合，值为中文。

### 3. `BasicThemer2/Strings.Designer.cs`
沿用现有自动生成风格，为每个 key 追加 `internal static string Xxx` 强类型属性（保留 `ResourceManager` / `Culture` 结构）。

### 4. `BasicThemer2/BasicThemer2.csproj`
新增：
```xml
<EmbeddedResource Include="Strings.zh-CN.resx">
  <Generator>ResXFileCodeGenerator</Generator>
  <LastGenOutput>Strings.zh-CN.Designer.cs</LastGenOutput>
</EmbeddedResource>
```
（zh-CN 卫星资源无需配套强类型类，resx 即够用。`Strings.Designer.cs` 的 `ResourceManager` 会按 `Strings.Culture` 自动回退查找卫星资源。）

### 5. `BasicThemer2/BasicThemer2.Designer.cs`
- 静态文本改为上一层设计：保留原字面值，但由 `ApplyLanguage()` 在运行时覆盖（避免破坏设计器渲染）。
- 新增顶层控件：`Label LanguageLabel`（"语言"）＋`ComboBox LangCombo`（两项：`English` / `中文`，功能值映射 en / zh-CN）。
- 新增水印 `Label WatermarkLabel`（放在 InfoLabel 下方），初始 `Text = Watermark`。

### 6. `BasicThemer2/BasicThemer2.cs`
- 构造函数在 `InitializeComponent()` 后调用 `ApplyLanguage()`。
- 新增：
  - `private void ApplyLanguage()`：将各控件 `.Text` 置为对应资源值；重拼 `InfoLabel` 的 `{ver}`；处理 `ExclsOrInclsLabel`、`MsOrErrLabel`（"ms"/"Err!"）、`LangCombo` 选中值同步。
  - `private void InitLanguageSetting()`：从新注册表键读 `Language`，未设置则按 `Thread.CurrentThread.CurrentUICulture.Name` 判断系统是否为 zh → 设 `Strings.Culture`。
  - `LangCombo_SelectedIndexChanged`：写新注册表键 + 重设 `Strings.Culture` + 重调 `ApplyLanguage()`（即时切换）。
- 把所有用户可见硬编码字符串替换为 `Strings.Xxx`（含 `updateCheck` 内 MessageBox、`dbgBtn_Click`、`OpenLogBtn_Click` 的 MessageBox 等）。含占位符的用 `string.Format`（如 `DbgInfo`、`CmdVersion`）。
- 新增语言注册表键常量，如 `SOFTWARE\BasicThemer2\Localization`；**不读写** `SOFTWARE\Ingan121\BasicThemer2`。

### 7. `BasicThemer2/Program.cs`
- `Main()` 开头在 `Application.EnableVisualStyles()` 后、`Application.Run` 前调用 `InitLanguageSetting()`（设在主窗体内或静态公共方法）。
- help / 版本 / 管理员提示的字符串改读 `Strings.*`。

### 8. `BasicThemer2/Properties/AssemblyInfo.cs`
- `AssemblyVersion` 与 `AssemblyFileVersion` 改为 `1.0.0`。

---

## 验证方式
1. 运行 `build.bat`，确认 net4.0 与 net4.8 均编译通过、无新增 warning。
2. 运行 exe：系统中文环境首次启动默认中文；`LangCombo` 切换英文→中文即时整窗刷新；版本行显示 `BasicThemer 2 v1.0.0 by Ingan121`，其下方出现水印行。
3. 确认语言写入独立注册表键（`HKCU\SOFTWARE\BasicThemer2\Localization`），未改动 `Ingan121\BasicThemer2`。
4. 缺少某个资源 key 时，resx 机制回退显示英文，不崩溃。