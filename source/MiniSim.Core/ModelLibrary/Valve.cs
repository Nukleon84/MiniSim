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
    public enum FlowMode { Compressible, Incompressible, Multiphase };
    public enum ValveCharacteristic
    {
        Linear,
        Parabolic,
        SquareRoot,
        QuickOpening,
        EqualPercentage,
        Hyperbolic,
        User
    };

    public class Valve : ProcessUnit
    {
        private Variable dp;
        private Variable p1;
        private Variable p2;
        private Variable KVS;
        private Variable KV;

        private Variable Position;
        private Variable Opening;


        FlowMode _mode = FlowMode.Incompressible;
        ValveCharacteristic _characteristicCurve = ValveCharacteristic.Linear;

        public FlowMode Mode
        {
            get
            {
                return _mode;
            }

            set
            {
                _mode = value;
            }
        }

        public ValveCharacteristic CharacteristicCurve
        {
            get
            {
                return _characteristicCurve;
            }

            set
            {
                _characteristicCurve = value;
            }
        }

        public Valve(string name, ThermodynamicSystem system) : base(name, system)
        {
            Class = "Valve";
            Icon.IconType = IconTypes.Valve;
            MaterialPorts.Add(new Port<MaterialStream>("In", PortDirection.In, 1));
            MaterialPorts.Add(new Port<MaterialStream>("Out", PortDirection.Out, 1));

            dp = system.VariableFactory.CreateVariable("DP", "Pressure Drop", PhysicalDimension.Pressure);
            p1 = system.VariableFactory.CreateVariable("P1", "Pressure in valve inlet", PhysicalDimension.Pressure);
            p2 = system.VariableFactory.CreateVariable("P2", "Pressure in valve outlet", PhysicalDimension.Pressure);

            KVS = new Variable("KVS", 10, 0, 1e6, SI.cum / SI.h / METRIC.bar, "Nominal valve coefficient");
            KV = new Variable("KV", 10, 0, 1e6, SI.cum / SI.h / METRIC.bar, "Effective valve coefficient");

            Opening = new Variable("Open", 50, 0, 100, SI.none, "Valve opening in %");
            Position = new Variable("Pos", 50, 0, 100, SI.none, "Valve position in %");

            dp.LowerBound = 0;
            dp.SetValue(0);
            AddVariable(dp);
            AddVariable(KVS);
            AddVariable(KV);
            AddVariable(Opening);
            AddVariable(Position);
            AddVariable(p1);
            AddVariable(p2);
        }
               
        public Valve SetCharacteristicCurve(ValveCharacteristic curveType)
        {
            CharacteristicCurve = curveType;
            return this;
        }

        public Valve SetMode(FlowMode mode)
        {
            Mode = mode;
            return this;
        }

        public override void CreateEquations(AlgebraicSystem problem)
        {
            int NC = System.Components.Count;
            var In = FindMaterialPort("In");
            var Out = FindMaterialPort("Out");


            for (int i = 0; i < NC; i++)
            {
                var cindex = i;
                AddEquationToEquationSystem(problem,
                    Sym.Sum(0, In.NumberOfStreams, (j) => In.Streams[j].Bulk.ComponentMolarflow[cindex])
                        - (Sym.Sum(0, Out.NumberOfStreams, (j) => Out.Streams[j].Bulk.ComponentMolarflow[cindex])), "Mass Balance");
            }

            AddEquationToEquationSystem(problem, (p1 / 1e4) - (Sym.Par(In.Streams[0].Pressure) / 1e4), "Pressure Balance");
            AddEquationToEquationSystem(problem, (p2 / 1e4) - (Sym.Par(Out.Streams[0].Pressure) / 1e4), "Pressure Balance");
            AddEquationToEquationSystem(problem, (dp / 1e4) - (Sym.Par(p1 - p2) / 1e4), "Pressure drop");


            if (CharacteristicCurve != ValveCharacteristic.User)
                AddEquationToEquationSystem(problem, (Opening) - (GetCharacteristicCurve()), "Performance");
            AddEquationToEquationSystem(problem, (KV) - (Opening / 100 * KVS), "Performance");

            if (Mode == FlowMode.Compressible)
            {
                AddEquationToEquationSystem(problem, (Sym.Convert(Out.Streams[0].Bulk.TotalVolumeflow, (SI.m ^ 3) / SI.h)) - (KV * Sym.Sqrt(Sym.Convert(dp, METRIC.bar) * Out.Streams[0].Bulk.Density / 999.07)), "Performance");
            }
            else if (Mode == FlowMode.Incompressible)
            {
            }
            else if (Mode == FlowMode.Multiphase)
            {
            }

            //Isenthalpic Valve
            AddEquationToEquationSystem(problem,
          ((Sym.Sum(0, In.NumberOfStreams, (i) => In.Streams[i].Bulk.SpecificEnthalpy * In.Streams[i].Bulk.TotalMolarflow) / 1e4))
          - (Sym.Par(Sym.Sum(0, Out.NumberOfStreams, (i) => Out.Streams[i].Bulk.SpecificEnthalpy * Out.Streams[i].Bulk.TotalMolarflow)) / 1e4), "Heat Balance");

            base.CreateEquations(problem);
        }

        public override ProcessUnit Initialize()
        {
            var In = FindMaterialPort("In");
            var Out = FindMaterialPort("Out");

            int NC = System.Components.Count;

            if (!p2.IsFixed)
                p2.SetValue(In.Streams[0].Pressure.Val() - dp.Val());
            else
                dp.SetValue(p1.Val() - p2.Val());


            for (int i = 0; i < NC; i++)
            {
                Out.Streams[0].Bulk.ComponentMolarflow[i].SetValue(Sym.Sum(0, In.NumberOfStreams, (j) => In.Streams[j].Bulk.ComponentMolarflow[i]).Val());
            }

            Out.Streams[0].Temperature.SetValue(In.Streams[0].Temperature.Val());
            Out.Streams[0].Pressure.SetValue(p2.Val());
            Out.Streams[0].VaporFraction.SetValue(In.Streams[0].VaporFraction.Val());
                       
            Out.Streams[0].FlashPT();
            return this;
        }


        Expression GetCharacteristicCurve()
        {
            switch (CharacteristicCurve)
            {
                case ValveCharacteristic.Linear:
                    return Position;
                case ValveCharacteristic.Parabolic:
                    return 0.01 * Sym.Pow(Position, 2);
                case ValveCharacteristic.SquareRoot:
                    return 10 * Sym.Sqrt(Position);
                case ValveCharacteristic.QuickOpening:
                    return Sym.Par(10 * Position) / (Sym.Sqrt(1.0 + 9.9e-3 * Sym.Pow(Position, 2)));
                case ValveCharacteristic.EqualPercentage:
                    return Sym.Par(0.01 * Sym.Pow(Position, 2)) / (Sym.Sqrt(2.0 - 1e-8 * Sym.Pow(Position, 4)));
                case ValveCharacteristic.Hyperbolic:
                    return Sym.Par(0.1 * Position) / (Sym.Sqrt(1.0 - 9.9e-5 * Sym.Pow(Position, 2)));
                default:
                    return Position;
            }

        }


    }
}
