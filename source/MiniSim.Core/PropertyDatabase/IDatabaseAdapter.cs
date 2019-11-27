using System;
using System.Collections.Generic;
using MiniSim.Core.Thermodynamics;

namespace MiniSim.Core.PropertyDatabase
{
    interface IDatabaseAdapter
    {
        void FillBIPs(ThermodynamicSystem system);
        Substance FindComponent(string name);
        List<string> ListComponents(string pattern);
        void SetLogCallback(Action<string> callback);
    }
}