using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Core.Interfaces;
using Svg.Core.Tools;
using Svg.Interfaces;
using Svg.Transforms;

namespace Svg.Core
{
    public class SvgDrawingCanvas : IDisposable, ICanInvalidateCanvas
    {
        private readonly ObservableCollection<SvgVisualElement> _selectedElements = new ObservableCollection<SvgVisualElement>();
        private readonly ObservableCollection<ITool> _tools;
        private List<IToolCommand> _toolSelectors = null;
        private SvgDocument _document;
        private Bitmap _rawImage;
        private bool _initialized = false;
        private ITool _activeTool;

        public event EventHandler CanvasInvalidated;
        public event EventHandler ToolCommandsChanged;

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
            _tools.CollectionChanged += OnToolsChanged;
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

        public IEnumerable<IEnumerable<IToolCommand>> ToolCommands
        {
            get
            {
                // prepare tool commands
                var commands = Tools.Select(t => t.Commands.OrderBy(tc => tc.Sort))
                    .OrderBy(t => t.FirstOrDefault()?.Sort ?? int.MaxValue)
                    .Cast<IEnumerable<IToolCommand>>()
                    .ToList();

                // prepare tool selectors
                var toolSelectors = EnsureToolSelectors().OrderBy(s => s.Sort);

                commands.Insert(0, toolSelectors);

                return commands;
            }
        }

        public PointF RelativeTranslate => Engine.Factory.CreatePointF(Translate.X/ZoomFactor, Translate.Y/ZoomFactor);

        public PointF Translate { get; set; }

        public float ZoomFactor { get; set; }

        public int ScreenWidth { get; private set; }

        public int ScreenHeight { get; private set; }

        public ITool ActiveTool
        {
            get { return _activeTool; }
            private set
            {
                _activeTool = value;
                if (_activeTool != null)
                {
                    _activeTool.IsActive = true;
                }
                foreach (var otherTool in Tools.Where(t => t != _activeTool && t.ToolUsage == ToolUsage.Explicit))
                {
                    otherTool.IsActive = false;
                }
            }
        }

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

        public async Task EnsureInitialized()
        {
            if (!_initialized)
            {
                foreach (var tool in Tools)
                    await tool.Initialize(this);

                ActiveTool = Tools.FirstOrDefault(t => t.ToolUsage == ToolUsage.Explicit);

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
        public IList<SvgVisualElement> GetElementsUnder(RectangleF selectionRectangle, SelectionType selectionType, int maxItems = int.MaxValue)
        {
            var selected = new List<SvgVisualElement>();
            // to speed up selection, this only takes first-level children into account!
            var children = Document?.Children.OfType<SvgVisualElement>() ?? Enumerable.Empty<SvgVisualElement>();
            // go through children in reverse order so we follow the z-index
            foreach (var child in children.Reverse())
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

                if (selected.Count >= maxItems)
                    break;
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

        public RectangleF CalculateDocumentBounds()
        {
            RectangleF documentSize = Engine.Factory.CreateRectangleF(0, 0, 0, 0);
            
            foreach (var element in Document.Children.OfType<SvgVisualElement>())
            {
                RectangleF bounds = element.Bounds;
                var m = element.Transforms?.GetMatrix();
                if (m != null)
                    bounds = m.TransformRectangle(bounds);

                documentSize = documentSize.UnionAndCopy(bounds);
            }
            
            return documentSize;
        }

        public void AddItemInScreenCenter(SvgVisualElement element)
        {
            var z = ZoomFactor;
            var halfRelWidth = ScreenWidth / z / 2;
            var halfRelHeight = ScreenHeight / z / 2;
            var childBounds = element.Bounds;
            var halfRelChildWidth = childBounds.Width / 2;
            var halfRelChildHeight = childBounds.Height / 2;

            SvgTranslate tl = new SvgTranslate(-RelativeTranslate.X + halfRelWidth - halfRelChildWidth, -RelativeTranslate.Y + halfRelHeight - halfRelChildHeight);
            element.Transforms.Add(tl);

            Document.Children.Add(element);

            FireInvalidateCanvas();
        }
        
        private IList<IToolCommand> EnsureToolSelectors()
        {
            if (_toolSelectors == null)
            {
                _toolSelectors = Tools.Where(t => t.ToolUsage == ToolUsage.Explicit).Select(t =>
                        new ToolCommand(t, t.Name, (obj) =>
                        {
                            ActiveTool = t;
                            FireToolCommandsChanged();
                        }, iconName: t.IconName, sortFunc: (tcmd) => ActiveTool == tcmd.Tool ? 0 : 100))
                        .Cast<IToolCommand>().OrderBy(c => c.Sort).ToList();
            }
            return _toolSelectors;
        }

        public void FireInvalidateCanvas()
        {
            CanvasInvalidated?.Invoke(this, EventArgs.Empty);
        }

        public void FireToolCommandsChanged()
        {
            ToolCommandsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnToolsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _toolSelectors = null;
            FireToolCommandsChanged();
        }
    }
}
