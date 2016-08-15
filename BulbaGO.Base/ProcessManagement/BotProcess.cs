using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BulbaGO.Base.Bots;

namespace BulbaGO.Base.ProcessManagement
{
    public delegate void BotProgressChangedEventHandler(BotProgress progress);

    public class BotProcess : AsyncProcess
    {

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        public string WindowTitle { get; private set; }
        public event BotProgressChangedEventHandler BotProgressChanged;

        private IntPtr _botProcessParent;

        public BotProcess(Bot bot) : base(bot)
        {
        }

        public async Task<bool> Start(CancellationToken ct, IntPtr botProcessParent)
        {
            _botProcessParent = botProcessParent;
            ct.ThrowIfCancellationRequested();
            var startInfo = new ProcessStartInfo
            {
                FileName = Bot.BotConfig.BotExecutablePath,
                Arguments = Bot.Username,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = false,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Minimized,
                WorkingDirectory = Bot.BotConfig.BotFolder
            };

            await Start(startInfo, ct);
            if (State != ProcessState.Started)
            {
                State = ProcessState.StartFailed;
                return false;
            }

            State = ProcessState.Initializing;
            if (await Initialize(ct))
            {
                State = ProcessState.Running;
                return true;
            }
            State = ProcessState.InitializationFailed;
            return false;
        }

        protected override void OnProcessStateChanged(ProcessState previousState, ProcessState state)
        {
            if (state == ProcessState.IPBan)
            {
                try
                {
                    SocksWebProxyProcess.BlockedIps.Add(Bot.ProxyProcess.ExitAddress);

                }
                catch (Exception)
                {
                }
                State = ProcessState.Error;
                return;
            }
            base.OnProcessStateChanged(previousState, state);
        }

        protected override async Task<bool> Initialize(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return await base.Initialize(ct);
        }

        //private bool _attached;
        protected override void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            base.Process_OutputDataReceived(sender, e);
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                Bot.BotConfig.ProcessOutputData(this, e.Data);
                //if (!_attached)
                //{
                //    if (_botProcessParent != default(IntPtr))
                //    {
                //        SetParent(Process.MainWindowHandle, _botProcessParent);
                //    }
                //    _attached = true;
                //}
            }
            try
            {

                var textLength = GetWindowTextLength(Process.MainWindowHandle);
                var wndStr = new StringBuilder(textLength);
                var l = GetWindowText(Process.MainWindowHandle, wndStr, wndStr.Capacity);
                var windowText = wndStr.ToString();
                if (WindowTitle != windowText)
                {
                    WindowTitle = windowText;
                    BotProgressChanged?.Invoke(new BotProgress {BotTitle = WindowTitle});
                }
            }
            catch { }
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowTextLength", SetLastError = true)]
        static extern int GetWindowTextLength(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
    }

    public class WindowHandleInfo
    {
        private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

        private IntPtr _MainHandle;

        public WindowHandleInfo(IntPtr handle)
        {
            this._MainHandle = handle;
        }

        public List<IntPtr> GetAllChildHandles()
        {
            List<IntPtr> childHandles = new List<IntPtr>();

            GCHandle gcChildhandlesList = GCHandle.Alloc(childHandles);
            IntPtr pointerChildHandlesList = GCHandle.ToIntPtr(gcChildhandlesList);

            try
            {
                EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
                EnumChildWindows(this._MainHandle, childProc, pointerChildHandlesList);
            }
            finally
            {
                gcChildhandlesList.Free();
            }

            return childHandles;
        }

        private bool EnumWindow(IntPtr hWnd, IntPtr lParam)
        {
            GCHandle gcChildhandlesList = GCHandle.FromIntPtr(lParam);

            if (gcChildhandlesList == null || gcChildhandlesList.Target == null)
            {
                return false;
            }

            List<IntPtr> childHandles = gcChildhandlesList.Target as List<IntPtr>;
            childHandles.Add(hWnd);

            return true;
        }
    }

}
