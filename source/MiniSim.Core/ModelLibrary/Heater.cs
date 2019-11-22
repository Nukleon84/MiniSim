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
using MiniSim.Core.Thermodynamics.Routines;

namespace MiniSim.Core.ModelLibrary
{
    public class Heater : ProcessUnit
    {
        private Variable dp;
        private Variable p;
        private Variable T;
        private Variable VF;
        private Variable Q;
        private Variable[] r;
        public Heater(string name, ThermodynamicSystem system) : base(name, system)
        {
            Class = "Heater";

            MaterialPorts.Add(new Port<MaterialStream>("In", PortDirection.In, 1) {WidthFraction=0, HeightFraction=0.5, Normal = PortNormal.Left });
            MaterialPorts.Add(new Port<MaterialStream>("Out", PortDirection.Out, 1) { WidthFraction = 1, HeightFraction = 0.5, Normal = PortNormal.Right });
            HeatPorts.Add(new Port<HeatStream>("Duty", PortDirection.In, 1));

            dp = system.VariableFactory.CreateVariable("DP", "Pressure Drop", PhysicalDimension.Pressure);
            p = system.VariableFactory.CreateVariable("P", "Pressure in heater outlet", PhysicalDimension.Pressure);
            T = system.VariableFactory.CreateVariable("T", "Temperature in heater outlet", PhysicalDimension.Temperature);
            VF = system.VariableFactory.CreateVariable("VF", "Vapor fraction in heater outlet", PhysicalDimension.MolarFraction);
            Q = system.VariableFactory.CreateVariable("Q", "Heat Duty", PhysicalDimension.HeatFlow);
            dp.LowerBound = 0;
            dp.SetValue(0);

            AddVariable(dp);
            AddVariable(p);
            AddVariable(T);
            AddVariable(VF);
            AddVariable(Q);

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
            var Duty = FindHeatPort("Duty");

            if (Duty.IsConnected)
            {
                Q.IsFixed = true;
                Q.SetValue(0);
            }
            
            for (int i = 0; i < NC; i++)
            {
                var cindex = i;

                Expression reactingMoles = 0;

                if (ChemistryBlock != null)
                {
                    reactingMoles = ChemistryBlock.GetReactingMolesExpression(r, System.Components[i]);
                }


                AddEquationToEquationSystem(problem,
                    (Sym.Sum(0, In.NumberOfStreams, (j) => In.Streams[j].Bulk.ComponentMolarflow[cindex]) + reactingMoles)
                        - (Sym.Sum(0, Out.NumberOfStreams, (j) => Out.Streams[j].Bulk.ComponentMolarflow[cindex])), "Mass Balance");
            }


            if (ChemistryBlock != null)
            {
                foreach (var reac in ChemistryBlock.Reactions)
                {
                    AddEquationToEquationSystem(problem, reac.GetDefiningEquation(Out.Streams[0]), "Reaction rate equation");
                }
            }

            AddEquationToEquationSystem(problem, (p / 1e4) - (Sym.Par(In.Streams[0].Pressure - dp) / 1e4), "Pressure Balance");

            if (!VF.IsFixed)
                AddEquationToEquationSystem(problem, (VF) - (Out.Streams[0].VaporFraction), "Vapor Fraction");


            foreach (var outlet in Out.Streams)
            {
                AddEquationToEquationSystem(problem, (outlet.Pressure / 1e4) - (p / 1e4), "Pressure drop");
                AddEquationToEquationSystem(problem, (outlet.Temperature / 1e3) - (T / 1e3), "Heat Balance");
            }

            if (Duty.IsConnected)
            {
                if (Duty.Direction == PortDirection.In)
                {
                    AddEquationToEquationSystem(problem,
                ((Sym.Sum(0, In.NumberOfStreams, (i) => In.Streams[i].Bulk.SpecificEnthalpy * In.Streams[i].Bulk.TotalMolarflow + Duty.Streams[0].Q) / 1e4))
                - (Sym.Par(Sym.Sum(0, Out.NumberOfStreams, (i) => Out.Streams[i].Bulk.SpecificEnthalpy * Out.Streams[i].Bulk.TotalMolarflow)) / 1e4), "Heat Balance");
                }
                else
                {
                    AddEquationToEquationSystem(problem,
             ((Sym.Sum(0, In.NumberOfStreams, (i) => In.Streams[i].Bulk.SpecificEnthalpy * In.Streams[i].Bulk.TotalMolarflow) / 1e6))
             - (Sym.Par(Sym.Sum(0, Out.NumberOfStreams, (i) => Out.Streams[i].Bulk.SpecificEnthalpy * Out.Streams[i].Bulk.TotalMolarflow) + Duty.Streams[0].Q) / 1e6), "Heat Balance");
                }
            }
            else
            {
                AddEquationToEquationSystem(problem,
              ((Sym.Sum(0, In.NumberOfStreams, (i) => In.Streams[i].Bulk.SpecificEnthalpy * In.Streams[i].Bulk.TotalMolarflow + Q) / 1e4))
              - (Sym.Par(Sym.Sum(0, Out.NumberOfStreams, (i) => Out.Streams[i].Bulk.SpecificEnthalpy * Out.Streams[i].Bulk.TotalMolarflow)) / 1e4), "Heat Balance");
            }
            base.CreateEquations(problem);

        }

        public override ProcessUnit EnableChemistry(Chemistry chem)
        {
            base.EnableChemistry(chem);

            if (ChemistryBlock != null)
            {
                r = new Variable[ChemistryBlock.Reactions.Count];
                for (int i = 0; i < ChemistryBlock.Reactions.Count; i++)
                {
                    r[i] = System.VariableFactory.CreateVariable("r", "Reacting molar flow", PhysicalDimension.MolarFlow);
                    r[i].Subscript = (i + 1).ToString();
                    r[i].LowerBound = -1e6;
                }
                AddVariables(r);
            }

            return this;
        }

        public override ProcessUnit Initialize()
        {
            var In = FindMaterialPort("In");
            var Out = FindMaterialPort("Out");
            var Duty = FindHeatPort("Duty");
            int NC = System.Components.Count;


            if (!p.IsFixed)
                p.SetValue(In.Streams[0].Pressure.Val());

            for (int i = 0; i < NC; i++)
            {
                Out.Streams[0].Bulk.ComponentMolarflow[i].SetValue(In.Streams.Sum(s => s.Bulk.ComponentMolarflow[i].Val()));
            }


            if (ChemistryBlock != null)
            {
                foreach (var reac in ChemistryBlock.Reactions)
                {
                    foreach (var comp in reac.Stoichiometry)
                    {
                        if (Math.Abs(Out.Streams[0].Bulk.ComponentMolarflow[comp.Index].Val()) < 1e-10 && Math.Abs(comp.StoichiometricFactor) > 1e-6)
                            Out.Streams[0].Bulk.ComponentMolarflow[comp.Index].SetValue(1e-6);
                    }
                }
            }

            Out.Streams[0].InitializeFromMolarFlows();
            if (T.IsFixed)
                Out.Streams[0].Temperature.SetValue(T.Val());
            else
                Out.Streams[0].Temperature.SetValue(In.Streams[0].Temperature.Val());

            Out.Streams[0].Pressure.SetValue(p.Val() - dp.Val());
            Out.Streams[0].VaporFraction.SetValue(In.Streams[0].VaporFraction.Val());

            if (T.IsFixed)
                Out.Streams[0].FlashPT();

            if (VF.IsFixed)
            {
                Out.Streams[0].VaporFraction.SetValue(VF.Val());
                Out.Streams[0].FlashPZ();

                Out.Streams[0].VaporFraction.Fix(VF.Val());

                if (VF.Val() == 0)
                    Out.Streams[0].State = PhaseState.BubblePoint;
                if (VF.Val() == 1)
                    Out.Streams[0].State = PhaseState.DewPoint;

                T.SetValue(Out.Streams[0].Temperature.Val());
            }


            if (Q.IsFixed)
            {
                //   flash.CalculateTP(Out.Streams[0]);
                //  flash.CalculatePQ(Out.Streams[0], In.Streams[0].Mixed.SpecificEnthalpy.ValueInSI * In.Streams[0].Mixed.TotalMolarflow.ValueInSI);
            }
            else
            {
                if (Duty.IsConnected)
                {
                 /*   if (Duty.Direction == PortDirection.In)
                        Duty.Streams[0].Q.ValueInSI = -(In.Streams[0].Mixed.SpecificEnthalpy * In.Streams[0].Mixed.TotalMolarflow - Out.Streams[0].Mixed.SpecificEnthalpy * Out.Streams[0].Mixed.TotalMolarflow).Eval(eval);
                    else
                        Duty.Streams[0].Q.ValueInSI = (In.Streams[0].Mixed.SpecificEnthalpy * In.Streams[0].Mixed.TotalMolarflow - Out.Streams[0].Mixed.SpecificEnthalpy * Out.Streams[0].Mixed.TotalMolarflow).Eval(eval);
*/

                }
                else
                {
                    Q.SetValue(-(In.Streams[0].Bulk.SpecificEnthalpy.Val() * In.Streams[0].Bulk.TotalMolarflow.Val() - Out.Streams[0].Bulk.SpecificEnthalpy.Val() * Out.Streams[0].Bulk.TotalMolarflow.Val()));
                }
            }


            return this;
        }
    }
}
