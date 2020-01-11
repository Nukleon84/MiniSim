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
        ProcessUnit _source;
        ProcessUnit _sink;

        public ProcessUnit Source { get => _source; set => _source = value; }
        public ProcessUnit Sink { get => _sink; set => _sink = value; }
 

        public BaseStream(string name, ThermodynamicSystem system)
        {
            Name = name;
            System = system;
        }
    }
}
