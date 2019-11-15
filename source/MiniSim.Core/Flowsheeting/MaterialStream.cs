﻿using MiniSim.Core.Expressions;
using MiniSim.Core.Numerics;
using MiniSim.Core.Thermodynamics;
using MiniSim.Core.Thermodynamics.Routines;
using MiniSim.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Flowsheeting
{
    public class MaterialStream : BaseStream
    {
        #region Fields
        Phase _bulk;
        Phase _liquid;
        Phase _vapor;
        List<Phase> _phases;
        Variable[] K;
        Variable vf;
        Variable vfm;
        PhaseState _state = PhaseState.LiquidVapor;

        Expression rachfordRice;

        private Variable _temperature;
        private Variable _pressure;
        #endregion

        #region Properties
        public Variable Pressure
        {
            get
            {
                return _pressure;
            }

            set
            {
                _pressure = value;
            }
        }

        public Variable Temperature
        {
            get
            {
                return _temperature;
            }

            set
            {
                _temperature = value;
            }
        }

        public Phase Bulk { get => _bulk; set => _bulk = value; }
        public PhaseState State { get => _state; set => _state = value; }
        public Phase Liquid { get => _liquid; set => _liquid = value; }
        public Phase Vapor { get => _vapor; set => _vapor = value; }
        public Variable[] KValues { get => K; set => K = value; }
        public Variable VaporFraction { get => vf; set => vf = value; }
        #endregion

        public MaterialStream(string name, ThermodynamicSystem system) : base(name, system)
        {
            Class = "MaterialStream";

            Bulk = new Phase("", system);
            Liquid = new Phase("L", system);
            Vapor = new Phase("V", system);

            Temperature = System.VariableFactory.CreateVariable("T", "Temperature", PhysicalDimension.Temperature);
            Pressure = System.VariableFactory.CreateVariable("P", "Pressure", PhysicalDimension.Pressure);

            VaporFraction = System.VariableFactory.CreateVariable("VF", "Vapor fraction (molar)", PhysicalDimension.MolarFraction);
            vfm = System.VariableFactory.CreateVariable("VFM", "Vapor fraction (mass)", PhysicalDimension.MassFraction);

            VaporFraction.LowerBound = -0.1;
            VaporFraction.UpperBound = 1.1;

            foreach (var vari in Bulk.Variables)
                vari.Group = "Bulk";

            AddVariables(Temperature, Pressure, VaporFraction, vfm);

            _phases = new List<Phase> { Bulk, Liquid, Vapor };
            foreach (var phase in _phases)
            {
                AddVariables(phase.TotalMolarflow);
                AddVariables(phase.TotalMassflow);
                AddVariables(phase.ComponentMassflow);
                AddVariables(phase.ComponentMassFraction);
                AddVariables(phase.ComponentMolarflow);
                AddVariables(phase.ComponentMolarFraction);
                AddVariables(phase.SpecificEnthalpy);
            }

            Liquid.SpecificEnthalpy.BindTo(new EnthalpyRoute(System, Temperature, Pressure, Liquid.ComponentMolarFraction, PhaseState.Liquid));
            Vapor.SpecificEnthalpy.BindTo(new EnthalpyRoute(System, Temperature, Pressure, Vapor.ComponentMolarFraction, PhaseState.Vapor));

            KValues = new Variable[System.Components.Count];

            for (int i = 0; i < System.Components.Count; i++)
            {
                KValues[i] = system.VariableFactory.CreateVariable("K", "Equilibrium distribution coefficient", PhysicalDimension.Dimensionless);
                KValues[i].Subscript = System.Components[i].ID;
                KValues[i].Group = "Equilibrium";
                KValues[i].LowerBound = 1e-14;
                KValues[i].UpperBound = 1e6;

                System.EquationFactory.EquilibriumCoefficient(System, KValues[i], Temperature, Pressure, Liquid.ComponentMolarFraction, Vapor.ComponentMolarFraction, i);
            }
            AddVariables(KValues);

            int NC = System.Components.Count;

            var x = Bulk.ComponentMolarFraction;
            rachfordRice = Sym.Sum(0, NC, i => x[i] * (1 - K[i]) / (1 + VaporFraction * (K[i] - 1)));


        }

        public PhaseState UpdatePhaseState()
        {
            if (VaporFraction.IsFixed)
            {
                if (VaporFraction.Val() == 0)
                    return PhaseState.BubblePoint;
                if (VaporFraction.Val() == 1)
                    return PhaseState.DewPoint;
                else
                    return PhaseState.LiquidVapor;
            }

            var oldVF = VaporFraction.Val();

            rachfordRice.Reset();
            rachfordRice.GradientValue = 1;
            VaporFraction.SetValue(0);
            var rrAt0 = rachfordRice.Val();
            rachfordRice.Reset();
            rachfordRice.GradientValue = 1;
            VaporFraction.SetValue(1);
            var rrAt1 = rachfordRice.Val();
            rachfordRice.Reset();
            var eps = -1e8;

            if (rrAt0 > 0 && rrAt1 > 0)
            {
                if (State == PhaseState.LiquidVapor || State == PhaseState.BubblePoint)
                    State = PhaseState.Liquid;
                if (State == PhaseState.Vapor || State == PhaseState.DewPoint)
                    State = PhaseState.LiquidVapor;
            }
            if (rrAt0 > 0 && Math.Abs(rrAt1) < eps)
            {
                State = PhaseState.BubblePoint;
            }

            if (rrAt0 < 0 && rrAt1 > 0)
            {
                State = PhaseState.LiquidVapor;
            }

            if (Math.Abs(rrAt0) < eps && rrAt1 < 0)
            {
                State = PhaseState.DewPoint;
            }

            if (rrAt0 < 0 && rrAt1 < 0)
            {
                if (State == PhaseState.LiquidVapor || State == PhaseState.DewPoint)
                    State = PhaseState.Vapor;
                if (State == PhaseState.Liquid || State == PhaseState.BubblePoint)
                    State = PhaseState.LiquidVapor;
            }

            rachfordRice.GradientValue = 0;
            VaporFraction.SetValue(oldVF);
            return State;
        }


        public MaterialStream Flash()
        {
            if (Temperature.IsFixed && Pressure.IsFixed)
                FlashPT();
            if (VaporFraction.IsFixed && Pressure.IsFixed)
                FlashPZ();
            return this;
        }

        public MaterialStream FlashPT()
        {
            FlashP(VaporFraction);
            return this;
        }

        public MaterialStream FlashPZ()
        {
            FlashP(Temperature);
            return this;
        }

        void FlashP(Variable var)
        {
            int NC = System.Components.Count;

            // InitializeFromMolarFlows();

            if (var == VaporFraction)
            {
                //Evaluate Rachford-Rice at the edge cases of the VLE area
                var oldVF = VaporFraction.Val();

                rachfordRice.Reset();
                VaporFraction.SetValue(0);
                var rrAt0 = rachfordRice.Val();
                rachfordRice.Reset();
                VaporFraction.SetValue(1);
                var rrAt1 = rachfordRice.Val();
                rachfordRice.Reset();


                if (rrAt0 > 0 && rrAt1 > 0)
                {
                    State = PhaseState.Liquid;
                    VaporFraction.SetValue(0);
                }
                if (rrAt0 > 0 && rrAt1 == 0)
                {
                    State = PhaseState.BubblePoint;
                    VaporFraction.SetValue(0);
                }

                if (rrAt0 < 0 && rrAt1 > 0)
                    State = PhaseState.LiquidVapor;

                if (rrAt0 == 0 && rrAt1 < 0)
                {
                    State = PhaseState.DewPoint;
                    VaporFraction.SetValue(1);
                }

                if (rrAt0 < 0 && rrAt1 < 0)
                {
                    State = PhaseState.Vapor;
                    VaporFraction.SetValue(1);
                }
            }
            else
            {
                if (VaporFraction.Val() == 0)
                    State = PhaseState.BubblePoint;
                else if (VaporFraction.Val() == 1)
                    State = PhaseState.DewPoint;
                else
                    State = PhaseState.LiquidVapor;

            }

            if (State == PhaseState.LiquidVapor || State == PhaseState.BubblePoint || State == PhaseState.DewPoint)
                ScalarNewtonRaphson.Solve(rachfordRice, var, 20);

            Vapor.TotalMolarflow.SetValue(VaporFraction.Val() * Bulk.TotalMolarflow.Val());
            Liquid.TotalMolarflow.SetValue(Bulk.TotalMolarflow.Val() - Vapor.TotalMolarflow.Val());



            rachfordRice.Reset();
            rachfordRice.GradientValue = 0;
            for (int i = 0; i < NC; i++)
            {
                Liquid.ComponentMolarFraction[i].SetValue((Bulk.ComponentMolarFraction[i].Val() / (1 + VaporFraction.Val() * (K[i].Val() - 1))));
                Vapor.ComponentMolarFraction[i].SetValue((Liquid.ComponentMolarFraction[i].Val() * K[i].Val()));

                Liquid.ComponentMolarflow[i].SetValue(Liquid.ComponentMolarFraction[i].Val() * Liquid.TotalMolarflow.Val());
                Vapor.ComponentMolarflow[i].SetValue(Vapor.ComponentMolarFraction[i].Val() * Vapor.TotalMolarflow.Val());
            }

            InitializeMassflows();
            InitializeMassFractions();
            InitializeEnthalpies();
        }

        #region Initialization
        public MaterialStream CopyFrom(MaterialStream original)
        {
            Temperature.SetValue(original.Temperature.Val());
            Pressure.SetValue(original.Pressure.Val());

            for (int i = 0; i < System.Components.Count; i++)
            {
                Bulk.ComponentMolarflow[i].SetValue(original.Bulk.ComponentMolarflow[i].Val());
            }

            InitializeFromMolarFlows();

            return this;

        }


        public MaterialStream InitializeFromMolarFlows()
        {
            int NC = System.Components.Count;

            InitializePhaseMolarflows();
            InitializeMolarFractions();
            InitializeMassflows();
            InitializeMassFractions();
            return this;
        }
        public MaterialStream InitializeFromMassFlows()
        {
            int NC = System.Components.Count;
            for (int i = 0; i < NC; i++)
            {
                Bulk.ComponentMolarflow[i].SetValue(Bulk.ComponentMassflow[i].Val() / Unit.Convert(System.Components[i].MolarWeight.InternalUnit, SI.kg / SI.mol, System.Components[i].MolarWeight.Val()));
            }

            InitializeFromMolarFlows();
            return this;
        }

        public MaterialStream InitializeFromMolarFractions()
        {
            int NC = System.Components.Count;

            var SZ = Bulk.ComponentMolarFraction.Sum(v => v.Val());

            for (int i = 0; i < NC; i++)
            {
                Bulk.ComponentMolarFraction[i].SetValue(Bulk.ComponentMolarFraction[i].Val() / SZ);

                Bulk.ComponentMolarflow[i].SetValue(Bulk.ComponentMolarFraction[i].Val() * Bulk.TotalMolarflow.Val());
            }
            InitializePhaseMolarflows();
            InitializeMolarFractions();
            InitializeMassflows();
            InitializeMassFractions();
            return this;
        }


        void InitializeEnthalpies()
        {
            Vapor.SpecificEnthalpy.Reset();
            Liquid.SpecificEnthalpy.Reset();
            Bulk.SpecificEnthalpy.SetValue((Vapor.TotalMolarflow.Val() * Vapor.SpecificEnthalpy.Val() + Liquid.TotalMolarflow.Val() * Liquid.SpecificEnthalpy.Val()) / Bulk.TotalMolarflow.Val());

        }
        void InitializePhaseMolarflows()
        {
            int NC = System.Components.Count;
            Bulk.TotalMolarflow.SetValue(Bulk.ComponentMolarflow.Sum(v => v.Val()));
            for (int i = 0; i < NC; i++)
            {
                Liquid.ComponentMolarflow[i].SetValue(0.5 * Bulk.ComponentMolarflow[i].Val());
                Vapor.ComponentMolarflow[i].SetValue(0.5 * Bulk.ComponentMolarflow[i].Val());
            }
            Liquid.TotalMolarflow.SetValue(Liquid.ComponentMolarflow.Sum(v => v.Val()));
            Vapor.TotalMolarflow.SetValue(Vapor.ComponentMolarflow.Sum(v => v.Val()));
        }

        void InitializeMassflows()
        {
            int NC = System.Components.Count;
            for (int i = 0; i < NC; i++)
            {
                Bulk.ComponentMassflow[i].SetValue(Bulk.ComponentMolarflow[i].Val() * Unit.Convert(System.Components[i].MolarWeight.InternalUnit, SI.kg / SI.mol, System.Components[i].MolarWeight.Val()));
                Liquid.ComponentMassflow[i].SetValue(Liquid.ComponentMolarflow[i].Val() * Unit.Convert(System.Components[i].MolarWeight.InternalUnit, SI.kg / SI.mol, System.Components[i].MolarWeight.Val()));
                Vapor.ComponentMassflow[i].SetValue(Vapor.ComponentMolarflow[i].Val() * Unit.Convert(System.Components[i].MolarWeight.InternalUnit, SI.kg / SI.mol, System.Components[i].MolarWeight.Val()));
            }
            Bulk.TotalMassflow.SetValue(Bulk.ComponentMassflow.Sum(v => v.Val()));
            Liquid.TotalMassflow.SetValue(Liquid.ComponentMassflow.Sum(v => v.Val()));
            Vapor.TotalMassflow.SetValue(Vapor.ComponentMassflow.Sum(v => v.Val()));
        }

        void InitializeMolarFractions()
        {
            int NC = System.Components.Count;
            for (int i = 0; i < NC; i++)
            {
                Bulk.ComponentMolarFraction[i].SetValue(Bulk.ComponentMolarflow[i].Val() / Bulk.TotalMolarflow.Val());
                Liquid.ComponentMolarFraction[i].SetValue(Liquid.ComponentMolarflow[i].Val() / Liquid.TotalMolarflow.Val());
                Vapor.ComponentMolarFraction[i].SetValue(Vapor.ComponentMolarflow[i].Val() / Vapor.TotalMolarflow.Val());
            }
        }

        void InitializeMassFractions()
        {
            int NC = System.Components.Count;
            for (int i = 0; i < NC; i++)
            {

                Bulk.ComponentMassFraction[i].SetValue(Bulk.ComponentMassflow[i].Val() / Bulk.TotalMassflow.Val());

                if (Liquid.TotalMassflow.Val() > 0)
                    Liquid.ComponentMassFraction[i].SetValue(Liquid.ComponentMassflow[i].Val() / Liquid.TotalMassflow.Val());
                else
                    Liquid.ComponentMassFraction[i].SetValue(0);
                if (Vapor.TotalMassflow.Val() > 0)
                    Vapor.ComponentMassFraction[i].SetValue(Vapor.ComponentMassflow[i].Val() / Vapor.TotalMassflow.Val());
                else
                    Vapor.ComponentMassFraction[i].SetValue(0);

            }
        }

        #endregion

        public override void CreateEquations(AlgebraicSystem problem)
        {
            int NC = System.Components.Count;

            //Binding Equations for calculated properties: These properties can not be FIXed. They can only be SPECed

            Liquid.SpecificEnthalpy.BindTo(new EnthalpyRoute(System, Temperature, Pressure, Liquid.ComponentMolarFraction, PhaseState.Liquid));
            Vapor.SpecificEnthalpy.BindTo(new EnthalpyRoute(System, Temperature, Pressure, Vapor.ComponentMolarFraction, PhaseState.Vapor));
            vfm.BindTo(Vapor.TotalMassflow / Sym.Max(1e-14, Bulk.TotalMassflow));
            Vapor.TotalMolarflow.BindTo(Bulk.TotalMolarflow - Liquid.TotalMolarflow);

            for (int i = 0; i < System.Components.Count; i++)
            {
                System.EquationFactory.EquilibriumCoefficient(System, KValues[i], Temperature, Pressure, Liquid.ComponentMolarFraction, Vapor.ComponentMolarFraction, i);
            }

            foreach (var phase in _phases)
            {
                phase.TotalMassflow.BindTo(Sym.Sum(phase.ComponentMassflow));
                for (int i = 0; i < NC; i++)
                {
                    //phase.ComponentMolarflow[i].BindTo(phase.ComponentMolarFraction[i] * phase.TotalMolarflow);                   
                    phase.ComponentMassFraction[i].BindTo(phase.ComponentMassflow[i] / Sym.Max(1e-14, phase.TotalMassflow));
                    phase.ComponentMassflow[i].BindTo(phase.ComponentMolarflow[i] * Sym.Convert(System.Components[i].MolarWeight, System.VariableFactory.Internal.UnitDictionary[PhysicalDimension.MolarWeight]));
                }
            }

            //Equations that are solved in the iteration            
            foreach (var phase in _phases)
            {
                for (int i = 0; i < NC; i++)
                {
                    AddEquationToEquationSystem(problem, phase.ComponentMolarflow[i] - phase.ComponentMolarFraction[i] * phase.TotalMolarflow);
                }
            }
            AddEquationToEquationSystem(problem, (Sym.Par(VaporFraction * Vapor.SpecificEnthalpy + Sym.Par(1 - VaporFraction) * Liquid.SpecificEnthalpy)/1e3 - Bulk.SpecificEnthalpy/1e3), "Enthalpy Balance");
            AddEquationToEquationSystem(problem, (Sym.Par(1 - VaporFraction) * Bulk.TotalMolarflow - Liquid.TotalMolarflow), "Mole Balance");

            var x = Liquid.ComponentMolarFraction;
            var z = Bulk.ComponentMolarFraction;
            var y = Vapor.ComponentMolarFraction;
            var alpha = VaporFraction;
            for (int i = 0; i < NC; i++)
            {
                /*  foreach (var phase in _phases)
                  {
                      AddEquationToEquationSystem(problem, phase.ComponentMolarflow[i] - (phase.ComponentMolarFraction[i] * phase.TotalMolarflow));
                  }*/

                //  if (System.EquilibriumMethod.EquilibriumApproach == EquilibriumApproach.GammaPhi && System.EquilibriumMethod.Activity == ActivityMethod.Ideal)
                {
                    AddEquationToEquationSystem(problem, z[i] - Sym.Par(1 + alpha * (K[i] - 1))* x[i], "Equilibrium");
                    AddEquationToEquationSystem(problem, K[i] * x[i] - y[i], "Equilibrium");
                }
                /*  else
                  {
                      AddEquationToEquationSystem(problem, z[i] - x[i] * (1 + alpha * (K[i] - 1)), "Equilibrium");
                      AddEquationToEquationSystem(problem, K[i] * z[i] - y[i] * (1 + alpha * (K[i] - 1)), "Equilibrium");
                  }*/
            }

            //Sumz=1, only when not all z are fixed
            if (!Bulk.ComponentMolarFraction.All(c => c.IsFixed))
                AddEquationToEquationSystem(problem, Sym.Sum(Bulk.ComponentMolarFraction) - 1, "Mole Balance");
            //Sumx-sumy=0
            AddEquationToEquationSystem(problem, new VLEFlashEquation(this), "Equilibrium");

            base.CreateEquations(problem);
        }
    }
}
