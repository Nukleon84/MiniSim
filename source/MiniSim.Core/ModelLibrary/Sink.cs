using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniSim.Core.Expressions;
using MiniSim.Core.Flowsheeting;
using MiniSim.Core.Numerics;
using MiniSim.Core.Thermodynamics;
using MiniSim.Core.UnitsOfMeasure;

namespace MiniSim.Core.ModelLibrary
{
    public class Sink : ProcessUnit
    {
        public Sink(string name, ThermodynamicSystem system) : base(name, system)
        {
            Class = "Sink";


            MaterialPorts.Add(new Port<MaterialStream>("In", PortDirection.In, 1) { WidthFraction = 0, HeightFraction = 0.5, Normal = PortNormal.Left });

        }

        public override void CreateEquations(AlgebraicSystem problem)
        {
            foreach (var vari in Variables)
            {
                vari.Children.Clear();
            }

            base.CreateEquations(problem);
        }

        public override ProcessUnit Initialize()
        {
            
            return this;
        }

    }
}
