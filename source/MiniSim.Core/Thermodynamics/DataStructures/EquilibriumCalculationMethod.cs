using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Thermodynamics
{
    public class EquilibriumCalculationMethod
    {
        EquilibriumApproach _equilibriumApproach = EquilibriumApproach.GammaPhi;
        ActivityMethod _activity = ActivityMethod.Ideal;
        EquationOfState _equationOfState = EquationOfState.Ideal;
        FugacityMethod _fugacity = FugacityMethod.Ideal;
        AllowedPhases _allowedPhases = AllowedPhases.VLE;
        bool _allowHenryComponents = false;
        bool _poyntingCorrection = false;

        public EquilibriumApproach EquilibriumApproach
        {
            get
            {
                return _equilibriumApproach;
            }

            set
            {
                _equilibriumApproach = value;
            }
        }

        public ActivityMethod Activity
        {
            get
            {
                return _activity;
            }

            set
            {
                _activity = value;
            }
        }

        public EquationOfState EquationOfState
        {
            get
            {
                return _equationOfState;
            }

            set
            {
                _equationOfState = value;
            }
        }

        public FugacityMethod Fugacity
        {
            get
            {
                return _fugacity;
            }

            set
            {
                _fugacity = value;
            }
        }      

        public AllowedPhases AllowedPhases
        {
            get
            {
                return _allowedPhases;
            }

            set
            {
                _allowedPhases = value;
            }
        }

        public bool AllowHenryComponents
        {
            get
            {
                return _allowHenryComponents;
            }

            set
            {
                _allowHenryComponents = value;
            }
        }

        public bool PoyntingCorrection
        {
            get
            {
                return _poyntingCorrection;
            }

            set
            {
                _poyntingCorrection = value;
            }
        }
    }
}
