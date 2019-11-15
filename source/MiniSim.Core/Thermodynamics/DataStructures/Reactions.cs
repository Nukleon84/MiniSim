using MiniSim.Core.Expressions;
using MiniSim.Core.Flowsheeting;
using MiniSim.Core.Thermodynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Thermodynamics
{
    public enum ReactionType { CONV, EQLA, EQVM, EQLM };

    public class StoichiometryPair
    {
        Substance _component;
        double _stoichiometricFactor;
        int _index = -1;

        public Substance Component
        {
            get
            {
                return _component;
            }

            set
            {
                _component = value;
            }
        }

        public double StoichiometricFactor
        {
            get
            {
                return _stoichiometricFactor;
            }

            set
            {
                _stoichiometricFactor = value;
            }
        }

        public int Index
        {
            get
            {
                return _index;
            }

            set
            {
                _index = value;
            }
        }

        public StoichiometryPair(int index, Substance component, double factor)
        {
            Index = index;
            Component = component;
            StoichiometricFactor = factor;
        }
    }
    public class Reaction
    {
        ReactionType _type = ReactionType.CONV;
        List<StoichiometryPair> _stoichiometry = new List<StoichiometryPair>();
        List<double> _coefficients = new List<double>();
        double _reactionEnthalpy = 0.0;
        public List<StoichiometryPair> Stoichiometry
        {
            get
            {
                return _stoichiometry;
            }

            set
            {
                _stoichiometry = value;
            }
        }

        public ReactionType Type
        {
            get
            {
                return _type;
            }

            set
            {
                _type = value;
            }
        }

        public List<double> Coefficients
        {
            get
            {
                return _coefficients;
            }

            set
            {
                _coefficients = value;
            }
        }

        public double ReactionEnthalpy
        {
            get
            {
                return _reactionEnthalpy;
            }

            set
            {
                _reactionEnthalpy = value;
            }
        }

        public double GetStoichiometricFactor(Substance comp)
        {
            var pair = Stoichiometry.FirstOrDefault(s => s.Component == comp);
            if (pair != null)
                return pair.StoichiometricFactor;
            else
                return 0;

        }

        Expression GetFactor(MaterialStream stream)
        {
            switch (Type)
            {
                case ReactionType.EQLA:
                    Expression lnK = Coefficients[0];
                    if (Coefficients.Count > 1)
                        lnK += Coefficients[1] / stream.Temperature;
                    if (Coefficients.Count > 2)
                        lnK += Coefficients[2] * Sym.Ln(stream.Temperature);
                    if (Coefficients.Count > 3)
                        lnK += Coefficients[3] * stream.Temperature;
                    if (Coefficients.Count > 4)
                        lnK += Coefficients[4] * Sym.Pow(stream.Temperature, 2.0);
                    if (Coefficients.Count > 5)
                        lnK += Coefficients[5] * Sym.Pow(stream.Temperature, 3.0);
                    return Sym.Exp(lnK);

            }
            return 1;
        }

        Expression GetDrivingForce(MaterialStream stream)
        {
            Expression drivingForce = 1;
            switch (Type)
            {
                case ReactionType.EQLA:
                    {
                        foreach (var stoic in Stoichiometry)
                        {
                        //    drivingForce *= Sym.Pow(Sym.Par(stream.Gamma[stoic.Index] * stream.Liquid.ComponentMolarFraction[stoic.Index]), stoic.StoichiometricFactor);
                        }
                        break;
                    }
                default:
                    break;
            }
            return drivingForce;
        }

        public Equation GetDefiningEquation(MaterialStream stream)
        {
            return new Equation(GetFactor(stream)-GetDrivingForce(stream));
        }
    }

    public class Chemistry
    {
        string _label;
        List<Reaction> _reactions = new List<Reaction>();

        public string Label
        {
            get
            {
                return _label;
            }

            set
            {
                _label = value;
            }
        }

        public List<Reaction> Reactions
        {
            get
            {
                return _reactions;
            }

            set
            {
                _reactions = value;
            }
        }



        public Expression GetReactingMolesExpression(Expression[] reactionRates, Substance comp)
        {
            Expression reactingMoles = 0;
            for (int j = 0; j < Reactions.Count; j++)
            {
                var nu = Reactions[j].GetStoichiometricFactor(comp);
                if (Math.Abs(nu) > 1e-16)
                {
                    reactingMoles += nu * reactionRates[j];
                }
            }
            return reactingMoles;
        }

    }

}
