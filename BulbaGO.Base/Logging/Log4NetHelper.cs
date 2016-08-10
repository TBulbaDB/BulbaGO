using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var layout = new PatternLayout();
            layout.ConversionPattern = "%date [%thread] %-5level - %message%newline";
            layout.ActivateOptions();
            appender.Layout = layout;
            appender.Threshold = threshold;
            appender.ActivateOptions();

            return appender;
        }
    }
}
