﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Svg.Editor.Events;
using Svg.Editor.Extensions;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;
using Svg.Editor.Properties;
using Svg.Editor.Services;
using Svg.Editor.Tools;
using Svg.Editor.UndoRedo;
using Svg.Interfaces;

namespace Svg.Editor
{
    public sealed class SvgDrawingCanvas : IDisposable, ICanInvalidateCanvas, INotifyPropertyChanged, ISvgDrawingCanvas
    {
        #region Private fields and properties

        private readonly ObservableCollection<SvgVisualElement> _selectedElements;
        private readonly ObservableCollection<ITool> _tools;
        private ObservableCollection<IEnumerable<IToolCommand>> _toolCommands;
        private List<IToolCommand> _toolSelectors;
        private SvgDocument _document;
        private bool _initialized;
        private ITool _activeTool;
        private bool _isDebugEnabled;
        private bool _documentIsDirty;
        private PointF _zoomFocus;
        private IDisposable _onGestureToken;
        private IGestureRecognizer _gestureRecognizer;

        private Subject<string> _propertyChangedSubject = new Subject<string>();
        private readonly ISchedulerProvider _schedulerProvider;

        private IUndoRedoService UndoRedoService { get; }

        #endregion

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

        public ObservableCollection<IEnumerable<IToolCommand>> ToolCommands
        {
            get
            {
                if (_toolCommands == null)
                {
                    _toolCommands = new ObservableCollection<IEnumerable<IToolCommand>>();
                    var cmds = GetCommands();
                    foreach (var cmd in cmds)
                    {
                        _toolCommands.Add(cmd);
                    }
                }
                return _toolCommands;
            }
        }

        public List<Func<SvgVisualElement, Task>> DefaultEditors { get; } = new List<Func<SvgVisualElement, Task>>();

        public PointF Translate { get; set; }

        public float ZoomFactor { get; set; }

        public PointF ZoomFocus
        {
            get { return _zoomFocus ?? (_zoomFocus = PointF.Create(0, 0)); }
            set { _zoomFocus = value; }
        }

        public bool ZoomEnabled { get; set; } = true;

        public int ScreenWidth { get; set; }

        public int ScreenHeight { get; set; }

        public PointF ScreenCenter => PointF.Create((float) ScreenWidth / 2, (float) ScreenHeight / 2);

        public RectangleF Constraints { get; set; }

        public ConstraintsMode ConstraintsMode { get; set; }
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

        public Color BackgroundColor { get; set; }

        public IGestureRecognizer GestureRecognizer
        {
            get { return _gestureRecognizer; }
            set
            {
                _onGestureToken?.Dispose();
                if (value == null) return;
                _gestureRecognizer = value;
                _onGestureToken = _gestureRecognizer.RecognizedGestures.Subscribe(async g => await OnGesture(g));
            }
        }

        #endregion

        public event EventHandler CanvasInvalidated;
        public event EventHandler ToolCommandsChanged;

        public SvgDrawingCanvas()
        {
            Translate = PointF.Create(0f, 0f);
            ZoomFactor = 1f;

            BackgroundColor = SvgEngine.Factory.Colors.White;

            _selectedElements = new ObservableCollection<SvgVisualElement>();
            _selectedElements.CollectionChanged += OnSelectionChanged;

            UndoRedoService = SvgEngine.Resolve<IUndoRedoService>();
            GestureRecognizer = SvgEngine.Resolve<IGestureRecognizer>();
            _schedulerProvider = SvgEngine.Resolve<ISchedulerProvider>();

            var toolProvider = SvgEngine.Resolve<ToolFactoryProvider>();

            _tools = new ObservableCollection<ITool>();

            foreach (var tool in toolProvider.ToolFactories)
            {
                _tools.Add(tool.Invoke());
            }

            _tools.CollectionChanged += OnToolsChanged;

            _propertyChangedSubject.Throttle(TimeSpan.FromMilliseconds(250)).Subscribe(OnPropertyChanged);
        }

        /// <summary>
        /// Called by the platform specific input event detector whenever the user interacts with the model
        /// </summary>
        /// <param name="ev"></param>
        public async Task OnEvent(UserInputEvent ev)
        {
            await EnsureInitialized();

            // call gesture recognizer first
            GestureRecognizer?.OnNext(ev);

            foreach (var tool in Tools.OrderBy(t => t.InputOrder))
            {
                await tool.OnUserInput(ev, this);
            }
        }

        /// <summary>
        /// Called when a gesture - like tap, double tap, long press, etc - is recognized.
        /// </summary>
        /// <param name="gesture"></param>
        public async Task OnGesture(UserGesture gesture)
        {
            await EnsureInitialized();

            foreach (var tool in Tools.OrderBy(t => t.GestureOrder))
            {
                await tool.OnGesture(gesture);
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

            SetInitialTransformation();

            switch (ConstraintsMode)
            {
                case ConstraintsMode.FillUniform:
                    ApplyConstraintsFillUniform();
                    break;
                case ConstraintsMode.FitUniform:
                    ApplyConstraintsFitUniform();
                    break;
            }

            // apply global panning and zooming
            renderer.Translate(Translate.X, Translate.Y);
            renderer.Scale(ZoomFactor, ZoomFocus.X, ZoomFocus.Y);

            // draw default background
            renderer.FillEntireCanvasWithColor(BackgroundColor);

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

        private void ApplyConstraintsFillUniform()
        {
            if (Constraints == null || Constraints == RectangleF.Empty) return;

            // if zoom is totally out of bounds, reset
            if (ScreenWidth / ZoomFactor > Constraints.Width || ScreenHeight / ZoomFactor > Constraints.Height)
            {
                ZoomFactor = Math.Max(ScreenWidth / Constraints.Width,
                    ScreenHeight / Constraints.Height);
                Translate = PointF.Create(ScreenWidth / ZoomFactor > Constraints.Width ? 0 : Translate.X,
                    ScreenHeight / ZoomFactor > Constraints.Height ? 0 : Translate.Y);
            }

            // adjust the translate according to the constraints

            var constraintTopLeft = PointF.Create(Constraints.Left, Constraints.Top) * ZoomFactor;
            var constraintBottomRight = PointF.Create(Constraints.Right, Constraints.Bottom) * ZoomFactor;
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

        private void ApplyConstraintsFitUniform()
        {
            if (Constraints == null || Constraints == RectangleF.Empty) return;

            // if zoom is totally out of bounds, reset
            if (ScreenWidth / ZoomFactor > Constraints.Width && ScreenHeight / ZoomFactor > Constraints.Height)
            {
                ZoomFactor = Math.Min(ScreenWidth / Constraints.Width,
                    ScreenHeight / Constraints.Height);
                ZoomFocus = PointF.Empty;
                Translate = PointF.Create((ScreenWidth - Constraints.Width * ZoomFactor) / 2,
                    (ScreenHeight - Constraints.Height * ZoomFactor) / 2);
                // this should replace "ZoomFocus = PointF.Empty;" but doesn't work when ZoomFactor < 1
                //+ (ZoomFocus - ScreenToCanvas(0, 0)) * (ZoomFactor - 1);
                return;
            }

            var constraintTopLeft = PointF.Create(Constraints.Left, Constraints.Top) * ZoomFactor;
            var constraintBottomRight = PointF.Create(Constraints.Right, Constraints.Bottom) * ZoomFactor;
            var screenTopLeft = ScreenToCanvas(0, 0) * ZoomFactor;
            var screenBottomRight = ScreenToCanvas(ScreenWidth, ScreenHeight) * ZoomFactor;
            var marginX = (ScreenWidth - Constraints.Width * ZoomFactor) / 2;
            var marginY = (ScreenHeight - Constraints.Height * ZoomFactor) / 2;
            if (marginX < 0) marginX = 0;
            if (marginY < 0) marginY = 0;

            // adjust the translate according to the constraints
            if (screenTopLeft.X < constraintTopLeft.X)
            {
                Translate.X += screenTopLeft.X - constraintTopLeft.X + marginX;
            }
            else if (screenBottomRight.X > constraintBottomRight.X)
            {
                Translate.X += screenBottomRight.X - constraintBottomRight.X - marginX;
            }

            if (screenTopLeft.Y < constraintTopLeft.Y)
            {
                Translate.Y += screenTopLeft.Y - constraintTopLeft.Y + marginY;
            }
            else if (screenBottomRight.Y > constraintBottomRight.Y)
            {
                Translate.Y += screenBottomRight.Y - constraintBottomRight.Y - marginY;
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
            var halfFingerThickness = 20 / ZoomFactor;
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

        public async Task AddItemInScreenCenter(SvgDocument document)
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

            await AddItemInScreenCenter(element);

            MergeSvgDefs(Document, document);
        }

        public async Task AddItemInScreenCenter(SvgVisualElement element)
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

            //SvgTranslate tl = new SvgTranslate(centerPosX, centerPosY);
            //element.Transforms.Add(tl);
            //element.ID = $"{element.ElementName}_{Guid.NewGuid():N}";
            var m = element.CreateTranslation(centerPosX, centerPosY);
            element.SetTransformationMatrix(m);

            UndoRedoService.ExecuteCommand(new UndoableActionCommand
            (
                "Add child element",
                o =>
                {
                    Document.Children.Add(element);
                    FireInvalidateCanvas();
                },
                o =>
                {
                    Document.Children.Remove(element);
                    FireInvalidateCanvas();
                })
            );

            // this has to happen after the element is added, because the undoable command of
            // the merge is concatenated to the add child command
            if (element.OwnerDocument != null)
                MergeSvgDefs(Document, element.OwnerDocument);

            // invoke the defaulteditors for the element
            foreach (var defaultEditor in DefaultEditors)
            {
                await defaultEditor.Invoke(element);
            }

        }

        private void MergeSvgDefs(SvgDocument target, SvgDocument source)
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
                        {
                            UndoRedoService.ExecuteCommand(new UndoableActionCommand
                            (
                                "Add def",
                                o => docDefs.Children.Add(defChild),
                                o => docDefs.Children.Remove(defChild)
                            ), hasOwnUndoRedoScope: false);
                        }
                    }
                }
            }
        }

        public Matrix GetCanvasTransformationMatrix()
        {
            var m1 = SvgEngine.Factory.CreateMatrix();
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
        public void SaveDocumentWithBoundsAsViewbox(Stream stream)
        {
            var oldWidth = Document.Width;
            var oldHeight = Document.Height;
            var oldViewBox = Document.ViewBox;

            try
            {
                var documentSize = Document.CalculateDocumentBounds();
                SetDocumentViewbox(documentSize);
                Document.Write(stream);

                FireToolCommandsChanged();
            }
            finally
            {
                Document.ViewBox = oldViewBox;
                Document.Width = oldWidth;
                Document.Height = oldHeight;
            }

            DocumentIsDirty = false;
        }

        /// <summary>
        /// Stores the document with a viewbox that surrounds the current screen capture
        /// then resets the viewbox
        /// </summary>
        /// <param name="stream"></param>
        public void SaveDocumentWithScreenAsViewbox(Stream stream)
        {
            var oldWidth = Document.Width;
            var oldHeight = Document.Height;
            var oldViewBox = Document.ViewBox;
            var minXminY = ScreenToCanvas(0, 0);
            var drawingClip = RectangleF.Create(minXminY, SizeF.Create(ScreenWidth / ZoomFactor, ScreenHeight / ZoomFactor));

            try
            {
                SetDocumentViewbox(drawingClip);
                Document.Write(stream);

                FireToolCommandsChanged();
            }
            finally
            {
                Document.ViewBox = oldViewBox;
                Document.Width = oldWidth;
                Document.Height = oldHeight;
            }

            DocumentIsDirty = false;
        }

        public string GetToolPropertiesJson()
        {
            var properties = Tools.ToDictionary(t => t.GetType().FullName, t => t.Properties);
            return JsonConvert.SerializeObject(properties);
        }

        public Bitmap CaptureDocumentBitmap(int maxSize = 4096, Color backgroundColor = null)
        {
            var documentBounds = Document.CalculateDocumentBounds();

            // determine width and height of the bitmap by the minimum of the whole document's and the constraint's size
            var drawingWidth = (int) Math.Round(Math.Min(documentBounds.Width, Constraints?.Width ?? float.MaxValue));
            var drawingHeight =
                (int) Math.Round(Math.Min(documentBounds.Height, Constraints?.Height ?? float.MaxValue));

            // adjust width and height of the resulting bitmap to the maxSize parameter
            var bitmapWidth = drawingWidth;
            var bitmapHeight = drawingHeight;

            if (bitmapWidth > maxSize)
            {
                var factor = bitmapWidth / maxSize;
                bitmapWidth /= factor;
                bitmapHeight /= factor;
            }

            if (bitmapHeight > maxSize)
            {
                var factor = bitmapHeight / maxSize;
                bitmapWidth /= factor;
                bitmapHeight /= factor;
            }

            var bitmap = Bitmap.Create(bitmapWidth, bitmapHeight);

            return RenderBitmap(bitmap, backgroundColor, documentBounds);
        }

        public Bitmap CaptureScreenBitmap(Color backgroundColor = null)
        {
            if (ScreenWidth == 0 || ScreenHeight == 0) throw new InvalidOperationException($"Cannot capture screen when {nameof(ScreenWidth)} or {nameof(ScreenHeight)} of {GetType().Name} are not initialized.");

            var drawingWidth = (int) Math.Round(Math.Min(ScreenWidth / ZoomFactor, Constraints?.Width ?? float.MaxValue));
            var drawingHeight = (int) Math.Round(Math.Min(ScreenHeight / ZoomFactor, Constraints?.Height ?? float.MaxValue));
            var drawingClip = RectangleF.Create(ScreenToCanvas(0, 0), SizeF.Create(drawingWidth, drawingHeight));
            var bitmap = Bitmap.Create(drawingWidth, drawingHeight);

            return RenderBitmap(bitmap, backgroundColor, drawingClip);
        }

        private Bitmap RenderBitmap(Bitmap bitmap, Color backgroundColor, RectangleF drawingClip)
        {
            if (drawingClip == null || drawingClip == RectangleF.Empty) return bitmap;

            // stash the old values
            var oldWidth = Document.Width;
            var oldHeight = Document.Height;
            var oldViewBox = Document.ViewBox;

            try
            {
                SetDocumentViewbox(drawingClip);
                Document.Draw(bitmap, backgroundColor ?? SvgEngine.Factory.Colors.Black);

                return bitmap;
            }
            finally
            {
                // reset to old values
                Document.ViewBox = oldViewBox;
                Document.Width = oldWidth;
                Document.Height = oldHeight;
            }
        }

        private void SetDocumentViewbox(RectangleF drawingSize)
        {
            var x = Math.Max(drawingSize.X, Constraints?.X ?? float.MinValue);
            var y = Math.Max(drawingSize.Y, Constraints?.Y ?? float.MinValue);
            var width = Math.Min(drawingSize.Width, Constraints?.Width ?? float.MaxValue);
            var height = Math.Min(drawingSize.Height, Constraints?.Height ?? float.MaxValue);
            Document.Width = new SvgUnit(SvgUnitType.Pixel, width);
            Document.Height = new SvgUnit(SvgUnitType.Pixel, height);
            Document.ViewBox = new SvgViewBox(x, y, width, height);
            Document.AspectRatio = new SvgAspectRatio(SvgPreserveAspectRatio.xMidYMid, true);
        }

        public void FireInvalidateCanvas()
        {
            _schedulerProvider.MainScheduer.Schedule(this, (s, st) =>
                {
                    CanvasInvalidated?.Invoke(s, EventArgs.Empty);
                    return null;
                }
            );
        }
        
        public void FireToolCommandsChanged()
        {
            ResetToolCommands();
            
            _propertyChangedSubject.OnNext(nameof(ToolCommands));
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

            // check if the document has a viewBox and set translate and zoom accordingly
            if (newDocument == null || newDocument.ViewBox.Equals(SvgViewBox.Empty))
            {
                Translate = PointF.Create(0f, 0f);
                ZoomFactor = 1f;
            }
            else
            {
                CalculateInitialTransformation = true;
            }

            // re-render
            FireInvalidateCanvas();
        }

        private bool CalculateInitialTransformation { get; set; }

        private void SetInitialTransformation()
        {
            if (!CalculateInitialTransformation) return;

            float scaleX;
            float scaleY;
            float minX;
            float minY;
            Document.ViewBox.CalculateTransform(Document.AspectRatio, ScreenWidth, ScreenHeight,
                out scaleX, out scaleY, out minX, out minY);

            ZoomFactor = Math.Min(1 / scaleX, 1 / scaleY);
            ZoomFocus = PointF.Empty;
            Translate = PointF.Create(-Document.ViewBox.MinX * ZoomFactor, -Document.ViewBox.MinY * ZoomFactor);

            // we need to reset the viewBox for correct rendering afterwards
            Document.ViewBox = SvgViewBox.Empty;

            CalculateInitialTransformation = false;
        }

        private void OnDocumentContentModified(object sender, SvgElement e)
        {
            if (e is SvgFragment || !(e is SvgVisualElement)) return;

            DocumentIsDirty = true;
        }

        private IEnumerable<IEnumerable<IToolCommand>> GetCommands()
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

        private void ResetToolCommands()
        {
            if (_toolCommands == null)
                return;

            _toolCommands.Clear();
            var cmds = GetCommands();
            foreach (var cmd in cmds)
            {
                _toolCommands.Add(cmd);
            }
        }

        private class SelectToolCommand : ToolCommand
        {
            private readonly ISvgDrawingCanvas _canvas;

            public SelectToolCommand(ISvgDrawingCanvas canvas, ITool tool, string name, string iconName)
                : base(tool, name, _ => { }, iconName: iconName)
            {
                if (canvas == null) throw new ArgumentNullException(nameof(canvas));
                _canvas = canvas;
            }

            public override int Sort => -1;

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
            System.Diagnostics.Debug.WriteLine($"Propertychanged: {propertyName}");

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public enum ConstraintsMode { FitUniform, FillUniform }
}
