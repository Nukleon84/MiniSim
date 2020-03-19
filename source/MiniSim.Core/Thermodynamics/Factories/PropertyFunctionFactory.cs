using MiniSim.Core.Expressions;
using MiniSim.Core.Thermodynamics.Routines;
using MiniSim.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Thermodynamics
{
    public class PropertyFunctionFactory
    {


        public Variable ActivityCoefficient(ThermodynamicSystem system, Variable g, Variable T, List<Variable> x, int index)
        {
            Expression liquidPart = null;

            switch (system.EquilibriumMethod.EquilibriumApproach)
            {
                case EquilibriumApproach.GammaPhi:
                    switch (system.EquilibriumMethod.Activity)
                    {
                        case ActivityMethod.UNIQUAC:
                            {
                                // var gamma = new ActivityCoefficientUNIQUAC(system, T, x, index);
                                // liquidPart = gamma;
                                break;
                            }
                        case ActivityMethod.NRTL:
                            {
                                var gamma = new ActivityCoefficientNRTL(system, T, x, index);
                                liquidPart = gamma;
                                break;
                            }
                        case ActivityMethod.Wilson:
                            {
                                //var gamma = new ActivityCoefficientWilson(system, T, x, index);
                                //liquidPart = gamma;
                                break;
                            }
                        default:
                            liquidPart = 1.0;
                            break;
                    }

                    break;
                case EquilibriumApproach.PhiPhi:
                    {
                        switch (system.EquilibriumMethod.EquationOfState)
                        {
                            case EquationOfState.SoaveRedlichKwong:
                                liquidPart = 1.0;
                                break;

                            default:
                                throw new NotSupportedException("Only SoaveRedlichKwong allowed");
                        }
                        break;
                    }
            }
            g.Subscript = system.Components[index].ID;
            g.BindTo(liquidPart);
            return g;
        }

        public Variable EquilibriumCoefficientLLE(ThermodynamicSystem system, Variable K, Variable T, Variable p, List<Variable> x1, List<Variable> x2, int index)
        {
            Expression liquid1Part = null;
            Expression liquid2Part = null;
            var currentComponent = system.Components[index];

            if (String.IsNullOrEmpty(K.Subscript))
                K.Subscript = system.Components[index].ID;

            switch (system.EquilibriumMethod.EquilibriumApproach)
            {
                case EquilibriumApproach.GammaPhi:
                    switch (system.EquilibriumMethod.Activity)
                    {
                        case ActivityMethod.UNIQUAC:
                            {
                                // liquid1Part = new ActivityCoefficientUNIQUAC(system, T, x1, index);
                                //  liquid2Part = new ActivityCoefficientUNIQUAC(system, T, x2, index);
                                K.BindTo(liquid2Part / liquid1Part);
                                break;
                            }
                        case ActivityMethod.NRTL:
                            {
                                liquid1Part = new ActivityCoefficientNRTL(system, T, x1, index);
                                liquid2Part = new ActivityCoefficientNRTL(system, T, x2, index);
                                K.BindTo(liquid2Part / liquid1Part);
                                break;
                            }
                        case ActivityMethod.Wilson:
                            throw new NotSupportedException("Cannot calculate LLE with Wilson Activity Coefficient Model");
                        default:
                            throw new NotSupportedException("Cannot calculate LLE without Activity Coefficient Model");
                    }

                    break;
                case EquilibriumApproach.PhiPhi:
                    {
                        throw new NotSupportedException("Only Activity Coefficient Models allowed for LLE");
                    }
            }
            return K;
        }

        public Variable EquilibriumCoefficient(ThermodynamicSystem system, Variable K, Variable T, Variable p, List<Variable> x, List<Variable> y, int index)
        {
            Expression liquidPart = null;
            Expression vaporPart = p;

            var currentComponent = system.Components[index];

            if (String.IsNullOrEmpty(K.Subscript))
                K.Subscript = system.Components[index].ID;

            switch (system.EquilibriumMethod.EquilibriumApproach)
            {
                case EquilibriumApproach.GammaPhi:
                    switch (system.EquilibriumMethod.Activity)
                    {
                        case ActivityMethod.UNIQUAC:
                            {
                                /*   var gamma = new ActivityCoefficientUNIQUAC(system, T, x, index);
                                   if (currentComponent.IsInert)
                                       liquidPart = new MixtureHenryCoefficient(system, T, x, index);
                                   else
                                       liquidPart = gamma * GetVaporPressure(system, currentComponent, T);
                                   K.BindTo(liquidPart / vaporPart);*/
                                break;
                            }
                        case ActivityMethod.NRTL:
                            {
                                var gamma = new ActivityCoefficientNRTL(system, T, x, index);

                                /*if (currentComponent.IsInert)
                                    liquidPart = new MixtureHenryCoefficient(system, T, x, index);
                                else*/
                                liquidPart = gamma * GetVaporPressureExpression(system, currentComponent, T);
                                K.BindTo(liquidPart / vaporPart);
                                break;
                            }
                        case ActivityMethod.Wilson:
                            {
                                //    var gamma = new ActivityCoefficientWilson(system, T, x, index);
                                //     liquidPart = gamma * GetVaporPressure(system, currentComponent, T);
                                //    K.BindTo(liquidPart / vaporPart);
                                break;
                            }
                        default:
                            //liquidPart = GetVaporPressure(system, currentComponent, T);
                            //  if (currentComponent.IsInert)
                            //      liquidPart = new MixtureHenryCoefficient(system, T, x, index);
                            //   else
                            liquidPart = GetVaporPressureExpression(system, currentComponent, T);

                            K.BindTo(liquidPart / vaporPart);
                            break;
                    }

                    break;
                case EquilibriumApproach.PhiPhi:
                    {
                        switch (system.EquilibriumMethod.EquationOfState)
                        {
                            case EquationOfState.SoaveRedlichKwong:
                                //   var eos = new K_EOS_SRK(system, T, p, x, y, index);
                                //   K.BindTo(eos);
                                break;

                            default:
                                throw new NotSupportedException("Only SoaveRedlichKwong allowed");
                        }
                        break;
                    }
            }



            return K;
        }

        public Variable GetAverageVaporDensityExpression(ThermodynamicSystem system, Variable T, Variable p, List<Variable> y)
        {
            var NC = system.Components.Count;


            if ((system.EquilibriumMethod.EquilibriumApproach == EquilibriumApproach.PhiPhi && system.EquilibriumMethod.EquationOfState == EquationOfState.SoaveRedlichKwong) ||
                (system.EquilibriumMethod.EquilibriumApproach == EquilibriumApproach.GammaPhi && system.EquilibriumMethod.Fugacity == FugacityMethod.SoaveRedlichKwong))
            {
                /* var rhoV = new VOL_SRK(system, T, p, y);
                 Variable prop = new Variable("DENV" + "(" + T.FullName + ")", 1);
                 prop.Subscript = "SRK";
                 prop.BindTo(1.0 / rhoV);
                 return prop;*/
                var R = new Variable("R", 8.3144621, SI.J / SI.mol / SI.K);
                var expression = p / (R * T);
                Variable prop = new Variable("DENV" + "(" + T.FullName + ")", 1);
                prop.Subscript = "SRK";
                prop.BindTo(expression);
                return prop;
            }
            else
            {
                var R = new Variable("R", 8.3144621, SI.J / SI.mol / SI.K);
                var expression = p / (R * T);
                Variable prop = new Variable("DENV" + "(" + T.FullName + ")", 1);
                prop.Subscript = "ideal";
                prop.BindTo(expression);
                return prop;
            }


        }

        public Variable GetAverageMolarWeightExpression(ThermodynamicSystem system, Variable[] z)
        {
            var NC = system.Components.Count;
            var molw = Sym.Sum(0, NC, j => Sym.Convert(system.Components[j].MolarWeight, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.MolarWeight]) * z[j]);
            Variable prop = new Variable("MOLW", 1);
            prop.Subscript = "avg";
            prop.BindTo(molw);
            return prop;
        }

        public Variable GetAverageVaporViscosityExpression(ThermodynamicSystem system, Variable[] y, Variable T, Variable p)
        {
            var NC = system.Components.Count;
            var visv = Sym.Sum(0, NC, j => y[j] * Sym.Sqrt(Sym.Convert(system.Components[j].MolarWeight, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.MolarWeight])) *
            GetVaporViscosityExpression(system, system.Components[j], T)) / Sym.Sum(0, NC, j => y[j] * Sym.Sqrt(Sym.Convert(system.Components[j].MolarWeight, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.MolarWeight])));
            Variable prop = new Variable("VISV" + "(" + T.FullName + ")", 1);
            prop.Subscript = "avg";
            prop.BindTo(visv);
            return prop;
        }

        public Variable GetVaporDensityExpression(ThermodynamicSystem system, Substance comp, Variable T, Variable p)
        {
            var R = new Variable("R", 8.3144621, SI.J / SI.mol / SI.K);
            var expression = p / (R * T);
            // expression *= Unit.GetConversionFactor(SI.mol / (SI.m ^ 3), system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.MolarDensity]);
            Variable prop = new Variable("DENV" + "(" + T.FullName + ")", 1);
            prop.Subscript = comp.ID;
            prop.BindTo(expression);
            return prop;
        }

        public Variable GetLiquidDensityExpression(ThermodynamicSystem system, Substance comp, Variable T)
        {
            var func = comp.GetFunction(EvaluatedProperties.LiquidDensity);
            var TC = comp.GetConstant(ConstantProperties.CriticalTemperature);
            var expression = system.CorrelationFactory.CreateExpression(func.Type, func, T, TC, null);
            expression *= Unit.GetConversionFactor(func.YUnit, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.MolarDensity]);

            Variable prop = new Variable(system.CorrelationFactory.GetVariableNameForProperty(func.Property) + "(" + T.FullName + ")", 1);
            prop.LowerBound = 1e-6;
            prop.UpperBound = 1e8;
            prop.Subscript = comp.ID;
            prop.BindTo(expression);
            return prop;

        }

        public Variable GetVaporViscosityExpression(ThermodynamicSystem system, Substance comp, Variable T)
        {
            var func = comp.GetFunction(EvaluatedProperties.VaporViscosity);
            var expression = system.CorrelationFactory.CreateExpression(func.Type, func, T, null, null);
            expression *= Unit.GetConversionFactor(func.YUnit, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.DynamicViscosity]);

            Variable prop = new Variable(system.CorrelationFactory.GetVariableNameForProperty(func.Property) + "(" + T.FullName + ")", 1);
            prop.LowerBound = 0;
            prop.Subscript = comp.ID;
            prop.BindTo(expression);
            return prop;
        }


        public Variable GetLiquidViscosityExpression(ThermodynamicSystem system, Substance comp, Variable T)
        {
            var func = comp.GetFunction(EvaluatedProperties.LiquidViscosity);
            var expression = system.CorrelationFactory.CreateExpression(func.Type, func, T, null, null);
            expression *= Unit.GetConversionFactor(func.YUnit, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.DynamicViscosity]);

            Variable prop = new Variable(system.CorrelationFactory.GetVariableNameForProperty(func.Property) + "(" + T.FullName + ")", 1);
            prop.LowerBound = 0;
            prop.Subscript = comp.ID;
            prop.BindTo(expression);
            return prop;
        }


        public Variable GetVaporPressureExpression(ThermodynamicSystem system, Substance comp, Variable T)
        {
            var func = comp.GetFunction(EvaluatedProperties.VaporPressure);

            var expr = system.CorrelationFactory.CreateExpression(func.Type, func, T, comp.GetConstant(ConstantProperties.CriticalTemperature), comp.GetConstant(ConstantProperties.CriticalPressure));
            expr *= Unit.GetConversionFactor(func.YUnit, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.Pressure]);

            //var exprmax = system.CorrelationFactory.CreateExpression(func.Type, func, comp.GetConstant(ConstantProperties.CriticalTemperature), comp.GetConstant(ConstantProperties.CriticalTemperature), comp.GetConstant(ConstantProperties.CriticalPressure));
            //var maxVal = exprmax.Eval(new Evaluator());


            Variable prop = new Variable(system.CorrelationFactory.GetVariableNameForProperty(func.Property) + "(" + T.FullName + ")", 1);
            prop.Subscript = comp.ID;
            // prop.UpperBound = 1e9;
            prop.BindTo(expr);
            return prop;
        }

        public Variable GetVaporEnthalpyExpression(ThermodynamicSystem sys, Substance comp, Variable T)
        {
            var compIndex = sys.Components.IndexOf(comp);
            return GetVaporEnthalpyExpression(sys, compIndex, T);
        }
        public Variable GetLiquidEnthalpyExpression(ThermodynamicSystem sys, Substance comp, Variable T)
        {
            var compIndex = sys.Components.IndexOf(comp);
            return GetLiquidEnthalpyExpression(sys, compIndex, T);
        }

        public Variable GetVaporEnthalpyExpression(ThermodynamicSystem sys, int idx, Variable T)
        {
            Variable Tref = sys.EnthalpyMethod.PureComponentEnthalpies[idx].Tref;
            Expression expr = null;
            var comp = sys.Components[idx];

            if (sys.EnthalpyMethod.PureComponentEnthalpies[idx].ReferenceState == ReferencePhase.Vapor)
            {
                expr = sys.EnthalpyMethod.PureComponentEnthalpies[idx].Href
                   + Sym.Par(sys.EquationFactory.GetIdealGasHeatCapacityIntegralExpression(sys, comp, T, Tref));
            }
            else
            {
                if (sys.EnthalpyMethod.PureComponentEnthalpies[idx].PhaseChangeAtSystemTemperature)
                {
                    expr = sys.EnthalpyMethod.PureComponentEnthalpies[idx].Href
                        + Sym.Par(sys.EquationFactory.GetLiquidHeatCapacityIntegralExpression(sys, comp, T, Tref))
                        + sys.EquationFactory.GetEnthalpyOfVaporizationExpression(sys, comp, T);
                }
                else
                {
                    expr = sys.EnthalpyMethod.PureComponentEnthalpies[idx].Href
                        + Sym.Par(sys.EquationFactory.GetLiquidHeatCapacityIntegralExpression(sys, comp, sys.EnthalpyMethod.PureComponentEnthalpies[idx].TPhaseChange, Tref))
                        + sys.EquationFactory.GetEnthalpyOfVaporizationExpression(sys, comp, sys.EnthalpyMethod.PureComponentEnthalpies[idx].TPhaseChange)
                        + Sym.Par(sys.EquationFactory.GetIdealGasHeatCapacityIntegralExpression(sys, comp, T, sys.EnthalpyMethod.PureComponentEnthalpies[idx].TPhaseChange));
                }
            }
            Variable prop = new Variable("hV" + "(" + T.FullName + ")", 1);
            prop.Subscript = sys.Components[idx].ID;
            prop.BindTo(Sym.Par(expr));
            return prop;
        }

        public Variable GetLiquidEnthalpyExpression(ThermodynamicSystem sys, int idx, Variable T)
        {
            Variable Tref = sys.EnthalpyMethod.PureComponentEnthalpies[idx].Tref;
            Expression expr = null;
            var comp = sys.Components[idx];

            if (sys.EnthalpyMethod.PureComponentEnthalpies[idx].ReferenceState == ReferencePhase.Liquid)
            {
                expr = sys.EnthalpyMethod.PureComponentEnthalpies[idx].Href
                          + Sym.Par(sys.EquationFactory.GetLiquidHeatCapacityIntegralExpression(sys, comp, T, Tref));
            }
            else
            {
                if (sys.EnthalpyMethod.PureComponentEnthalpies[idx].PhaseChangeAtSystemTemperature)
                {
                    expr = sys.EnthalpyMethod.PureComponentEnthalpies[idx].Href
                        + Sym.Par(sys.EquationFactory.GetIdealGasHeatCapacityIntegralExpression(sys, comp, T, Tref))
                        - sys.EquationFactory.GetEnthalpyOfVaporizationExpression(sys, comp, T);
                }
                else
                {
                    expr = sys.EnthalpyMethod.PureComponentEnthalpies[idx].Href
                        + Sym.Par(sys.EquationFactory.GetIdealGasHeatCapacityIntegralExpression(sys, comp, sys.EnthalpyMethod.PureComponentEnthalpies[idx].TPhaseChange, Tref))
                        - sys.EquationFactory.GetEnthalpyOfVaporizationExpression(sys, comp, sys.EnthalpyMethod.PureComponentEnthalpies[idx].TPhaseChange)
                        + Sym.Par(sys.EquationFactory.GetLiquidHeatCapacityIntegralExpression(sys, comp, T, sys.EnthalpyMethod.PureComponentEnthalpies[idx].TPhaseChange));
                }
            }


            Variable prop = new Variable("hL" + "(" + T.FullName + ")", 1);
            prop.Subscript = sys.Components[idx].ID;
            prop.BindTo(expr);
            return prop;
        }


        public Variable GetEnthalpyOfVaporizationExpression(ThermodynamicSystem system, Substance comp, Variable T)
        {
            var func = comp.GetFunction(EvaluatedProperties.HeatOfVaporization);

            var expr = system.CorrelationFactory.CreateExpression(func.Type, func, T, comp.GetConstant(ConstantProperties.CriticalTemperature), comp.GetConstant(ConstantProperties.CriticalPressure));
            expr *= Unit.GetConversionFactor(func.YUnit, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.SpecificMolarEnthalpy]);

            Variable prop = new Variable(system.CorrelationFactory.GetVariableNameForProperty(func.Property) + "(" + T.FullName + ")", 1);
            prop.LowerBound = 0;
            prop.Subscript = comp.ID;
            prop.BindTo(expr);
            return prop;


        }

        public Variable GetIdealGasHeatCapacityExpression(ThermodynamicSystem system, Substance comp, Variable T)
        {
            var func = comp.GetFunction(EvaluatedProperties.IdealGasHeatCapacity);

            var expr = system.CorrelationFactory.CreateExpression(func.Type, func, T, comp.GetConstant(ConstantProperties.CriticalTemperature), comp.GetConstant(ConstantProperties.CriticalPressure));
            expr *= Unit.GetConversionFactor(func.YUnit, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.HeatCapacity]);

            Variable prop = new Variable(system.CorrelationFactory.GetVariableNameForProperty(func.Property) + "(" + T.FullName + ")", 1);
            prop.Subscript = comp.ID;
            prop.BindTo(expr);
            return prop;


        }

        public Variable GetLiquidHeatCapacityExpression(ThermodynamicSystem system, Substance comp, Variable T)
        {
            var func = comp.GetFunction(EvaluatedProperties.LiquidHeatCapacity);

            var expr = system.CorrelationFactory.CreateExpression(func.Type, func, T, comp.GetConstant(ConstantProperties.CriticalTemperature), comp.GetConstant(ConstantProperties.CriticalPressure));
            expr *= Unit.GetConversionFactor(func.YUnit, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.HeatCapacity]);

            Variable prop = new Variable(system.CorrelationFactory.GetVariableNameForProperty(func.Property) + "(" + T.FullName + ")", 1);
            prop.Subscript = comp.ID;
            prop.BindTo(expr);
            return prop;
        }


        public Expression GetIdealGasHeatCapacityIntegralExpression(ThermodynamicSystem system, Substance comp, Variable T, Variable Tref)
        {
            Expression expression = null;
            var func = comp.GetFunction(EvaluatedProperties.IdealGasHeatCapacity);
            switch (func.Type)
            {
                case FunctionType.Polynomial:
                    expression = system.CorrelationFactory.CreateIntegratedExpression(FunctionType.PolynomialIntegrated, func, T, Tref);
                    expression *= Unit.GetConversionFactor(func.YUnit, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.HeatCapacity]);
                    break;

                case FunctionType.AlyLee:
                    expression = system.CorrelationFactory.CreateIntegratedExpression(FunctionType.Dippr117, func, T, Tref);
                    expression *= Unit.GetConversionFactor(func.YUnit, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.HeatCapacity]);
                    break;
            }
            Variable prop = new Variable(system.CorrelationFactory.GetVariableNameForProperty(func.Property) + "_INT" + "(" + T.FullName + ")", 1);
            prop.Subscript = comp.ID;
            prop.BindTo(expression);
            return prop;


        }
        public Expression GetLiquidHeatCapacityIntegralExpression(ThermodynamicSystem system, Substance comp, Variable T, Variable Tref)
        {
            Expression expression = null;
            var func = comp.GetFunction(EvaluatedProperties.LiquidHeatCapacity);
            switch (func.Type)
            {
                case FunctionType.Polynomial:
                    expression = system.CorrelationFactory.CreateIntegratedExpression(FunctionType.PolynomialIntegrated, func, T, Tref);
                    expression *= Unit.GetConversionFactor(func.YUnit, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.HeatCapacity]);
                    break;

                case FunctionType.AlyLee:
                    expression = system.CorrelationFactory.CreateIntegratedExpression(FunctionType.Dippr117, func, T, Tref);
                    expression *= Unit.GetConversionFactor(func.YUnit, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.HeatCapacity]);
                    break;
            }
            Variable prop = new Variable(system.CorrelationFactory.GetVariableNameForProperty(func.Property) + "_INT" + "(" + T.FullName + ")", 1);
            prop.Subscript = comp.ID;
            prop.BindTo(expression);
            return prop;
        }

    }
}
