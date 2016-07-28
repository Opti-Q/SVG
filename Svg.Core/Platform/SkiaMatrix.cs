using System;
using SkiaSharp;
using Svg.Interfaces;

namespace Svg.Platform
{
    // maybe copy from http://stackoverflow.com/questions/15817888/fast-rotation-transformation-matrix-multiplications ?
    // see also exmplanations at https://www.willamette.edu/~gorr/classes/GeneralGraphics/Transforms/transforms2d.htm
    public class SkiaMatrix : Matrix
    {
        private SKMatrix _m;

        public SkiaMatrix()
        {
            _m = SKMatrix.MakeIdentity();
        }

        public SkiaMatrix(SKMatrix src)
        {
            _m = new SKMatrix();
            _m.Persp0 = src.Persp0;
            _m.Persp1 = src.Persp1;
            _m.Persp2 = src.Persp2;
            _m.ScaleX = src.ScaleX;
            _m.ScaleY = src.ScaleY;
            _m.SkewX = src.SkewX;
            _m.SkewY = src.SkewY;
            _m.TransX = src.TransX;
            _m.TransY = src.TransY;
        }

        public SkiaMatrix(SKMatrix src, bool copy)
        {
            _m = src;
        }

        public SkiaMatrix(float[] e)
        {
            _m = new SKMatrix()
            {
                ScaleX = e[0],
                SkewX = e[1],
                TransX = e[2],
                SkewY = e[3],
                ScaleY = e[4],
                TransY = e[5],
                Persp0 = e[6],
                Persp1 = e[7],
                Persp2 = e[8]
            };
        }

        //
        // Summary:
        //     Initializes a new instance of the System.Drawing.Drawing2D.Matrix class with
        //     the specified elements.
        //
        // see: https://msdn.microsoft.com/en-us/library/system.drawing.drawing2d.matrix(v=vs.110).aspx
        public SkiaMatrix(float scaleX, float rotateX, float rotateY, float scaleY, float transX, float transY)
        {
            _m = new SKMatrix();

            /*
             * In android, rotateX and rotateY are switched for whatever reason!!
             */
            _m.ScaleX = scaleX;
            _m.SkewX = rotateY;
            _m.TransX = transX;
            _m.SkewY = rotateX;
            _m.ScaleY = scaleY;
            _m.TransY = transY;
            _m.Persp0= 0;
            _m.Persp1 = 0;
            _m.Persp2 = 1;

            /*      see:https://github.com/google/skia/blob/master/src/core/SkMatrix.cpp
             *      [scale-x    skew-x      trans-x]   [X]   [X']
             *      [skew-y     scale-y     trans-y] * [Y] = [Y']
             *      [persp-0    persp-1     persp-2]   [1]   [1 ]
            */

        }

        public SKMatrix Matrix => _m;

        public override bool IsIdentity
        {
            get
            {
                return _m.ScaleX == 1f && _m.SkewX == 0f && _m.TransX == 0f &&
                       _m.SkewY == 0f && _m.ScaleY == 1f && _m.TransY == 0f &&
                       _m.Persp0 == 0f && _m.Persp1 == 0f && _m.Persp2 == 1f;
            }
        }

        public override void Invert()
        {
            //// copied from SkMatrix::invertNonIdentity
            //// see: https://github.com/google/skia/blob/master/src/core/SkMatrix.cpp
            //if (IsIdentity)
            //    return;

            //var m = _m;

            //bool isScaleMatrix = m.ScaleX != 1f || m.ScaleY != 1f;
            //bool isTranslateMatrix = m.TransX != 0f || m.TransY != 0f;
            //bool isRotateMatrix = m.SkewX != 0f || m.SkewX != 0f;

            //if (!isRotateMatrix && isScaleMatrix && isTranslateMatrix)
            //{
            //    var invX = m.ScaleX;
            //    var invY = m.ScaleY;
            //    if (invX == 0 || invY == 0)
            //    {
            //        // not invertible
            //        return;
            //    }
            //    invX = 1/invX;
            //    invY = 1/invY;
            //    m.SkewX = 0;
            //    m.SkewY = 0;
            //    m.Persp0 = 0;
            //    m.Persp1 = 0;

            //    m.ScaleX = invX;
            //    m.ScaleY = invY;
            //    m.Persp2 = 1;
            //    m.TransX = -m.TransX*invX;
            //    m.TransY = -m.TransY*invY;

            //    _m = m;
            //    return;
            //}
            //else if (!isRotateMatrix && isTranslateMatrix)
            //{
            //    m.TransX = -m.TransX;
            //    m.TransY = -m.TransY;
            //    _m = m;
            //    return;
            //}

            var m = _m;
            float det = m.ScaleX * (m.ScaleY * m.Persp2 - m.Persp1 * m.TransY) -
             m.SkewX * (m.SkewY * m.Persp2 - m.TransY * m.Persp0) +
             m.TransX * (m.SkewY * m.Persp1 - m.ScaleY * m.Persp0);

            float invdet = 1 / det;

            var m1 = new SKMatrix();
            m1.ScaleX = (m.ScaleY * m.Persp2 - m.Persp1 * m.TransY) * invdet;
            m1.SkewX = (m.TransX * m.Persp1 - m.SkewX * m.Persp2) * invdet;
            m1.TransX = (m.SkewX * m.TransY - m.TransX * m.ScaleY) * invdet;
            m1.SkewY = (m.TransY * m.Persp0 - m.SkewY * m.Persp2) * invdet;
            m1.ScaleY = (m.ScaleX * m.Persp2 - m.TransX * m.Persp0) * invdet;
            m1.TransY = (m.SkewY * m.TransX - m.ScaleX * m.TransY) * invdet;
            m1.Persp0 = (m.SkewY * m.Persp1 - m.Persp0 * m.ScaleY) * invdet;
            m1.Persp1 = (m.Persp0 * m.SkewX - m.ScaleX * m.Persp1) * invdet;
            m1.Persp2 = (m.ScaleX * m.ScaleY - m.SkewY * m.SkewX) * invdet;
            _m = m1;

            //SKMatrix m;
            //if (_m.TryInvert(out m))
            //{
            //    _m = m;
            //}
        }

        public override void Scale(float width, float height)
        {
            Scale(width, height, MatrixOrder.Prepend);
        }

        public override void Scale(float width, float height, MatrixOrder order)
        {
            var m = SKMatrix.MakeScale(width, height);

            if (order == MatrixOrder.Append)
                _m = Multiply(_m, m);
            else
                _m = Multiply(m, _m);
        }

        public override void Translate(float left, float top)
        {
            Translate(left, top, MatrixOrder.Prepend);
        }

        public override void Translate(float left, float top, MatrixOrder order)
        {
            var m = SKMatrix.MakeTranslation(left, top);

            if (order == MatrixOrder.Append)
                _m = Multiply(_m, m);
            else
                _m = Multiply(m, _m);
        }
        
        /// <summary>
        /// Does a pre-pend multiplication
        /// </summary>
        /// <param name="matrix"></param>
        public override void Multiply(Matrix matrix)
        {
            Multiply(matrix, MatrixOrder.Prepend);
        }

        public override void Multiply(Matrix matrix, MatrixOrder order)
        {
            var other = (SkiaMatrix)matrix;

            if (order == MatrixOrder.Append)
                _m = Multiply(_m, other.Matrix);
            else
                _m = Multiply(other.Matrix, _m);
        }

        public override void Rotate(float angle, MatrixOrder order)
        {
            var mr = SKMatrix.MakeRotation((float)DegreeToRadian(angle));
            //var mr = SKMatrix.MakeRotationDegrees(angle);

            if (order == MatrixOrder.Append)
                _m = Multiply(_m, mr);
            else
                _m = Multiply(mr, _m);
        }

        public override void RotateAt(float angle, PointF midPoint, MatrixOrder order)
        {
            var m1 = SKMatrix.MakeTranslation(midPoint.X, midPoint.Y);
            var m2 = SKMatrix.MakeRotation((float)DegreeToRadian(angle));
            var m3 = SKMatrix.MakeTranslation(-midPoint.X, -midPoint.Y);

            var m12 = CreateMatrix(Multiply(GetElements(m2), GetElements(m1)));
            var mr = CreateMatrix(Multiply(GetElements(m3), GetElements(m12)));

            //var mr = SKMatrix.MakeRotationDegrees(angle, midPoint.X, midPoint.Y);

            if (order == MatrixOrder.Append)
                _m = Multiply(_m, mr);
            else
                _m = Multiply(mr, _m);
        }

        public override void Rotate(float fAngle)
        {
            Rotate(fAngle, MatrixOrder.Prepend);
        }

        public override void Shear(float f, float f1)
        {
            var m = SKMatrix.MakeSkew(f, f1);

            _m = CreateMatrix(Multiply(GetElements(m), GetElements(_m)));
        }

        public override RectangleF TransformRectangle(RectangleF bound)
        {
            var start = Engine.Factory.CreatePointF(bound.X, bound.Y);
            var end = Engine.Factory.CreatePointF(bound.X + bound.Width, bound.Y + bound.Height);
            var pts = new[] { start, end };

            TransformPoints(pts);

            return Engine.Factory.CreateRectangleF(start.X, start.Y, end.X - start.X, end.Y - start.Y);
        }

        public override void TransformVectors(PointF[] points)
        {

            foreach (var point in points)
            {
                //// see http://math.stackexchange.com/questions/29257/find-2d-point-given-2d-point-and-transformation-matrix
                //// transformvectors needs to IGNORE the translation! (see: http://stackoverflow.com/questions/3265169/matrix-transformpoints-vs-transformvectors)
                //point.X = (_m.ScaleX * point.X) + (_m.SkewX * point.X); // + _m.TransX;
                //point.Y = (_m.SkewY * point.Y) + (_m.ScaleY * point.Y); // + _m.TransY;


                // see http://referencesource.microsoft.com/#WindowsBase/Base/System/Windows/Media/Matrix.cs,e4b18483d8c1404d
                float xadd = point.Y * _m.SkewY /*+ _m.TransX*/;
                float yadd = point.X * _m.SkewX /*+ _m.TransY*/;
                point.X *= _m.ScaleX;
                point.X += xadd;
                point.Y *= _m.ScaleY;
                point.Y += yadd;
            }
        }

        public override void TransformPoints(PointF[] points)
        {
            foreach (var point in points)
            {
                //// see http://math.stackexchange.com/questions/29257/find-2d-point-given-2d-point-and-transformation-matrix
                //point.X = (_m.ScaleX*point.X) + (_m.SkewX*point.X) + _m.TransX;
                //point.Y = (_m.SkewY*point.Y) + (_m.ScaleY*point.Y) + _m.TransY;

                // see http://referencesource.microsoft.com/#WindowsBase/Base/System/Windows/Media/Matrix.cs,e4b18483d8c1404d
                float xadd = point.Y * _m.SkewY + _m.TransX;
                float yadd = point.X * _m.SkewX + _m.TransY;
                point.X *= _m.ScaleX;
                point.X += xadd;
                point.Y *= _m.ScaleY;
                point.Y += yadd;
            }
        }
        
        public override float[] Elements
        {
            get
            {
                var res = new float[9]
                {
                    _m.ScaleX,
                    _m.SkewX,
                    _m.TransX,
                    _m.SkewY,
                    _m.ScaleY,
                    _m.TransY,
                    _m.Persp0,
                    _m.Persp1,
                    _m.Persp2,
                };
                return res;
            }
        }

        public override float OffsetX
        {
            get { return _m.TransX; }
        }

        public override float OffsetY
        {
            get { return _m.TransY; }
        }

        public override float ScaleX
        {
            get { return _m.ScaleX; }
        }

        public override float ScaleY
        {
            get { return _m.ScaleY; }
        }

        private static float[] GetElements(SKMatrix m)
        {
            return new float[9]
                {
                    m.ScaleX,
                    m.SkewX,
                    m.TransX,
                    m.SkewY,
                    m.ScaleY,
                    m.TransY,
                    m.Persp0,
                    m.Persp1,
                    m.Persp2,
                };
        }

        private static SKMatrix CreateMatrix(float[] e)
        {
            return new SKMatrix()
            {
                ScaleX = e[0],
                SkewX = e[1],
                TransX = e[2],
                SkewY = e[3],
                ScaleY = e[4],
                TransY = e[5],
                Persp0 = e[6],
                Persp1 = e[7],
                Persp2 = e[8]
            };
        }

        private static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        private SKMatrix Multiply(SKMatrix m1, SKMatrix m2)
        {
            return CreateMatrix(Multiply(GetElements(m1), GetElements(m2)));
        }

        public static implicit operator SkiaMatrix(SKMatrix other)
        {
            return new SkiaMatrix(other, true);
        }

        public static implicit operator SKMatrix(SkiaMatrix other)
        {
            return other.Matrix;
        }

        public override Matrix Clone()
        {
            return new SkiaMatrix(_m);
        }

        public override void Dispose()
        {
        }
    }
}