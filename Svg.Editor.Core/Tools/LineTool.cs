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
    public interface ILineOptionsInputService
    {
        Task<int[]> GetUserInput(string title, IEnumerable<string> markerStartOptions, int markerStartSelected, IEnumerable<string> lineStyleOptions, int dashSelected, IEnumerable<string> markerEndOptions, int markerEndSelected);
    }

    public class LineTool : UndoableToolBase
    {
        private const double MinMovedDistance = 10.0;
        private const double MaxPointerDistance = 20.0;

        private static ILineOptionsInputService LineOptionsInputServiceProxy => Engine.Resolve<ILineOptionsInputService>();

        private double _movedDistance;
        private SvgLine _currentLine;
        private bool _multiplePointersRegistered;
        private Brush _brush;
        private Pen _pen;
        private bool _isActive;
        private MovementType _movementType;
        private bool _moveRegistered;
        private ITool _activatedFrom;

        private Brush BlueBrush => _brush ?? (_brush = Engine.Factory.CreateSolidBrush(Engine.Factory.CreateColorFromArgb(255, 80, 210, 210)));
        private Pen BluePen => _pen ?? (_pen = Engine.Factory.CreatePen(BlueBrush, 5));
        private IEnumerable<SvgMarker> Markers { get; }

        private SvgUnitCollection StrokeDashArray { get; }

        #region Public properties

        public string LineStyleIconName { get; set; } = "ic_line_style_white_48dp.png";

        public override int InputOrder => 300;

        public string[] MarkerStartIds
        {
            get
            {
                object markerIds;
                if (!Properties.TryGetValue("markerstartids", out markerIds))
                    markerIds = Enumerable.Empty<string>();
                return (string[]) markerIds;
            }
        }

        public string[] MarkerStartNames
        {
            get
            {
                object markerNames;
                if (!Properties.TryGetValue("markerstartnames", out markerNames))
                    markerNames = Enumerable.Empty<string>();
                return (string[]) markerNames;
            }
        }

        public string[] MarkerEndIds
        {
            get
            {
                object markerIds;
                if (!Properties.TryGetValue("markerendids", out markerIds))
                    markerIds = Enumerable.Empty<string>();
                return (string[]) markerIds;
            }
        }

        public string[] MarkerEndNames
        {
            get
            {
                object markerNames;
                if (!Properties.TryGetValue("markerendnames", out markerNames))
                    markerNames = Enumerable.Empty<string>();
                return (string[]) markerNames;
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

        public override bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive == value) return;
                _isActive = value;
                // if tool was activated, reduce selection to a single line and set it as current line
                _currentLine = _isActive ? Canvas.SelectedElements.OfType<SvgLine>().FirstOrDefault() : null;
                Canvas.SelectedElements.Clear();
                if (_currentLine != null) Canvas.SelectedElements.Add(_currentLine);
                Canvas.FireInvalidateCanvas();
            }
        }

        public string SelectedMarkerStartId { get; set; }

        public string SelectedMarkerEndId { get; set; }

        public string SelectedLineStyle { get; set; }

        #endregion

        #region Event handlers

        public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            if (oldDocument != null) UnWatchDocument(oldDocument);
            WatchDocument(newDocument);
            InitializeDefinitions(newDocument);
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
            if (IsActive && args.RemovedChild == _currentLine)
            {
                _currentLine = null;
                Canvas.FireInvalidateCanvas();
            }
        }

        private void InitializeDefinitions(SvgDocument document)
        {
            var definitions = document.Children.OfType<SvgDefinitionList>().FirstOrDefault();
            if (definitions == null)
            {
                definitions = new SvgDefinitionList();
                document.Children.Insert(0, definitions);
            }

            foreach (var marker in Markers)
            {
                if (document.GetElementById(marker.ID) != null) continue;

                definitions.Children.Add(marker);
            }
        }

        #endregion

        public LineTool(IDictionary<string, object> properties, IUndoRedoService undoRedoService) : base("Line", properties, undoRedoService)
        {
            IconName = "ic_mode_edit_white_48dp.png";
            ToolUsage = ToolUsage.Explicit;
            ToolType = ToolType.Create;

            #region Init markers

            var markers = new List<SvgMarker>();
            var marker = new SvgMarker { ID = "arrowStart", Orient = new SvgOrient() { IsAuto = true }, RefX = new SvgUnit(SvgUnitType.Pixel, -2.5f), MarkerWidth = 2 };
            marker.Children.Add(new SvgPath
            {
                PathData = new SvgPathSegmentList(new SvgPathSegment[]
                {
                    new SvgMoveToSegment(PointF.Create(0, -2.0f)),
                    new SvgLineSegment(PointF.Create(0, -2.0f), PointF.Create(0, 2f)),
                    new SvgLineSegment(PointF.Create(0, 2.0f), PointF.Create(-4.0f, 0)),
                    new SvgClosePathSegment()
                }),
                Stroke = SvgColourServer.ContextStroke, // inherit stroke color from parent/aka context
                Fill = SvgColourServer.ContextFill, // inherit stroke color from parent/aka context
            });
            markers.Add(marker);
            marker = new SvgMarker { ID = "arrowEnd", Orient = new SvgOrient() { IsAuto = true }, RefX = new SvgUnit(SvgUnitType.Pixel, 2.5f), MarkerWidth = 2 };
            marker.Children.Add(new SvgPath
            {
                PathData = new SvgPathSegmentList(new SvgPathSegment[]
                {
                    new SvgMoveToSegment(PointF.Create(0, -2.0f)),
                    new SvgLineSegment(PointF.Create(0, -2.0f), PointF.Create(0, 2.0f)),
                    new SvgLineSegment(PointF.Create(0, 2.0f), PointF.Create(4.0f, 0)),
                    new SvgClosePathSegment()
                }),
                Stroke = SvgColourServer.ContextStroke, // inherit stroke color from parent/aka context
                Fill = SvgColourServer.ContextFill, // inherit stroke color from parent/aka context
            });
            markers.Add(marker);
            marker = new SvgMarker { ID = "circle", Orient = new SvgOrient() { IsAuto = true }/*, RefX = new SvgUnit(SvgUnitType.Pixel, -1.5f)*/, MarkerWidth = 2 };
            marker.Children.Add(new SvgEllipse
            {
                RadiusX = 1.5f,
                RadiusY = 1.5f,
                Stroke = SvgColourServer.ContextStroke, // inherit stroke color from parent/aka context
                Fill = SvgColourServer.ContextFill, // inherit stroke color from parent/aka context
            });
            markers.Add(marker);

            Markers = markers;

            #endregion

            StrokeDashArray = new SvgUnitCollection
            {
                new SvgUnit(SvgUnitType.Pixel, 10),
                new SvgUnit(SvgUnitType.Pixel, 10)
            };
        }

        public override async Task Initialize(SvgDrawingCanvas ws)
        {
            await base.Initialize(ws);

            IsActive = false;

            SelectedMarkerStartId = MarkerStartIds.FirstOrDefault();
            SelectedMarkerEndId = MarkerEndIds.FirstOrDefault();
            SelectedLineStyle = LineStyles.FirstOrDefault();

            Commands = new List<IToolCommand>
            {
                new ChangeLineStyleCommand(ws, this, "Line style")
            };
        }

        public override Task OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            var p = @event as PointerEvent;
            if (ws.ActiveTool.ToolType == ToolType.Select && p?.EventType == EventType.PointerUp)
            {
                var pointerDiff = p.Pointer1Position - p.Pointer1Down;
                var pointerDistance = Math.Abs(pointerDiff.X) + Math.Abs(pointerDiff.Y);
                // determine if active by searching through selection and determining whether pointer was put on selected element
                // if there are selected elements and pointer was put down on one of them, activate tool, otherwise deactivate
                if (pointerDistance < 5.0f &&
                    ws.SelectedElements.Count == 1 &&
                    ws.GetElementsUnderPointer<SvgVisualElement>(p.Pointer1Position).Any(eup => eup is SvgLine && ws.SelectedElements.First() == eup))
                {
                    // save the active tool for restoring later
                    _activatedFrom = ws.ActiveTool;
                    ws.ActiveTool = this;
                    ws.FireInvalidateCanvas();
                }
            }

            if (!IsActive)
            {
                return Task.FromResult(true);
            }

            if (!_moveRegistered && p?.PointerCount == 1 && (p.EventType == EventType.PointerUp || p.EventType == EventType.Cancel))
            {
                _currentLine = ws.GetElementsUnder<SvgLine>(ws.GetPointerRectangle(p.Pointer1Position),
                            SelectionType.Intersect).FirstOrDefault();

                if (_currentLine != null)
                {
                    ws.SelectedElements.Clear();
                    ws.SelectedElements.Add(_currentLine);
                }
                else
                {
                    ws.SelectedElements.Clear();

                    if (_activatedFrom != null)
                    {
                        ws.ActiveTool = _activatedFrom;
                        _activatedFrom = null;
                    }
                }

                ws.FireToolCommandsChanged();
                ws.FireInvalidateCanvas();
            }

            if (p?.EventType == EventType.PointerDown)
            {
                if (p.PointerCount == 1)
                {
                    _moveRegistered = false;
                    _multiplePointersRegistered = false;
                }
                else
                {
                    _multiplePointersRegistered = true;
                }

                if (_currentLine != null)
                {
                    var canvasPointer1Position = ws.ScreenToCanvas(p.Pointer1Position);
                    var points = _currentLine.GetTransformedLinePoints();
                    _movementType = Math.Abs(canvasPointer1Position.X - points[1].X) <= MaxPointerDistance &&
                                 Math.Abs(canvasPointer1Position.Y - points[1].Y) <= MaxPointerDistance ? MovementType.End :
                                 Math.Abs(canvasPointer1Position.X - points[0].X) <= MaxPointerDistance &&
                                 Math.Abs(canvasPointer1Position.Y - points[0].Y) <= MaxPointerDistance ? MovementType.Start :
                                 _currentLine.GetBoundingBox().Contains(canvasPointer1Position) ? MovementType.StartEnd : MovementType.None;
                    if (_movementType != MovementType.None)
                    {
                        UndoRedoService.ExecuteCommand(new UndoableActionCommand("Edit line", o => { }));
                    }
                }
            }

            if (_multiplePointersRegistered)
                return Task.FromResult(true);

            var e = @event as MoveEvent;
            if (e != null)
            {
                _moveRegistered = true;
                var startX = e.Pointer1Down.X;
                var startY = e.Pointer1Down.Y;
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
                _movedDistance = Math.Sqrt(Math.Pow(rect.Width, 2) + Math.Pow(rect.Height, 2));

                if (_movedDistance >= MinMovedDistance)
                {
                    var relativeStart = ws.ScreenToCanvas(e.Pointer1Down);
                    var relativeEnd = ws.ScreenToCanvas(e.Pointer1Position);

                    if (_currentLine == null)
                    {
                        _currentLine = new SvgLine
                        {
                            Stroke = new SvgColourServer(Engine.Factory.CreateColorFromArgb(255, 0, 0, 0)),
                            Fill = SvgPaintServer.None,
                            StrokeWidth = new SvgUnit(SvgUnitType.Pixel, 2),
                            StartX = new SvgUnit(SvgUnitType.Pixel, relativeStart.X),
                            StartY = new SvgUnit(SvgUnitType.Pixel, relativeStart.Y),
                            EndX = new SvgUnit(SvgUnitType.Pixel, relativeStart.X),
                            EndY = new SvgUnit(SvgUnitType.Pixel, relativeStart.Y),
                            MarkerStart = CreateUriFromId(SelectedMarkerStartId),
                            MarkerEnd = CreateUriFromId(SelectedMarkerEndId)
                        };

                        ws.SelectedElements.Add(_currentLine);

                        if (SelectedLineStyle == "dashed")
                        {
                            _currentLine.StrokeDashArray = StrokeDashArray.Clone();
                        }

                        // capture variables for use in lambda
                        var children = ws.Document.Children;
                        var capturedCurrentLine = _currentLine;
                        UndoRedoService.ExecuteCommand(new UndoableActionCommand("Add new line", o =>
                        {
                            children.Add(capturedCurrentLine);
                            ws.FireInvalidateCanvas();
                        }, o =>
                        {
                            children.Remove(capturedCurrentLine);
                            ws.FireInvalidateCanvas();
                        }));

                        _movementType = MovementType.End;
                    }
                    else
                    {
                        var m = _currentLine.Transforms.GetMatrix();
                        m.Invert();
                        m.TransformPoints(new[] { relativeEnd });

                        // capture _currentLine for use in lambda
                        var capturedCurrentLine = _currentLine;

                        switch (_movementType)
                        {
                            case MovementType.End:
                                // capture variables for use in lambda
                                var formerEndX = _currentLine.EndX;
                                var formerEndY = _currentLine.EndY;
                                UndoRedoService.ExecuteCommand(new UndoableActionCommand("Move line end", o =>
                                {
                                    capturedCurrentLine.EndX = new SvgUnit(SvgUnitType.Pixel, relativeEnd.X);
                                    capturedCurrentLine.EndY = new SvgUnit(SvgUnitType.Pixel, relativeEnd.Y);
                                    ws.FireInvalidateCanvas();
                                }, o =>
                                {
                                    capturedCurrentLine.EndX = formerEndX;
                                    capturedCurrentLine.EndY = formerEndY;
                                    ws.FireInvalidateCanvas();
                                }), hasOwnUndoRedoScope: false);
                                break;
                            case MovementType.Start:
                                // capture variables for use in lambda
                                var formerStartX = _currentLine.StartX;
                                var formerStartY = _currentLine.StartY;
                                UndoRedoService.ExecuteCommand(new UndoableActionCommand("Move line start", o =>
                                {
                                    capturedCurrentLine.StartX = new SvgUnit(SvgUnitType.Pixel, relativeEnd.X);
                                    capturedCurrentLine.StartY = new SvgUnit(SvgUnitType.Pixel, relativeEnd.Y);
                                    ws.FireInvalidateCanvas();
                                }, o =>
                                {
                                    capturedCurrentLine.StartX = formerStartX;
                                    capturedCurrentLine.StartY = formerStartY;
                                    ws.FireInvalidateCanvas();
                                }), hasOwnUndoRedoScope: false);
                                break;
                            case MovementType.StartEnd:
                                // TODO: move both start and end points
                                break;
                        }
                    }

                }
            }

            return Task.FromResult(true);
        }

        public override async Task OnDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
            await base.OnDraw(renderer, ws);

            if (_currentLine != null)
            {
                renderer.Graphics.Save();

                var radius = (int) (MaxPointerDistance / ws.ZoomFactor);
                var points = _currentLine.GetTransformedLinePoints();
                renderer.DrawCircle(points[0].X - (radius >> 1), points[0].Y - (radius >> 1), radius, BluePen);
                renderer.DrawCircle(points[1].X - (radius >> 1), points[1].Y - (radius >> 1), radius, BluePen);

                renderer.Graphics.Restore();
            }
        }

        private static Uri CreateUriFromId(string markerEndId, string exception = "none")
        {
            return markerEndId != exception ? new Uri($"url(#{markerEndId})", UriKind.Relative) : null;
        }

        /// <summary>
        /// This command changes the line style of selected items, or the global line style, if no items are selected.
        /// </summary>
        private class ChangeLineStyleCommand : ToolCommand
        {
            private readonly SvgDrawingCanvas _canvas;

            public ChangeLineStyleCommand(SvgDrawingCanvas canvas, LineTool tool, string name)
                : base(tool, name, o => { }, iconName: tool.LineStyleIconName, sortFunc: tc => 500)
            {
                _canvas = canvas;
            }

            public override async void Execute(object parameter)
            {
                var t = (LineTool) Tool;

                var markerStartId = t._currentLine != null
                    ? t._currentLine.MarkerStart?.OriginalString?.Replace("url(#", null)?.TrimEnd(')') ?? "none"
                    : t.SelectedMarkerStartId;
                var lineStyle = t._currentLine != null
                    ? string.IsNullOrEmpty(t._currentLine.StrokeDashArray?.ToString()) ? "normal" : "dashed"
                    : t.SelectedLineStyle;
                var markerEndId = t._currentLine != null
                    ? t._currentLine.MarkerEnd?.OriginalString?.Replace("url(#", null)?.TrimEnd(')') ?? "none"
                    : t.SelectedMarkerEndId;

                int markerStartIndex;
                int lineStyleIndex;
                int markerEndIndex;

                markerStartIndex = Array.IndexOf(t.MarkerStartIds, markerStartId);
                lineStyleIndex = Array.IndexOf(t.LineStyles, lineStyle);
                markerEndIndex = Array.IndexOf(t.MarkerEndIds, markerEndId);

                var selectedOptions = await LineOptionsInputServiceProxy.GetUserInput("Choose line options",
                    t.MarkerStartNames, markerStartIndex,
                    t.LineStyleNames, lineStyleIndex,
                    t.MarkerEndNames, markerEndIndex);

                var selectedMarkerStartId = t.MarkerStartIds[selectedOptions[0]];
                var selectedLineStyle = t.LineStyles[selectedOptions[1]];
                var selectedMarkerEndId = t.MarkerEndIds[selectedOptions[2]];

                if (t._currentLine != null)
                {
                    var formerCurrentLine = t._currentLine;
                    var formerMarkerStart = t._currentLine.MarkerStart;
                    var formerMarkerEnd = t._currentLine.MarkerEnd;
                    var formerStrokeDashArray = t._currentLine.StrokeDashArray;
                    t.UndoRedoService.ExecuteCommand(new UndoableActionCommand(Name, o =>
                    {
                        // change the line style of all selected items
                        formerCurrentLine.MarkerStart = CreateUriFromId(selectedMarkerStartId);
                        formerCurrentLine.MarkerEnd = CreateUriFromId(selectedMarkerEndId);
                        formerCurrentLine.StrokeDashArray = selectedLineStyle == "dashed" ? t.StrokeDashArray.Clone() : null;
                        _canvas.FireInvalidateCanvas();
                    }, o =>
                    {
                        formerCurrentLine.MarkerStart = formerMarkerStart;
                        formerCurrentLine.MarkerEnd = formerMarkerEnd;
                        formerCurrentLine.StrokeDashArray = formerStrokeDashArray;
                        _canvas.FireInvalidateCanvas();
                    }));
                    // don't change the global line style when items are selected
                    return;
                }

                var formerSelectedMarkerStartId = t.SelectedMarkerStartId;
                var formerSelectedMarkerEndId = t.SelectedMarkerEndId;
                var formerSelectedLineStyle = t.SelectedLineStyle;
                t.UndoRedoService.ExecuteCommand(new UndoableActionCommand(Name, o =>
                {
                    t.SelectedMarkerStartId = selectedMarkerStartId;
                    t.SelectedMarkerEndId = selectedMarkerEndId;
                    t.SelectedLineStyle = selectedLineStyle;
                }, o =>
                {
                    t.SelectedMarkerStartId = formerSelectedMarkerStartId;
                    t.SelectedMarkerEndId = formerSelectedMarkerEndId;
                    t.SelectedLineStyle = formerSelectedLineStyle;
                }));
            }

            public override bool CanExecute(object parameter)
            {
                return Tool.IsActive;
            }
        }

        private enum MovementType { None, Start, End, StartEnd }
    }
}
