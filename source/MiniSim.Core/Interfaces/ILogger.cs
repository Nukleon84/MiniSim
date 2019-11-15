using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Interfaces
{
    public interface ILogger
    {
        void Log(string message);
        void Info(string message);
        void Succcess(string message);
        void Debug(string message);
        void Warning(string message);
        void Error(string message);
        void Write(string message);
    }
}
