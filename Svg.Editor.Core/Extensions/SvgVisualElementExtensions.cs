using System;
using System.IO;
using System.Linq;
using Svg.Editor.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Extensions
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

            m.RotateAt(angleDegrees, PointF.Create(b.X + b.Width/2, b.Y + b.Height/2), MatrixOrder.Prepend);

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

        public static bool HasConstraints(this SvgElement e, params string[] attributes)
        {
            if (!attributes.Any()) return true;

            string constraints;
            return e.CustomAttributes.TryGetValue(ToolBase.ConstraintsCustomAttributeKey, out constraints) &&
                   !string.IsNullOrEmpty(constraints) && constraints.Split(',').Any(attributes.Contains);
        }

        public static void AddConstraints(this SvgElement e, params string[] attributes)
        {
            if (!attributes.Any()) return;

            string constraints;
            if (e.CustomAttributes.TryGetValue(ToolBase.ConstraintsCustomAttributeKey, out constraints) &&
                !string.IsNullOrEmpty(constraints))
            {
                var joined = string.Join(",",
                    constraints.Split(',')
                        .Where(x => !attributes.Contains(x)));
                if (!string.IsNullOrEmpty(joined))
                    constraints = $"{constraints},{joined}";
            }
            else
            {
                constraints = $"{string.Join(",", attributes)}";
            }
            e.CustomAttributes[ToolBase.ConstraintsCustomAttributeKey] = constraints;
        }

        public static SizeF GetImageSize(this SvgImage image)
        {
            // handle data/uri embedded images (http://en.wikipedia.org/wiki/Data_URI_scheme)
            if (image.Href.IsAbsoluteUri && image.Href.Scheme == "data")
            {
                string uriString = image.Href.OriginalString;
                int dataIdx = uriString.IndexOf(",") + 1;
                if (dataIdx <= 0 || dataIdx + 1 > uriString.Length)
                    throw new Exception("Invalid data URI");

                // we're assuming base64, as ascii encoding would be *highly* unsusual for images
                // also assuming it's png or jpeg mimetype
                byte[] imageBytes = Convert.FromBase64String(uriString.Substring(dataIdx));
                using (var stream = new MemoryStream(imageBytes))
                {
                    var img = Engine.Factory.CreateImageFromStream(stream);
                    return SizeF.Create(img.Width, img.Height);
                }
            }

            if (!image.Href.IsAbsoluteUri && image.OwnerDocument.BaseUri != null)
            {
                image.Href = new Uri(image.OwnerDocument.BaseUri, image.Href);
            }

            //// should work with http: and file: protocol urls
            //var httpRequest = WebRequest.Create(uri);
            //using (WebResponse webResponse = httpRequest.GetResponse())
            using (var webResponse = Engine.Resolve<IWebRequest>().GetResponse(image.Href))
            {
                using (var stream = webResponse.GetResponseStream())
                {
                    stream.Position = 0;
                    if (image.Href.LocalPath.ToLowerInvariant().EndsWith(".svg"))
                    {
                        var doc = SvgDocument.Open<SvgDocument>(stream);
                        doc.BaseUri = image.Href;
                        return doc.GetDimensions();
                    }
                    else
                    {
                        var img = Engine.Factory.CreateBitmapFromStream(stream);
                        return SizeF.Create(img.Width, img.Height);
                    }
                }
            }
        }
    }
}
