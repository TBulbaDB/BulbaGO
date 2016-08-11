using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulbaGO.Base.Bots;
using log4net;
using log4net.Core;

namespace BulbaGO.Base.ProcessManagement
{
    public delegate void ProcessStateChangedEventHandler(ProcessState state);

    public abstract class AsyncProcess : IDisposable
    {
        protected Bot Bot { get; set; }
        protected AsyncProcess(Bot bot)
        {
            Bot = bot;
            Logger = LogManager.GetLogger(bot.Username);
        }
        protected ILog Logger { get; set; }

        public ProcessState State
        {
            get { return _state; }
            set
            {
                var previousState = _state;
                _state = value;
                if (_state != previousState)
                {
                    OnProcessStateChanged(_state);
                }
            }
        }

        public Process Process { get; private set; }
        private ProcessState _state;

        protected virtual Task<bool> Start(ProcessStartInfo processStartInfo)
        {
            var tcs = new TaskCompletionSource<bool>();
            State = ProcessState.Starting;
            if (processStartInfo == null)
            {
                State = ProcessState.Error;
                tcs.SetException(new ArgumentNullException(nameof(processStartInfo)));
                return tcs.Task;
            }

            Process = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = processStartInfo
            };
            Process.Exited += Process_Exited;
            Process.ErrorDataReceived += Process_ErrorDataReceived;
            Process.OutputDataReceived += Process_OutputDataReceived;

            if (Process.Start())
            {
                ChildProcessTracker.AddProcess(Process);
                Process.BeginOutputReadLine();
                Process.BeginErrorReadLine();
                State = ProcessState.Started;
                tcs.SetResult(true);
            }

            return tcs.Task;
        }



        protected virtual async Task<bool> Initialize()
        {
            return true;
        }

        public virtual async Task<bool> Stop()
        {
            State=ProcessState.Terminating;
            Process?.Dispose();
            return true;
        }

        public event ProcessStateChangedEventHandler ProcessStateChanged;
        public event DataReceivedEventHandler OutputDataReceived;
        public event DataReceivedEventHandler ErrorDataReceived;
        public event EventHandler Exited;

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OutputDataReceived?.Invoke(sender, e);
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            State = ProcessState.Error;
            ErrorDataReceived?.Invoke(sender, e);
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            if (State != ProcessState.Terminating)
            {
                Logger.Error("Process has unexpectedly terminated.");
            }
            State = ProcessState.Terminated;
            Exited?.Invoke(sender, e);
        }

        private void OnProcessStateChanged(ProcessState state)
        {
            Logger.Info($"{this.GetType().Name} {state}");
            ProcessStateChanged?.Invoke(state);
        }

        public void Dispose()
        {
            Logger.Debug($"{this.GetType().Name} Disposing");
            Process?.Dispose();
        }

        protected virtual void OnOutputDataReceived()
        {
        }
    }
}
