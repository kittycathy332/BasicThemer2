<div align="center">

<img src="assets/Bitmap603.ico" alt="" width="30" height="30" style="vertical-align: middle; display: inline-block; margin-right: 12px;" /><b style="font-size: 2.0em; vertical-align: middle;">BasicThemer 2</b>

**A C# / .NET port of the classic Win7 basic theme applied to Windows Vista–11 — without disabling DWM.**

`BasicTheme.ahk` → compiled into a tray-friendly WinForms tool, __keeping DWM composition running__.

</div>

---

## ✨ Features

- Applies the **Windows 7 basic theme** to Windows Vista / 7 / 8 / 8.1 / 10 / 11
- Keeps **DWM composition** enabled while running
- **Tray icon** with a quick context menu (Show / Exit)
- **Exclusion list** — exclude specific processes and external windows from theming
- **Whitelist mode** for inverted behavior
- Configurable **timer speed**, pause, and verbose logging
- **Automatic update check** against the fork's `gitVersion.txt`
- **Dual-language UI** (简体中文 / English) with live switching

---

## 📸 Screenshot

![Screenshot](https://raw.githubusercontent.com/kittycathy332/BasicThemer2/master/assets/%EC%8A%A4%ED%81%AC%EB%A6%B0%EC%83%B7(241).png)

---

## 🚀 Building

Built with **Visual Studio 2019**. Two .NET Framework targets are produced by `build.bat`:

| Target          | Output                            |
|-----------------|-----------------------------------|
| .NET Framework 4.0 | `bin\v4.0\Release\BasicThemer2.exe` |
| .NET Framework 4.8 | `bin\v4.8\Release\BasicThemer2.exe` |

Both versions are **self-contained single-file** executables (localization is embedded — no extra DLLs required).

---

## 📖 References

[[1] Detect active window changed using C# without polling](https://stackoverflow.com/questions/4372055/detect-active-window-changed-using-c-sharp-without-polling)  
[[2] Disabling Aero Glass transparency for WPF window](https://stackoverflow.com/questions/10674540/disabling-aero-glass-transparency-for-wpf-window)  
[[3] Tray icon app - WinForms](https://rightnowdo.tistory.com/entry/C-%EC%9D%91%EC%9A%A9-Tray-icon-%EC%9D%91%EC%9A%A9%ED%94%84%EB%A1%9C%EA%B7%B8%EB%9E%A8-%EB%A7%8C%EB%93%A4%EA%B8%B0)  
[[4] Get inner size and position of an external window in C#](https://stackoverflow.com/questions/38806944/get-inner-size-and-position-of-a-external-window-in-c-sharp)  
[[5] Determine the window titlebar height (WinForms)](https://social.msdn.microsoft.com/Forums/windows/en-US/93999f2e-1ce8-429a-a4bc-4521acd27b18/how-to-determine-the-window-titlebar-height?forum=winforms)  
[[6] Hunit blog](https://hunit.tistory.com/348)  
[[7] Closing a file after File.Create](https://stackoverflow.com/questions/5156254/closing-a-file-after-file-create)  
[[8] Reading data from a website using C#](https://stackoverflow.com/questions/4758283/reading-data-from-a-website-using-c-sharp)  
[[9] Compare version numbers without Split](https://stackoverflow.com/questions/7568147/compare-version-numbers-without-using-split-function)

---

## 📜 Credit(s)

- **Original author:** [Ingan121 / BasicThemer2](https://github.com/Ingan121/BasicThemer2)
- **Original AHK version (slow):** [BasicTheme.ahk](https://github.com/Ingan121/files/blob/master/BasicTheme.ahk)
- **Assisted by:** Trae ([Global](https://www.trae.ai/) / [CN](https://www.trae.com.cn/))