using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using log4net.Core;

namespace BulbaGO.Base.ProcessManagement
{
    public enum ProcessState
    {
        NotCreated,
        Created,
        Starting,
        Started,
        StartFailed,
        Initializing,
        InitializationFailed,
        Running,
        Terminating,
        Terminated,
        Error
    }
}
