﻿using System;
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
    public class BotProcessContainer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BotProcessContainer));
        private readonly Bot _bot;
        public Process BotProcess { get; private set; }

        public BotProcessContainer(Bot bot)
        {
            _bot = bot;
        }

        public void Start()
        {
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
                //you may allow for the process to be re-used (started = false) 
                //but I'm not sure about the guarantees of the Exited event in such a case
                throw new InvalidOperationException("Could not start process: " + BotProcess);
            }
            ChildProcessTracker.AddProcess(BotProcess);
            BotProcess.BeginOutputReadLine();
            BotProcess.BeginErrorReadLine();
            BotProcess.WaitForExit();
        }

        private void BotProcess_Exited(object sender, EventArgs e)
        {

        }

        private void BotProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {

        }

        private void TerminateProcess()
        {
            try
            {
                BotProcess.Kill();
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occured while killing the bot process: ({ex.GetType().Name}) {ex.Message}");
            }

        }

        private void BotProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                if (
                    e.Data.Contains(
                        "(ATTENTION) No usable PokeStops found in your area. Is your maximum distance too small?"))
                {
                    TerminateProcess();
                    _bot.Restart();
                }
                Logger.Debug($"{_bot.Username} {e.Data}");
            }
        }
    }
}
