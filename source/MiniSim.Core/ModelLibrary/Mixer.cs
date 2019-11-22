using MiniSim.Core.Flowsheeting;
using MiniSim.Core.Thermodynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniSim.Core.Numerics;
using MiniSim.Core.Expressions;
using MiniSim.Core.UnitsOfMeasure;

namespace MiniSim.Core.ModelLibrary
{
    public class Mixer : ProcessUnit
    {
        private Variable dp;
        private Variable p;
        public Mixer(string name, ThermodynamicSystem system) : base(name, system)
        {
            Class = "Mixer";

            MaterialPorts.Add(new Port<MaterialStream>("In", PortDirection.In, -1) { WidthFraction = 0, HeightFraction = 0.5, Normal = PortNormal.Left });
            MaterialPorts.Add(new Port<MaterialStream>("Out", PortDirection.Out, 1) { WidthFraction = 1, HeightFraction = 0.5, Normal = PortNormal.Right });

            dp = system.VariableFactory.CreateVariable("DP", "Pressure Drop", PhysicalDimension.Pressure);
            p = system.VariableFactory.CreateVariable("P", "Pressure in mixer", PhysicalDimension.Pressure);
            dp.LowerBound = -1e8;
            dp.SetValue(0);
            AddVariable(dp);
            AddVariable(p);
        }

        public override void CreateEquations(AlgebraicSystem problem)
        {
            foreach (var vari in Variables)
            {
                
                vari.Children.Clear();
            }

            int NC = System.Components.Count;
            var In = FindMaterialPort("In");
            var Out = FindMaterialPort("Out");

            for (int i = 0; i < NC; i++)
            {
                var cindex = i;
                AddEquationToEquationSystem(problem,
                    Sym.Sum(0, In.NumberOfStreams, (j) => In.Streams[j].Bulk.ComponentMolarflow[cindex])
                        -(Sym.Sum(0, Out.NumberOfStreams, (j) => Out.Streams[j].Bulk.ComponentMolarflow[cindex])), "Mass Balance");

            }


            AddEquationToEquationSystem(problem, (p / 1e4)-(Sym.Par(Sym.Min(In.Streams[0].Pressure, In.Streams[1].Pressure) - dp) / 1e4), "Pressure Balance");

            foreach (var outlet in Out.Streams)
            {
                AddEquationToEquationSystem(problem, (outlet.Pressure / 1e4)-(Sym.Par(p) / 1e4), "Pressure drop");
            }

            AddEquationToEquationSystem(problem,
                ((Sym.Sum(0, In.NumberOfStreams, (i) => In.Streams[i].Bulk.SpecificEnthalpy * In.Streams[i].Bulk.TotalMolarflow) / 1e6))
                -(Sym.Sum(0, Out.NumberOfStreams, (i) => Out.Streams[i].Bulk.SpecificEnthalpy * Out.Streams[i].Bulk.TotalMolarflow) / 1e6), "Heat Balance");

            base.CreateEquations(problem);

        }


        public override ProcessUnit Initialize()
        {
            var In = FindMaterialPort("In");
            var Out = FindMaterialPort("Out");
            int NC = System.Components.Count;


            p.SetValue(Math.Min(In.Streams[0].Pressure.Val(), In.Streams[1].Pressure.Val()));


            for (int i = 0; i < NC; i++)
            {
                Out.Streams[0].Bulk.ComponentMolarflow[i].SetValue(In.Streams.Sum(s => s.Bulk.ComponentMolarflow[i].Val()));
            }

            Out.Streams[0].Temperature.SetValue((In.Streams.Sum(s => s.Temperature.Val()) / In.NumberOfStreams));
            Out.Streams[0].Pressure.SetValue(p.Val());

            Out.Streams[0].InitializeFromMolarFlows();
            Out.Streams[0].FlashPT();
            return this;
        }
    }
}
