using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net.Appender;
using log4net.Core;

namespace BulbaGO.UI.Logging
{
    public class TextBoxAppender : AppenderSkeleton
    {
        delegate void LogAppendCallback(LoggingEvent loggingEvent);

        private readonly Form _form;
        private readonly TextBox _textBox;
        private readonly string _loggerName;

        public TextBoxAppender(Form form, TextBox textBox, string loggerName)
        {
            _form = form;
            _textBox = textBox;
            _loggerName = loggerName;
        }
        
        protected override void Append(LoggingEvent loggingEvent)
        {
            if (loggingEvent.LoggerName != _loggerName) return;
            if (_textBox.InvokeRequired)
            {
                var cb=new LogAppendCallback(Append);
                _form.Invoke(cb, new object[] {loggingEvent});
            }
            else
            {
                _textBox.AppendText(RenderLoggingEvent(loggingEvent));

            }
        }


    }
}
