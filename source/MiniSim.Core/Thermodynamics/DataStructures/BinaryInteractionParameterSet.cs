using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Thermodynamics
{
    public class DiffusionCoefficients : BinaryInteractionParameterSet
    {
        public DiffusionCoefficients(ThermodynamicSystem system)
        {
            _system = system;
            NC = _system.Components.Count;

            Name = "DVIJ0";
            Matrices.Add("A", new double[NC, NC]);
        }
    }

    public class NRTL : BinaryInteractionParameterSet
    {
        public NRTL(ThermodynamicSystem system)
        {
            _system = system;
            NC = _system.Components.Count;

            Name = "NRTL";
            Matrices.Add("A", new double[NC, NC]);
            Matrices.Add("B", new double[NC, NC]);
            Matrices.Add("C", new double[NC, NC]);
            Matrices.Add("D", new double[NC, NC]);
            Matrices.Add("E", new double[NC, NC]);
            Matrices.Add("F", new double[NC, NC]);

        }
    }

    public class ModifiedUNIQUAC : BinaryInteractionParameterSet
    {
        public ModifiedUNIQUAC(ThermodynamicSystem system)
        {
            _system = system;
            NC = _system.Components.Count;

            Name = "MODUNIQUAC";
            Matrices.Add("A", new double[NC, NC]);
            Matrices.Add("B", new double[NC, NC]);
            Matrices.Add("C", new double[NC, NC]);
            Matrices.Add("D", new double[NC, NC]);
            Matrices.Add("E", new double[NC, NC]);
            Matrices.Add("F", new double[NC, NC]);

        }
    }
    public class UNIQUAC : BinaryInteractionParameterSet
    {
        public UNIQUAC(ThermodynamicSystem system)
        {
            _system = system;
            NC = _system.Components.Count;

            Name = "UNIQUAC";
            Matrices.Add("A", new double[NC, NC]);
            Matrices.Add("B", new double[NC, NC]);
            Matrices.Add("C", new double[NC, NC]);
            Matrices.Add("D", new double[NC, NC]);
            Matrices.Add("E", new double[NC, NC]);
            Matrices.Add("F", new double[NC, NC]);

        }
    }

    public class HENRY : BinaryInteractionParameterSet
    {
        public HENRY(ThermodynamicSystem system)
        {
            _system = system;
            NC = _system.Components.Count;

            Name = "HENRY";
            Matrices.Add("A", new double[NC, NC]);
            Matrices.Add("B", new double[NC, NC]);
            Matrices.Add("C", new double[NC, NC]);
            Matrices.Add("D", new double[NC, NC]);

        }
    }
    public class WILSON : BinaryInteractionParameterSet
    {
        public WILSON(ThermodynamicSystem system)
        {
            _system = system;
            NC = _system.Components.Count;

            Name = "WILSON";
            Matrices.Add("A", new double[NC, NC]);
            Matrices.Add("B", new double[NC, NC]);
            Matrices.Add("C", new double[NC, NC]);
            Matrices.Add("D", new double[NC, NC]);

        }
    }
    public class SRK : BinaryInteractionParameterSet
    {
        public SRK(ThermodynamicSystem system)
        {
            _system = system;
            NC = _system.Components.Count;

            Name = "SRK";
            Matrices.Add("kij", new double[NC, NC]);
        }
    }
    public class BinaryInteractionParameterSet
    {
        protected int NC;
        protected ThermodynamicSystem _system;
        string _name;
        Dictionary<string, double[,]> _matrices = new Dictionary<string, double[,]>();


        public BinaryInteractionParameterSet SetParam(string matrix, int i, int j, double value)
        {
            if (i >= 0 && j >= 0 && Matrices.ContainsKey(matrix))
                Matrices[matrix][i, j] = value;
            return this;
        }

        public BinaryInteractionParameterSet SetParamSymmetric(string matrix, Substance c1, Substance c2, double value)
        {
            var i = _system.Components.IndexOf(c1);
            var j = _system.Components.IndexOf(c2);

            if (i >= 0 && j >= 0 && Matrices.ContainsKey(matrix))
            {
                Matrices[matrix][i, j] = value;
                Matrices[matrix][j, i] = value;
            }
            return this;
        }

        public BinaryInteractionParameterSet SetParam(string matrix, Substance c1, Substance c2, double value)
        {
            var i = _system.Components.IndexOf(c1);
            var j = _system.Components.IndexOf(c2);

            if (i >= 0 && j >= 0 && Matrices.ContainsKey(matrix))
                Matrices[matrix][i, j] = value;
            return this;
        }

        public BinaryInteractionParameterSet SetParamPair(string matrix, Substance c1, Substance c2, double value, double othervalue)
        {
            var i = _system.Components.IndexOf(c1);
            var j = _system.Components.IndexOf(c2);

            if (i >= 0 && j >= 0 && Matrices.ContainsKey(matrix))
            {
                Matrices[matrix][i, j] = value;
                Matrices[matrix][j, i] = othervalue;
            }
            return this;
        }

        public double GetParam(string matrix, Substance c1, Substance c2)
        {
            var i = _system.Components.IndexOf(c1);
            var j = _system.Components.IndexOf(c2);

            if (i >= 0 && j >= 0 && Matrices.ContainsKey(matrix))
                return Matrices[matrix][i, j];
            return 0;
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

        public Dictionary<string, double[,]> Matrices
        {
            get
            {
                return _matrices;
            }

            set
            {
                _matrices = value;
            }
        }
    }
}
