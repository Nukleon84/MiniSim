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
    public class BlackBoxReactor : ProcessUnit
    {
        private Variable dp;
        private Variable p;
        private Variable T;
        private Variable Q;
        private Variable[] DHR;
        private Variable[] R;
        double[,] _stochiometry;

        int _numberOfReactions = 1;

        public BlackBoxReactor(string name, ThermodynamicSystem system, int numberOfReactions) : base(name, system)
        {
            Class = "Reactor";
            _numberOfReactions = numberOfReactions;
            
            MaterialPorts.Add(new Port<MaterialStream>("In", PortDirection.In, 1) { WidthFraction = 0, HeightFraction = 0.5, Normal = PortNormal.Left });
            MaterialPorts.Add(new Port<MaterialStream>("Out", PortDirection.Out, 1) { WidthFraction = 1, HeightFraction = 0.5, Normal = PortNormal.Right });

            R = new Variable[_numberOfReactions];
            DHR = new Variable[_numberOfReactions];
            _stochiometry = new double[_numberOfReactions, System.Components.Count];

            for (int i = 0; i < _numberOfReactions; i++)
            {
                R[i] = system.VariableFactory.CreateVariable("R", (i + 1).ToString(), "Converted Molar Flow for reaction " + (i + 1).ToString(), PhysicalDimension.MolarFlow);
                DHR[i] = system.VariableFactory.CreateVariable("DHR", (i + 1).ToString(), "Reaction enthalpy", PhysicalDimension.SpecificMolarEnthalpy);
                DHR[i].SetValue(0);
                DHR[i].IsFixed = true;
            }

            dp = system.VariableFactory.CreateVariable("DP", "Pressure Drop", PhysicalDimension.Pressure);
            p = system.VariableFactory.CreateVariable("P", "Pressure in heater outlet", PhysicalDimension.Pressure);
            T = system.VariableFactory.CreateVariable("T", "Temperature in heater outlet", PhysicalDimension.Temperature);
            Q = system.VariableFactory.CreateVariable("Q", "Heat Duty", PhysicalDimension.HeatFlow);


            dp.LowerBound = -1e9;
            dp.SetValue(0);
            AddVariable(dp);
            AddVariable(p);
            AddVariable(T);
            AddVariables(R);
            AddVariable(Q);
            AddVariables(DHR);
        }
        public BlackBoxReactor DefineRateEquation(int reactionNumber, Expression rate)
        {
            if (reactionNumber > 0 && reactionNumber < _numberOfReactions + 1)
            {
                R[reactionNumber - 1].BindTo(rate);
            }
            return this;
        }
        public BlackBoxReactor AddStochiometry(int reactionNumber, string compID, double factor)
        {
            var compIndex = System.Components.FindIndex(c => c.ID == compID);

            if (compIndex >= 0 && reactionNumber > 0)
            {
                _stochiometry[reactionNumber - 1, compIndex] = factor;
            }

            return this;
        }

        public override void CreateEquations(AlgebraicSystem problem)
        {
            int NC = System.Components.Count;
            var In = FindMaterialPort("In");
            var Out = FindMaterialPort("Out");

            Expression DHRtotal = 0;

            for (int i = 0; i < NC; i++)
            {
                var cindex = i;

                Expression reactingMoles = 0;
                for (int j = 0; j < _numberOfReactions; j++)
                {
                    if (Math.Abs(_stochiometry[j, i]) > 1e-16)
                    {
                        reactingMoles += _stochiometry[j, i] * R[j];

                        if (Math.Abs(DHR[j].Val()) > 1e-16)
                        {
                            DHRtotal += reactingMoles * DHR[j];
                        }
                    }
                }

                AddEquationToEquationSystem(problem,
                    Sym.Sum(0, In.NumberOfStreams, (j) => In.Streams[j].Bulk.ComponentMolarflow[cindex] + reactingMoles) - (Sym.Sum(0, Out.NumberOfStreams, (j) => Out.Streams[j].Bulk.ComponentMolarflow[cindex])), "Mass Balance");

            }

            AddEquationToEquationSystem(problem, (p / 1e4) - (Sym.Par(In.Streams[0].Pressure - dp) / 1e4), "Pressure Balance");


            foreach (var outlet in Out.Streams)
            {
                AddEquationToEquationSystem(problem, (outlet.Pressure / 1e4) - (p / 1e4), "Pressure drop");
                AddEquationToEquationSystem(problem, (outlet.Temperature / 1e3) - (T / 1e3), "Heat Balance");
            }

            AddEquationToEquationSystem(problem,
          ((Sym.Sum(0, In.NumberOfStreams, (i) => In.Streams[i].Bulk.SpecificEnthalpy * In.Streams[i].Bulk.TotalMolarflow + Q + DHRtotal) / 1e4))
          - (Sym.Par(Sym.Sum(0, Out.NumberOfStreams, (i) => Out.Streams[i].Bulk.SpecificEnthalpy * Out.Streams[i].Bulk.TotalMolarflow)) / 1e4), "Heat Balance");

            base.CreateEquations(problem);
        }

        public override ProcessUnit Initialize()
        {
            var In = FindMaterialPort("In");
            var Out = FindMaterialPort("Out");
            int NC = System.Components.Count;

            if (!p.IsFixed)
                p.SetValue(In.Streams[0].Pressure.Val());
            
            for (int i = 0; i < NC; i++)
            {
                Out.Streams[0].Bulk.ComponentMolarflow[i].SetValue(Sym.Sum(0, In.NumberOfStreams, (j) => In.Streams[j].Bulk.ComponentMolarflow[i]).Val());
            }

            Out.Streams[0].Temperature.SetValue(T.Val());
            Out.Streams[0].Pressure.SetValue(p.Val() - dp.Val());
            Out.Streams[0].VaporFraction.SetValue(In.Streams[0].VaporFraction.Val());

            Out.Streams[0].InitializeFromMolarFlows();

            if (T.IsFixed)
                Out.Streams[0].FlashPT();
            else
                Out.Streams[0].FlashPZ();

            Q.SetValue(-(In.Streams[0].Bulk.SpecificEnthalpy * In.Streams[0].Bulk.TotalMolarflow - Out.Streams[0].Bulk.SpecificEnthalpy * Out.Streams[0].Bulk.TotalMolarflow).Val());
            return this;
        }
    }
}
