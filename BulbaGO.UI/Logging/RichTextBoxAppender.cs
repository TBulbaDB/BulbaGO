using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net.Appender;
using log4net.Core;

namespace BulbaGO.UI.Logging
{
    public class RichTextBoxAppender : AppenderSkeleton
    {
        delegate void LogAppendCallback(LoggingEvent loggingEvent);

        private readonly Form _form;
        private readonly RichTextBox _textBox;
        private readonly string _loggerName;

        public RichTextBoxAppender(Form form, RichTextBox textBox, string loggerName)
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
                AppendText(_textBox,RenderLoggingEvent(loggingEvent),GetColor(loggingEvent.Level));
            }
        }

        private static Color GetColor(Level level)
        {
            if (level == Level.Debug)
            {
                return Color.LightGray;
            }
            if (level == Level.Warn)
            {
                return Color.Yellow;
            }
            if (level == Level.Error)
            {
                return Color.Red;
            }
            return Color.Green;
        }

        private static void AppendText(RichTextBox box, string text, Color color, bool addNewLine = false)
        {
            if (addNewLine)
            {
                text += Environment.NewLine;
            }

            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
            box.ScrollToCaret();
        }


    }
}
