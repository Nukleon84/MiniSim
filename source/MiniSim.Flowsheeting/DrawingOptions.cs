using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.FlowsheetDrawing
{
    public class DrawingOptions
    {
        public bool ShowTemperature = true;
        public bool ShowPressure = true;
        public bool ShowMassFlow = true;
        public bool ShowConnectors = false;
        public bool ShowStreamColors = false;
        public bool StreamColorMassBased = false;
        public Dictionary<string, string> ColorMap = new Dictionary<string, string>();

        public bool ShowVaporStreams = true;

        public bool ShowStreamWidth = false;
        public float MaxWidth = 8.0f;
        public float MinWidth = 2.0f;

    }
}
