
using System;
using System.Drawing;
using System.Linq;
using Svg.Interfaces;

namespace Svg
{
    public abstract class Matrix : IDisposable
    {
        public abstract void Scale(float width, float height);
        public abstract void Scale(float width, float height, MatrixOrder append);
        public abstract void Translate(float left, float top, MatrixOrder append);
        public abstract void TransformVectors(PointF[] points);
        public abstract void Translate(float left, float top);
        public abstract void Multiply(Matrix matrix);
        public abstract void Multiply(Matrix matrix, MatrixOrder prepend);
        public abstract void TransformPoints(PointF[] points);
        public abstract void RotateAt(float f, PointF midPoint, MatrixOrder prepend);
        public abstract void Rotate(float fAngle, MatrixOrder append);
        public abstract Matrix Clone();
        public abstract float[] Elements { get; }
        public abstract float OffsetX { get;  }
        public abstract float OffsetY { get;  }
        public abstract float ScaleX { get; }
        public abstract float ScaleY { get; }
        public abstract bool IsIdentity { get; }

        public abstract void Rotate(float fAngle);
        public abstract void Shear(float f, float f1);
        public virtual void Dispose()
        { }
        public override string ToString()
        {
            var e = Elements;
            return $"[{e[0]};{e[1]};{e[2]}],[{e[3]};{e[4]};{e[5]}],[{e[6]};{e[7]};{e[8]}]";
        }
        public override bool Equals(object obj)
        {
            var matrix = obj as Matrix;
            if (matrix == null)
                return false;

            return Elements.SequenceEqual(matrix.Elements);
        }

        /// <summary>
        /// Multiplies matriy a with b like "a*b"
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public float[] Multiply(float[] a, float[] b)
        {
            //var a = new float[9];
            //var b = new float[9];
            //_m.GetValues(a);
            //other._m.GetValues(b);

            var a1 = new float[3, 3];
            a1[0, 0] = a[0];
            a1[0, 1] = a[1];
            a1[0, 2] = a[2];
            a1[1, 0] = a[3];
            a1[1, 1] = a[4];
            a1[1, 2] = a[5];
            a1[2, 0] = a[6];
            a1[2, 1] = a[7];
            a1[2, 2] = a[8];

            var b1 = new float[3, 3];
            b1[0, 0] = b[0];
            b1[0, 1] = b[1];
            b1[0, 2] = b[2];
            b1[1, 0] = b[3];
            b1[1, 1] = b[4];
            b1[1, 2] = b[5];
            b1[2, 0] = b[6];
            b1[2, 1] = b[7];
            b1[2, 2] = b[8];


            var r = MultiplyMatrix(a1, b1);
            var result = new float[]
            {
                r[0, 0], r[0, 1], r[0, 2],
                r[1, 0], r[1, 1], r[1, 2],
                r[2, 0], r[2, 1], r[2, 2],
            };
            return result;
            //_m.SetValues();
        }

        /// <summary>
        /// Multiplies matriy a with b like "a*b"
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public float[,] MultiplyMatrix(float[,] a, float[,] b)
        {
            int rA = a.GetLength(0);
            int cA = a.GetLength(1);
            int rB = b.GetLength(0);
            int cB = b.GetLength(1);
            float temp = 0;
            float[,] kHasil = new float[rA, cB];
            if (cA != rB)
            {
                throw new InvalidOperationException("matrix cannot be multiplied");
            }
            else
            {
                for (int i = 0; i < rA; i++)
                {
                    for (int j = 0; j < cB; j++)
                    {
                        temp = 0;
                        for (int k = 0; k < cA; k++)
                        {
                            temp += a[i, k] * b[k, j];
                        }
                        kHasil[i, j] = temp;
                    }
                }
                return kHasil;
            }
        }


        //public static bool operator ==(Matrix c1, Matrix c2)
        //{
        //    if (c1 == null && c2 != null)
        //        return false;

        //    if (c1 != null && c2 == null)
        //        return false;

        //    return c1.Elements.SequenceEqual(c2.Elements);
        //}

        //public static bool operator !=(Matrix c1, Matrix c2)
        //{
        //    if (c1 == null && c2 != null)
        //        return true;

        //    if (c1 != null && c2 == null)
        //        return true;

        //    return !c1.Elements.SequenceEqual(c2.Elements);
        //}
        public abstract void Invert();
    }
}