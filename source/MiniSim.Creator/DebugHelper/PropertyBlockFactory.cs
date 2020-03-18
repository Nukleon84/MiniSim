using MiniSim.Core.PropertyDatabase;
using MiniSim.Core.Thermodynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Creator.DebugHelper
{
    static class PropertyBlockFactory
    {
        internal static ThermodynamicSystem CreateSystem1()
        {
            var db = new ChemSepAdapter();
            var system = new ThermodynamicSystem("System1","NRTL");

            var c1 = db.FindComponent("Ethanol").RenameID("EtOH");
            var c2 = db.FindComponent("Water").RenameID("H2O");
            system.AddComponent(c1);
            system.AddComponent(c2);

            db.FillBIPs(system);

            return system;
        }
        internal static ThermodynamicSystem CreateSystem2()
        {
            var db = new ChemSepAdapter();
            var system = new ThermodynamicSystem("System2", "Ideal");

            var c1 = db.FindComponent("Benzene").RenameID("Benzol");
            var c2 = db.FindComponent("Toluene").RenameID("Toluol");
            var c3 = db.FindComponent("P-xylene").RenameID("Xylol");
            system.AddComponent(c1);
            system.AddComponent(c2);
            system.AddComponent(c3);

            db.FillBIPs(system);

            return system;
        }

        internal static ThermodynamicSystem CreateSystem3()
        {
            var db = new ChemSepAdapter();
            var system = new ThermodynamicSystem("System3", "NRTL");

            var c1 = db.FindComponent("Acetone").RenameID("Acetone");
            var c2 = db.FindComponent("Methanol").RenameID("Methanol");
            var c3 = db.FindComponent("Isopropanol").RenameID("Isopropanol");
            system.AddComponent(c1);
            system.AddComponent(c2);
            system.AddComponent(c3);

            db.FillBIPs(system);

            return system;
        }

    }
}
