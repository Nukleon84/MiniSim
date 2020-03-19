using MiniSim.Core.Expressions;
using MiniSim.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Thermodynamics
{
    public class PureEnthalpyFunction
    {
        public Substance Component { get; set; }
        public Variable Tref { get; set; }
        public Variable Href { get; set; }
        public Variable TPhaseChange { get; set; }
        public bool PhaseChangeAtSystemTemperature { get; set; }
        public ReferencePhase ReferenceState { get; set; }

        public static PureEnthalpyFunction Create(ThermodynamicSystem sys, Substance comp)
        {
            var func = new PureEnthalpyFunction();
            func.Component = comp;
            func.Tref = sys.VariableFactory.CreateVariable("Tref", "Reference temperature for enthalpy calculation", PhysicalDimension.Temperature);
            func.Href = sys.VariableFactory.CreateVariable("Href", "Reference enthalpy for enthalpy calculation", PhysicalDimension.SpecificMolarEnthalpy);
            func.PhaseChangeAtSystemTemperature = true;
            func.TPhaseChange = sys.VariableFactory.CreateVariable("TPC", "Temperature of phase change for enthalpy calculation", PhysicalDimension.Temperature);
            func.ReferenceState = ReferencePhase.Vapor;
            return func;
        }
    }

    public class EnthalpyCalculationMethod
    {
        ExcessEnthalpyMethod _excessEnthalpyMethod = ExcessEnthalpyMethod.Ideal;
        string _name = "ENTH";
        List<PureEnthalpyFunction> _pureComponentEnthalpies = new List<PureEnthalpyFunction>();

        public ExcessEnthalpyMethod ExcessEnthalpyMethod
        {
            get
            {
                return _excessEnthalpyMethod;
            }

            set
            {
                _excessEnthalpyMethod = value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }

        public List<PureEnthalpyFunction> PureComponentEnthalpies
        {
            get
            {
                return _pureComponentEnthalpies;
            }

            set
            {
                _pureComponentEnthalpies = value;
            }
        }
    }

}
