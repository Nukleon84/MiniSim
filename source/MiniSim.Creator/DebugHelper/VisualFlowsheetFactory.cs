using MiniSim.Core.Flowsheeting;
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
        internal static VisualFlowsheet CreateTrivialDemo()
        {
            var flowsheet = new VisualFlowsheet();
            flowsheet.Name = "Demo Flowsheet - Trivial";

            var source = flowsheet.AddSource("Feed", null, 400, 400);
            var flash = flowsheet.AddFlash("F001", null, 600, 400);
            var vapo = flowsheet.AddSink("S101", null, 800, 100);
            var liqu = flowsheet.AddSink("S102", null, 800, 700);

            flowsheet.Connect(source, "Out", flash, "In", "S001");
            flowsheet.Connect(flash, "Vap", vapo, "In", "S002");
            flowsheet.Connect(flash, "Liq", liqu, "In", "S003");

            return flowsheet;
        }

        internal static VisualFlowsheet CreateTrivialDemo2()
        {
            var flowsheet = new VisualFlowsheet();
            flowsheet.Name = "Demo Flowsheet - Small";

            var source = flowsheet.AddSource("Feed", null, 400, 400);
            var flash = flowsheet.AddFlash("F001", null, 600, 400);
            var flash2 = flowsheet.AddFlash("F002", null, 800, 600);
            var vapo = flowsheet.AddSink("Vapo1", null, 800, 100);
            var vapo2 = flowsheet.AddSink("Vapo2", null, 1000, 400);
            var liqu = flowsheet.AddSink("Liqu", null, 1000, 800);

            flowsheet.Connect(source, "Out", flash, "In", "S001");
            flowsheet.Connect(flash, "Vap", vapo, "In", "S002");
            flowsheet.Connect(flash, "Liq", flash2, "In", "S003");
            flowsheet.Connect(flash2, "Vap", vapo2, "In", "S004");
            flowsheet.Connect(flash2, "Liq", liqu, "In", "S005");

            return flowsheet;
        }

    }
}
