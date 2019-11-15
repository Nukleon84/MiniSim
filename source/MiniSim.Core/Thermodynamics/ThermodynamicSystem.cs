
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Thermodynamics
{
    public class ThermodynamicSystem
    {
        string _name;
        List<Substance> _components = new List<Substance>();
        CorrelationFactory _correlationFactory = new CorrelationFactory();
        VariableFactory _variableFactory = new VariableFactory();
        EquilibriumCalculationMethod _equilibriumMethod = new EquilibriumCalculationMethod();
        EnthalpyCalculationMethod _enthalpyMethod = new EnthalpyCalculationMethod();
        PropertyFunctionFactory _equationFactory = new PropertyFunctionFactory();

        List<BinaryInteractionParameterSet> _binaryParameters = new List<BinaryInteractionParameterSet>();

        List<Chemistry> _chemistryBlocks = new List<Chemistry>();

        #region Properties
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

        public List<Substance> Components
        {
            get
            {
                return _components;
            }

            set
            {
                _components = value;
            }
        }


        public VariableFactory VariableFactory
        {
            get
            {
                return _variableFactory;
            }

            set
            {
                _variableFactory = value;
            }
        }

        public EquilibriumCalculationMethod EquilibriumMethod
        {
            get
            {
                return _equilibriumMethod;
            }

            set
            {
                _equilibriumMethod = value;
            }
        }

        public EnthalpyCalculationMethod EnthalpyMethod
        {
            get
            {
                return _enthalpyMethod;
            }

            set
            {
                _enthalpyMethod = value;
            }
        }

        public List<BinaryInteractionParameterSet> BinaryParameters { get => _binaryParameters; set => _binaryParameters = value; }
        public List<Chemistry> ChemistryBlocks { get => _chemistryBlocks; set => _chemistryBlocks = value; }
        public CorrelationFactory CorrelationFactory { get => _correlationFactory; set => _correlationFactory = value; }
        public PropertyFunctionFactory EquationFactory { get => _equationFactory; set => _equationFactory = value; }


        #endregion

        public ThermodynamicSystem()
        {

        }
        public ThermodynamicSystem(string name, string baseMethod = "Ideal", string uomset = "default")
        {
            Name = name;
            MakeDefault(baseMethod);

            if (uomset.ToLower() == "default")
                VariableFactory.SetOutputDimensions(UnitsOfMeasure.UnitSet.CreateDefault());
            if (uomset == "SI")
                VariableFactory.SetOutputDimensions(UnitsOfMeasure.UnitSet.CreateSI());

        }

        public Substance GetComponentById(string id)
        {
            return Components.FirstOrDefault(c => c.ID == id);
        }


        public IList<string> GetComponentIds()
        {
            return Components.Select(c => c.ID).ToList();
        }

        public IList<string> GetComponentNames()
        {
            return Components.Select(c => c.Name).ToList();
        }

        public IList<string> GetComponentCASNumbers()
        {
            return Components.Select(c => c.CasNumber).ToList();
        }
        public IList<double> GetComponentMolarWeights()
        {
            return Components.Select(c => c.MolarWeight.Val()).ToList();
        }

        public ThermodynamicSystem AddComponent(Substance comp)
        {
            Components.Add(comp);
            var enthalpy = PureEnthalpyFunction.Create(this, comp);
            enthalpy.ReferenceState = PhaseState.Vapor;
            enthalpy.Tref.SetValue(298.15);
            EnthalpyMethod.PureComponentEnthalpies.Add(enthalpy);

            return this;
        }
        public BinaryInteractionParameterSet GetBinaryParameters(string name)
        {
            return BinaryParameters.FirstOrDefault(s => s.Name == name);
        }
        public void MakeDefault(string baseMethod)
        {
            switch (baseMethod)
            {

                case "SRK":
                    EquilibriumMethod.EquilibriumApproach = EquilibriumApproach.PhiPhi;
                    EquilibriumMethod.Fugacity = FugacityMethod.SoaveRedlichKwong;
                    EquilibriumMethod.EquationOfState = EquationOfState.SoaveRedlichKwong;
                    EquilibriumMethod.Activity = ActivityMethod.Ideal;
                    EquilibriumMethod.AllowHenryComponents = false;
                    EquilibriumMethod.PoyntingCorrection = false;
                    EquilibriumMethod.AllowedPhases = AllowedPhases.VLE;
                    break;
                case "NRTL":
                    EquilibriumMethod.EquilibriumApproach = EquilibriumApproach.GammaPhi;
                    EquilibriumMethod.Fugacity = FugacityMethod.Ideal;
                    EquilibriumMethod.Activity = ActivityMethod.NRTL;
                    EquilibriumMethod.AllowHenryComponents = false;
                    EquilibriumMethod.PoyntingCorrection = false;
                    EquilibriumMethod.AllowedPhases = AllowedPhases.VLE;
                    break;
                case "NRTLRP":
                    EquilibriumMethod.EquilibriumApproach = EquilibriumApproach.GammaPhi;
                    EquilibriumMethod.Fugacity = FugacityMethod.Ideal;
                    EquilibriumMethod.Activity = ActivityMethod.NRTLRP;
                    EquilibriumMethod.AllowHenryComponents = false;
                    EquilibriumMethod.PoyntingCorrection = false;
                    EquilibriumMethod.AllowedPhases = AllowedPhases.VLE;
                    break;
                case "UNIQUAC":
                    EquilibriumMethod.EquilibriumApproach = EquilibriumApproach.GammaPhi;
                    EquilibriumMethod.Fugacity = FugacityMethod.Ideal;
                    EquilibriumMethod.Activity = ActivityMethod.UNIQUAC;
                    EquilibriumMethod.AllowHenryComponents = false;
                    EquilibriumMethod.PoyntingCorrection = false;
                    EquilibriumMethod.AllowedPhases = AllowedPhases.VLE;
                    break;
                default:
                    EquilibriumMethod.EquilibriumApproach = EquilibriumApproach.GammaPhi;
                    EquilibriumMethod.Fugacity = FugacityMethod.Ideal;
                    EquilibriumMethod.Activity = ActivityMethod.Ideal;
                    EquilibriumMethod.AllowHenryComponents = false;
                    EquilibriumMethod.PoyntingCorrection = false;
                    EquilibriumMethod.AllowedPhases = AllowedPhases.VLE;
                    break;
            }
        }
    }
}
