
using MiniSim.Core.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Interfaces
{
    public interface ISolver
    {
        bool Solve(AlgebraicSystem system);
    }
}
