using CSparse.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Numerics
{
    public enum MatrixScalingFrequency { Once, Always, OnDemand};

    public class MatrixScalingLogSum
    {
        /**
 * Calculates the matrix rescaling factors so that the scaled
 * matrix has its entries near to unity in the sense that the sum of the
 * squares of the logarithms of the entries is minimized.
 * 
 * @TODO: add documentation
 * @author alberto trivellato (alberto.trivellato@gmail.com)
 * @see Xin Huang, Preprocessing and Postprocessing in Linear Optimization
 * @see A. Chang and J.K. Reid. On the automatic scaling of matrices for Gaussian elimination, 
 * 		Journal of the Institute of Mathematics and Its Applications 10 (1972)
 * @see Gajulapalli, Lasdon "Scaling Sparse Matrices for Optimization Algorithms"
 */
        private readonly double basis = 10;

        public MatrixScalingLogSum() : this(2)
        {

        }

        public MatrixScalingLogSum(double basis)
        {
            this.basis = basis;

        }

        /**
	 * Gauss-Seidel scaling for a sparse matrix: 
	 * <br>AScaled = U.A.V, with A mxn matrix, U, V diagonal.
	 * Returns the two scaling matrices
	 * <br>U[i,i] = base^x[i], i=0,...,m
	 * <br>V[i,i] = base^y[i], i=0,...,n
	 * 
	 * @see Gajulapalli, Lasdon "Scaling Sparse Matrices for Optimization Algorithms", algorithms 1 and 2
	 */
        public void GetMatrixScalingFactors(CompressedColumnStorage<double> A, out double[] U, out double[] V)
        {
            int m = A.RowCount;
            int n = A.ColumnCount;
            double log10_b = Math.Log10(basis);

            //Setup for Gauss-Seidel Iterations
            int[] R = new int[m];
            int[] C = new int[n];
            double[] t = new double[1];
            double[] a = new double[m];
            double[] b = new double[n];
            bool[,] Z = new bool[m, n];
            foreach (var e in A.EnumerateIndexed())
            {
                var i = e.Item1;
                var j = e.Item2;
                var aij = e.Item3;


                R[i] = R[i] + 1;
                C[j] = C[j] + 1;
                Z[i, j] = true;
                t[0] = -(Math.Log10(Math.Abs(aij)) / log10_b + 0.5);
                a[i] = a[i] + t[0];
                b[j] = b[j] + t[0];
            }

            for (int i = 0; i < m; i++)
            {
                a[i] = a[i] / ((double)R[i]);
            }
            for (int j = 0; j < n; j++)
            {
                b[j] = b[j] / ((double)C[j]);
            }

            int[] xx = new int[m];
            int[] yy = new int[n];
            int[] previousXX = null;
            int[] previousYY = null;
            bool stopX = false;
            bool stopY = false;
            int maxIteration = 8;
            int l = 0;
            for (l = 0; l <= maxIteration && !(stopX && stopY); l++)
            {
                double[] tt = new double[m];
                Array.Copy(a, 0, tt, 0, m);
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (Z[i, j]/*true*/)
                        {
                            tt[i] = tt[i] - ((double)yy[j]) / ((double)R[i]);
                        }
                    }
                }
                for (int k = 0; k < m; k++)
                {
                    xx[k] = (int)Math.Round(tt[k]);
                }
                if (previousXX == null)
                {
                    previousXX = xx;
                }
                else
                {
                    bool allEquals = true;
                    for (int k = 0; k < m && allEquals; k++)
                    {
                        allEquals = (xx[k] == previousXX[k]);
                    }
                    stopX = allEquals;
                    previousXX = xx;
                }

                tt = new double[n];
                Array.Copy(b, 0, tt, 0, n);
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (Z[i, j]/*true*/)
                        {
                            tt[j] = tt[j] - ((double)xx[i]) / ((double)C[j]);
                        }
                    }
                }
                for (int k = 0; k < n; k++)
                {
                    yy[k] = (int)Math.Round(tt[k]);
                }
                if (previousYY == null)
                {
                    previousYY = yy;
                }
                else
                {
                    bool allEquals = true;
                    for (int k = 0; k < n && allEquals; k++)
                    {
                        allEquals = (yy[k] == previousYY[k]);
                    }
                    stopY = allEquals;
                    previousYY = yy;
                }
            }


            U = new double[m];
            V = new double[n];

            var maxabs = 10;

            for (int k = 0; k < m; k++)
            {
                xx[k] = Math.Min(maxabs, Math.Max(-maxabs, xx[k]));
                U[k] = Math.Pow(basis, xx[k]);
            }
            for (int k = 0; k < n; k++)
            {
                yy[k] = Math.Min(maxabs, Math.Max(-maxabs, yy[k]));
                V[k] = Math.Pow(basis, yy[k]);
            }

        }

    }
}
