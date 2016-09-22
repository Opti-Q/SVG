using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Gestures;
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
        #region Private fields

        private const double MaxPointerDistance = 20.0;
        private static ILineOptionsInputService LineOptionsInputServiceProxy => Engine.Resolve<ILineOptionsInputService>();
        private SvgLine _currentLine;
        private Brush _brush;
        private Pen _pen;
        private bool _isActive;
        private MovementHandle _movementHandle;
        private ITool _activatedFrom;

        #endregion

        #region Private properties

        private Brush BlueBrush => _brush ?? (_brush = Engine.Factory.CreateSolidBrush(Engine.Factory.CreateColorFromArgb(255, 80, 210, 210)));
        private Pen BluePen => _pen ?? (_pen = Engine.Factory.CreatePen(BlueBrush, 5));
        private IEnumerable<SvgMarker> Markers { get; }
        private SvgUnitCollection StrokeDashArray { get; }

        #endregion

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

        public LineTool(IDictionary<string, object> properties, IUndoRedoService undoRedoService) : base("Line", properties, undoRedoService)
        {
            IconName = "ic_mode_edit_white_48dp.png";
            ToolUsage = ToolUsage.Explicit;
            ToolType = ToolType.Create;
            HandleDragExit = true;

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
                new SvgUnit(SvgUnitType.Pixel, 3),
                new SvgUnit(SvgUnitType.Pixel, 3)
            };
        }

        #region Overrides

        public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            if (oldDocument != null) UnWatchDocument(oldDocument);
            WatchDocument(newDocument);
            InitializeDefinitions(newDocument);
        }

        protected override async Task OnTap(TapGesture tap)
        {
            await base.OnTap(tap);

            if (!IsActive) return;

            _currentLine = Canvas.GetElementsUnder<SvgLine>(Canvas.GetPointerRectangle(tap.Position),
                        SelectionType.Intersect).FirstOrDefault();

            if (_currentLine != null)
            {
                Canvas.SelectedElements.Clear();
                Canvas.SelectedElements.Add(_currentLine);
            }
            else
            {
                Canvas.SelectedElements.Clear();

                if (_activatedFrom != null)
                {
                    Canvas.ActiveTool = _activatedFrom;
                    _activatedFrom = null;
                }
            }

            Canvas.FireToolCommandsChanged();
            Canvas.FireInvalidateCanvas();
        }

        protected override async Task OnLongPress(LongPressGesture longPress)
        {
            await base.OnLongPress(longPress);

            if (Canvas.ActiveTool.ToolType != ToolType.Select) return;

            var line = Canvas.GetElementsUnderPointer<SvgLine>(longPress.Position).FirstOrDefault();
            if (line != null)
            {
                Canvas.SelectedElements.Clear();
                Canvas.SelectedElements.Add(line);

                // save the active tool for restoring later
                _activatedFrom = Canvas.ActiveTool;
                Canvas.ActiveTool = this;
                Canvas.FireInvalidateCanvas();
            }
        }

        protected override async Task OnDrag(DragGesture drag)
        {
            await base.OnDrag(drag);

            if (!IsActive) return;

            if (drag.State == DragState.Exit)
            {
                _movementHandle = MovementHandle.None;
                return;
            }

            var relativeEnd = Canvas.ScreenToCanvas(drag.Position);

            if (_currentLine == null)
            {
                var relativeStart = Canvas.ScreenToCanvas(drag.Start);

                SelectLine(CreateLine(relativeStart));

                // capture variables for use in lambda
                var children = Canvas.Document.Children;
                var capturedCurrentLine = _currentLine;
                UndoRedoService.ExecuteCommand(new UndoableActionCommand("Add new line", o =>
                {
                    children.Add(capturedCurrentLine);
                    Canvas.FireInvalidateCanvas();
                }, o =>
                {
                    children.Remove(capturedCurrentLine);
                    Canvas.FireInvalidateCanvas();
                }));

                _movementHandle = MovementHandle.End;
            }
            else
            {
                var m = _currentLine.Transforms.GetMatrix();
                m.Invert();
                m.TransformPoints(new[] { relativeEnd });

                // capture _currentLine for use in lambda
                var capturedCurrentLine = _currentLine;

                if (_movementHandle == MovementHandle.None)
                {
                    var canvasPointer1Position = Canvas.ScreenToCanvas(drag.Start);
                    var points = _currentLine.GetTransformedLinePoints();
                    _movementHandle = Math.Abs(canvasPointer1Position.X - points[1].X) <= MaxPointerDistance &&
                                 Math.Abs(canvasPointer1Position.Y - points[1].Y) <= MaxPointerDistance ? MovementHandle.End :
                                 Math.Abs(canvasPointer1Position.X - points[0].X) <= MaxPointerDistance &&
                                 Math.Abs(canvasPointer1Position.Y - points[0].Y) <= MaxPointerDistance ? MovementHandle.Start :
                                 _currentLine.GetBoundingBox().Contains(canvasPointer1Position) ? MovementHandle.StartEnd : MovementHandle.None;
                    if (_movementHandle != MovementHandle.None)
                    {
                        UndoRedoService.ExecuteCommand(new UndoableActionCommand("Edit line", o => { }));
                    }
                }

                switch (_movementHandle)
                {
                    case MovementHandle.End:
                        // capture variables for use in lambda
                        var formerEndX = _currentLine.EndX;
                        var formerEndY = _currentLine.EndY;
                        UndoRedoService.ExecuteCommand(new UndoableActionCommand("Move line end", o =>
                        {
                            capturedCurrentLine.EndX = new SvgUnit(SvgUnitType.Pixel, relativeEnd.X);
                            capturedCurrentLine.EndY = new SvgUnit(SvgUnitType.Pixel, relativeEnd.Y);
                            Canvas.FireInvalidateCanvas();
                        }, o =>
                        {
                            capturedCurrentLine.EndX = formerEndX;
                            capturedCurrentLine.EndY = formerEndY;
                            Canvas.FireInvalidateCanvas();
                        }), hasOwnUndoRedoScope: false);
                        break;
                    case MovementHandle.Start:
                        // capture variables for use in lambda
                        var formerStartX = _currentLine.StartX;
                        var formerStartY = _currentLine.StartY;
                        UndoRedoService.ExecuteCommand(new UndoableActionCommand("Move line start", o =>
                        {
                            capturedCurrentLine.StartX = new SvgUnit(SvgUnitType.Pixel, relativeEnd.X);
                            capturedCurrentLine.StartY = new SvgUnit(SvgUnitType.Pixel, relativeEnd.Y);
                            Canvas.FireInvalidateCanvas();
                        }, o =>
                        {
                            capturedCurrentLine.StartX = formerStartX;
                            capturedCurrentLine.StartY = formerStartY;
                            Canvas.FireInvalidateCanvas();
                        }), hasOwnUndoRedoScope: false);
                        break;
                    case MovementHandle.StartEnd:
                        // TODO: move both start and end points
                        break;
                }
            }
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

        #endregion

        #region Private helpers

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
        
        private SvgLine CreateLine(PointF relativeStart)
        {
            var line = new SvgLine
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

            if (SelectedLineStyle == "dashed")
            {
                line.StrokeDashArray = StrokeDashArray.Clone();
            }

            return line;
        }

        private void SelectLine(SvgLine line)
        {
            _currentLine = line;
            Canvas.SelectedElements.Clear();
            Canvas.SelectedElements.Add(line);
        }

        private static Uri CreateUriFromId(string markerEndId, string exception = "none")
        {
            return markerEndId != exception ? new Uri($"url(#{markerEndId})", UriKind.Relative) : null;
        }

        #endregion

        #region Inner types

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

        private enum MovementHandle { None, Start, End, StartEnd }

        #endregion
    }
}
