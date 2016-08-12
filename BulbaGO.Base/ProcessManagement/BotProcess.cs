using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulbaGO.Base.Bots;

namespace BulbaGO.Base.ProcessManagement
{
    public class BotProcess : AsyncProcess
    {
        public BotProcess(Bot bot) : base(bot)
        {
        }

        public async Task<bool> Start()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = NecroBot.BotExecutablePath,
                Arguments = Bot.Username,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = false,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Normal,
                WorkingDirectory = NecroBot.BotFolder
            };

            await Start(startInfo);
            if (State != ProcessState.Started)
            {
                State = ProcessState.StartFailed;
                return false;
            }

            State = ProcessState.Initializing;
            if (await Initialize())
            {
                State = ProcessState.Running;
                return true;
            }
            State = ProcessState.InitializationFailed;
            return false;
        }

        protected override async Task<bool> Initialize()
        {
            return await base.Initialize();
        }

        protected override void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            base.Process_OutputDataReceived(sender, e);
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                if (e.Data.Contains("(ATTENTION) No usable PokeStops found in your area. Is your maximum distance too small?"))
                {
                    Logger.Error("Bot reported no pokestops around, possible ip ban, restarting bot.");
                    State = ProcessState.Error;
                    //TerminateProcess();
                    //_bot.Restart();
                    return;
                }
                if (e.Data.Contains("INVALID PROXY"))
                {
                    Logger.Error("Bot reported invalid proxy, restarting bot.");
                    //TerminateProcess();
                    //_bot.Restart();
                    State = ProcessState.Error;
                    return;
                }
                if (e.Data.Contains("ERROR"))
                {
                    Logger.Error("Bot reported an error, restarting bot.");
                    //TerminateProcess();
                    //_bot.Restart();
                    State = ProcessState.Error;
                    return;
                }
            }
        }
    }

}
