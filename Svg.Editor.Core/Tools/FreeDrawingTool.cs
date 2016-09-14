using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Core.Interfaces;
using Svg.Core.UndoRedo;
using Svg.Interfaces;
using Svg.Pathing;

namespace Svg.Core.Tools
{
    public interface IFreeDrawingOptionsInputService
    {
        Task<int[]> GetUserInput(string title, IEnumerable<string> lineStyleOptions, int lineStyleSelected, IEnumerable<string> strokeWidthOptions, int strokeWidthSelected);
    }

    public class FreeDrawingTool : UndoableToolBase
    {
        private const double MinMovedDistance = 6.0;

        private static IFreeDrawingOptionsInputService FreeDrawingOptionsInputServiceProxy => Engine.Resolve<IFreeDrawingOptionsInputService>();

        private double _movedDistance;
        private SvgPath _currentPath;
        private bool _drawingEnabled;
        private PointF _lastCanvasPointerPosition;
        private bool _isActive;

        private SvgUnitCollection StrokeDashArray { get; } = new SvgUnitCollection
        {
            new SvgUnit(SvgUnitType.Pixel, 25),
            new SvgUnit(SvgUnitType.Pixel, 25)
        };

        #region Public properties

        public string LineStyleIconName { get; set; } = "ic_line_style_white_48dp.png";

        public override bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive == value) return;
                _isActive = value;
                if (!_isActive) return;
                Canvas.SelectedElements.Clear();
                Canvas.FireInvalidateCanvas();
            }
        }

        public string[] LineStyles
        {
            get
            {
                object lineStyles;
                if (!Properties.TryGetValue("linestyles", out lineStyles))
                    lineStyles = Enumerable.Empty<string>();
                return (string[]) lineStyles;
            }
        }

        public string[] LineStyleNames
        {
            get
            {
                object lineStyleNames;
                if (!Properties.TryGetValue("linestylenames", out lineStyleNames))
                    lineStyleNames = Enumerable.Empty<string>();
                return (string[]) lineStyleNames;
            }
        }

        public int[] StrokeWidths
        {
            get
            {
                object strokeWidths;
                if (!Properties.TryGetValue("strokewidths", out strokeWidths))
                    strokeWidths = Enumerable.Empty<int>();
                return (int[]) strokeWidths;
            }
        }

        public string[] StrokeWidthNames
        {
            get
            {
                object strokeWidthNames;
                if (!Properties.TryGetValue("strokewidthnames", out strokeWidthNames))
                    strokeWidthNames = Enumerable.Empty<string>();
                return (string[]) strokeWidthNames;
            }
        }

        public string SelectedLineStyle { get; set; }

        public int SelectedStrokeWidth { get; set; }

        #endregion

        #region Event handlers

        public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            if (oldDocument != null) UnWatchDocument(oldDocument);
            WatchDocument(newDocument);
        }

        private void UnWatchDocument(SvgDocument svgDocument)
        {
            svgDocument.ChildRemoved -= SvgDocumentOnChildRemoved;
        }

        private void WatchDocument(SvgDocument svgDocument)
        {
            svgDocument.ChildRemoved -= SvgDocumentOnChildRemoved;
            svgDocument.ChildRemoved += SvgDocumentOnChildRemoved;
        }

        private void SvgDocumentOnChildRemoved(object sender, ChildRemovedEventArgs args)
        {
            if (IsActive && args.RemovedChild == _currentPath)
            {
                _currentPath = null;
                Canvas.FireInvalidateCanvas();
            }
        }

        #endregion

        public FreeDrawingTool(IDictionary<string, object> properties, IUndoRedoService undoRedoService) : base("Free draw", properties, undoRedoService)
        {
            IconName = "ic_brush_white_48dp.png";
            ToolUsage = ToolUsage.Explicit;
        }

        public override async Task Initialize(SvgDrawingCanvas ws)
        {
            await base.Initialize(ws);

            IsActive = false;

            SelectedLineStyle = LineStyles.FirstOrDefault();
            SelectedStrokeWidth = StrokeWidths.FirstOrDefault();

            Commands = new List<IToolCommand>
            {
                new ChangeLineStyleCommand(this, "Line style", ws)
            };
        }

        public override Task OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            if (!IsActive)
            {
                return Task.FromResult(true);
            }

            var p = @event as PointerEvent;
            if (p?.PointerCount == 1 && (p.EventType == EventType.PointerUp || p.EventType == EventType.Cancel))
            {
                _currentPath = null;
                _lastCanvasPointerPosition = null;
                _drawingEnabled = false;
            }

            if (p?.EventType == EventType.PointerDown)
            {
                _drawingEnabled = p.PointerCount == 1;
                //_lastCanvasPointerPosition = ws.ScreenToCanvas(p.Pointer1Down);
            }

            if (!_drawingEnabled)
                return Task.FromResult(true);

            var e = @event as MoveEvent;
            if (e != null)
            {
                var startX = _lastCanvasPointerPosition?.X ?? e.Pointer1Down.X;
                var startY = _lastCanvasPointerPosition?.Y ?? e.Pointer1Down.Y;
                var endX = e.Pointer1Position.X;
                var endY = e.Pointer1Position.Y;

                if (startX > endX)
                {
                    var t = startX;
                    startX = endX;
                    endX = t;
                }
                if (startY > endY)
                {
                    var t = startY;
                    startY = endY;
                    endY = t;
                }
                var rect = RectangleF.Create(startX, startY, endX - startX, endY - startY);

                // drawing only counts if length is not too small
                _movedDistance = Math.Sqrt(Math.Pow(rect.Width, 2) + Math.Pow(rect.Height, 2)) / ws.ZoomFactor;

                if (_movedDistance >= MinMovedDistance)
                {
                    var canvasStartPosition = ws.ScreenToCanvas(e.Pointer1Down);
                    var canvasPointerPosition = ws.ScreenToCanvas(e.Pointer1Position);

                    if (_currentPath == null)
                    {

                        _currentPath = new SvgPath
                        {
                            Stroke = new SvgColourServer(Engine.Factory.CreateColorFromArgb(255, 0, 0, 0)),
                            Fill = SvgPaintServer.None,
                            StrokeWidth = new SvgUnit(SvgUnitType.Pixel, SelectedStrokeWidth),
                            PathData = new SvgPathSegmentList(new List<SvgPathSegment> { new SvgMoveToSegment(canvasStartPosition) }),
                            StrokeLineCap = SvgStrokeLineCap.Round,
                            StrokeLineJoin = SvgStrokeLineJoin.Round
                        };

                        if (SelectedLineStyle == "dashed")
                        {
                            _currentPath.StrokeDashArray = StrokeDashArray.Clone();
                        }

                        _currentPath.CustomAttributes.Add("iclnosnapping", "");

                        var capturedCurrentPath = _currentPath;
                        UndoRedoService.ExecuteCommand(new UndoableActionCommand("Add new freedrawing path", o =>
                        {
                            ws.Document.Children.Add(capturedCurrentPath);
                            ws.FireInvalidateCanvas();
                        }, o =>
                        {
                            ws.Document.Children.Remove(capturedCurrentPath);
                            ws.FireInvalidateCanvas();
                        }));
                    }

                    // Quadratic bezier curve to the approximate of the pointer position
                    var nextControlPoint = _lastCanvasPointerPosition ?? _currentPath.PathData.Last.End;
                    var nextEndPoint = (nextControlPoint + canvasPointerPosition) / 2;

                    _currentPath.PathData.Add(new SvgQuadraticCurveSegment(_currentPath.PathData.Last.End, nextControlPoint, nextEndPoint));

                    _lastCanvasPointerPosition = canvasPointerPosition;

                    ws.FireInvalidateCanvas();
                }
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// This command changes the line style of selected items, or the global line style, if no items are selected.
        /// </summary>
        private class ChangeLineStyleCommand : ToolCommand
        {
            private readonly SvgDrawingCanvas _canvas;

            public ChangeLineStyleCommand(FreeDrawingTool tool, string name, SvgDrawingCanvas canvas)
                : base(tool, name, o => { }, iconName: tool.LineStyleIconName, sortFunc: tc => 500)
            {
                _canvas = canvas;
            }

            private new FreeDrawingTool Tool => (FreeDrawingTool) base.Tool;

            public override async void Execute(object parameter)
            {
                var t = Tool;

                var selectedOptions = await FreeDrawingOptionsInputServiceProxy.GetUserInput("Choose line options",
                    t.LineStyleNames, Array.IndexOf(t.LineStyles, t.SelectedLineStyle), t.StrokeWidthNames, Array.IndexOf(t.StrokeWidths, t.SelectedStrokeWidth));

                if (selectedOptions?.Length != 2) return;

                var formerSelectedStrokeWidth = t.SelectedStrokeWidth;
                var formerSelectedLineStyle = t.SelectedLineStyle;
                t.UndoRedoService.ExecuteCommand(new UndoableActionCommand(Name, o =>
                {
                    t.SelectedStrokeWidth = t.StrokeWidths[selectedOptions[0]];
                    t.SelectedLineStyle = t.LineStyles[selectedOptions[1]];
                }, o =>
                {
                    t.SelectedStrokeWidth = formerSelectedStrokeWidth;
                    t.SelectedLineStyle = formerSelectedLineStyle;
                }));
            }

            public override bool CanExecute(object parameter)
            {
                return Tool.IsActive;
            }
        }
    }
}
