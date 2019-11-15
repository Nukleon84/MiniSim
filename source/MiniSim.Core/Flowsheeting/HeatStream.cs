using MiniSim.Core.Expressions;
using MiniSim.Core.Numerics;
using MiniSim.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Flowsheeting
{
    public class HeatStream : BaseStream
    {
        Variable q;

        public HeatStream(string name, Thermodynamics.ThermodynamicSystem system) : base(name, system)
        {
            Class = "HeatStream";
            Q = system.VariableFactory.CreateVariable("Q", "Heat Duty", PhysicalDimension.HeatFlow);
            AddVariable(Q);
        }

        public Variable Q
        {
            get
            {
                return q;
            }

            set
            {
                q = value;
            }
        }

        public override void CreateEquations(AlgebraicSystem problem)
        {
            base.CreateEquations(problem);
        }
    }

}
