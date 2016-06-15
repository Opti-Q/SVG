using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Svg.Core.Events;
using Svg.Core.Interfaces;
using Svg.Core.Tools;
using Svg.Interfaces;

namespace Svg.Core
{
    public class SvgDrawingCanvas : IDisposable, ICanInvalidateCanvas
    {
        private readonly ObservableCollection<SvgElement> _selectedElements = new ObservableCollection<SvgElement>();
        private readonly ObservableCollection<ITool> _tools;
        private SvgDocument _document;
        private Bitmap _rawImage;
        private ISvgRenderer _svgRenderer;

        public event EventHandler CanvasInvalidated;

        public void InvalidateCanvas()
        {
            CanvasInvalidated?.Invoke(this, EventArgs.Empty);
        }

        public SvgDrawingCanvas()
        {
            Translate = Svg.Engine.Factory.CreatePointF(0f, 0f);
            ZoomFactor = 1f;

            _tools = new ObservableCollection<ITool>
            { 
                    new ZoomTool(),
            //        new SelectionTool(),
            //        new MoveSvgTool(),
                    new PanTool(),
                    new GridTool(this), // must be after zoom and pan tools!
            //        new SnappingTool(),
            };
        }

        public ObservableCollection<SvgElement> SelectedElements => _selectedElements;

        public ObservableCollection<ITool> Tools => _tools;

        public SvgDocument Document
        {
            get
            {
                if(_document == null)
                    _document = new SvgDocument();
                return _document;
            }
            set { _document = value; }
        }

        public Bitmap GetOrCreate(int width, int height)
        {
            return _rawImage ?? (_rawImage = Engine.Factory.CreateBitmap(width, height));
        }

        public PointF Translate { get; set; }

        public float ZoomFactor { get; set; }

        /// <summary>
        /// Called by the platform specific input event detector whenever the user interacts with the model
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="view"></param>
        public void OnEvent(UserInputEvent ev)
        {
            foreach (var tool in Tools)
            {
                tool.OnUserInput(ev, this);
            }
        }

        /// <summary>
        /// Called by platform specific implementation to allow tools to draw something onto the canvas
        /// </summary>
        /// <param name="renderer"></param>
        public void OnDraw(IRenderer renderer)
        {
            // draw default background
            renderer.FillEntireCanvasWithColor(Engine.Factory.Colors.White);

            // prerender step (e.g. gridlines, etc.)
            foreach (var tool in Tools)
            {
                tool.OnPreDraw(renderer, this);
            }

            // render svg step
            renderer.Graphics.Transform = renderer.Matrix; // set constant transform matrix, so svg document can be panned, zoomed, etc.
            Document.ViewBox = new SvgViewBox(0, 0, renderer.Width/ZoomFactor, renderer.Height/ZoomFactor); // set viewbox to have the same dimension as the canvas! (otherwise it would get clipped)
            Document.Draw(GetOrCreateRenderer(renderer.Graphics));

            // post render step (e.g. selection borders, etc.)
            foreach (var tool in Tools)
            {
                tool.OnDraw(renderer, this);
            }
        }

        public IEnumerable<SvgVisualElement> GetElementsUnder(float x, float y)
        {
            var hitRectangle = Svg.Engine.Factory.CreateRectangleF(x, y, 10, 10);
            return Document.Children.OfType<SvgVisualElement>().Where(c => c.Visible && c.Displayable && c.Bounds.IntersectsWith(hitRectangle));
            //return Enumerable.Empty<SvgVisualElement>();
        }

        public void Dispose()
        {
            _rawImage?.Dispose();
            _svgRenderer?.Dispose();

            foreach(var tool in Tools)
                tool.Dispose();
        }

        public IEnumerable<IEnumerable<IToolCommand>> ToolCommands => Tools.Select(t => t.Commands);

        private ISvgRenderer GetOrCreateRenderer(Graphics graphics)
        {
            //return _svgRenderer ?? (_svgRenderer = SvgRenderer.FromGraphics(graphics));
            return SvgRenderer.FromGraphics(graphics);
        }
    }
}
