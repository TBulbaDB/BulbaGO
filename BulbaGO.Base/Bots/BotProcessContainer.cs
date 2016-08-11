using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulbaGO.Base.ProcessHandling;
using BulbaGO.Base.SocksProxy;
using log4net;

namespace BulbaGO.Base.Bots
{
    public enum BotProcessState
    {
        Unknown,
        Starting,
        Started,
        Terminating,
        Terminated,
        CouldNotStart
    }

    public class BotProcessContainer : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BotProcessContainer));
        private readonly Bot _bot;
        public Process BotProcess { get; private set; }
        public BotProcessState State { get; private set; }

        public event EventHandler ProcessExited;

        public BotProcessContainer(Bot bot)
        {
            _bot = bot;
        }

        public async Task Start()
        {
            Logger.Info($"{_bot.Username} Launching bot process. (Current Process State: {State})");
            State = BotProcessState.Starting;
            BotProcess = new Process();
            BotProcess.EnableRaisingEvents = true;
            BotProcess.StartInfo = new ProcessStartInfo
            {
                FileName = NecroBot.BotExecutablePath,
                Arguments = _bot.Username,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = false,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Normal,
                WorkingDirectory = NecroBot.BotFolder
            };

            BotProcess.OutputDataReceived += BotProcess_OutputDataReceived; ;

            BotProcess.ErrorDataReceived += BotProcess_ErrorDataReceived;
            BotProcess.Exited += BotProcess_Exited;

            var started = BotProcess.Start();
            if (!started)
            {
                State = BotProcessState.CouldNotStart;
                throw new InvalidOperationException("Could not start process: " + BotProcess);
            }
            ChildProcessTracker.AddProcess(BotProcess);
            BotProcess.BeginOutputReadLine();
            BotProcess.BeginErrorReadLine();
            State = BotProcessState.Started;
            Logger.Info($"{_bot.Username} Successfully launched bot process with pid {BotProcess.Id}.");
            await Task.Delay(100);
        }

        private void BotProcess_Exited(object sender, EventArgs e)
        {
            OnProcessExited();
        }

        private void BotProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                Logger.Error($"{_bot.Username} {e.Data}");
            }
        }

        public void TerminateProcess()
        {
            State = BotProcessState.Terminating;
            try
            {
                BotProcess.Kill();
            }
            catch (Exception ex)
            {
                Logger.Error($"{_bot.Username} An error occured while killing the bot process: ({ex.GetType().Name}) {ex.Message}");
            }
            State = BotProcessState.Terminated;

        }

        private void BotProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                Logger.Debug($"{_bot.Username} {e.Data}");

                if (e.Data.Contains(
                        "(ATTENTION) No usable PokeStops found in your area. Is your maximum distance too small?"))
                {
                    Logger.Error($"{_bot.Username} Bot reported no pokestops around, possible ip ban, restarting bot.");
                    TerminateProcess();
                    _bot.Restart();
                    return;
                }
                if (e.Data.Contains("INVALID PROXY"))
                {
                    Logger.Error($"{_bot.Username} Bot reported invalid proxy, restarting bot.");
                    TerminateProcess();
                    _bot.Restart();
                    return;
                }
                if (e.Data.Contains("ERROR"))
                {
                    Logger.Error($"{_bot.Username} Bot reported an error, restarting bot.");
                    TerminateProcess();
                    _bot.Restart();
                    return;
                }
            }
        }

        public void Dispose()
        {
            State = BotProcessState.Terminating;
            TerminateProcess();
            State = BotProcessState.Terminated;
        }

        protected virtual void OnProcessExited()
        {
            if (State == BotProcessState.Terminating || State == BotProcessState.Terminated) return;
            ProcessExited?.Invoke(this, EventArgs.Empty);
        }
    }
}
