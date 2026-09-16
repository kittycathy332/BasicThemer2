using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BasicThemer2
{
    public partial class BasicThemer2 : Form
    {
        #region Variables

        // Global
        public string logs;
        public string[] args = Environment.GetCommandLineArgs();
        public RegistryKey bt2ConfReg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Ingan121\\BasicThemer2", RegistryKeyPermissionCheck.ReadWriteSubTree);
        public Version ver = new Version(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion);
        public bool isMainLoopRunning = false;
        public IntPtr lastHwnd;
        public bool isDebugBuild = false;

        // Localization (stored in a dedicated registry key, independent of the original software settings)
        private const string LangRegPath = "SOFTWARE\\BasicThemer2\\Localization";
        private const string RunRegPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private bool _isSettingLang = false;

        // Configurations
        public int timerSpeed = 100;
        public bool allowShowDisplay = false;
        public bool dontHide = false;

        // Definitions
        private const int DWMWA_NCRENDERING_POLICY = 2;
        private const int DWMNCRP_DISABLED = 1;
        private const int DWMNCRP_ENABLED = 0;
        private int ClientHeight, WindowHeight, HeightDifference;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        #endregion

        #region Main

        public BasicThemer2()
        {
            InitializeComponent();

            // Initialize language and apply localized Texts
            InitLanguageSetting();
            ApplyLanguage();

            // Load configurations from registry
            if (bt2ConfReg.GetValueNames().Contains("Exclusions"))
            {
                string[] excls = bt2ConfReg.GetValue("Exclusions").ToString().Split(new char[] { '|' });
                ExclListBox.Items.Clear();
                for (int i = 0; i < excls.Length; i++)
                {
                    if (!string.IsNullOrEmpty(excls[i]))
                    {
                        ExclListBox.Items.Add(excls[i]);
                    }
                }
            }
            else
            {
                saveExclList();
            }

            if (bt2ConfReg.GetValueNames().Contains("ExclExtWnds"))
            {
                if (bt2ConfReg.GetValue("ExclExtWnds").ToString() == "0")
                {
                    ExclExtWndsChkBox.Checked = false;
                }
            } else
            {
                bt2ConfReg.SetValue("ExclExtWnds", 1, RegistryValueKind.DWord);
            }

            if (bt2ConfReg.GetValueNames().Contains("TimerSpeed"))
            {
                timerSpeed = (int)bt2ConfReg.GetValue("TimerSpeed");
                TimerSpeedBox.Text = timerSpeed.ToString();
            }
            else
            {
                bt2ConfReg.SetValue("TimerSpeed", 100, RegistryValueKind.DWord);
            }

            if (bt2ConfReg.GetValueNames().Contains("WhitelistMode"))
            {
                if (bt2ConfReg.GetValue("WhitelistMode").ToString() == "1")
                {
                    WhitelistModeChkBox.Checked = true;
                }
            }
            else
            {
                bt2ConfReg.SetValue("WhitelistMode", 0, RegistryValueKind.DWord);
            }

            if (bt2ConfReg.GetValueNames().Contains("AutoUpdChk"))
            {
                if (bt2ConfReg.GetValue("AutoUpdChk").ToString() == "0")
                {
                    AutoUpdChkChkBox.Checked = false;
                }
            }
            else
            {
                bt2ConfReg.SetValue("AutoUpdChk", 1, RegistryValueKind.DWord);
            }

            // Reflect whether the app is registered to start with Windows
            AutoStartChkBox.Checked = IsStartupEnabled("BasicThemer2");

            // Start main window detection loop
            StartMainLoop();

            // Process command-line arguments
            if (args.Any(x => x.Contains("hidetray")))
            {
                notifyIcon1.Visible = false;
            }

            if (args.Any(x => x.Contains("enablelogging")))
            {
                DoLogChkBox.Checked = true;
            }

            if (args.Any(x => x.Contains("noautoupdchk")))
            {
                AutoUpdChkChkBox.Checked = false;
            }

            // Check for updates automatically if configured so
            if (AutoUpdChkChkBox.Checked)
            {
                updateCheck();
            }

            // Log init complete and configurations
            log("[Application initialization complete (Version " + ver + (IsAdministrator() ? ")]" : ", Not admin)]"), true);
            log("[Current exclusions: " + getExclListAsString() + "]", true);
            log("[TimerSpeed: " + timerSpeed.ToString() + "]", true);
            log("[ExclExtWnds:" + ExclExtWndsChkBox.Checked.ToString() + "]", true);
            log("[Whitelist mode: " + WhitelistModeChkBox.Checked.ToString() + "]", true);
            log("[Automatic update check: " + AutoUpdChkChkBox.Checked.ToString() + "]", true);
            log("[System caption height: " + SystemInformation.CaptionHeight + "]", true);
        }

        #endregion

        #region Functions

        /// <summary>
        /// Reads the language preference from the dedicated registry key; falls back to the
        /// system UI language (Chinese) on first run. Never touches Ingan121\BasicThemer2.
        /// </summary>
        public static void InitLanguageSetting()
        {
            string saved = null;
            try
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\BasicThemer2\\Localization");
                saved = key.GetValue("Language") as string;
                key.Close();
            }
            catch { }

            if (saved == "zh" || saved == "en")
            {
                Strings.Culture = new System.Globalization.CultureInfo(saved == "zh" ? "zh-CN" : "en");
            }
            else
            {
                bool isChinese = Thread.CurrentThread.CurrentUICulture.Name.StartsWith("zh");
                Strings.Culture = new System.Globalization.CultureInfo(isChinese ? "zh-CN" : "en");
            }
        }

        /// <summary>
        /// (Re-)applies all localized UI texts for the active culture.
        /// </summary>
        private void ApplyLanguage()
        {
            ExclsOrInclsLabel.Text = WhitelistModeChkBox.Checked ? Strings.IncLabels : Strings.ExclsOrInclsLabel;
            notifyIcon1.Text = Strings.AppName;
            showToolStripMenuItem.Text = Strings.Show;
            exitToolStripMenuItem.Text = Strings.Exit;
            ExitWndBtn.Text = Strings.Exit;
            BrandLabel.Text = Strings.BrandLine;
            InfoLabel.Text = "v" + ver.ToString();
            RevModeChkBox.Text = Strings.RevertingMode;
            ExclExtWndsChkBox.Text = Strings.ExclExtWnds;
            PauseChkBox.Text = Strings.Pause;
            linkLabel1.Text = Strings.GitHub;
            DoLogChkBox.Text = Strings.EnableLogging;
            OpenLogBtn.Text = Strings.OpenLogFile;
            AddBtn.Text = Strings.Add;
            DelBtn.Text = Strings.Delete;
            label1.Text = Strings.TimerSpeed;
            MsOrErrLabel.Text = Strings.Ms;
            WhitelistModeChkBox.Text = Strings.WhitelistMode;
            AutoUpdChkChkBox.Text = Strings.AutoUpdChk;
            UpdChkBtn.Text = Strings.CheckForUpdates;
            LanguageLabel.Text = Strings.Language;
            WatermarkLabel.Text = Strings.Watermark;
            dbgBtn.Text = Strings.DebugBtn;
            ForkLinkLabel.Text = Strings.ForkLink;
            AutoStartChkBox.Text = Strings.StartWithWindows;
            this.Text = Strings.AppName;

            // Reflow controls whose positions depend on localized text length so
            // no label/checkbox overlaps its following control in any language
            LayoutForLanguage();

            // Sync the language combo (guard against re-entrant save triggers)
            bool oldGuard = _isSettingLang;
            _isSettingLang = true;
            bool isChinese = Strings.Culture != null && Strings.Culture.Name.StartsWith("zh");
            LangCombo.SelectedIndex = isChinese ? 1 : 0;
            _isSettingLang = oldGuard;
        }

        /// <summary>
        /// Reflows controls whose positions depend on localized text length so that no
        /// label/checkbox overlaps the control that follows it in any language.
        /// </summary>
        private void LayoutForLanguage()
        {
            // Push the language combo right after its label
            LangCombo.Left = LanguageLabel.Right + 4;

            // Move the "check for updates" button after the auto-update checkbox and
            // size it to fit its own text, while keeping it inside the form bounds
            int btnX = AutoUpdChkChkBox.Right + 4;
            int btnWidth = TextRenderer.MeasureText(UpdChkBtn.Text, UpdChkBtn.Font).Width + 16;
            int maxWidth = this.ClientSize.Width - btnX - 8;
            if (btnWidth > maxWidth) btnWidth = maxWidth;
            if (btnWidth < 60) btnWidth = 60;
            UpdChkBtn.Left = btnX;
            UpdChkBtn.Width = btnWidth;

            // Stretch the upper UI to fill the window width: the exclusion box and the
            // whitelist checkbox follow the current window width
            ExclListBox.Width = this.ClientSize.Width - 24;
            WhitelistModeChkBox.Left = this.ClientSize.Width - WhitelistModeChkBox.Width - 10;

            // Row: [文本框] ... [Add] [Delete]  两个按钮贴右，文本框拉长
            // 先按当前本地化文本测量按钮宽度，保证不会文字被截断
            int addBtnW = TextRenderer.MeasureText(AddBtn.Text, AddBtn.Font).Width + 20;
            int delBtnW = TextRenderer.MeasureText(DelBtn.Text, DelBtn.Font).Width + 20;
            if (addBtnW < 48) addBtnW = 48;
            if (delBtnW < 60) delBtnW = 60;
            AddBtn.Width = addBtnW;
            DelBtn.Width = delBtnW;

            DelBtn.Left = this.ClientSize.Width - DelBtn.Width - 12;
            AddBtn.Left = DelBtn.Left - AddBtn.Width - 4;
            ExclAddNameBox.Width = AddBtn.Left - ExclAddNameBox.Left - 4;

            // Reflow the Timer speed row: label grows with its localized text, then the
            // numeric box and "ms" suffix follow it from left to right
            TimerSpeedBox.Left = label1.Right + 4;
            MsOrErrLabel.Left = TimerSpeedBox.Right + 4;

            // Place each bottom hyperlink right after its label text, shifting automatically
            // with the localized text length
            linkLabel1.Left = BrandLabel.Right + 4;
            ForkLinkLabel.Left = WatermarkLabel.Right + 4;

            // Right-align the debug button on the language row so it never overlaps the combo
            dbgBtn.Left = this.ClientSize.Width - dbgBtn.Width - 10;
            dbgBtn.Top = LangCombo.Top;
        }

        /// <summary>
        /// Switches language live when the user changes the combo; persists to the dedicated key.
        /// </summary>
        private void LangCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isSettingLang) return;

            string lang = LangCombo.SelectedIndex == 1 ? "zh" : "en";
            Strings.Culture = new System.Globalization.CultureInfo(lang == "zh" ? "zh-CN" : "en");

            try
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\BasicThemer2\\Localization");
                key.SetValue("Language", lang, RegistryValueKind.String);
                key.Close();
            }
            catch { }

            ApplyLanguage();
        }

        private void StartMainLoop()
        {
            if (!isMainLoopRunning)
            {
                isMainLoopRunning = true;
                Task.Factory.StartNew(() =>
                {
                    for (; ; )
                    {
                        if (!PauseChkBox.Checked)
                        {
                            if (lastHwnd != GetForegroundWindow() && GetForegroundWindow() != IntPtr.Zero)
                            {
                                log("[New window detected!] lastHwnd: " + lastHwnd.ToString() + ", GetForegroundWindow(): " + GetForegroundWindow().ToString());
                                try
                                {
                                // Get the client and full window sizes and compare them to check if the window is extended
                                if (!GetClientRect(GetForegroundWindow(), out RECT rct))
                                    {
                                        log("[ERROR in GetClientRect]");
                                        return;
                                    }
                                    ClientHeight = rct.Bottom - rct.Top + 1;

                                    if (!GetWindowRect(GetForegroundWindow(), out rct))
                                    {
                                        log("[ERROR in GetWindowRect]");
                                        return;
                                    }
                                    WindowHeight = rct.Bottom - rct.Top + 1;

                                    HeightDifference = WindowHeight - ClientHeight;
                                    bool Extended = WindowHeight - ClientHeight <= SystemInformation.CaptionHeight;

                                // Apply the basic theme to the window if it is not extended or if the "Exclude all windows with..." checkbox is not checked
                                if (!Extended | !ExclExtWndsChkBox.Checked) RemoveDwmFrameByHwnd(GetForegroundWindow(), RevModeChkBox.Checked);

                                    log(ReturnEmptyIfSo(GetWindowTitleOfHwnd(GetForegroundWindow())) + " / " + ReturnEmptyIfSo(GetProcessNameOfHwnd(GetForegroundWindow())) + " / " + GetForegroundWindow().ToString() + " (" + ClientHeight + ", " + WindowHeight + ", " + HeightDifference + ", " + (Extended ? "Extended" : "Not extended") + ")");
                                }
                                catch (Exception ex)
                                {
                                    log(ex.ToString(), true);
                                }
                            }
                            lastHwnd = GetForegroundWindow();
                            Thread.Sleep(timerSpeed);
                        }
                        else
                        {
                            isMainLoopRunning = false;
                            log("[Stopping main loop...]", true);
                            break;
                        }
                    }
                });
                log("[Started main loop]", true);
            }
        }
            
        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            allowShowDisplay = true;
            Visible = true;
            BringToFront();
            WindowState = FormWindowState.Normal;
            log("[UI Visibility: " + (Visible ? "Visible" : "Invisible") + "]");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) => Exit();

        private void ExitWndBtn_Click(object sender, EventArgs e) => Exit();

        private void RevModeChkBox_CheckedChanged(object sender, EventArgs e)
        {
            // Apply the setting immediately when not paused
            if (!PauseChkBox.Checked)
            {
                RemoveDwmFrameByHwnd(GetForegroundWindow(), RevModeChkBox.Checked);
            }
            log("[Reverting Mode: " + RevModeChkBox.Checked.ToString() + "]");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!dontHide)
            {
                e.Cancel = true;
                Visible = false;
                log("[UI Visibility: " + (Visible ? "Visible" : "Invisible") + "]");
            }
        }

        private void Form1_Load(object sender, EventArgs e) => Visible = false;

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Process.Start("https://github.com/Ingan121/BasicThemer2");

        private void OpenLogBtn_Click(object sender, EventArgs e)
        {
            DoLogChkBox.Checked = false;
            try
            {
                Process.Start("BasicThemer2.log");
            } catch
            {
                new Thread(() =>
                {
                    MessageBox.Show(Strings.MsgLogFileNotExist, Strings.AppName);
                }).Start();
            }
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            string exename = ExclAddNameBox.Text;
            ExclListBox.Items.Add(exename.EndsWith(".exe") ? exename : (exename.EndsWith("/noexe") ? exename.Remove(exename.Length - 6) : exename + ".exe"));
            log("New process was added to the exclusion / inclusion list: " + exename);
            saveExclList();
        }

        private void DelBtn_Click(object sender, EventArgs e)
        {
            ListBox.SelectedObjectCollection selectedItems = new ListBox.SelectedObjectCollection(ExclListBox);
            selectedItems = ExclListBox.SelectedItems;

            if (ExclListBox.SelectedIndex != -1)
            {
                for (int i = selectedItems.Count - 1; i >= 0; i--)
                {
                    ExclListBox.Items.Remove(selectedItems[i]);
                }
            }
            saveExclList();
        }

        private void PauseChkBox_CheckedChanged(object sender, EventArgs e)
        {
            RemoveDwmFrameByHwnd(GetForegroundWindow(), RevModeChkBox.Checked);
            if (!PauseChkBox.Checked)
            {
                StartMainLoop();
            }
            log("[Pause: " + PauseChkBox.Checked.ToString() + "]", true);
        }

        private void DoLogChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (DoLogChkBox.Checked)
            {
                log("[Logging started]");
            }
            else
            {
                logs += string.Format("\n{0} : [Logging stopped]", DateTime.Now);
                FileInfo LogFileInfo = new FileInfo("BasicThemer2.log");
                if (!LogFileInfo.Exists || LogFileInfo.Length == 0)
                {
                    logs = logs.Substring(1);
                }
                File.AppendAllText("BasicThemer2.log", logs);
                logs = "";
            }
        }

        protected override void SetVisibleCore(bool value)
        {
            if (args.Any(x => x.Contains("showui")))
            {
                allowShowDisplay = true;
            }
            if (args.Any(x => x.Contains("dontHide")))
            {
                dontHide = true;
            }

            base.SetVisibleCore(allowShowDisplay ? value : allowShowDisplay);
        }

        private void NotifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                allowShowDisplay = true;
                Visible = dontHide || !Visible;
                log("[UI Visibility: " + (Visible ? "Visible" : "Invisible") + "]", true);
            }
        }

        public void Exit()
        {
            Close();
            Dispose();
            Properties.Settings.Default.Save();
            log("[Application exiting...]", true);
            DoLogChkBox.Checked = false;
            Application.Exit();
        }

        public void RemoveDwmFrameByHwnd(IntPtr hwnd, Boolean revert)
        {
            var policyParameter = DWMNCRP_DISABLED;

            bool condition = ExclListBox.FindString(GetProcessNameOfHwnd(hwnd)) == ListBox.NoMatches;
            if (WhitelistModeChkBox.Checked)
            {
                condition = !condition;
            }

            if (condition)
            {
                if (revert)
                {
                    policyParameter = DWMNCRP_ENABLED;
                }

                DwmSetWindowAttribute(hwnd, DWMWA_NCRENDERING_POLICY, ref policyParameter, sizeof(int));
            }
        }

        private string GetProcessNameOfHwnd(IntPtr hwnd)
        {
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            return Process.GetProcessById((int)pid).ProcessName;
        }

        private string GetMainWndNameOfProcOfHwnd(IntPtr hwnd)
        {
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            return Process.GetProcessById((int)pid).MainWindowTitle;
        }

        private string GetWindowTitleOfHwnd(IntPtr hwnd)
        {
            const int nChars = 256;
            IntPtr handle = IntPtr.Zero;
            StringBuilder Buff = new StringBuilder(nChars);
            handle = hwnd;

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public string ReturnEmptyIfSo(String str) => string.IsNullOrEmpty(str) ? "{{Empty}}" : str;

        private void dbgBtn_Click(object sender, EventArgs e) // Small debug button located at bottom right
        {
            MessageBox.Show(string.Format(Strings.DbgInfo, lastHwnd.ToString(), GetForegroundWindow().ToString(), isMainLoopRunning.ToString()), Strings.DbgInfoTitle);
        }

        private void ForkLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/kittycathy332/BasicThemer2");
        }

        private void TimerSpeedBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int timerSpeedInput = int.Parse(TimerSpeedBox.Text);

                if (timerSpeedInput < 0)
                {
                    throw new Exception();
                }

                timerSpeed = timerSpeedInput;
                bt2ConfReg.SetValue("TimerSpeed", timerSpeed, RegistryValueKind.DWord);
                log("[TimerSpeed: " + timerSpeed.ToString() + "]", true);
                MsOrErrLabel.Text = Strings.Ms;
            }
            catch
            {
                MsOrErrLabel.Text = Strings.Err;
            }
        }

        public void log(string str, bool alwaysLog = false)
        {
            if (DoLogChkBox.Checked || alwaysLog)
            {
                logs += string.Format("\n{0} : " + str, DateTime.Now);
            }
        }

        private void ExclExtWndsChkBox_CheckedChanged(object sender, EventArgs e)
        {
            log("[ExclExtWnds:" + ExclExtWndsChkBox.Checked.ToString() + "]", true);
            bt2ConfReg.SetValue("ExclExtWnds", ExclExtWndsChkBox.Checked, RegistryValueKind.DWord);
        }

        private void WhitelistModeChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (WhitelistModeChkBox.Checked)
            {
                ExclsOrInclsLabel.Text = Strings.IncLabels;
            }
            else
            {
                ExclsOrInclsLabel.Text = Strings.ExclsOrInclsLabel;
            }
            
            log("[Whitelist mode: " + WhitelistModeChkBox.Checked.ToString() + "]", true);
            bt2ConfReg.SetValue("WhitelistMode", WhitelistModeChkBox.Checked, RegistryValueKind.DWord);
        }

        private void AutoUpdChkChkBox_CheckedChanged(object sender, EventArgs e)
        {
            log("[Automatic update check: " + AutoUpdChkChkBox.Checked.ToString() + "]", true);
            bt2ConfReg.SetValue("AutoUpdChk", AutoUpdChkChkBox.Checked, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Toggles the "start with Windows" registration in HKCU Run.
        /// </summary>
        private void AutoStartChkBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                using (RegistryKey runKey = Registry.CurrentUser.OpenSubKey(RunRegPath, true))
                {
                    if (AutoStartChkBox.Checked)
                    {
                        runKey.SetValue("BasicThemer2", "\"" + Application.ExecutablePath + "\"");
                    }
                    else
                    {
                        runKey.DeleteValue("BasicThemer2", false);
                    }
                }
            }
            catch (Exception ex)
            {
                log("[Auto start error: " + ex.Message + "]", true);
            }
            log("[Auto start: " + AutoStartChkBox.Checked.ToString() + "]", true);
        }

        private bool IsStartupEnabled(string valueName)
        {
            try
            {
                using (RegistryKey runKey = Registry.CurrentUser.OpenSubKey(RunRegPath, false))
                {
                    if (runKey == null) return false;
                    return runKey.GetValue(valueName) != null;
                }
            }
            catch
            {
                return false;
            }
        }

        private string getExclListAsString()
        {
            string value = "";
            for (int i = 0; i < ExclListBox.Items.Count; i++)
            {
                value = value + ExclListBox.Items[i].ToString() + "|"; //use | for separator as it cannot be used for filenames
            }
            return value;
        }

        private void UpdChkBtn_Click(object sender, EventArgs e)
        {
            UpdChkBtn.Enabled = false;
            UpdChkBtn.Text = Strings.CheckingForUpdates;
            LayoutForLanguage();

            updateCheck(true, () =>
            {
                // completion runs on the worker thread; restore the button on the UI thread
                if (!IsHandleCreated) return;
                BeginInvoke((MethodInvoker)delegate
                {
                    if (IsDisposed) return;
                    UpdChkBtn.Text = Strings.CheckForUpdates;
                    UpdChkBtn.Enabled = true;
                    LayoutForLanguage();
                });
            });
        }

        private void saveExclList()
        {
            bt2ConfReg.SetValue("Exclusions", getExclListAsString(), RegistryValueKind.String);
        }

        private void updateCheck(bool alertLatest = false, Action onComplete = null)
        {
            Task.Factory.StartNew(() =>
            {
                log("[Checking for updates...]", true);
                try
                {
                    // raw.githubusercontent.com requires TLS 1.2, which is not enabled by
                    // default on .NET 4.0. Force it on (moniker 3072 = Tls12) before connecting.
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)((int)ServicePointManager.SecurityProtocol | 3072);

                    Version version = null;
                    var req = (HttpWebRequest)WebRequest.Create("https://raw.githubusercontent.com/kittycathy332/BasicThemer2/master/latest.txt");
                    // GitHub rejects requests without a User-Agent; give it an explicit short
                    // timeout so the check always returns promptly instead of hanging
                    req.UserAgent = "BasicThemer2/" + ver.ToString();
                    req.Timeout = 5000;
                    req.ReadWriteTimeout = 5000;
                    using (var resp = (HttpWebResponse)req.GetResponse())
                    using (var sr = new StreamReader(resp.GetResponseStream()))
                    {
                        string latestVerStr = sr.ReadToEnd().Trim(); // the version file ends with a newline
                        Version parsed;
                        if (Version.TryParse(latestVerStr, out parsed))
                        {
                            version = parsed;
                        }
                    }
                    if (version == null)
                    {
                        throw new InvalidDataException("Unrecognized version string");
                    }
                    log("[Lastest version found: " + version + "]", true);

                    int compare = ver.CompareTo(version);

                    if (compare < 0)
                    {
                        // A newer version is available -> use the exclamation sound
                        System.Media.SystemSounds.Exclamation.Play();
                        if (MessageBox.Show(Strings.MsgNewVerAvailable, Strings.AppName, MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            Process.Start("https://github.com/kittycathy332/BasicThemer2/releases");
                        }
                    }
                    else
                    {
                        if (alertLatest)
                        {
                            // Up to date (or local build newer) -> use the asterisk sound
                            System.Media.SystemSounds.Asterisk.Play();
                            if (compare == 0)
                            {
                                MessageBox.Show(Strings.MsgLatestVersion, Strings.AppName);
                            }
                            else
                            {
                                MessageBox.Show(Strings.MsgUnreleasedVersion, Strings.AppName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log(ex.ToString(), true);
                    // On an automatic check only log the failure; only the manual
                    // "check for updates" button should alert the user about it
                    if (alertLatest)
                    {
                        System.Media.SystemSounds.Exclamation.Play();
                        MessageBox.Show(Strings.MsgUpdateFailed, Strings.AppName);
                    }
                }
                finally
                {
                    if (onComplete != null) onComplete();
                }
            });
        }

        #endregion

        #region DLL Imports

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("DwmApi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

        #endregion
    }
}
