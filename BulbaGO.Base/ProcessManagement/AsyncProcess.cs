using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BulbaGO.Base.Bots;
using log4net;
using log4net.Core;

namespace BulbaGO.Base.ProcessManagement
{
    public delegate void ProcessStateChangedEventHandler(ProcessState previousState, ProcessState state);
    public delegate void ProcessExited(AsyncProcess process);

    public abstract class AsyncProcess : IDisposable
    {
        protected Bot Bot { get; set; }
        protected AsyncProcess(Bot bot)
        {
            Bot = bot;
            Logger = LogManager.GetLogger(bot.Username);
        }

        public ILog Logger { get; set; }
        public event ProcessExited ProcessExited;

        public ProcessState State
        {
            get { return _state; }
            set
            {
                var previousState = _state;
                _state = value;
                if (_state != previousState)
                {
                    OnProcessStateChanged(previousState, _state);
                }
            }
        }

        public Process Process { get; private set; }
        private ProcessState _state;

        protected virtual Task<bool> Start(ProcessStartInfo processStartInfo, CancellationToken ct)
        {
            Logger.Info($"Starting {this.GetType().Name}...");
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
                Logger.Info($"Started {this.GetType().Name}...");
                ChildProcessTracker.AddProcess(Process);
                Process.BeginOutputReadLine();
                Process.BeginErrorReadLine();
                State = ProcessState.Started;
                tcs.SetResult(true);
            }

            return tcs.Task;
        }



        protected virtual async Task<bool> Initialize(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return true;
        }

        public virtual async Task<bool> Stop()
        {
            //ct.ThrowIfCancellationRequested();
            if (State != ProcessState.Terminating || State != ProcessState.Terminated)
            {
                State = ProcessState.Terminating;
            }
            return true;
        }

        protected virtual void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                Logger.Debug(e.Data);
            }
        }

        protected virtual void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (State != ProcessState.Terminating && State != ProcessState.Terminated)
            {
                State = ProcessState.Error;
            }

        }

        protected virtual void Process_Exited(object sender, EventArgs e)
        {
            if (State != ProcessState.Terminating)
            {
                Logger.Error($"{GetType().Name} has unexpectedly terminated.");
            }
            //State = ProcessState.Terminated;
            ProcessExited?.Invoke(this);
        }

        protected virtual void OnProcessStateChanged(ProcessState previousState, ProcessState state)
        {
            Logger.Debug($"{GetType().Name} State changed from {previousState} to {state}");
            switch (state)
            {
                case ProcessState.NotCreated:
                    break;
                case ProcessState.Created:
                    break;
                case ProcessState.Starting:
                    break;
                case ProcessState.Started:
                    break;
                case ProcessState.Initializing:
                    break;
                case ProcessState.Running:
                    Logger.Info($"Successfully launched {GetType().Name} with pid {Process.Id}.");
                    break;
                case ProcessState.StartFailed:
                case ProcessState.InitializationFailed:
                case ProcessState.Terminating:
                    if (Process == null || Process.HasExited) break;
                    Logger.Warn($"Termination of {GetType().Name} with pid {Process?.Id} is requested.");
                    Process?.Kill();
                    Logger.Warn($"{GetType().Name} with pid {Process?.Id} is terminated.");
                    break;
                case ProcessState.Terminated:
                    break;
                case ProcessState.Error:
                    State=ProcessState.Terminating;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
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
