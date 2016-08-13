using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulbaGO.Base.ProcessManagement;

namespace BulbaGO.Base.Bots
{
    public interface IBotConfig
    {
        string BotName { get; }
        string BotFolder { get;  }
        string BotConfigFolder { get; }
        string BotExecutablePath { get; }
        void CreateBotConfig(Bot bot);
        void ProcessOutputData(BotProcess botProcess, string data);
    }
}
