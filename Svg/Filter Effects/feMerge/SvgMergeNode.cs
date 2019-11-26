using System;
using System.Collections.Generic;
using System.Text;


namespace Svg.FilterEffects
{

	[SvgElement("feMergeNode")]
    public class SvgMergeNode : SvgElement
    {
        [SvgAttribute("in")]
        public string Input
        {
            get { return this.Attributes.GetAttribute<string>("in"); }
            set { this.Attributes["in"] = value; }
        }

		public override SvgElement DeepCopy()
		{
		    var e = (SvgMergeNode)base.DeepCopy<SvgMergeNode>();
		    e.Input = this.Input;
		    return e;
		}

    }
}