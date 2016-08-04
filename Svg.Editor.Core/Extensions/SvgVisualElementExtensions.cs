using System;
using Svg.Interfaces;

namespace Svg
{
    public static class SvgVisualElementExtensions
    {
        public static Matrix CreateOriginRotation(this SvgVisualElement e, float angleDegrees)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            var m = e.Transforms.GetMatrix();
            var inv = m.Clone();
            inv.Invert();
            var b = inv.TransformRectangle(e.GetBoundingBox());
            m.RotateAt(angleDegrees, PointF.Create(b.Width/2, b.Height/2), MatrixOrder.Prepend);

            return m;
        }

        public static Matrix CreateTranslation(this SvgVisualElement e, float tx, float ty)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            var m = e.Transforms.GetMatrix();
            m.Translate(tx, ty, MatrixOrder.Append);
            return m;
        }

        public static void SetTransformationMatrix(this SvgVisualElement e, Matrix m)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (e.Transforms.Count != 1)
            {
                e.Transforms.Clear();
                if (m != null)
                    e.Transforms.Add(m);
            }
            else if (m != null)
            {
                e.Transforms[0] = m;
            }
            else
            {
                e.Transforms.Clear();
            }
        }

        //public static void SetTransformationMatrix(this SvgVisualElement e, Matrix m)
        //{
        //    if (e == null)
        //        throw new ArgumentNullException(nameof(e));

        //    e.Transforms.Clear();
        //    if (m != null)
        //        e.Transforms.Add(m);
        //}
    }
}
