using System.Globalization;

namespace Svg.Transforms
{
    public sealed class SvgScale : SvgTransform
    {
        private float scaleFactorX;
        private float scaleFactorY;
        private Matrix matrix;

        public float X
        {
            get { return this.scaleFactorX; }
            set { this.scaleFactorX = value;
                matrix = null;
            }
        }

        public float Y
        {
            get { return this.scaleFactorY; }
            set { this.scaleFactorY = value;
                matrix = null;
            }
        }

        public override Matrix Matrix
        {
            get
            {
                if (matrix != null)
                    return matrix;
                matrix = SvgEngine.Factory.CreateMatrix();
                matrix.Scale(this.X, this.Y);
                return matrix;
            }
        }

        public override string WriteToString()
        {
            if (this.X == this.Y) return string.Format(CultureInfo.InvariantCulture, "scale({0})", this.X);
            return string.Format(CultureInfo.InvariantCulture, "scale({0}, {1})", this.X, this.Y);
        }

        public SvgScale(float x) : this(x, x) { }

        public SvgScale(float x, float y)
        {
            this.scaleFactorX = x;
            this.scaleFactorY = y;
        }

		public override object Clone()
		{
			return new SvgScale(this.X, this.Y);
		}
    }
}
