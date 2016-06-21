using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
        private bool _initialized = false;

        public event EventHandler CanvasInvalidated;
        public event EventHandler ToolCommandsChanged;

        public void FireInvalidateCanvas()
        {
            CanvasInvalidated?.Invoke(this, EventArgs.Empty);
        }

        public void FireToolCommandsChanged()
        {
            ToolCommandsChanged?.Invoke(this, EventArgs.Empty);
        }

        public SvgDrawingCanvas()
        {
            Translate = Svg.Engine.Factory.CreatePointF(0f, 0f);
            ZoomFactor = 1f;

            _tools = new ObservableCollection<ITool>
            {
                    new GridTool(angle:27.3f), // must be before movetool!
                    new MoveTool(), // must be before pantool as it decides whether or not it is active based on selection
                    new PanTool(),
                    new ZoomTool(),
                    new TextTool(),
                    new SelectionTool(),
            };
        }

        public SvgDocument Document
        {
            get
            {
                if (_document == null)
                {
                    _document = new SvgDocument();
                    _document.ViewBox = SvgViewBox.Empty;

                    OnDocumentChanged(null, _document);
                }
                return _document;
            }
            set
            {
                var oldDocument = _document;
                _document = value;
                if (_document != null)
                {
                    _document.ViewBox = SvgViewBox.Empty;
                }

                OnDocumentChanged(oldDocument, _document);
            }
        }

        private void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            // fire document changed
            foreach (var tool in Tools)
                tool.OnDocumentChanged(oldDocument, newDocument);

            // selection is not valid anymore
            SelectedElements.Clear();

            // also reset translate and zoomfactor
            Translate = Svg.Engine.Factory.CreatePointF(0f, 0f);
            ZoomFactor = 1f;

            // re-render
            FireInvalidateCanvas();
        }

        public ObservableCollection<SvgVisualElement> SelectedElements => _selectedElements;

        public ObservableCollection<ITool> Tools => _tools;

        public IEnumerable<IEnumerable<IToolCommand>> ToolCommands => Tools.Select(t => t.Commands);

        public PointF RelativeTranslate => Engine.Factory.CreatePointF(Translate.X/ZoomFactor, Translate.Y/ZoomFactor);

        public PointF Translate { get; set; }

        public float ZoomFactor { get; set; }

        public int ScreenWidth { get; private set; }

        public int ScreenHeight { get; private set; }

        /// <summary>
        /// Called by the platform specific input event detector whenever the user interacts with the model
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="view"></param>
        public async Task OnEvent(UserInputEvent ev)
        {
            await EnsureInitialized();

            foreach (var tool in Tools)
            {
                await tool.OnUserInput(ev, this);
            }
        }
        
        /// <summary>
        /// Called by platform specific implementation to allow tools to draw something onto the canvas
        /// </summary>
        /// <param name="renderer"></param>
        public async Task OnDraw(IRenderer renderer)
        {
            // make sure all tools have been initialized successfully
            await EnsureInitialized();

            this.ScreenWidth = renderer.Width;
            this.ScreenHeight = renderer.Height;

            // apply global panning and zooming
            renderer.Translate(Translate.X, Translate.Y);
            renderer.Scale(ZoomFactor, 0f, 0f);

            // draw default background
            renderer.FillEntireCanvasWithColor(Engine.Factory.Colors.White);
            
            // prerender step (e.g. gridlines, etc.)
            foreach (var tool in Tools)
            {
                await tool.OnPreDraw(renderer, this);
            }

            // render svg step
            renderer.Graphics.Save();
            Document.Draw(GetOrCreateRenderer(renderer.Graphics));
            renderer.Graphics.Restore();

            // post render step (e.g. selection borders, etc.)
            foreach (var tool in Tools)
            {
                await tool.OnDraw(renderer, this);
            }
        }

        private async Task EnsureInitialized()
        {
            if (!_initialized)
            {
                foreach (var tool in Tools)
                    await tool.Initialize(this);

                _initialized = true;
                
                FireToolCommandsChanged();
            }
        }
        
        public void Dispose()
        {
            _rawImage?.Dispose();

            foreach(var tool in Tools)
                tool.Dispose();
        }

        private ISvgRenderer GetOrCreateRenderer(Graphics graphics)
        {
            return SvgRenderer.FromGraphics(graphics);
        }

        public Bitmap GetOrCreate(int width, int height)
        {
            return _rawImage ?? (_rawImage = Engine.Factory.CreateBitmap(width, height));
        }

        /// <summary>
        /// Returns a rectangle with width and height 20px that surrounds the given point
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public RectangleF GetPointerRectangle(PointF p)
        {
            float halfFingerThickness = 10 / ZoomFactor;
            return Engine.Factory.CreateRectangleF(p.X - halfFingerThickness, p.Y - halfFingerThickness, halfFingerThickness * 2, halfFingerThickness * 2); // "10 pixel fat finger"
        }

        /// <summary>
        /// the selection rectangle must be in absolute screen coordinates (so not transformed by canvas.Translate or canvas.ZoomFactor)
        /// </summary>
        /// <param name="selectionRectangle"></param>
        /// <param name="selectionType"></param>
        /// <returns></returns>
        public IList<SvgVisualElement> GetElementsUnder(RectangleF selectionRectangle, SelectionType selectionType)
        {
            var selected = new List<SvgVisualElement>();
            // to speed up selection, this only takes first-level children into account!
            var children = Document?.Children.OfType<SvgVisualElement>() ?? Enumerable.Empty<SvgVisualElement>();
            foreach (var child in children)
            {
                // get its transformed boundingbox (renderbounds)
                var renderBounds = child.RenderBounds;

                // then check if it intersects with selectionrectangle
                if (selectionType == SelectionType.Intersect && selectionRectangle.IntersectsWith(renderBounds))
                {
                    selected.Add(child);
                }
                // then check if the selectionrectangle contains it
                else if (selectionType == SelectionType.Contain && selectionRectangle.Contains(renderBounds))
                {
                    selected.Add(child);
                }
            }
            return selected;
        }

        /// <summary>
        /// gets all visual elements under the given pointer (a 20px rectangle surrounding the given point to simulate thick finger)
        /// </summary>
        /// <param name="pointer1Position"></param>
        /// <returns></returns>
        public IList<SvgVisualElement> GetElementsUnderPointer(PointF pointer1Position)
        {
            return GetElementsUnder(GetPointerRectangle(pointer1Position), SelectionType.Intersect);
        }
    }
}
