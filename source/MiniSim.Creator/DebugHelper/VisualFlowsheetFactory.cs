using MiniSim.Core.Flowsheeting;
using MiniSim.Core.Thermodynamics;
using MiniSim.Creator.Flowsheeting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Creator.DebugHelper
{
    static class VisualFlowsheetFactory
    {
        internal static VisualFlowsheet CreateTrivialDemo(ThermodynamicSystem sys)
        {
            var flowsheet = new VisualFlowsheet();
            flowsheet.Name = "Demo Flowsheet - Trivial";

            var source = flowsheet.AddSource("Feed", null, 400, 400);
            var flash = flowsheet.AddFlash("F001", null, 600, 400);
            var vapo = flowsheet.AddSink("S101", null, 800, 100);
            var liqu = flowsheet.AddSink("S102", null, 800, 700);

            flowsheet.Connect(source, "Out", flash, "In", sys, "S001");
            flowsheet.Connect(flash, "Vap", vapo, "In", sys, "S002");
            flowsheet.Connect(flash, "Liq", liqu, "In", sys, "S003");

            return flowsheet;
        }

        internal static VisualFlowsheet CreateTrivialDemo2(ThermodynamicSystem sys)
        {
            var flowsheet = new VisualFlowsheet();
            flowsheet.Name = "Demo Flowsheet - Small";

            var source = flowsheet.AddSource("Feed", sys, 400, 400);
            var flash = flowsheet.AddFlash("F001", sys, 600, 400);
            var flash2 = flowsheet.AddFlash("F002", sys, 800, 600);
            var vapo = flowsheet.AddSink("Vapo1", sys, 800, 100);
            var vapo2 = flowsheet.AddSink("Vapo2", sys, 1000, 400);
            var liqu = flowsheet.AddSink("Liqu", sys, 1000, 800);

            flowsheet.Connect(source, "Out", flash, "In", sys, "S001");
            flowsheet.Connect(flash, "Vap", vapo, "In", sys, "S002");
            flowsheet.Connect(flash, "Liq", flash2, "In", sys, "S003");
            flowsheet.Connect(flash2, "Vap", vapo2, "In", sys, "S004");
            flowsheet.Connect(flash2, "Liq", liqu, "In", sys, "S005");

            return flowsheet;
        }

        internal static VisualFlowsheet CreateTrivialDemo3()
        {
            var flowsheet = new VisualFlowsheet();
            flowsheet.Name = "Demo Flowsheet - Empty";


            return flowsheet;
        }


    }
}
