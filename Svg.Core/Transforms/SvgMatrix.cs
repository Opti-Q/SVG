﻿using System.Collections.Generic;
using System.Globalization;
using Svg.Platform;

namespace Svg.Transforms
{
	/// <summary>
	/// The class which applies custom transform to this Matrix (Required for projects created by the Inkscape).
	/// </summary>
    public sealed class SvgMatrix : SvgTransform
    {
    	private List<float> points;
	    private Matrix matrix;
        public List<float> Points
        {
            get { return this.points; }
            set
            {
                this.points = value;
                matrix = null;
            }
        }

	    private const int ScaleX = 0;
	    private const int ScaleY = 3;
	    private const int RotateX = 1;
	    private const int RotateY = 2;
	    private const int TranslateX = 4;
        private const int TranslateY = 5;

        public override Matrix Matrix
        {
            get
            {
                if (matrix != null)
                    return matrix;

                /* according to http://tutorials.jenkov.com/svg/svg-transformation.html
                 * a matrix like
                 *      sx  rx  tx
                 *      ry  sy  ty
                 *      0   0   1
                 * 
                 * is specified like: transform="matrix(sx,rx,ry,sy,tx,ty)"
                 * sx, sy is scaling
                 * rx, ry is rotation/skew
                 * tx, ty is translation
                 */
                matrix = Engine.Factory.CreateMatrix(
                    this.points[ScaleX],
                    this.points[RotateX],
                    this.points[RotateY],
                    this.points[ScaleY],
                    this.points[TranslateX],
                    this.points[TranslateY]
                    //this.points[0],
                    //this.points[1],
                    //this.points[3],
                    //this.points[4],
                    //this.points[2],
                    //this.points[5]
                );

                return matrix;
            }
        }

        public override string WriteToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "matrix({0}, {1}, {2}, {3}, {4}, {5})",
                this.points[0], this.points[1], this.points[2], this.points[3], this.points[4], this.points[5]);
        }

        public SvgMatrix(List<float> m)
        {
        	this.points = m;
        }


		public override object Clone()
		{
			return new SvgMatrix(this.Points);
		}

    }
}