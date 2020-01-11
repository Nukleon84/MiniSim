using MiniSim.Core.Expressions;
using MiniSim.Core.Numerics;
using MiniSim.Core.Reporting;
using MiniSim.Core.Thermodynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Flowsheeting
{
    public abstract class ProcessUnit : BaseSimulationElement
    {
        List<Port<MaterialStream>> _materialPorts = new List<Port<MaterialStream>>();
        List<Port<HeatStream>> _heatPorts = new List<Port<HeatStream>>();
        Chemistry _chemistryBlock;

        public List<Port<MaterialStream>> MaterialPorts
        {
            get
            {
                return _materialPorts;
            }

            set
            {
                _materialPorts = value;
            }
        }

        public List<Port<HeatStream>> HeatPorts
        {
            get
            {
                return _heatPorts;
            }

            set
            {
                _heatPorts = value;
            }
        }

        public Chemistry ChemistryBlock
        {
            get
            {
                return _chemistryBlock;
            }

            set
            {
                _chemistryBlock = value;
            }
        }

        public ProcessUnit(string name, ThermodynamicSystem system)
        {
            Name = name;
            System = system;
        }

        public ProcessUnit Connect(string portName, BaseStream stream)
        {
            if (stream is MaterialStream)
            {
                var materialPort = FindMaterialPort(portName);
                if (materialPort != null)
                {
                    if (materialPort.Direction == PortDirection.In)
                        stream.Sink = this;
                    if (materialPort.Direction == PortDirection.Out)
                        stream.Source = this;

                    materialPort.Connect(stream as MaterialStream);
                }
                else
                    throw new InvalidOperationException("Port " + portName + " not found");
            }

            if (stream is HeatStream)
            {
                var heatPort = FindHeatPort(portName);
                if (heatPort != null)
                    heatPort.Connect(stream as HeatStream);
                else
                    throw new InvalidOperationException("Port " + portName + " not found");
            }
            return this;
        }

        public ProcessUnit EnableChemistry(string label)
        {
            var chem = System.ChemistryBlocks.FirstOrDefault(c => c.Label == label);
            if (chem != null)
                EnableChemistry(chem);
            return this;
        }

        public virtual ProcessUnit EnableChemistry(Chemistry chem)
        {
            if (chem == null)
                return this;
            ChemistryBlock = chem;

            return this;
        }

        public ProcessUnit DisableChemistry()
        {
            ChemistryBlock = null;
            return this;
        }


        public virtual ProcessUnit Initialize()
        {
            return this;
        }
        /// <summary>
        /// Solves the unit together with the output material streams as a single flowsheet. When using this method, the unit has to be specified fully.
        /// </summary>
        public virtual ProcessUnit Solve()
        {
            var decomp = new DecompositionSolver(new NoLogger());

            var flowsheet = new Flowsheet(Name);
            flowsheet.AddUnit(this);
            foreach (var stream in MaterialPorts.Where(p => p.Direction == PortDirection.Out && p.IsConnected).Select(p => p.Streams.ToArray()))
            {
                flowsheet.AddMaterialStreams(stream);
            }
            decomp.Solve(flowsheet);
            return this;

        }

        public ProcessUnit RotatePorts(int rotations)
        {
            double x0 = 0.5;
            double y0 = 0.5;

            var angle = -rotations * Math.PI / 2;

            foreach (var port in MaterialPorts)
            {
                var x = port.WidthFraction;
                var y = port.HeightFraction;
                port.WidthFraction = x0 + (x - x0) * Math.Cos(angle) + (y - y0) * Math.Sin(angle);
                port.HeightFraction = y0 - (x - x0) * Math.Sin(angle) + (y - y0) * Math.Cos(angle);
                
                if (rotations > 0)
                    for (int i = 0; i < rotations; i++)
                    {
                        if (port.Normal == PortNormal.Left)
                            port.Normal = PortNormal.Up;
                        else if (port.Normal == PortNormal.Up)
                            port.Normal = PortNormal.Right;
                        else if (port.Normal == PortNormal.Right)
                            port.Normal = PortNormal.Down;
                        else
                            port.Normal = PortNormal.Left;
                    }
                else
                    for (int i = 0; i < -rotations; i++)
                    {
                        if (port.Normal == PortNormal.Left)
                            port.Normal = PortNormal.Down;
                        else if (port.Normal == PortNormal.Down)
                            port.Normal = PortNormal.Right;
                        else if (port.Normal == PortNormal.Right)
                            port.Normal = PortNormal.Up;
                        else
                            port.Normal = PortNormal.Left;
                    }
            }
            return this;
        }


        public Port<MaterialStream> FindMaterialPort(string portName)
        {
            return MaterialPorts.FirstOrDefault(p => p.Name == portName);
        }

        public Port<HeatStream> FindHeatPort(string portName)
        {
            return HeatPorts.FirstOrDefault(p => p.Name == portName);
        }



    }
}
