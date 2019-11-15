using MiniSim.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Reporting
{
    public class StringBuilderLogger : ILogger
    {
        StringBuilder _sb = new StringBuilder();

        public void Debug(string message)
        {
            _sb.AppendLine(message);
        }

        public void Error(string message)
        {
            _sb.AppendLine(message);
        }

        public void Info(string message)
        {
            _sb.AppendLine(message);
        }

        public void Log(string message)
        {
            _sb.AppendLine(message);
        }

        public void Succcess(string message)
        {
            _sb.AppendLine(message);
        }

        public void Warning(string message)
        {
            _sb.AppendLine(message);
        }

        public void Write(string message)
        {
            _sb.Append(message);
        }

        public string Flush()
        {
            var result= _sb.ToString();
            _sb.Clear();
            return result;
        }
    }
}
