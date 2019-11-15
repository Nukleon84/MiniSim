using MiniSim.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Thermodynamics
{
    public enum MethodTypes { RKS, Uniquac, ModUniquac};

    public class MethodParameters
    {
        MethodTypes _method;
        Dictionary<string, Variable> _parameters = new Dictionary<string, Variable>();

        public MethodTypes Method
        {
            get
            {
                return _method;
            }

            set
            {
                _method = value;
            }
        }

        public Dictionary<string, Variable> Parameters
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
    }
}
