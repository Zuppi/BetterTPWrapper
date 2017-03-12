using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BetterTPWrapper
{
    public partial class BetterTPWrapper : Form
    {
        public class LogItem
        {
            private string logLine;

            public LogItem(string logLine)
            {
                this.logLine = logLine;
            }

            public override string ToString()
            {
                return logLine;
            }

        }
        private bool allowVisible;     // ContextMenu's Show command used
                                       //private bool allowClose;       // ContextMenu's Exit command used

        System.Diagnostics.Process betterTP;

        public BetterTPWrapper()
        {
            InitializeComponent();
            allowVisible = false;
            //allowClose = false;
            betterTP = null;
           
            OpenBetterTP();
        }

        private void OpenBetterTP()
        {
            string betterTPPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "bettertp.exe");
            if (!File.Exists(betterTPPath)){
                AddLogLine("----- No BetterTP found. Please place the executable in same folder with bettertp.exe -----");
            }
            else
            {
                betterTP = new System.Diagnostics.Process();
                betterTP.StartInfo.FileName = betterTPPath;
                betterTP.StartInfo.CreateNoWindow = true;
                betterTP.StartInfo.UseShellExecute = false;
                betterTP.StartInfo.RedirectStandardOutput = true;
                betterTP.StartInfo.RedirectStandardError = true;
                betterTP.OutputDataReceived += OutputHandler;
                betterTP.ErrorDataReceived += OutputHandler;
                betterTP.Start();
                betterTP.BeginErrorReadLine();
                betterTP.BeginOutputReadLine();
            }
            
           
        }

        void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            try
            {
                listBoxLog.Invoke((MethodInvoker)(() =>
                {
                    if (!String.IsNullOrEmpty(outLine.Data) && outLine.Data[0] != '[')
                    {
                        listBoxLog.Items.Add(new LogItem(outLine.Data));
                    }

                }));
            }
            
            catch
            {}
        }

        private void AddLogLine(string lineText)
        {
            listBoxLog.Items.Add(new LogItem(lineText));
            
        }

        protected override void SetVisibleCore(bool value)
        {
            if (!allowVisible)
            {
                value = false;
                if (!this.IsHandleCreated) CreateHandle();
            }
            base.SetVisibleCore(value);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                allowVisible = true;
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.BringToFront();
            }
        }

        private void betterTPWrapper_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (betterTP != null)
            {
                if (!betterTP.HasExited)
                {
                    betterTP.Kill();
                }
            }         
        }

        private void BetterTPWrapper_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon.Visible = true;
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon.Visible = false;
                this.Activate();
                listBoxLog.TopIndex = listBoxLog.Items.Count - 1;
            }
        }
    }
}
