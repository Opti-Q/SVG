
using System;
using System.Drawing;
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
        public abstract void TransformPoints(PointF[] points);
        public abstract void RotateAt(float f, PointF midPoint, MatrixOrder prepend);
        public abstract void Rotate(float fAngle, MatrixOrder append);
        public abstract Matrix Clone();
        public abstract float[] Elements { get; }
        public abstract float OffsetX { get;  }
        public abstract float OffsetY { get;  }
        public abstract void Rotate(float fAngle);
        public abstract void Shear(float f, float f1);
        public virtual void Dispose()
        { }
        public override string ToString()
        {
            var e = Elements;
            return $"[{e[0]};{e[1]};{e[2]}],[{e[3]};{e[4]};{e[5]}],[{e[6]};{e[7]};{e[8]}]";
        }
    }
}