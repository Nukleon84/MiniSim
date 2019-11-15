using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Numerics
{
    public struct Vector : ICloneable
    {
        #region Fields

        readonly int _ndim;
        double[] _vector;
        #endregion

        #region Properties
        public int Size
        {
            get { return _ndim; }
        }
        public double this[int i]
        {
            get
            {
                if (i < 0 || i > _ndim)
                {
                    throw new ArgumentException("Vector index is out of range!");
                }
                return _vector[i];
            }
            set
            {
                if (i < 0 || i > _ndim)
                {
                    throw new ArgumentException("Vector index is out of range!");
                }
                _vector[i] = value;
            }
        }
        #endregion

        #region Constructors
        public Vector(int ndim) : this(ndim, 0)
        {

        }
        public Vector(int ndim, double defaultValue)
        {
            this._ndim = ndim;
            this._vector = new double[ndim];
            for (int i = 0; i < ndim; i++)
            {
                _vector[i] = defaultValue;
            }
        }

        public Vector(double[] vector)
        {
            this._ndim = vector.Length;
            this._vector = vector;
        }
        #endregion


        public Vector Clone()
        {
            Vector v = new Vector(_vector);
            v._vector = (double[])_vector.Clone();
            return v;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public Vector SwapVectorEntries(int m, int n)
        {
            double temp = _vector[m];
            _vector[m] = _vector[n];
            _vector[n] = temp;
            return new Vector(_vector);
        }

        public override string ToString()
        {
            string str = "[";
            for (int i = 0; i < _ndim - 1; i++)
            {
                str += _vector[i].ToString() + ", ";

            }

            str += _vector[_ndim - 1].ToString() + "]";
            return str;
        }

        public double[] ToDouble()
        {
            return _vector;
        }

        #region Comparison

        public override bool Equals(object obj)
        {
            return ((obj is Vector) && this.Equals((Vector)obj));
        }

        public bool Equals(Vector v)
        {
            return _vector == v._vector;
        }

        public override int GetHashCode()
        {
            return _vector.GetHashCode();
        }

        public static bool operator ==(Vector v1, Vector v2)
        {
            return v1.Equals(v2);
        }
        public static bool operator !=(Vector v1, Vector v2)
        {
            return !v1.Equals(v2);
        }
        #endregion

        #region Arithmetic Operators
        public static Vector operator +(Vector v)
        {
            return v;
        }
        public static Vector operator +(Vector v1, Vector v2)
        {
            if (v1.Size != v2.Size)
                throw new Exception("Vector size must be equal for addition");
            Vector result = new Vector(v1._ndim);
            for (int i = 0; i < v1._ndim; i++)
            {
                result[i] = v1[i] + v2[i];
            }
            return result;
        }

        public static Vector operator -(Vector v)
        {
            Vector result = new Vector(v._ndim);
            for (int i = 0; i < v._ndim; i++)
            {
                result[i] = -v[i];
            }
            return result;
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            if (v1.Size != v2.Size)
                throw new Exception("Vector size must be equal for addition");
            Vector result = new Vector(v1._ndim);
            for (int i = 0; i < v1._ndim; i++)
            {
                result[i] = v1[i] - v2[i];
            }
            return result;
        }

        public static Vector operator *(Vector v, double d)
        {
            Vector result = new Vector(v._ndim);
            for (int i = 0; i < v._ndim; i++)
            {
                result[i] = v[i] * d;
            }
            return result;
        }
        public static Vector operator *(double d, Vector v)
        {
            Vector result = new Vector(v._ndim);
            for (int i = 0; i < v._ndim; i++)
            {
                result[i] = d * v[i];
            }
            return result;
        }

        public static Vector operator /(Vector v, double d)
        {
            Vector result = new Vector(v._ndim);
            for (int i = 0; i < v._ndim; i++)
            {
                result[i] = v[i] / d;
            }
            return result;
        }
        public static Vector operator /(double d, Vector v)
        {
            Vector result = new Vector(v._ndim);
            for (int i = 0; i < v._ndim; i++)
            {
                result[i] = v[i] / d;
            }
            return result;
        }

        #endregion

        #region Vector Functions
        public static double DotProduct(Vector v1, Vector v2)
        {
            if (v1.Size != v2.Size)
                throw new Exception("Vector size must be equal for addition");

            double result = 0.0;

            for (int i = 0; i < v1._ndim; i++)
            {
                result += v1[i] * v2[i];
            }
            return result;

        }

        public double GetNorm()
        {
            double result = 0.0;
            for (int i = 0; i < _ndim; i++)
            {
                result += _vector[i] * _vector[i];
            }
            return System.Math.Sqrt(result);
        }

        public double GetNormSquare()
        {
            double result = 0.0;
            for (int i = 0; i < _ndim; i++)
            {
                result += _vector[i] * _vector[i];
            }
            return result;
        }

        public void Normalize()
        {
            double norm = GetNorm();
            if (norm == 0)
            {
                throw new Exception("Tried to normalize a _vector with the norm of zero!");
            }
            for (int i = 0; i < _ndim; i++)
            {
                _vector[i] /= norm;
            }

        }

        public Vector GetNormalizedVector()
        {
            Vector result = new Vector(_vector);
            result.Normalize();
            return result;
        }

        public static Vector CrossProduct(Vector v1, Vector v2)
        {
            if (v1._ndim != 3)
                throw new Exception("Vector v1 must bei 3 dimensional");
            if (v2._ndim != 3)
                throw new Exception("Vector v2 must bei 3 dimensional");

            var result = new Vector(3);
            result[0] = v1[1] * v2[2] - v1[2] * v2[1];
            result[1] = v1[2] * v2[0] - v1[0] * v2[2];
            result[2] = v1[0] * v2[1] - v1[1] * v2[0];
            return result;
        }

        #endregion

    }
}
