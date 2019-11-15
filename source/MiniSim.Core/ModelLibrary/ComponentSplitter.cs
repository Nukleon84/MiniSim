using MiniSim.Core.Expressions;
using MiniSim.Core.Flowsheeting;
using MiniSim.Core.Numerics;
using MiniSim.Core.Thermodynamics;
using MiniSim.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.ModelLibrary
{
    public class ComponentSplitter : ProcessUnit
    {
        private Variable dp;
        private Variable p;
        private Variable[] SplitFactors;
        public ComponentSplitter(string name, ThermodynamicSystem system) : base(name, system)
        {
            Class = "CSplitter";

            MaterialPorts.Add(new Port<MaterialStream>("In", PortDirection.In, 1));
            MaterialPorts.Add(new Port<MaterialStream>("Out1", PortDirection.Out, 1));
            MaterialPorts.Add(new Port<MaterialStream>("Out2", PortDirection.Out, 1));

            dp = system.VariableFactory.CreateVariable("DP", "Pressure Drop", PhysicalDimension.Pressure);
            p = system.VariableFactory.CreateVariable("P", "Pressure in splitter", PhysicalDimension.Pressure);

            SplitFactors = new Variable[system.Components.Count];
            for (int i = 0; i < system.Components.Count; i++)
            {
                SplitFactors[i] = system.VariableFactory.CreateVariable("K", "Component Split factor (molar)", PhysicalDimension.MolarFraction);
                SplitFactors[i].Subscript = system.Components[i].ID;
            }
            dp.LowerBound = 0;
            AddVariable(dp);
            AddVariables(SplitFactors);
            AddVariable(p);
        }

        public override void CreateEquations(AlgebraicSystem problem)
        {

            int NC = System.Components.Count;
            var In = FindMaterialPort("In");
            var Out1 = FindMaterialPort("Out1");
            var Out2 = FindMaterialPort("Out2");


            for (int i = 0; i < NC; i++)
            {
                var cindex = i;
                AddEquationToEquationSystem(problem,
                   (SplitFactors[cindex] * In.Streams[0].Bulk.ComponentMolarflow[cindex])
                        - (Out1.Streams[0].Bulk.ComponentMolarflow[cindex]), "Mass Balance");

            }
            for (int i = 0; i < NC; i++)
            {
                var cindex = i;
                AddEquationToEquationSystem(problem,
                   (Sym.Par(1 - SplitFactors[cindex]) * In.Streams[0].Bulk.ComponentMolarflow[cindex])
                        - (Out2.Streams[0].Bulk.ComponentMolarflow[cindex]), "Mass Balance");

            }

            AddEquationToEquationSystem(problem, (p / 1e4) - ((In.Streams[0].Pressure - dp) / 1e4), "Pressure drop");

            AddEquationToEquationSystem(problem, (Out1.Streams[0].Pressure / 1e4) - (Sym.Par(p) / 1e4), "Pressure Balance");
            AddEquationToEquationSystem(problem, (Out1.Streams[0].Temperature / 1e3) - (Sym.Par(In.Streams[0].Temperature) / 1e3), "Temperature Balance");
            AddEquationToEquationSystem(problem, (Out2.Streams[0].Pressure / 1e4) - (Sym.Par(p) / 1e4), "Pressure Balance");
            AddEquationToEquationSystem(problem, (Out2.Streams[0].Temperature / 1e3) - (Sym.Par(In.Streams[0].Temperature) / 1e3), "Temperature Balance");


            base.CreateEquations(problem);

        }


        public override ProcessUnit Initialize()
        {
            var In = FindMaterialPort("In");
            var Out1 = FindMaterialPort("Out1");
            var Out2 = FindMaterialPort("Out2");
            int NC = System.Components.Count;


            p.SetValue(In.Streams[0].Pressure.Val());

            for (int i = 0; i < NC; i++)
            {
                Out1.Streams[0].Bulk.ComponentMolarflow[i].SetValue(SplitFactors[i].Val() * In.Streams.Sum(s => s.Bulk.ComponentMolarflow[i].Val()));
                Out2.Streams[0].Bulk.ComponentMolarflow[i].SetValue((1 - SplitFactors[i].Val()) * In.Streams.Sum(s => s.Bulk.ComponentMolarflow[i].Val()));
            }

            Out1.Streams[0].Temperature.SetValue(In.Streams[0].Temperature.Val());
            Out2.Streams[0].Temperature.SetValue(In.Streams[0].Temperature.Val());
            Out1.Streams[0].Pressure.SetValue(p.Val());
            Out2.Streams[0].Pressure.SetValue(p.Val());

            Out1.Streams[0].InitializeFromMolarFlows();
            Out2.Streams[0].InitializeFromMolarFlows();


            Out1.Streams[0].FlashPT();
            Out2.Streams[0].FlashPT();
            return this;
        }
    }
}
