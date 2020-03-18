using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Creator.Messaging
{
    public class UpdateSolverStatusMessage
    {
        string _solverStatus;
        string _color;

        public string SolverStatus { get => _solverStatus; set => _solverStatus = value; }
        public string Color { get => _color; set => _color = value; }

        public UpdateSolverStatusMessage(string message, string color)
        {
            SolverStatus = message;
            Color = color;
        }
    }
}
