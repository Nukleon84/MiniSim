using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Creator.Messaging
{
   public  class UpdateEquationStatusMessage:BaseMessage
    {
        int _numberOfEquations;
        int _numberOfVariables;

        public int NumberOfEquations { get => _numberOfEquations; set => _numberOfEquations = value; }
        public int NumberOfVariables { get => _numberOfVariables; set => _numberOfVariables = value; }

        public UpdateEquationStatusMessage(int equations, int variables)
        {
            NumberOfVariables = variables;
            NumberOfEquations = equations;
        }
    }
}
