using MiniSim.Core.Expressions;
using MiniSim.Core.Thermodynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Thermodynamics.Routines
{
    public class EnthalpyRoute : Expression

    {
        ThermodynamicSystem _system;
        int NC;
        Variable T;
        Variable p;
        List<Variable> x;

        Expression _htotal;
        Expression[] _hi;
        PhaseState _phase = PhaseState.Liquid;

        private List<Expression> _parameters = new List<Expression>();

        public List<Expression> Parameters
        {
            get
            {
                return _parameters;
            }

            set
            {
                _parameters = value;
            }
        }

        public PhaseState Phase
        {
            get
            {
                return _phase;
            }

            set
            {
                _phase = value;
            }
        }

        public EnthalpyRoute(ThermodynamicSystem system, Variable T, Variable p, List<Variable> x, PhaseState phase) : base("H" + phase, () => 1, (vari) => 0)
        {
            Name = "H" + (phase == PhaseState.Liquid ? "L" : "V");
            _system = system;

            this.T = T;
            this.p = p;
            this.x = x;

            Parameters.Add(T);
            Parameters.Add(p);
            foreach (var comp in x)
                Parameters.Add(comp);

            NC = _system.Components.Count;

            _hi = new Expression[NC];


            for (int i = 0; i < NC; i++)
            {
                if (phase == PhaseState.Liquid)
                    _hi[i] = x[i] * _system.EquationFactory.GetLiquidEnthalpyExpression(_system, i, T);
                else
                    _hi[i] = x[i] * _system.EquationFactory.GetVaporEnthalpyExpression(_system, i, T);
            }


            _htotal = (Sym.Sum(0, NC, (idx) => _hi[idx]));

            ValueFunc = () => _htotal.Val();
            DiffFunc = (vari) => _htotal.Diff(vari);            
            this.AddChildren(_htotal);
        }


        public override string ToString()
        {
            return Name + "(T,p,x)";
        }

    }
}
