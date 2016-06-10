using System.Drawing;
using System.Linq;

namespace Svg.FilterEffects
{
	[SvgElement("feMerge")]
    public class SvgMerge : SvgFilterPrimitive
    {
        public override void Process(ImageBuffer buffer)
        {
            var children = this.Children.OfType<SvgMergeNode>().ToList();
            var inputImage = buffer[children.First().Input];
            var result = Engine.Factory.CreateBitmap(inputImage.Width, inputImage.Height);
            using (var g = Engine.Factory.CreateGraphicsFromImage(result))
            {
                foreach (var child in children)
                {
                    g.DrawImage(buffer[child.Input], Engine.Factory.CreateRectangleF(0, 0, inputImage.Width, inputImage.Height),
                                0, 0, inputImage.Width, inputImage.Height, GraphicsUnit.Pixel);
                }
                g.Flush();
            }
            result.Save(@"C:\test.png");
            buffer[this.Result] = result;
        }

		public override SvgElement DeepCopy()
		{
            return DeepCopy<SvgMerge>();
		}

    }
}