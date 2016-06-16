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
        private readonly ObservableCollection<SvgVisualElement> _selectedElements = new ObservableCollection<SvgVisualElement>();
        private readonly ObservableCollection<ITool> _tools;
        private SvgDocument _document;
        private Bitmap _rawImage;

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
                    new PanTool(this),
                    new ZoomTool(this),
            //        new SelectionTool(),
            //        new MoveSvgTool(),
                    new GridTool(this), // must be after zoom and pan tools!
            //        new SnappingTool(),
            };
        }

        public ObservableCollection<SvgVisualElement> SelectedElements => _selectedElements;

        public ObservableCollection<ITool> Tools => _tools;

        public SvgDocument Document
        {
            get
            {
                if (_document == null)
                {
                    _document = new SvgDocument();
                    _document.ViewBox = SvgViewBox.Empty;
                }
                return _document;
            }
            set
            {
                _document = value;
                if (_document != null)
                {
                    _document.ViewBox = SvgViewBox.Empty;
                }
            }
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
            //Debug.WriteLine($"{ev}");

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

            foreach(var tool in Tools)
                tool.Dispose();
        }

        public IEnumerable<IEnumerable<IToolCommand>> ToolCommands => Tools.Select(t => t.Commands);

        private ISvgRenderer GetOrCreateRenderer(Graphics graphics)
        {
            return SvgRenderer.FromGraphics(graphics);
        }
    }
}
