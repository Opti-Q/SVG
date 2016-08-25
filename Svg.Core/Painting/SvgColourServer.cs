using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Svg.Interfaces;

namespace Svg
{
    public sealed class SvgColourServer : SvgPaintServer
    {
    	
    	/// <summary>
        /// An unspecified <see cref="SvgPaintServer"/>.
        /// </summary>
        public static readonly SvgPaintServer NotSet = new SvgColourServer(Engine.Factory.Colors.Black);
        /// <summary>
        /// A <see cref="SvgPaintServer"/> that should inherit from its parent.
        /// </summary>
        public static readonly SvgPaintServer Inherit = new SvgColourServer(Engine.Factory.Colors.Black);
        /// <summary>
        /// A <see cref="SvgPaintServer"/> that should get the stroke color from its context object (e.g. marker or svg use)
        /// </summary>
        public static readonly SvgPaintServer ContextStroke = new SvgColourServer(Engine.Factory.Colors.Black);
        /// <summary>
        /// A <see cref="SvgPaintServer"/> that should get the fill color from its context object (e.g. marker or svg use)
        /// </summary>
        public static readonly SvgPaintServer ContextFill = new SvgColourServer(Engine.Factory.Colors.Black);

        public SvgColourServer()
            : this(Engine.Factory.Colors.Black)
        {
        }

        public SvgColourServer(Color colour)
        {
            this._colour = colour;
        }

        private Color _colour;

        public Color Colour
        {
            get { return this._colour; }
            set { this._colour = value; }
        }

        public override Brush GetBrush(SvgVisualElement styleOwner, ISvgRenderer renderer, float opacity, bool forStroke = false)
        {
            //is none?
            if (this == SvgPaintServer.None) return Engine.Factory.CreateSolidBrush(Engine.Factory.Colors.Transparent);
                
            int alpha = (int)((opacity * (this.Colour.A/255.0f) ) * 255);
            Color colour = Engine.Factory.CreateColorFromArgb(alpha, this.Colour);

            return Engine.Factory.CreateSolidBrush(colour);
        }

        public override string ToString()
        {
        	if(this == SvgPaintServer.None)
        		return "none";
        	else if(this == SvgColourServer.NotSet)
        		return "";
            else if (this == SvgColourServer.Inherit)
                return "inherit";
            else if (this == SvgColourServer.ContextFill)
                return "context-fill";
            else if (this == SvgColourServer.ContextStroke)
                return "context-stroke";

            Color c = this.Colour;

            // Return the name if it exists
            if (c.IsKnownColor)
            {
                return c.Name;
            }

            // Return the hex value
            return String.Format("#{0}", c.ToArgb().ToString("x").Substring(2));
        }


		public override SvgElement DeepCopy()
		{
			return DeepCopy<SvgColourServer>();
		}


		public override SvgElement DeepCopy<T>()
		{
			var newObj = base.DeepCopy<T>() as SvgColourServer;
			newObj.Colour = this.Colour;
			return newObj;

		}

        public override bool Equals(object obj)
        {
            var objColor = obj as SvgColourServer;
            if (objColor == null)
                return false;

            if ((this == SvgPaintServer.None && obj != SvgPaintServer.None) ||
                (this != SvgPaintServer.None && obj == SvgPaintServer.None) ||
                (this == SvgColourServer.NotSet && obj != SvgColourServer.NotSet) ||
                (this != SvgColourServer.NotSet && obj == SvgColourServer.NotSet) ||
                (this == SvgColourServer.Inherit && obj != SvgColourServer.Inherit) ||
                (this != SvgColourServer.Inherit && obj == SvgColourServer.Inherit)) return false;

            return this.GetHashCode() == objColor.GetHashCode();
        }

        public override int GetHashCode()
        {
            return _colour?.GetHashCode() ?? base.GetHashCode();
        }
    }
}
