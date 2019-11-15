﻿using MiniSim.Core.Expressions;
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
    public class Flash : ProcessUnit
    {
        private Variable dp;
        private Variable p;
        private Variable T;
        private Variable Q;
        private Variable VF;
        private Variable[] K;

        public Flash(string name, ThermodynamicSystem system) : base(name, system)
        {
            Class = "Flash";


            MaterialPorts.Add(new Port<MaterialStream>("In", PortDirection.In, 1));
            MaterialPorts.Add(new Port<MaterialStream>("Vap", PortDirection.Out, 1));
            MaterialPorts.Add(new Port<MaterialStream>("Liq", PortDirection.Out, 1));

            dp = system.VariableFactory.CreateVariable("DP", "Pressure Drop", PhysicalDimension.Pressure);
            p = system.VariableFactory.CreateVariable("P", "Pressure in flash", PhysicalDimension.Pressure);
            T = system.VariableFactory.CreateVariable("T", "Temperature in flash", PhysicalDimension.Temperature);
            Q = system.VariableFactory.CreateVariable("Q", "Heat Duty", PhysicalDimension.HeatFlow);
            VF = system.VariableFactory.CreateVariable("VF", "Vapor Fraction", PhysicalDimension.MolarFraction);

            K = new Variable[system.Components.Count];

            for (int i = 0; i < system.Components.Count; i++)
            {
                K[i] = system.VariableFactory.CreateVariable("K", "Equilibrium partition coefficient", PhysicalDimension.Dimensionless);
                K[i].Subscript = system.Components[i].ID;
                K[i].SetValue(1.2);
                K[i].UpperBound = 1e6;

            }
            dp.LowerBound = -1e10;
            dp.SetValue(0);

            AddVariable(p);
            AddVariable(T);
            AddVariable(Q);
            AddVariable(VF);
            AddVariable(dp);
            AddVariables(K);

        }

        public override void CreateEquations(AlgebraicSystem problem)
        {
            foreach (var vari in Variables)
            {                
                vari.Children.Clear();
            }

            int NC = System.Components.Count;
            var In = FindMaterialPort("In");
            var Vap = FindMaterialPort("Vap");
            var Liq = FindMaterialPort("Liq");

          //  Vap.Streams[0].State = PhaseState.DewPoint;
          //  Liq.Streams[0].State = PhaseState.BubblePoint;

            for (int i = 0; i < NC; i++)
            {
                var cindex = i;
                AddEquationToEquationSystem(problem,
                   (In.Streams[0].Bulk.ComponentMolarflow[cindex])
                        - (Vap.Streams[0].Bulk.ComponentMolarflow[cindex] + Liq.Streams[0].Bulk.ComponentMolarflow[cindex]), "Mass Balance");


            }


            AddEquationToEquationSystem(problem, (p / 1e4) - (Sym.Par(In.Streams[0].Pressure - dp) / 1e4), "Pressure drop");

            AddEquationToEquationSystem(problem, (Vap.Streams[0].Pressure / 1e4) - (Sym.Par(p) / 1e4), "Pressure Balance");
            AddEquationToEquationSystem(problem, (Vap.Streams[0].Temperature / 1e3) - (T / 1e3), "Temperature Balance");
            AddEquationToEquationSystem(problem, (Liq.Streams[0].Pressure / 1e4) - (Sym.Par(p) / 1e4), "Pressure Balance");
            AddEquationToEquationSystem(problem, (Liq.Streams[0].Temperature / 1e3) - (T / 1e3), "Temperature Balance");


            AddEquationToEquationSystem(problem, (In.Streams[0].Bulk.TotalMolarflow * VF) - (Vap.Streams[0].Bulk.TotalMolarflow), "Mass Balance");

            AddEquationToEquationSystem(problem,
    ((Sym.Sum(0, In.NumberOfStreams, (i) => In.Streams[i].Bulk.SpecificEnthalpy * In.Streams[i].Bulk.TotalMolarflow + Q) / 1e4))
    - (Sym.Par(Vap.Streams[0].Bulk.SpecificEnthalpy * Vap.Streams[0].Bulk.TotalMolarflow + Liq.Streams[0].Bulk.SpecificEnthalpy * Liq.Streams[0].Bulk.TotalMolarflow) / 1e4), "Heat Balance");


            for (int i = 0; i < NC; i++)
            {
                System.EquationFactory.EquilibriumCoefficient(System, K[i], T, p, Liq.Streams[0].Bulk.ComponentMolarFraction, Vap.Streams[0].Bulk.ComponentMolarFraction, i);
                AddEquationToEquationSystem(problem, (Vap.Streams[0].Bulk.ComponentMolarFraction[i]) - (K[i] * Liq.Streams[0].Bulk.ComponentMolarFraction[i]), "Equilibrium");
            }

            base.CreateEquations(problem);

        }


        public override ProcessUnit Initialize()
        {
            int NC = System.Components.Count;
            var In = FindMaterialPort("In");
            var Vap = FindMaterialPort("Vap");
            var Liq = FindMaterialPort("Liq");


            var flashStream = new MaterialStream("FLASH", System);
            flashStream.CopyFrom(In.Streams[0]);


            if (p.IsFixed)
                flashStream.Specify("P", p.Val());
            else if (dp.IsFixed)
                flashStream.Specify("P", In.Streams[0].Pressure.Val() - dp.Val());

            if (T.IsFixed)
                flashStream.Specify("T", T.Val());
            else
                flashStream.Specify("T", In.Streams[0].Temperature.Val());

            if (VF.IsFixed)
                flashStream.Specify("VF", VF.Val());
            else
                flashStream.Specify("VF", In.Streams[0].VaporFraction.Val());

            if (T.IsFixed && (p.IsFixed || dp.IsFixed))
                flashStream.FlashPT();
            else if (VF.IsFixed && (p.IsFixed || dp.IsFixed))
                flashStream.FlashPZ();
            else if (Q.IsFixed && (p.IsFixed || dp.IsFixed))
            {
                flashStream.Init("VF", In.Streams[0].GetVariable("VF").Val());
                flashStream.Init("T", In.Streams[0].GetVariable("T").Val());
                //flash.CalculatePQ(flashStream, In.Streams[0].Mixed.SpecificEnthalpy.ValueInSI * In.Streams[0].Mixed.TotalMolarflow.ValueInSI);
            }
            else if (Q.IsFixed)
            {
                flashStream.Init("VF", In.Streams[0].GetVariable("VF").Val());
                flashStream.Init("T", In.Streams[0].GetVariable("T").Val());
                //flash.CalculatePQ(flashStream, In.Streams[0].Mixed.SpecificEnthalpy.ValueInSI * In.Streams[0].Mixed.TotalMolarflow.ValueInSI);
            }
            else
            {
                flashStream.FlashPT();
            }

            for (int i = 0; i < NC; i++)
            {
                Vap.Streams[0].Bulk.ComponentMolarflow[i].SetValue(flashStream.Vapor.ComponentMolarflow[i].Val());
                Liq.Streams[0].Bulk.ComponentMolarflow[i].SetValue(flashStream.Liquid.ComponentMolarflow[i].Val());
            }

            if (T.IsFixed)
            {
                Vap.Streams[0].Temperature.SetValue(T.Val());
                Liq.Streams[0].Temperature.SetValue(T.Val());
            }
            else
            {
                Vap.Streams[0].Temperature.SetValue(flashStream.Temperature.Val());
                Liq.Streams[0].Temperature.SetValue(flashStream.Temperature.Val());
                T.SetValue(flashStream.Temperature.Val());
            }

            if (p.IsFixed)
            {
                Vap.Streams[0].Pressure.SetValue(p.Val());
                Liq.Streams[0].Pressure.SetValue(p.Val());
            }
            else
            {
                Vap.Streams[0].Pressure.SetValue(flashStream.Pressure.Val());
                Liq.Streams[0].Pressure.SetValue(flashStream.Pressure.Val());
                p.SetValue(flashStream.Pressure.Val());
            }

            if (!Q.IsFixed)
                Q.SetValue(-(In.Streams[0].Bulk.SpecificEnthalpy.Val() * In.Streams[0].Bulk.TotalMolarflow.Val() - Liq.Streams[0].Bulk.SpecificEnthalpy.Val() * Liq.Streams[0].Bulk.TotalMolarflow.Val() - Vap.Streams[0].Bulk.SpecificEnthalpy.Val() * Vap.Streams[0].Bulk.TotalMolarflow.Val()));
            if (!VF.IsFixed)
                VF.SetValue(flashStream.VaporFraction.Val());

            Vap.Streams[0].InitializeFromMolarFlows();
            Liq.Streams[0].InitializeFromMolarFlows();



            Vap.Streams[0].GetVariable("VF").SetValue(1);
            Liq.Streams[0].GetVariable("VF").SetValue(0);

            Vap.Streams[0].FlashPZ();
            Liq.Streams[0].FlashPZ();

            Vap.Streams[0].State = PhaseState.DewPoint;
            Liq.Streams[0].State = PhaseState.BubblePoint;

            return this;
        }
    }
}
