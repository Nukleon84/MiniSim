using MiniSim.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Reporting
{
    public class CompositeLogger:ILogger
    {
        List<ILogger> _loggers = new List<ILogger>();

        public void Register(ILogger logger)
        {
            if (!_loggers.Contains(logger))
                _loggers.Add(logger);
        }

        public void Debug(string message)
        {
            foreach (var logger in _loggers)
                logger.Debug(message);
        }

        public void Error(string message)
        {
            foreach (var logger in _loggers)
                logger.Error(message);
        }

        public void Info(string message)
        {
            foreach (var logger in _loggers)
                logger.Info(message);
        }

        public void Log(string message)
        {
            foreach (var logger in _loggers)
                logger.Log(message);
        }

        public void Write(string message)
        {
            foreach (var logger in _loggers)
                logger.Write(message);
        }

        public void Succcess(string message)
        {
            foreach (var logger in _loggers)
                logger.Succcess(message);
        }

        public void Warning(string message)
        {
            foreach (var logger in _loggers)
                logger.Warning(message);
        }
    }
}
