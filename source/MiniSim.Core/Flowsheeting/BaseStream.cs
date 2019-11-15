using MiniSim.Core.Thermodynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Flowsheeting
{
    public class BaseStream:BaseSimulationElement
    {

        public BaseStream(string name, ThermodynamicSystem system)
        {
            Name = name;
            System = system;
        }
    }
}
