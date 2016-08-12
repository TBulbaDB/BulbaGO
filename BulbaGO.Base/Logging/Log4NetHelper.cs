using System;
using System.IO;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace BulbaGO.Base.Logging
{
    public static class Log4NetHelper
    {

        public static void AddAppender(IAppender appender)
        {
            var repository = (Hierarchy)LogManager.GetRepository();
            repository.Root.Level = Level.All;
            repository.Root.AddAppender(appender);
            repository.Configured = true;
            repository.RaiseConfigurationChanged(EventArgs.Empty);
        }

        public static IAppender ConsoleAppender(Level threshold)
        {
            var appender = new ColoredConsoleAppender();
            appender.Name = "ConsoleAppender";
            appender.AddMapping(new ColoredConsoleAppender.LevelColors { ForeColor = ColoredConsoleAppender.Colors.Green, Level = Level.Info });
            appender.AddMapping(new ColoredConsoleAppender.LevelColors { ForeColor = ColoredConsoleAppender.Colors.Yellow, Level = Level.Warn });
            appender.AddMapping(new ColoredConsoleAppender.LevelColors { ForeColor = ColoredConsoleAppender.Colors.White, Level = Level.Debug });
            appender.AddMapping(new ColoredConsoleAppender.LevelColors { ForeColor = ColoredConsoleAppender.Colors.Red, Level = Level.Error });
            var layout = new PatternLayout();
            layout.ConversionPattern = "%date{HH:mm:ss} %logger [%thread] %-5level - %message%newline";
            layout.ActivateOptions();
            appender.Layout = layout;
            appender.Threshold = threshold;
            appender.ActivateOptions();

            return appender;
        }

        public static IAppender FileAppender(Level threshhold)
        {
            var appender=new FileAppender();
            appender.Name = "FileAppender";
            appender.AppendToFile = true;
            appender.Encoding=Encoding.UTF8;
            appender.ImmediateFlush = true;
            appender.File = GenerateLogFileName();
            appender.Threshold = threshhold;
            var layout = new PatternLayout();
            layout.ConversionPattern = "%date %logger [%thread] %-5level - %message%newline";
            layout.ActivateOptions();
            appender.Layout = layout;
            appender.ActivateOptions();
            return appender;
        }


        private static string GenerateLogFileName()
        {
            var fileName = $"{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.log";
            var logFolder = Path.Combine(Environment.CurrentDirectory, "Logs");
            Directory.CreateDirectory(logFolder);
            return Path.Combine(logFolder, fileName);
        }
    }
}
