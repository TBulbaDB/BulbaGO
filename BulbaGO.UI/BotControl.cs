using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BulbaGO.Base.Bots;
using BulbaGO.Base.Logging;
using BulbaGO.UI.Logging;
using log4net;
using log4net.Core;
using log4net.Layout;

namespace BulbaGO.UI
{


    public partial class BotControl : UserControl
    {
        public static BotControl GetInstance(Form form, Bot bot)
        {
            var instance = new BotControl { Bot = bot };
            instance.Dock = DockStyle.Fill;
            var appender = new RichTextBoxAppender(form, instance.BotLogRich, bot.Username);
            var layout = new PatternLayout();
            layout.ConversionPattern = "[%thread] %-5level - %message%newline";
            layout.ActivateOptions();
            appender.Layout = layout;
            appender.Threshold = Level.All;
            Log4NetHelper.AddAppender(appender);

            return instance;
        }

        public Bot Bot { get; set; }
        public BotControl()
        {
            InitializeComponent();
        }

        private void BotControl_Load(object sender, EventArgs e)
        {
            if (Bot != null)
            {
                BotName.Text = Bot.Username;

            }
        }

        private async void Start_Click(object sender, EventArgs e)
        {
            var progress = new Progress<BotProgress>(ReportProgress);
            Start.Enabled = false;
            Stop.Enabled = true;
            await Bot.Start(BotType.PokeMobBot, progress, ConsoleHolder.Handle);
        }

        private async void Stop_Click(object sender, EventArgs e)
        {
            Start.Enabled = true;
            Stop.Enabled = false;
            await Bot.Stop();
        }

        private void ReportProgress(BotProgress progress)
        {
            BotProcessTitle.Text = progress.BotTitle;
        }
    }
}
