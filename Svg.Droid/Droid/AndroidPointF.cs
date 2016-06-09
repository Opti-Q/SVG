using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Svg.Interfaces;

namespace Svg.Droid
{
    public class AndroidPointF : PointF
    {
        private System.Drawing.PointF _inner;

        public AndroidPointF()
        {
            _inner = new System.Drawing.PointF(0, 0);
        }
        public AndroidPointF(float x, float y)
        {
            _inner = new System.Drawing.PointF(x, y);
        }

        public AndroidPointF(System.Drawing.PointF inner)
        {
            _inner = inner;
        }

        public bool IsEmpty => _inner.IsEmpty;

        public float X
        {
            get { return _inner.X; }
            set { _inner.X = value; }
        }
        public float Y
        {
            get { return _inner.Y; }
            set { _inner.Y = value; }
        }

        public static implicit operator AndroidPointF(System.Drawing.PointF other)
        {
            return new AndroidPointF(other);
        }

        public static implicit operator System.Drawing.PointF(AndroidPointF other)
        {
            return other._inner;
        }
    }
}