using MiniSim.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Reporting
{
    public class NoLogger : ILogger
    {
        public void Debug(string message)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }

        public void Error(string message)
        {
           
        }

        public void Info(string message)
        {

        }

        public void Log(string message)
        {

        }
        public void Write(string message)
        {

        }
        public void Succcess(string message)
        {
          
        }

        public void Warning(string message)
        {

        }
    }
}
