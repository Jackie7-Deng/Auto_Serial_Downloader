using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace Auto_Serial_Downloader
{
    public partial class Form1 : Form
    {
        private bool _connected = false;
        private ushort _connectedVid;
        private ushort _connectedPid;
        private byte _connectedDeviceNumber;
        private bool _busy = false;                 // 正在下载/执行外部工具
        private DateTime _ignoreDisconnectUntil = DateTime.MinValue; // 在这个时间之前，timer 不触发 disconnect
        private int _missingCount = 0;              // 连续检测不到的次数

        public Form1()
        {
            InitializeComponent();

            txtVid.Text = Properties.Settings.Default.LastVid;
            txtPid.Text = Properties.Settings.Default.LastPid;
            txtSn.Text = string.IsNullOrWhiteSpace(Properties.Settings.Default.LastSn) ? "1" : Properties.Settings.Default.LastSn;
            txtCyusbPath.Text = Properties.Settings.Default.LastCyusbPath;

            openFileDialog1.Filter = "CYUSB config (*.cyusb)|*.cyusb|All files (*.*)|*.*";
            openFileDialog1.Title = "Select .cyusb file";

            SetUiConnected(false);
            timer1.Interval = 1000;
            timer1.Tick += timer1_Tick;
            timer1.Start();
        }
        private void SetBusy(bool busy)
        {
            // busy 时：禁用所有输入和按钮（但你希望保持 disconnect 状态的逻辑）
            btnDownload.Enabled = !busy && _connected;
            btnConnect.Enabled = !busy; // busy时不允许点disconnect，避免中途状态混乱（推荐）
            btnBrowse.Enabled = !busy; // 如果你有Browse按钮
            txtSn.Enabled = !busy;
            // VID/PID 是否可用取决于 _connected
            txtVid.Enabled = !busy && !_connected;
            txtPid.Enabled = !busy && !_connected;

        }

        private void SetUiConnected(bool connected)
        {
            txtVid.Enabled = !connected;
            txtPid.Enabled = !connected;

            // txtSn.Enabled = !connected;
            // btnBrowse.Enabled = !connected;

            btnConnect.Enabled = true;
            btnConnect.Text = connected ? "Disconnect" : "Connect";
            btnDownload.Enabled = connected;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(txtCyusbPath.Text) && File.Exists(txtCyusbPath.Text))
                    openFileDialog1.InitialDirectory = Path.GetDirectoryName(txtCyusbPath.Text);
            }
            catch { }

            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                txtCyusbPath.Text = openFileDialog1.FileName;
                SaveSettings();
            }
        }
        private string BuildFailMessage(ProcessResult r)
        {
            string detail = !string.IsNullOrWhiteSpace(r.StdErr) ? r.StdErr : r.StdOut;
            detail = TrimForDialog(detail, 1200);

            string msg = "Programming failed.";
            if (!string.IsNullOrWhiteSpace(detail))
                msg += "\n\n" + detail;
            else
                msg += $"\n\nExit code: {r.ExitCode}";

            return msg;
        }

        private async Task<ProcessResult> RunProcessCaptureAsync(string exePath, string args, string workingDir, int timeoutMs)
        {
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = args,
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var p = new Process { StartInfo = psi, EnableRaisingEvents = true };

            var so = new StringBuilder();
            var se = new StringBuilder();

            p.OutputDataReceived += (s, e) => { if (e.Data != null) so.AppendLine(e.Data); };
            p.ErrorDataReceived += (s, e) => { if (e.Data != null) se.AppendLine(e.Data); };

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            var r = new ProcessResult();

            using var cts = new CancellationTokenSource(timeoutMs);
            try
            {
                await p.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                r.TimedOut = true;
                try { p.Kill(entireProcessTree: true); } catch { }
                return r;
            }

            r.ExitCode = p.ExitCode;
            r.StdOut = so.ToString();
            r.StdErr = se.ToString();
            return r;
        }
        private async void btnDownload_Click(object sender, EventArgs e)
        {
            if (!_connected)
            {
                ShowError("Not connected.");
                return;
            }
            string vidNorm = "0x" + _connectedVid.ToString("X4");
            string pidNorm = "0x" + _connectedPid.ToString("X4");

            string cyusbPath = (txtCyusbPath.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(cyusbPath) || !File.Exists(cyusbPath))
            {
                ShowError("CYUSB file not found.");
                return;
            }

            if (!int.TryParse((txtSn.Text ?? "").Trim(), out int sn) || sn <= 0)
            {
                ShowError("Invalid SN.");
                return;
            }

            SaveSettings();

            string appDir = AppDomain.CurrentDomain.BaseDirectory;

            string fwExe = Path.Combine(appDir, "fwDownload.exe");
            if (!File.Exists(fwExe))
            {
                ShowError("fwDownload.exe not found.");
                return;
            }
            string args = $"-c \"{cyusbPath}\" -vid {vidNorm} -pid {pidNorm} -sn {sn}";

            _busy = true;
            _missingCount = 0;
            SetBusy(true);

            using var pf = new ProgressForm();
            pf.Owner = this;
            pf.Bounds = this.Bounds;
            pf.Show();
            pf.BringToFront();


            try
            {
                var result = await RunProcessCaptureAsync(fwExe, args, appDir, timeoutMs: 120000);

                pf.Close();

                if (result.TimedOut) { ShowError("Programming timeout."); return; }

                bool success = result.ExitCode == 0 ||
                               result.StdOut.IndexOf("Programming Completed successfully", StringComparison.OrdinalIgnoreCase) >= 0;

                if (success)
                {

                    _ignoreDisconnectUntil = DateTime.Now.AddSeconds(8);
                    _missingCount = 0;
                    MessageBox.Show(this, "Programming completed successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ShowError(BuildFailMessage(result));

            }
            finally
            {
                try { if (!pf.IsDisposed) pf.Close(); } catch { }
                _busy = false;
                SetBusy(false);
            }
        }

        private bool CheckDeviceCanOpen(ushort vid, ushort pid, out string reason, out byte devNum)
        {
            reason = "Device not found.";

            string appDir = AppDomain.CurrentDomain.BaseDirectory;

            if (!File.Exists(Path.Combine(appDir, "cyusbserial.dll")))
            {
                reason = "cyusbserial.dll not found.";
                devNum = 0;
                return false;
            }

            try
            {
                var st = CyUsbSerialNative.CyGetListofDevices(out byte total);
                if (st != CyUsbSerialNative.CY_RETURN_STATUS.CY_SUCCESS || total == 0)
                {
                    reason = "Device not found (driver not bound).";
                    devNum = 0;
                    return false;
                }

                var vp = new CyUsbSerialNative.CY_VID_PID { vid = vid, pid = pid };

                const int MaxMatch = 16;
                byte[] idList = new byte[MaxMatch];
                var infoList = new CyUsbSerialNative.CY_DEVICE_INFO[MaxMatch];
                byte infoListLength = (byte)infoList.Length;

                st = CyUsbSerialNative.CyGetDeviceInfoVidPid(vp, idList, infoList, out byte count, infoListLength);
                if (st != CyUsbSerialNative.CY_RETURN_STATUS.CY_SUCCESS || count == 0)
                {
                    reason = "Device not found (VID/PID mismatch or driver not bound).";
                    devNum = 0;
                    return false;
                }

                devNum = idList[0];
                st = CyUsbSerialNative.CyOpen(devNum, interfaceNum: 0, out IntPtr h);
                if (st != CyUsbSerialNative.CY_RETURN_STATUS.CY_SUCCESS || h == IntPtr.Zero)
                {
                    reason = "Device open failed.";
                    devNum = 0;
                    return false;
                }

                CyUsbSerialNative.CyClose(h);
                return true;
            }
            catch (BadImageFormatException)
            {
                reason = "cyusbserial.dll architecture mismatch (x64 required).";
                devNum = 0;
                return false;
            }
            catch (DllNotFoundException ex)
            {
                reason = "DLL load failed.\n\n" + ex.Message;
                devNum = 0;
                return false;
            }
            catch (EntryPointNotFoundException ex)
            {
                reason = "DLL entry point not found.\n\n" + ex.Message;
                devNum = 0;
                return false;
            }
            catch (Exception ex)
            {
                reason = "Device check failed.\n\n" + ex.Message;
                devNum = 0;
                return false;
            }
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.LastVid = (txtVid.Text ?? "").Trim();
            Properties.Settings.Default.LastPid = (txtPid.Text ?? "").Trim();
            Properties.Settings.Default.LastSn = (txtSn.Text ?? "").Trim();
            Properties.Settings.Default.LastCyusbPath = (txtCyusbPath.Text ?? "").Trim();
            Properties.Settings.Default.Save();
        }

        private static bool TryParseHex16(string input, out ushort value, out string normalized)
        {
            value = 0;
            normalized = null;
            if (string.IsNullOrWhiteSpace(input)) return false;

            string s = input.Trim();
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) s = s.Substring(2);
            if (s.Length < 1 || s.Length > 4) return false;

            if (!ushort.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value))
                return false;

            normalized = "0x" + value.ToString("X4");
            return true;
        }

        private static void ShowError(string msg)
        {
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static string TrimForDialog(string text, int maxChars)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            text = text.Trim();
            if (text.Length <= maxChars) return text;
            return text.Substring(text.Length - maxChars, maxChars);
        }

        private static ProcessResult RunProcessCapture(string exePath, string args, string workingDir, int timeoutMs)
        {
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = args,
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var p = new Process { StartInfo = psi };

            var so = new StringBuilder();
            var se = new StringBuilder();

            p.OutputDataReceived += (s, e) => { if (e.Data != null) so.AppendLine(e.Data); };
            p.ErrorDataReceived += (s, e) => { if (e.Data != null) se.AppendLine(e.Data); };

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            var r = new ProcessResult();

            if (!p.WaitForExit(timeoutMs))
            {
                r.TimedOut = true;
                try { p.Kill(entireProcessTree: true); } catch { }
                return r;
            }

            r.ExitCode = p.ExitCode;
            r.StdOut = so.ToString();
            r.StdErr = se.ToString();
            return r;
        }

        private class ProcessResult
        {
            public int ExitCode;
            public bool TimedOut;
            public string StdOut = "";
            public string StdErr = "";
        }

        private void txtVid_TextChanged(object sender, EventArgs e)
        {

        }

        private void Disconnect(string? message = null)
        {
            _connected = false;
            _connectedVid = 0;
            _connectedPid = 0;
            _connectedDeviceNumber = 0;

            SetUiConnected(false);

            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(this, message, "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (_connected)
            {
                Disconnect("Disconnected.");
                return;
            }

            if (!TryParseHex16(txtVid.Text, out ushort vid, out _)) { ShowError("Invalid VID."); return; }
            if (!TryParseHex16(txtPid.Text, out ushort pid, out _)) { ShowError("Invalid PID."); return; }

            if (!CheckDeviceCanOpen(vid, pid, out string reason, out byte devNum))
            {
                _connected = false;
                SetUiConnected(false);
                ShowError(reason);
                return;
            }

            _connected = true;
            _connectedVid = vid;
            _connectedPid = pid;
            _connectedDeviceNumber = devNum;

            SetUiConnected(true);
            MessageBox.Show(this, "Connected.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private bool IsDevicePresent(ushort vid, ushort pid)
        {
            var st = CyUsbSerialNative.CyGetListofDevices(out byte total);
            if (st != CyUsbSerialNative.CY_RETURN_STATUS.CY_SUCCESS || total == 0)
                return false;

            var vp = new CyUsbSerialNative.CY_VID_PID { vid = vid, pid = pid };

            byte[] idList = new byte[16];
            var infoList = new CyUsbSerialNative.CY_DEVICE_INFO[16];
            byte infoLen = (byte)infoList.Length;

            st = CyUsbSerialNative.CyGetDeviceInfoVidPid(vp, idList, infoList, out byte count, infoLen);
            return st == CyUsbSerialNative.CY_RETURN_STATUS.CY_SUCCESS && count > 0;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_busy) return;                         // 下载进行中不检查
            if (!_connected) return;
            if (DateTime.Now < _ignoreDisconnectUntil) return;

            bool present = IsDevicePresent(_connectedVid, _connectedPid);

            if (!present)
            {
                _missingCount++;
                // 例如：1 秒一次，连续 3 次=约 3 秒都没找到，才认为真的拔掉
                if (_missingCount >= 3)
                {
                    Disconnect("Device disconnected.");
                }
            }
            else
            {
                _missingCount = 0;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}