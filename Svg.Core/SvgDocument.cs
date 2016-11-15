using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;
using ExCSS;
using Svg.Css;
using System.Threading;
using System.Globalization;
using Svg.Interfaces;
using Svg.Interfaces.Xml;
using Svg.Transforms;

namespace Svg
{
    /// <summary>
    /// The class used to create and load SVG documents.
    /// </summary>
    public partial class SvgDocument : SvgFragment //, ITypeDescriptorContext
    {
        public static readonly int PointsPerInch = 96;
        private SvgElementIdManager _idManager;

        private Dictionary<string, IEnumerable<SvgFontFace>> _fontDefns = null;
        private IFileSystem _fileSystem;

        internal Dictionary<string, IEnumerable<SvgFontFace>> FontDefns()
        {
            if (_fontDefns == null)
            {
                _fontDefns = (from f in Descendants().OfType<SvgFontFace>()
                              group f by f.FontFamily into family
                              select family).ToDictionary(f => f.Key, f => (IEnumerable<SvgFontFace>) f);
            }
            return _fontDefns;
        }

        public event EventHandler<SvgElement> ContentModified;

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgDocument"/> class.
        /// </summary>
        public SvgDocument()
        {
            Ppi = PointsPerInch;
        }

        public Uri BaseUri { get; set; }

        /// <summary>
        /// Gets an <see cref="SvgElementIdManager"/> for this document.
        /// </summary>
        public virtual SvgElementIdManager IdManager
        {
            get
            {
                if (_idManager == null)
                {
                    _idManager = new SvgElementIdManager(this);
                }

                return _idManager;
            }
        }

        /// <summary>
        /// Overwrites the current IdManager with a custom implementation. 
        /// Be careful with this: If elements have been inserted into the document before,
        /// you have to take care that the new IdManager also knows of them.
        /// </summary>
        /// <param name="manager"></param>
        public void OverwriteIdManager(SvgElementIdManager manager)
        {
            _idManager = manager;
        }

        /// <summary>
        /// Gets or sets the Pixels Per Inch of the rendered image.
        /// </summary>
        public int Ppi { get; set; }

        /// <summary>
        /// Gets or sets an external Cascading Style Sheet (CSS)
        /// </summary>
        public string ExternalCSSHref { get; set; }

        internal ISvgSource SvgSource { get; set; }

        private IFileSystem FileSystem
        {
            get { return _fileSystem ?? (_fileSystem = Engine.Resolve<IFileSystem>()); }
        }

        /// <summary>
        /// Retrieves the <see cref="SvgElement"/> with the specified ID.
        /// </summary>
        /// <param name="id">A <see cref="string"/> containing the ID of the element to find.</param>
        /// <returns>An <see cref="SvgElement"/> of one exists with the specified ID; otherwise false.</returns>
        public virtual SvgElement GetElementById(string id)
        {
            return IdManager.GetElementById(id);
        }

        /// <summary>
        /// Retrieves the <see cref="SvgElement"/> with the specified ID.
        /// </summary>
        /// <param name="id">A <see cref="string"/> containing the ID of the element to find.</param>
        /// <returns>An <see cref="SvgElement"/> of one exists with the specified ID; otherwise false.</returns>
        public virtual TSvgElement GetElementById<TSvgElement>(string id) where TSvgElement : SvgElement
        {
            return (this.GetElementById(id) as TSvgElement);
        }

        /// <summary>
        /// Opens the document at the specified path and loads the SVG contents.
        /// </summary>
        /// <param name="path">A <see cref="string"/> containing the path of the file to open.</param>
        /// <returns>An <see cref="SvgDocument"/> with the contents loaded.</returns>
        /// <exception cref="FileNotFoundException">The document at the specified <paramref name="path"/> cannot be found.</exception>
        public static SvgDocument Open(string path)
        {
            return Open<SvgDocument>(path, null);
        }

        /// <summary>
        /// Opens the document at the specified path and loads the SVG contents.
        /// </summary>
        /// <param name="path">A <see cref="string"/> containing the path of the file to open.</param>
        /// <returns>An <see cref="SvgDocument"/> with the contents loaded.</returns>
        /// <exception cref="FileNotFoundException">The document at the specified <paramref name="path"/> cannot be found.</exception>
        public static T Open<T>(string path) where T : SvgDocument, new()
        {
            return Open<T>(path, null);
        }

        /// <summary>
        /// Opens the document at the specified path and loads the SVG contents.
        /// </summary>
        /// <param name="path">A <see cref="string"/> containing the path of the file to open.</param>
        /// <param name="entities">A dictionary of custom entity definitions to be used when resolving XML entities within the document.</param>
        /// <returns>An <see cref="SvgDocument"/> with the contents loaded.</returns>
        /// <exception cref="FileNotFoundException">The document at the specified <paramref name="path"/> cannot be found.</exception>
        public static T Open<T>(string path, Dictionary<string, string> entities) where T : SvgDocument, new()
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            var temp = new OpenResult<T>();
            OpenPartial(path, entities, temp);
            if (temp.Result != null)
                return temp.Result;

            var fs = Engine.Resolve<IFileSystem>();

            if (!fs.FileExists(path))
            {
                throw new FileNotFoundException($"The specified document cannot be found: {path}");
            }

            using (var stream = fs.OpenRead(path))
            {
                var doc = Open<T>(stream, entities);
                doc.BaseUri = new Uri(fs.GetFullPath(path));
                return doc;
            }
        }

        static partial void OpenPartial<T>(string path, Dictionary<string, string> entities, OpenResult<T> result) where T : SvgDocument, new();

        /// <summary>
        /// Attempts to open an SVG document from the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the SVG document to open.</param>
        public static T Open<T>(Stream stream) where T : SvgDocument, new()
        {
            return Open<T>(stream, null);
        }

        /// <summary>
        /// Allows to provide an arbitrary source (e.g. Android Asset)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="svgSource"></param>
        /// <returns></returns>
        public static T Open<T>(ISvgSource svgSource) where T : SvgDocument, new()
        {
            using (var str = svgSource.GetStream())
            {
                var doc = Open<T>(str, null);

                doc.SvgSource = svgSource;

                return doc;
            }
        }

        /// <summary>
        /// Attempts to create an SVG document from the specified string data.
        /// </summary>
        /// <param name="svg">The SVG data.</param>
        public static T FromSvg<T>(string svg) where T : SvgDocument, new()
        {
            if (string.IsNullOrEmpty(svg))
            {
                throw new ArgumentNullException("svg");
            }

            using (var strReader = new System.IO.StringReader(svg))
            {
                //var reader = new SvgTextReader(strReader, null);
                //reader.XmlResolver = new SvgDtdResolver();
                //reader.WhitespaceHandling = WhitespaceHandling.None;

                using (var reader = Engine.Factory.CreateSvgTextReader(strReader, null))
                    return Open<T>(reader);
            }
        }

        /// <summary>
        /// Opens an SVG document from the specified <see cref="Stream"/> and adds the specified entities.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the SVG document to open.</param>
        /// <param name="entities">Custom entity definitions.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="stream"/> parameter cannot be <c>null</c>.</exception>
        public static T Open<T>(Stream stream, Dictionary<string, string> entities) where T : SvgDocument, new()
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var settings = new XmlReaderSettings { NameTable = new NameTable() };
            var xmlns = new XmlNamespaceManager(settings.NameTable);
            foreach (var @namespace in SvgAttributeAttribute.Namespaces)
                xmlns.AddNamespace(@namespace.Key, @namespace.Value);
            var context = new XmlParserContext(null, xmlns, "", XmlSpace.Default);

            // Don't close the stream via a dispose: that is the client's job.
            //var reader = new SvgTextReader(stream, entities);
            //reader.XmlResolver = new SvgDtdResolver();
            //reader.WhitespaceHandling = WhitespaceHandling.None;
            using (var reader = Engine.Factory.CreateSvgTextReader(stream, entities))
                return Open<T>(reader);
        }

        private static T Open<T>(XmlReader reader) where T : SvgDocument, new()
        {
            var elementStack = new Stack<SvgElement>();
            bool elementEmpty;
            SvgElement element = null;
            SvgElement parent;
            T svgDocument = null;

            var styles = new List<ISvgNode>();

            while (reader.Read())
            {
                try
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            // Does this element have a value or children
                            // (Must do this check here before we progress to another node)
                            elementEmpty = reader.IsEmptyElement;
                            var factory = Engine.Resolve<ISvgElementFactory>();
                            // Create element
                            if (elementStack.Count > 0)
                            {
                                element = factory.CreateElement(reader, svgDocument);
                            }
                            else
                            {
                                svgDocument = factory.CreateDocument<T>(reader);
                                element = svgDocument;
                            }

                            // Add to the parents children
                            if (elementStack.Count > 0)
                            {
                                parent = elementStack.Peek();
                                if (parent != null && element != null)
                                {
                                    parent.Children.Add(element);
                                    parent.Nodes.Add(element);
                                }
                            }

                            // Push element into stack
                            elementStack.Push(element);

                            // Need to process if the element is empty
                            if (elementEmpty)
                            {
                                goto case XmlNodeType.EndElement;
                            }

                            break;
                        case XmlNodeType.EndElement:

                            // Pop the element out of the stack
                            element = elementStack.Pop();

                            if (element.Nodes.OfType<SvgContentNode>().Any())
                            {
                                element.Content = (from e in element.Nodes select e.Content).Aggregate((p, c) => p + c);
                            }
                            else
                            {
                                element.Nodes.Clear(); // No sense wasting the space where it isn't needed
                            }

                            var unknown = element as SvgUnknownElement;
                            if (unknown != null && unknown.ElementName == "style")
                            {
                                styles.Add(unknown);
                            }
                            break;
                        case XmlNodeType.CDATA:
                        case XmlNodeType.Text:
                            element = elementStack.Peek();
                            element.Nodes.Add(new SvgContentNode() { Content = reader.Value });
                            break;
                        case XmlNodeType.SignificantWhitespace:
                            if (elementStack.Count > 0 && elementStack.Peek() is SvgTextSpan)
                            {
                                element = elementStack.Peek();
                                element.Nodes.Add(new SvgContentNode() { Content = reader.Value });
                            }
                            break;
                        case XmlNodeType.EntityReference:
                            reader.ResolveEntity();
                            element = elementStack.Peek();
                            element.Nodes.Add(new SvgContentNode() { Content = reader.Value });
                            break;
                    }
                }
                catch (Exception exc)
                {
                    //Trace.TraceError(exc.Message);
                }
            }

            if (styles.Any())
            {
                var cssTotal = styles.Select((s) => s.Content).Aggregate((p, c) => p + Environment.NewLine + c);
                var cssParser = new Parser();
                var sheet = cssParser.Parse(cssTotal);
                AggregateSelectorList aggList;
                IEnumerable<BaseSelector> selectors;
                IEnumerable<SvgElement> elemsToStyle;

                foreach (var rule in sheet.StyleRules)
                {
                    aggList = rule.Selector as AggregateSelectorList;
                    if (aggList != null && aggList.Delimiter == ",")
                    {
                        selectors = aggList;
                    }
                    else
                    {
                        selectors = Enumerable.Repeat(rule.Selector, 1);
                    }

                    foreach (var selector in selectors)
                    {
                        elemsToStyle = svgDocument.QuerySelectorAll(rule.Selector.ToString());
                        foreach (var elem in elemsToStyle)
                        {
                            foreach (var decl in rule.Declarations)
                            {
                                elem.AddStyle(decl.Name, decl.Term.ToString(), rule.Selector.GetSpecificity());
                            }
                        }
                    }
                }
            }

            if (svgDocument != null) FlushStyles(svgDocument);
            return svgDocument;
        }

        private static void FlushStyles(SvgElement elem)
        {
            elem.FlushStyles();
            foreach (var child in elem.Children)
            {
                FlushStyles(child);
            }
        }

        ///// <summary>
        ///// Opens an SVG document from the specified <see cref="XmlDocument"/>.
        ///// </summary>
        ///// <param name="document">The <see cref="XmlDocument"/> containing the SVG document XML.</param>
        ///// <exception cref="ArgumentNullException">The <paramref name="document"/> parameter cannot be <c>null</c>.</exception>
        //public static SvgDocument Open(XmlDocument document)
        //{
        //    if (document == null)
        //    {
        //        throw new ArgumentNullException("document");
        //    }

        //    var reader = new SvgNodeReader(document, null);
        //    return Open<SvgDocument>(reader);
        //}

        public static Bitmap OpenAsBitmap(string path)
        {
            return null;
        }

        public static Bitmap OpenAsBitmap(IXmlDocument document)
        {
            return null;
        }

        /// <summary>
        /// Renders the <see cref="SvgDocument"/> to the specified <see cref="ISvgRenderer"/>.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> to render the document with.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="renderer"/> parameter cannot be <c>null</c>.</exception>
        public void Draw(ISvgRenderer renderer)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException("renderer");
            }

            renderer.SetBoundable(this);
            this.Render(renderer);
        }

        /// <summary>
        /// Renders the <see cref="SvgDocument"/> to the specified <see cref="Graphics"/>.
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> to be rendered to.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="graphics"/> parameter cannot be <c>null</c>.</exception>
        public void Draw(Graphics graphics)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }

            var renderer = SvgRenderer.FromGraphics(graphics);
            renderer.SetBoundable(this);
            this.Render(renderer);
        }

        /// <summary>
        /// Renders the <see cref="SvgDocument"/> and returns the image as a <see cref="Bitmap"/>.
        /// </summary>
        /// <returns>A <see cref="Bitmap"/> containing the rendered document.</returns>
        public virtual Bitmap Draw()
        {
            //Trace.TraceInformation("Begin Render");

            var size = GetDimensions();
            var bitmap = Bitmap.Create((int) Math.Round(size.Width), (int) Math.Round(size.Height));
            // 	bitmap.SetResolution(300, 300);
            try
            {
                Draw(bitmap);
            }
            catch
            {
                bitmap.Dispose();
                throw;
            }

            //Trace.TraceInformation("End Render");
            return bitmap;
        }


        /// <summary>
        /// Renders the <see cref="SvgDocument"/> into a given Bitmap <see cref="Bitmap"/>.
        /// </summary>
        public virtual void Draw(Bitmap bitmap)
        {
            Draw(bitmap, null);
        }


        /// <summary>
        /// Renders the <see cref="SvgDocument"/> into a given Bitmap <see cref="Bitmap"/>.
        /// </summary>
        public virtual void Draw(Bitmap bitmap, Color backgroundColor)
        {
            try
            {
                using (var renderer = SvgRenderer.FromImage(bitmap))
                {
                    if (backgroundColor != null)
                        renderer.FillBackground(backgroundColor);

                    renderer.SetBoundable(new GenericBoundable(0, 0, bitmap.Width, bitmap.Height));

                    //EO, 2014-12-05: Requested to ensure proper zooming (draw the svg in the bitmap size, ==> proper scaling)
                    //EO, 2015-01-09, Added GetDimensions to use its returned size instead of this.Width and this.Height (request of Icarrere).
                    var size = this.GetDimensions();
                    renderer.ScaleTransform(bitmap.Width / size.Width, bitmap.Height / size.Height);

                    //EO, 2014-12-05: Requested to ensure proper zooming out (reduce size). Otherwise it clip the image.
                    this.Overflow = SvgOverflow.Auto;

                    this.Render(renderer);
                }
            }
            catch
            {
                throw;
            }
        }

        public Bitmap DrawAllContents(Color backgroundColor = null)
        {
            var bounds = CalculateDocumentBounds();
            return DrawAllContents((int) bounds.Width, (int) bounds.Height, backgroundColor);
        }

        public Bitmap DrawAllContents(int maxWidth, int maxHeight, Color backgroundColor = null)
        {
            Bitmap bitmap = null;
            try
            {
                var bounds = CalculateDocumentBounds();
                var width = bounds.Width;
                var height = bounds.Height;

                if (width > maxWidth)
                {
                    var factor = maxWidth / width;
                    height = height * factor;
                    width = maxWidth;
                }
                if (height > maxHeight)
                {
                    var factor = maxHeight / height;
                    width = width * factor;
                    height = maxHeight;
                }

                bitmap = Bitmap.Create((int) width, (int) height);
                DrawAllContents(bitmap, backgroundColor);
                return bitmap;
            }
            catch
            {
                bitmap?.Dispose();
                throw;
            }
        }


        public Bitmap DrawAllContents(int maxWidthHeight, Color backgroundColor = null)
        {
            Bitmap bitmap = null;
            try
            {

                var bounds = CalculateDocumentBounds();
                var width = bounds.Width;
                var height = bounds.Height;

                var isPanorama = bounds.Width >= bounds.Height;
                if (isPanorama)
                {
                    if (bounds.Width > maxWidthHeight)
                    {
                        var factor = maxWidthHeight / bounds.Width;
                        height = height * factor;
                        width = maxWidthHeight;
                    }
                }
                else
                {
                    if (bounds.Height > maxWidthHeight)
                    {
                        var factor = maxWidthHeight / bounds.Height;
                        width = width * factor;
                        height = maxWidthHeight;
                    }
                }
                bitmap = Bitmap.Create((int) width, (int) height);
                DrawAllContents(bitmap, backgroundColor);
                return bitmap;
            }
            catch
            {
                bitmap?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Canculates the bounds of all visual children and then 
        /// adapts the X, Y, Width and Height properties of the document
        /// to contain those bounds
        /// </summary>
        public void AdaptCanvasSizeToElementBounds()
        {
            var bounds = CalculateDocumentBounds().InflateAndCopy(10, 10);
            X = new SvgUnit(SvgUnitType.Pixel, bounds.X);
            Y = new SvgUnit(SvgUnitType.Pixel, bounds.Y);
            Width = new SvgUnit(SvgUnitType.Pixel, bounds.Width);
            Height = new SvgUnit(SvgUnitType.Pixel, bounds.Height);

        }

        public void DrawAllContents(Bitmap bitmap, Color backgroundColor = null)
        {
            // draw document
            var oldX = X;
            var oldY = Y;
            var oldWidth = Width;
            var oldHeight = Height;
            var oldViewBox = ViewBox;
            var oldAspectRatio = AspectRatio;
            try
            {
                var bounds = CalculateDocumentBounds().InflateAndCopy(10, 10);
                X = new SvgUnit(SvgUnitType.Pixel, 0);
                Y = new SvgUnit(SvgUnitType.Pixel, 0);
                Width = new SvgUnit(SvgUnitType.Pixel, bounds.Width);
                Height = new SvgUnit(SvgUnitType.Pixel, bounds.Height);


                AspectRatio = new SvgAspectRatio(SvgPreserveAspectRatio.xMinYMin);
                ViewBox = new SvgViewBox(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                Draw(bitmap, backgroundColor);
            }
            finally
            {
                ViewBox = oldViewBox;
                AspectRatio = oldAspectRatio;
                Height = oldHeight;
                Width = oldWidth;
                X = oldX;
                Y = oldY;
            }
        }

        public RectangleF CalculateDocumentBounds()
        {
            RectangleF documentSize = null;

            foreach (var element in Children.OfType<SvgVisualElement>())
            {
                var bounds = element.GetBoundingBox();

                if (documentSize == null)
                    documentSize = bounds;
                else
                    documentSize = documentSize.UnionAndCopy(bounds);
            }

            return documentSize ?? RectangleF.Create();
        }

        public override void Write(IXmlTextWriter writer)
        {
            using (var c = Engine.Resolve<ICultureHelper>().UsingCulture(CultureInfo.InvariantCulture))
            {
                base.Write(writer);
            }
        }

        public void Write(Stream stream)
        {
            var xmlWriter = Engine.Factory.CreateXmlTextWriter(stream, Encoding.UTF8);

            xmlWriter.WriteStartDocument();
            //xmlWriter.WriteDocType("svg", "-//W3C//DTD SVG 1.1//EN", "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd", null);

            if (!String.IsNullOrEmpty(this.ExternalCSSHref))
                xmlWriter.WriteProcessingInstruction("xml-stylesheet", String.Format("type=\"text/css\" href=\"{0}\"", this.ExternalCSSHref));

            this.Write(xmlWriter);

            xmlWriter.Flush();
        }

        public void Write(string path)
        {
            using (var fs = Engine.Resolve<IFileSystem>().OpenWrite(path))
            {
                this.Write(fs);
            }
        }

        private class OpenResult<T>
            where T : SvgDocument, new()
        {
            public T Result { get; set; }
        }

        public void Resize(float scale)
        {
            if (this.Transforms == null)
                this.Transforms = new SvgTransformCollection();

            this.Transforms.Add(new SvgScale(scale, scale));

            this.Width = new SvgUnit(SvgUnitType.Pixel, this.Width * scale);
            this.Height = new SvgUnit(SvgUnitType.Pixel, this.Height * scale);
        }

        public void Transpose(float x, float y)
        {
            if (this.Transforms == null)
                this.Transforms = new SvgTransformCollection();

            this.Transforms.Add(new SvgTranslate(x, y));

            this.X += new SvgUnit(SvgUnitType.Pixel, x);
            this.Y += new SvgUnit(SvgUnitType.Pixel, y);
        }

        protected override void OnSubTreeChanged(SvgElement svgElement)
        {
            ContentModified?.Invoke(this, svgElement);
        }

        public override void Dispose()
        {
            foreach (var c in Descendants())
                c.Dispose();
            base.Dispose();
        }
    }
}
