using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Svg.Core.Annotations;
using Svg.Core.Events;
using Svg.Core.Interfaces;
using Svg.Core.Tools;
using Svg.Interfaces;

namespace Svg.Core
{
    public sealed class SvgDrawingCanvas : IDisposable, ICanInvalidateCanvas, INotifyPropertyChanged
    {
        private readonly ObservableCollection<SvgVisualElement> _selectedElements;
        private readonly ObservableCollection<ITool> _tools;
        private List<IToolCommand> _toolSelectors;
        private SvgDocument _document;
        private bool _initialized;
        private ITool _activeTool;
        private bool _isDebugEnabled;
        private bool _documentIsDirty;
        private PointF _zoomFocus;

        public event EventHandler CanvasInvalidated;
        public event EventHandler ToolCommandsChanged;

        public SvgDrawingCanvas(float constraintLeft = float.MinValue, float constraintTop = float.MinValue, float constraintRight = float.MaxValue, float constraintBottom = float.MaxValue)
        {
            ConstraintLeft = constraintLeft;
            ConstraintTop = constraintTop;
            ConstraintRight = constraintRight;
            ConstraintBottom = constraintBottom;

            Translate = PointF.Create(0f, 0f);
            ZoomFactor = 1f;

            _selectedElements = new ObservableCollection<SvgVisualElement>();
            _selectedElements.CollectionChanged += OnSelectionChanged;

            #region Tool properties

            // this part should be in the designer, when the iCL is created
            var gridToolProperties = new Dictionary<string, object>
            {
                { "angle", 30.0f },
                { "stepsizey", 20.0f },
                { "issnappingenabled", true }
            };

            var colorToolProperties = new Dictionary<string, object>
            {
                { "selectablecolors", new [] { "#000000","#FF0000","#00FF00","#0000FF","#FFFF00","#FF00FF","#00FFFF" } }
            };

            var lineToolProperties = new Dictionary<string, object>
            {
                { "markerstartids", new [] { "none", "arrowStart", "circle" } },
                { "markerstartnames", new [] { "---", "<--", "O--" } },
                { "markerendids", new [] { "none", "arrowEnd", "circle" } },
                { "markerendnames", new [] { "---", "-->", "--O" } },
                { "linestyles", new [] { "normal", "dashed" } },
                { "linestylenames", new [] { "-----", "- - -" } }
            };

            var freeDrawToolProperties = new Dictionary<string, object>
            {
                { "linestyles", new [] { "normal", "dashed" } },
                { "linestylenames", new [] { "-----", "- - -" } },
                { "strokewidths", new [] { 12, 24, 6 } },
                { "strokewidthnames", new [] { "normal", "thick", "thin" } }
            };

            var textToolProperties = new Dictionary<string, object>
            {
                { "fontsizes", new [] { 12f, 16f, 20f, 24f, 36f, 48f } },
                { "selectedfontsizeindex", 1 },
                { "fontsizenames", new [] { "12px", "16px", "20px", "24px", "36px", "48px" } }
            };

            //var zoomToolProperties = JsonConvert.SerializeObject(new Dictionary<string, object>
            //{
            //    { "minscale", 1.0f },
            //    { "maxscale", 5.0f }
            //}, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

            var zoomToolProperties = new Dictionary<string, object>();

            var panToolProperties = new Dictionary<string, object>();

            #endregion

            var undoRedoService = Engine.Resolve<IUndoRedoService>();

            _tools = new ObservableCollection<ITool>
            {
                    new GridTool(gridToolProperties, undoRedoService),
                    new MoveTool(undoRedoService),
                    new PanTool(panToolProperties),
                    new RotationTool(undoRedoService),
                    new ZoomTool(zoomToolProperties),
                    new SelectionTool(undoRedoService),
                    new TextTool(textToolProperties, undoRedoService),
                    new LineTool(lineToolProperties, undoRedoService),
                    new FreeDrawingTool(freeDrawToolProperties, undoRedoService),
                    new ColorTool(colorToolProperties, undoRedoService),
                    new StrokeStyleTool(undoRedoService),
                    new UndoRedoTool(undoRedoService),
                    new ArrangeTool(undoRedoService)
            };
            _tools.CollectionChanged += OnToolsChanged;
        }

        #region Public properties

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

        public bool DocumentIsDirty
        {
            get { return _documentIsDirty; }
            private set
            {
                if (_documentIsDirty != value)
                {
                    _documentIsDirty = value;
                    OnPropertyChanged();
                }
            }
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

        public PointF RelativeTranslate => PointF.Create(Translate.X / ZoomFactor, Translate.Y / ZoomFactor);

        public PointF Translate { get; set; }

        public float ZoomFactor { get; set; }

        public PointF ZoomFocus
        {
            get { return _zoomFocus ?? (_zoomFocus = PointF.Create(0, 0)); }
            set { _zoomFocus = value; }
        }

        public int ScreenWidth { get; set; }

        public int ScreenHeight { get; set; }

        public PointF ScreenCenter => PointF.Create((float) ScreenWidth / 2, (float) ScreenHeight / 2);

        public float ConstraintTop { get; set; }

        public float ConstraintLeft { get; set; }

        public float ConstraintRight { get; set; }

        public float ConstraintBottom { get; set; }

        /// <summary>
        /// If enabled, adds a DebugTool that brings some helpful visualizations
        /// </summary>
        public bool IsDebugEnabled
        {
            get { return _isDebugEnabled; }
            set
            {
                _isDebugEnabled = value;

                if (_isDebugEnabled)
                {
                    var dt = Tools.OfType<DebugTool>().FirstOrDefault();
                    if (dt == null)
                        Tools.Add(new DebugTool());
                }
                else
                {
                    var dt = Tools.OfType<DebugTool>().FirstOrDefault();
                    if (dt != null)
                        Tools.Remove(dt);
                }

            }
        }

        public ITool ActiveTool
        {
            get { return _activeTool; }
            set
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

        #endregion

        /// <summary>
        /// Called by the platform specific input event detector whenever the user interacts with the model
        /// </summary>
        /// <param name="ev"></param>
        public async Task OnEvent(UserInputEvent ev)
        {
            await EnsureInitialized();

            foreach (var tool in Tools.OrderBy(t => t.InputOrder))
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

            ScreenWidth = renderer.Width;
            ScreenHeight = renderer.Height;

            ApplyConstraints();

            // apply global panning and zooming
            renderer.Translate(Translate.X, Translate.Y);
            renderer.Scale(ZoomFactor, ZoomFocus.X, ZoomFocus.Y);

            // draw default background
            renderer.FillEntireCanvasWithColor(Engine.Factory.Colors.White);

            // prerender step (e.g. gridlines, etc.)
            foreach (var tool in Tools.OrderBy(t => t.PreDrawOrder))
            {
                await tool.OnPreDraw(renderer, this);
            }

            // render svg step
            renderer.Graphics.Save();
            Document.Draw(GetOrCreateRenderer(renderer.Graphics));
            renderer.Graphics.Restore();

            // post render step (e.g. selection borders, etc.)
            foreach (var tool in Tools.OrderBy(t => t.DrawOrder))
            {
                await tool.OnDraw(renderer, this);
            }
        }

        private void ApplyConstraints()
        {
            // check the constraints and if we have to zoom in to fit
            var constraintWidth = ConstraintRight - ConstraintLeft;
            var constraintHeight = ConstraintBottom - ConstraintTop;

            if (ScreenWidth / ZoomFactor > constraintWidth || ScreenHeight / ZoomFactor > constraintHeight)
            {
                ZoomFactor = Math.Max(ScreenWidth / constraintWidth,
                    ScreenHeight / constraintHeight);
                ZoomFocus = PointF.Create(0, 0);
                Translate = PointF.Create(ScreenWidth / ZoomFactor > constraintWidth ? 0 : Translate.X,
                    ScreenHeight / ZoomFactor > constraintHeight ? 0 : Translate.Y);
            }

            var constraintTopLeft = PointF.Create(ConstraintLeft, ConstraintTop) * ZoomFactor;
            var constraintBottomRight = PointF.Create(ConstraintRight, ConstraintBottom) * ZoomFactor;
            var screenTopLeft = ScreenToCanvas(0, 0) * ZoomFactor;
            var screenBottomRight = ScreenToCanvas(ScreenWidth, ScreenHeight) * ZoomFactor;

            if (screenTopLeft.X < constraintTopLeft.X)
            {
                Translate.X += screenTopLeft.X - constraintTopLeft.X;
            }

            if (screenTopLeft.Y < constraintTopLeft.Y)
            {
                Translate.Y += screenTopLeft.Y - constraintTopLeft.Y;
            }

            if (screenBottomRight.X > constraintBottomRight.X)
            {
                Translate.X += screenBottomRight.X - constraintBottomRight.X;
            }

            if (screenBottomRight.Y > constraintBottomRight.Y)
            {
                Translate.Y += screenBottomRight.Y - constraintBottomRight.Y;
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

        private ISvgRenderer GetOrCreateRenderer(Graphics graphics)
        {
            return SvgRenderer.FromGraphics(graphics);
        }

        public Bitmap CreateBitmap(int width, int height)
        {
            return Bitmap.Create(width, height);
        }

        /// <summary>
        /// Returns a rectangle with width and height 20px that surrounds the given point
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public RectangleF GetPointerRectangle(PointF p)
        {
            float halfFingerThickness = 20 / ZoomFactor;
            return RectangleF.Create(p.X - halfFingerThickness, p.Y - halfFingerThickness, halfFingerThickness * 2, halfFingerThickness * 2); // "10 pixel fat finger"
        }

        /// <summary>
        /// the selection rectangle must be in absolute screen coordinates (so not transformed by canvas.Translate or canvas.ZoomFactor)
        /// </summary>
        /// <param name="selectionRectangle"></param>
        /// <param name="selectionType"></param>
        /// <param name="maxItems"></param>
        /// <param name="recursionLevel"></param>
        /// <returns></returns>
        public IList<TElement> GetElementsUnder<TElement>(RectangleF selectionRectangle, SelectionType selectionType, int maxItems = int.MaxValue, int recursionLevel = 1)
            where TElement : SvgVisualElement
        {
            if (selectionRectangle == null)
                return new List<TElement>();

            // to speed up selection, this only takes first-level children into account!
            var children = Document?.Children.OfType<SvgVisualElement>() ?? Enumerable.Empty<SvgVisualElement>();

            return
                children.Reverse()
                    .SelectMany(
                        ch =>
                            ch.HitTest<TElement>(selectionRectangle, selectionType,
                                GetCanvasTransformationMatrix(), recursionLevel))
                    .Take(maxItems)
                    .ToList();
        }

        /// <summary>
        /// gets all visual elements under the given pointer (a 20px rectangle surrounding the given point to simulate thick finger)
        /// </summary>
        /// <param name="pointer1Position"></param>
        /// <param name="recursionLevel"></param>
        /// <returns></returns>
        public IList<TElement> GetElementsUnderPointer<TElement>(PointF pointer1Position, int recursionLevel = 1)
            where TElement : SvgVisualElement
        {
            return GetElementsUnder<TElement>(GetPointerRectangle(pointer1Position), SelectionType.Intersect, recursionLevel: recursionLevel);
        }

        public void AddItemInScreenCenter(SvgDocument document)
        {
            var visibleChildren =
                document.Children.OfType<SvgVisualElement>().Where(e => e.Displayable && e.Visible).ToList();

            var element = visibleChildren.First();
            if (visibleChildren.Count > 1)
            {
                var group = new SvgGroup
                {
                    Fill = document.Fill,
                    Stroke = document.Stroke
                };
                foreach (var visibleChild in visibleChildren)
                {
                    group.Children.Add(visibleChild);
                }
                element = group;
            }

            AddItemInScreenCenter(element);

            MergeSvgDefs(Document, document);
        }

        public void AddItemInScreenCenter(SvgVisualElement element)
        {
            var childBounds = element.GetBoundingBox();
            var halfRelChildWidth = childBounds.Width / 2;
            var halfRelChildHeight = childBounds.Height / 2;
            var centerPos = ScreenToCanvas((float) ScreenWidth / 2, (float) ScreenHeight / 2);
            var centerPosX = centerPos.X - halfRelChildWidth;
            var centerPosY = centerPos.Y - halfRelChildHeight;

            // make sure it is centered
            if (Math.Abs(childBounds.X) > float.Epsilon)
                centerPosX -= childBounds.X;
            if (Math.Abs(childBounds.Y) > float.Epsilon)
                centerPosY -= childBounds.Y;


            if (element.OwnerDocument != null)
                MergeSvgDefs(Document, element.OwnerDocument);

            //SvgTranslate tl = new SvgTranslate(centerPosX, centerPosY);
            //element.Transforms.Add(tl);
            //element.ID = $"{element.ElementName}_{Guid.NewGuid():N}";
            var m = element.CreateTranslation(centerPosX, centerPosY);
            element.SetTransformationMatrix(m);

            Document.Children.Add(element);

            FireInvalidateCanvas();
        }

        private static void MergeSvgDefs(SvgDocument target, SvgDocument source)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            if (source == null)
                return;

            if (target == source)
                return;

            var invisibleChildren = source.Children.Where(c => !(c is SvgVisualElement)).ToArray();
            var defs = invisibleChildren.FirstOrDefault(ic => ic.ElementName == "defs");
            if (defs != null)
            {
                var docDefs = target.Children.FirstOrDefault(c => c.ElementName == "defs");
                if (docDefs == null)
                    target.Children.Add(defs);
                else
                {
                    foreach (var defChild in defs.Children)
                    {
                        var docDefChild = docDefs.Children.FirstOrDefault(c => c.ID == defChild.ID);
                        if (docDefChild == null)
                            docDefs.Children.Add(defChild);
                    }
                }
            }
        }

        public Matrix GetCanvasTransformationMatrix()
        {
            var m1 = Engine.Factory.CreateMatrix();
            m1.Translate(Translate.X, Translate.Y);
            m1.Translate(ZoomFocus.X, ZoomFocus.Y);
            m1.Scale(ZoomFactor, ZoomFactor);
            m1.Translate(-ZoomFocus.X, -ZoomFocus.Y);
            return m1;
        }

        public PointF CanvasToScreen(float x, float y)
        {
            return CanvasToScreen(PointF.Create(x, y));
        }

        public PointF CanvasToScreen(PointF canvasPointF)
        {
            var point = canvasPointF.Clone();
            var m = GetCanvasTransformationMatrix();
            m.TransformPoints(new[] { point });
            return point;
        }

        public PointF ScreenToCanvas(float x, float y)
        {
            return ScreenToCanvas(PointF.Create(x, y));
        }

        public PointF ScreenToCanvas(PointF screenPointF)
        {
            var point = screenPointF.Clone();
            var m = GetCanvasTransformationMatrix();
            m.Invert();
            m.TransformPoints(new[] { point });
            return point;
        }

        /// <summary>
        /// Stores the document with a viewbox that surrounds all contained visual elements
        /// then resets the viewbox
        /// </summary>
        /// <param name="stream"></param>
        public void SaveDocument(Stream stream)
        {

            var oldX = Document.X;
            var oldY = Document.Y;
            var oldWidth = Document.Width;
            var oldHeight = Document.Height;
            var oldViewBox = Document.ViewBox;

            try
            {
                var documentSize = Document.CalculateDocumentBounds();
                Document.Width = new SvgUnit(SvgUnitType.Pixel, documentSize.Width);
                Document.Height = new SvgUnit(SvgUnitType.Pixel, documentSize.Height);
                Document.ViewBox = new SvgViewBox(documentSize.X, documentSize.Y, documentSize.Width, documentSize.Height);
                Document.Write(stream);

                FireToolCommandsChanged();
            }
            finally
            {
                Document.ViewBox = oldViewBox;
                Document.Width = oldWidth;
                Document.Height = oldHeight;
                Document.X = oldX;
                Document.Y = oldY;
            }

            DocumentIsDirty = false;
        }

        public void FireInvalidateCanvas()
        {
            CanvasInvalidated?.Invoke(this, EventArgs.Empty);
        }

        public void FireToolCommandsChanged()
        {
            ToolCommandsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            foreach (var tool in Tools)
                tool.Dispose();

            _document?.Dispose();
        }

        private IList<IToolCommand> EnsureToolSelectors()
        {
            if (_toolSelectors == null)
            {
                _toolSelectors = Tools.Where(t => t.ToolUsage == ToolUsage.Explicit)
                        .Select(t => new SelectToolCommand(this, t, t.Name, t.IconName))
                        .OrderBy(c => c.Sort)
                        .Cast<IToolCommand>()
                        .ToList();
            }
            return _toolSelectors;
        }

        private void OnSelectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FireToolCommandsChanged();
        }

        private void OnToolsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _toolSelectors = null;
            FireToolCommandsChanged();
        }

        private void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            if (oldDocument != null)
                oldDocument.ContentModified -= OnDocumentContentModified;
            if (newDocument != null)
            {
                newDocument.ContentModified -= OnDocumentContentModified;
                newDocument.ContentModified += OnDocumentContentModified;
            }
            DocumentIsDirty = false;

            // fire document changed
            foreach (var tool in Tools)
                tool.OnDocumentChanged(oldDocument, newDocument);

            oldDocument?.Dispose();

            // selection is not valid anymore
            SelectedElements.Clear();

            // also reset translate and zoomfactor
            Translate = PointF.Create(0f, 0f);
            ZoomFactor = 1f;

            // re-render
            FireInvalidateCanvas();
        }

        private void OnDocumentContentModified(object sender, SvgElement e)
        {
            DocumentIsDirty = true;
        }

        private class SelectToolCommand : ToolCommand
        {
            private readonly SvgDrawingCanvas _canvas;

            public SelectToolCommand(SvgDrawingCanvas canvas, ITool tool, string name, string iconName)
                : base(tool, name, (o) => { }, iconName: iconName)
            {
                if (canvas == null) throw new ArgumentNullException(nameof(canvas));
                _canvas = canvas;
            }

            public override void Execute(object parameter)
            {
                _canvas.ActiveTool = Tool;
                _canvas.FireToolCommandsChanged();
            }

            public override bool CanExecute(object parameter)
            {
                return _canvas.ActiveTool != Tool;
            }

            public override string GroupIconName
            {
                get { return _canvas.ActiveTool?.IconName; }
                set { }
            }

            public override string GroupName
            {
                get { return _canvas.ActiveTool?.Name; }
                set { }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
