using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace CommandLineWidget
{
    public partial class Form1 : Form
    {
        private IntPtr cmdPtr;
        private IntPtr folderViewPtr;

        public Form1()
        {
            InitializeComponent();

            folderViewPtr = FindWindow("Progman", null);
            folderViewPtr = FindWindowEx(folderViewPtr, IntPtr.Zero, "SHELLDLL_DefView", null);
            folderViewPtr = FindWindowEx(folderViewPtr, IntPtr.Zero, "SysListView32", "FolderView");
			
			SetWindowLong(this.Handle, -8, (int)folderViewPtr); //GWL_HWNDPARENT

            Process p = Process.Start("cmd.exe", "/k cd %userprofile%\\desktop");
            Thread.Sleep(500); // Allow the process to open it's window
            cmdPtr = p.MainWindowHandle;
            SetParent(p.MainWindowHandle, this.Handle);

            SetWindowLong(this.Handle, -20, (int)(GetWindowLong(this.Handle, -20) | 0x00000080L));

            SetWindowLong(p.MainWindowHandle, -16, (int)(0x10000000L));

            MoveWindow(cmdPtr, -2, -2, this.Width, this.Height, true);

            SetWindowPos(this.Handle, (IntPtr)1, this.Left, this.Top, this.Width, this.Height, 0);

            timer1.Start();
        }


        protected override void WndProc(ref Message message)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MOVE = 0xF010;
            const int WM_WINDOWPOSCHANGING = 0x0046;

            switch (message.Msg)
            {
                case WM_SYSCOMMAND:
                    int command = message.WParam.ToInt32() & 0xfff0;
                    if (command == SC_MOVE)
                    {
                        return;
                    }
                    break;

                case WM_WINDOWPOSCHANGING:
                    SetWindowPos(this.Handle, (IntPtr)1, this.Left, this.Top, this.Width, this.Height, 0x0004);
                    break;
            }

            base.WndProc(ref message);
        }

        protected override void OnResize(EventArgs e)
        {
            if (this.cmdPtr != IntPtr.Zero)
            {
                MoveWindow(cmdPtr, 0, 0, this.Width, this.Height, true);
            }
            base.OnResize(e);
        }


        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (cmdPtr != IntPtr.Zero)
            {
                PostMessage(cmdPtr, 0x0010, 0, 0);
            }
            base.OnHandleDestroyed(e);
        }

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("User32.dll")]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool repaint);

        [DllImport("User32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint message, int wparam, int lparam);

        [DllImport("User32.dll")]
        static extern void SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern void SetWindowPos(IntPtr hWnd, IntPtr hWndNew, int x, int y, int cx, int cy, UInt32 uFlags);

        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(int xPoint, int yPoint);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern IntPtr FindWindowEx(IntPtr hWnd, IntPtr hWndA, string lpClassName, string lpWindowName);

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Right - this.Width + 33;
            this.Top = 0;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //check if cmd still exists
            if (!IsWindow(cmdPtr))
            {
                Application.Exit();
            }
        }

    }
}
