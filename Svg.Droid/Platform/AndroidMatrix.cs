using System;
using Svg.Interfaces;

namespace Svg.Platform
{
    public class AndroidMatrix : Matrix
    {
        private Android.Graphics.Matrix _m;
        public AndroidMatrix()
        {
            _m = new Android.Graphics.Matrix();
        }
        public AndroidMatrix(Android.Graphics.Matrix src)
        {
            _m = new Android.Graphics.Matrix(src);
        }
        public AndroidMatrix(Android.Graphics.Matrix src, bool copy)
        {
            _m = src;
        }
        //
        // Summary:
        //     Initializes a new instance of the System.Drawing.Drawing2D.Matrix class with
        //     the specified elements.
        //
        // Parameters:
        //   m11:
        //     The value in the first row and first column of the new System.Drawing.Drawing2D.Matrix.
        //
        //   m12:
        //     The value in the first row and second column of the new System.Drawing.Drawing2D.Matrix.
        //
        //   m21:
        //     The value in the second row and first column of the new System.Drawing.Drawing2D.Matrix.
        //
        //   m22:
        //     The value in the second row and second column of the new System.Drawing.Drawing2D.Matrix.
        //
        //   dx:
        //     The value in the third row and first column of the new System.Drawing.Drawing2D.Matrix.
        //
        //   dy:
        //     The value in the third row and second column of the new System.Drawing.Drawing2D.Matrix.
        public AndroidMatrix(float i, float i1, float i2, float i3, float i4, float i5)
        {
            _m = new Android.Graphics.Matrix();
            _m.SetValues(new float[] { i, i1, i3, i4, i2, i5, 0, 0, 1 });
        }

        public override void Dispose()
        {
            _m.Dispose();
        }

        public override void Scale(float width, float height)
        {
            _m.SetScale(width, height);
        }

        public override void Scale(float width, float height, MatrixOrder order)
        {
            if(order == MatrixOrder.Append)
                _m.PostScale(width, height);
            else
                _m.PreScale(width, height);
        }

        public override void Translate(float left, float top, MatrixOrder order)
        {
            if (order == MatrixOrder.Append)
                _m.PostTranslate(left, top);
            else
                _m.PreTranslate(left, top);
        }

        public override void TransformVectors(PointF[] points)
        {
            var a = new float[points.Length*2];
            for (int i = 0; i < points.Length; i++)
            {
                a[i*2] = points[i].X;
                a[i * 2 + 1] = points[i].Y;
            }

            _m.MapVectors(a);

            for (int i = 0; i < points.Length; i++)
            {
                points[i].X = a[i * 2];
                points[i].Y = a[i * 2 + 1];
            }
        }

        public override void Translate(float left, float top)
        {
            _m.SetTranslate(left, top);
        }

        /// <summary>
        /// Does a pre-pend multiplication
        /// </summary>
        /// <param name="matrix"></param>
        public override void Multiply(Matrix matrix)
        {
            var other = (AndroidMatrix) matrix;

            this.Matrix.PreConcat(other.Matrix);

        }

        public override void TransformPoints(PointF[] points)
        {
            TransformVectors(points);
        }

        public override void RotateAt(float angle, PointF midPoint, MatrixOrder order)
        {
            if (order == MatrixOrder.Append)
                _m.PostRotate(angle, midPoint.X, midPoint.Y);
            else
                _m.PreRotate(angle, midPoint.X, midPoint.Y);
        }

        public override void Rotate(float angle, MatrixOrder order)
        {
            if (order == MatrixOrder.Append)
                _m.PostRotate(angle);
            else
                _m.PreRotate(angle);
        }

        public override void Rotate(float fAngle)
        {
            _m.SetRotate(fAngle);
        }

        public override Matrix Clone()
        {
            return new AndroidMatrix(_m);
        }

        public override float[] Elements
        {
            get
            {
                var res = new float[9];
                _m.GetValues(res);
                return res;
            }
        }

        public override float OffsetX
        {
            get
            {
                var vals = new float[9];
                _m.GetValues(vals);
                return vals[Android.Graphics.Matrix.MtransX];
            }
        }

        public override float OffsetY
        {
            get
            {
                var vals = new float[9];
                _m.GetValues(vals);
                return vals[Android.Graphics.Matrix.MtransY];
            }
        }

        public override float ScaleX
        {
            get
            {
                var vals = new float[9];
                _m.GetValues(vals);
                return vals[Android.Graphics.Matrix.MscaleX];
            }
        }

        public override float ScaleY
        {
            get
            {
                var vals = new float[9];
                _m.GetValues(vals);
                return vals[Android.Graphics.Matrix.MscaleY];
            }
        }

        public Android.Graphics.Matrix Matrix { get { return _m; }}

        public override void Shear(float f, float f1)
        {
            _m.SetSkew(f, f1);
        }

        public static implicit operator AndroidMatrix(Android.Graphics.Matrix other)
        {
            return new AndroidMatrix(other, true);
        }
        public static implicit operator Android.Graphics.Matrix(AndroidMatrix other)
        {
            return other.Matrix;
        }
    }
}