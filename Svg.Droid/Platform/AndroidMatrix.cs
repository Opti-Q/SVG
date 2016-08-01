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
        // see: https://msdn.microsoft.com/en-us/library/system.drawing.drawing2d.matrix(v=vs.110).aspx
        public AndroidMatrix(float scaleX, float rotateX, float rotateY, float scaleY, float transX, float transY)
        {
            _m = new Android.Graphics.Matrix();

            /*
             * In android, rotateX and rotateY are switched for whatever reason!!
             */
            var vals = new float[9];
            vals[Android.Graphics.Matrix.MscaleX] = scaleX;
            vals[Android.Graphics.Matrix.MskewX] = rotateY;
            vals[Android.Graphics.Matrix.MtransX] = transX;
            vals[Android.Graphics.Matrix.MskewY] = rotateX;
            vals[Android.Graphics.Matrix.MscaleY] = scaleY;
            vals[Android.Graphics.Matrix.MtransY] = transY;
            vals[Android.Graphics.Matrix.Mpersp0] = 0;
            vals[Android.Graphics.Matrix.Mpersp1] = 0;
            vals[Android.Graphics.Matrix.Mpersp2] = 1;

            _m.SetValues(vals);
        }

        public override void Dispose()
        {
            _m.Dispose();
        }

        public override void Invert()
        {
            _m.Invert(_m);
        }

        public override RectangleF TransformRectangle(RectangleF bound)
        {
            var start = PointF.Create(bound.X, bound.Y);
            var end = PointF.Create(bound.X + bound.Width, bound.Y + bound.Height);
            var pts = new[] { start, end };

            TransformPoints(pts);

            return RectangleF.Create(start.X, start.Y, end.X - start.X, end.Y - start.Y);
        }

        public override void Scale(float width, float height)
        {
            _m.PreScale(width, height);
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
            //_m.SetTranslate(left, top);
            _m.PreTranslate(left, top);
        }

        /// <summary>
        /// Does a pre-pend multiplication
        /// </summary>
        /// <param name="matrix"></param>
        public override void Multiply(Matrix matrix)
        {
            var other = (AndroidMatrix) matrix;

            _m.PreConcat(other.Matrix);

        }

        public override void Multiply(Matrix matrix, MatrixOrder order)
        {
            if (order == MatrixOrder.Append)
                _m.PostConcat((AndroidMatrix)matrix);
            else
                _m.PreConcat((AndroidMatrix)matrix);
        }

        public override void TransformPoints(PointF[] points)
        {
            var a = new float[points.Length * 2];
            for (int i = 0; i < points.Length; i++)
            {
                a[i * 2] = points[i].X;
                a[i * 2 + 1] = points[i].Y;
            }

            _m.MapPoints(a);

            for (int i = 0; i < points.Length; i++)
            {
                points[i].X = a[i * 2];
                points[i].Y = a[i * 2 + 1];
            }
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

        public override bool IsIdentity => _m.IsIdentity;

        public override void Rotate(float fAngle)
        {
            //_m.SetRotate(fAngle);
            _m.PreRotate(fAngle);
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

        public Android.Graphics.Matrix Matrix => _m;

        public override void Shear(float sx, float sy)
        {
            //_m.SetSkew(f, f1);
            _m.PreSkew(sx, sy);
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