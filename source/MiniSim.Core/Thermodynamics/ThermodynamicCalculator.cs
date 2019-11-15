using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniSim.Core.Expressions;

namespace MiniSim.Core.Thermodynamics
{
    public class ThermodynamicCalculator
    {
        ThermodynamicSystem _system;

        Dictionary<Substance, Dictionary<EvaluatedProperties, Tuple<Variable, Variable>>> _pureTDependentFunctions;

        Dictionary<Substance, Dictionary<PhaseState, Tuple<Variable, Variable>>> _enthalpyFunctions;

        public ThermodynamicCalculator(ThermodynamicSystem system)
        {
            _system = system;

            _pureTDependentFunctions = new Dictionary<Substance, Dictionary<EvaluatedProperties, Tuple<Variable, Variable>>>();
            _enthalpyFunctions = new Dictionary<Substance, Dictionary<PhaseState, Tuple<Variable, Variable>>>();

            foreach (var comp in _system.Components)
            {
                var functionDict = new Dictionary<EvaluatedProperties, Tuple<Variable, Variable>>();

                var T = system.VariableFactory.CreateVariable("T", "Temperature", UnitsOfMeasure.PhysicalDimension.Temperature);
                functionDict.Add(EvaluatedProperties.VaporPressure, new Tuple<Variable, Variable>(T, system.EquationFactory.GetVaporPressureExpression(system, comp, T)));
                functionDict.Add(EvaluatedProperties.HeatOfVaporization, new Tuple<Variable, Variable>(T, system.EquationFactory.GetEnthalpyOfVaporizationExpression(system, comp, T)));
                functionDict.Add(EvaluatedProperties.IdealGasHeatCapacity, new Tuple<Variable, Variable>(T, system.EquationFactory.GetIdealGasHeatCapacityExpression(system, comp, T)));
                functionDict.Add(EvaluatedProperties.LiquidHeatCapacity, new Tuple<Variable, Variable>(T, system.EquationFactory.GetLiquidHeatCapacityExpression(system, comp, T)));
                functionDict.Add(EvaluatedProperties.VaporViscosity, new Tuple<Variable, Variable>(T, system.EquationFactory.GetVaporViscosityExpression(system, comp, T)));
                functionDict.Add(EvaluatedProperties.LiquidViscosity, new Tuple<Variable, Variable>(T, system.EquationFactory.GetLiquidViscosityExpression(system, comp, T)));
                functionDict.Add(EvaluatedProperties.LiquidDensity, new Tuple<Variable, Variable>(T, system.EquationFactory.GetLiquidDensityExpression(system, comp, T)));
                _pureTDependentFunctions.Add(comp, functionDict);

                var enthalpyFunctionDict = new Dictionary<PhaseState, Tuple<Variable, Variable>>();

                enthalpyFunctionDict.Add(PhaseState.Liquid, new Tuple<Variable, Variable>(T, system.EquationFactory.GetLiquidEnthalpyExpression(system, comp, T)));
                enthalpyFunctionDict.Add(PhaseState.Vapor, new Tuple<Variable, Variable>(T, system.EquationFactory.GetVaporEnthalpyExpression(system, comp, T)));

                _enthalpyFunctions.Add(comp, enthalpyFunctionDict);
            }
        }


        public double GetEnthalpy(string substanceId, string phase, double T)
        {
            var substance = _system.GetComponentById(substanceId);

            Dictionary<PhaseState, Tuple<Variable, Variable>> map;
            if (_enthalpyFunctions.TryGetValue(substance, out map))
            {
                var pair = phase.Substring(0,1).ToLower() == "v" ? map[PhaseState.Vapor] : map[PhaseState.Liquid];
                pair.Item1.SetValue(T);
                pair.Item2.Reset();
                var val = pair.Item2.Val();
                return val;
            }
            return Double.NaN;
        }


        public double GetPureComponentProperty(string substanceId, string propertyId, double T)
        {
            var substance = _system.GetComponentById(substanceId);
            var property = _system.CorrelationFactory.GetPropertyForName(propertyId);
            return GetPureComponentProperty(substance, property, T);
        }

        public double GetPureComponentProperty(Substance substance, EvaluatedProperties property, double T)
        {
            Dictionary<EvaluatedProperties, Tuple<Variable, Variable>> map;
            if (_pureTDependentFunctions.TryGetValue(substance, out map))
            {
                Tuple<Variable, Variable> pair;
                if (map.TryGetValue(property, out pair))
                {
                    pair.Item1.SetValue(T);
                    pair.Item2.Reset();
                    var val = pair.Item2.Val();
                    return val;
                }
            }
            return Double.NaN;
        }
    }
}
