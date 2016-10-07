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
    public class EllipseTool : UndoableToolBase
    {
        #region Private fields

        private const double MaxPointerDistance = 20.0;
        private SvgEllipse _currentEllipse;
        private Brush _brush;
        private Pen _pen;
        private bool _isActive;
        private MovementHandle _movementHandle;
        private ITool _activatedFrom;
        private PointF _offset;
        private PointF _translate;

        #endregion

        #region Private properties

        private Brush BlueBrush => _brush ?? (_brush = Engine.Factory.CreateSolidBrush(Engine.Factory.CreateColorFromArgb(255, 80, 210, 210)));
        private Pen BluePen => _pen ?? (_pen = Engine.Factory.CreatePen(BlueBrush, 5));
        private IEnumerable<SvgMarker> Markers { get; }

        #endregion

        #region Public properties

        public string LineStyleIconName { get; set; } = "ic_line_style_white_48dp.png";

        public override int InputOrder => 300;

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
                _currentEllipse = _isActive ? Canvas.SelectedElements.OfType<SvgEllipse>().FirstOrDefault() : null;
                Canvas.SelectedElements.Clear();
                if (_currentEllipse != null) Canvas.SelectedElements.Add(_currentEllipse);
                Canvas.FireInvalidateCanvas();
            }
        }

        public string SelectedMarkerStartId { get; set; }

        public string SelectedMarkerEndId { get; set; }

        public string SelectedLineStyle { get; set; }

        #endregion

        public EllipseTool(IDictionary<string, object> properties, IUndoRedoService undoRedoService) : base("Ellipse", properties, undoRedoService)
        {
            IconName = "ic_panorama_fish_eye_white_48dp.png";
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

            _currentEllipse = Canvas.GetElementsUnder<SvgEllipse>(Canvas.GetPointerRectangle(tap.Position),
                        SelectionType.Intersect).FirstOrDefault();

            if (_currentEllipse != null)
            {
                Canvas.SelectedElements.Clear();
                Canvas.SelectedElements.Add(_currentEllipse);
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

        protected override async Task OnDoubleTap(DoubleTapGesture doubleTap)
        {
            await base.OnDoubleTap(doubleTap);

            if (Canvas.ActiveTool.ToolType != ToolType.Select) return;

            var ellipse = Canvas.GetElementsUnderPointer<SvgEllipse>(doubleTap.Position).FirstOrDefault();
            if (ellipse != null)
            {
                Canvas.SelectedElements.Clear();
                Canvas.SelectedElements.Add(ellipse);

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
                _offset = null;
                _translate = null;
                return;
            }

            var relativeStart = Canvas.ScreenToCanvas(drag.Start);
            var relativeEnd = Canvas.ScreenToCanvas(drag.Position);

            // turn them around if end is more left or top than start
            var diff = relativeEnd - relativeStart;
            if (diff.X < 0 || diff.Y < 0)
            {
                diff = relativeStart;
                relativeStart = relativeEnd;
                relativeEnd = diff;
            }

            if (_currentEllipse == null)
            {
                SelectEllipse(CreateEllipse(relativeStart));

                // capture variables for use in lambda
                var children = Canvas.Document.Children;
                var capturedCurrentLine = _currentEllipse;
                UndoRedoService.ExecuteCommand(new UndoableActionCommand("Add new ellipse", o =>
                {
                    children.Add(capturedCurrentLine);
                    Canvas.FireInvalidateCanvas();
                }, o =>
                {
                    children.Remove(capturedCurrentLine);
                    Canvas.FireInvalidateCanvas();
                }));

                _movementHandle = MovementHandle.BottomRight;
            }
            else
            {
                var matrix = _currentEllipse.Transforms.GetMatrix();
                matrix.Invert();
                matrix.TransformPoints(new[] { relativeEnd });

                // capture _currentEllipse for use in lambda
                var capturedCurrentEllipse = _currentEllipse;

                if (_movementHandle == MovementHandle.None)
                {
                    var canvasPointer1Position = Canvas.ScreenToCanvas(drag.Start);
                    var points = _currentEllipse.GetTransformedPoints();
                    _movementHandle = Math.Abs(canvasPointer1Position.X - points[0].X) * Canvas.ZoomFactor <= MaxPointerDistance &&
                                 Math.Abs(canvasPointer1Position.Y - points[0].Y) * Canvas.ZoomFactor <= MaxPointerDistance ? MovementHandle.TopLeft :
                                 Math.Abs(canvasPointer1Position.X - points[1].X) * Canvas.ZoomFactor <= MaxPointerDistance &&
                                 Math.Abs(canvasPointer1Position.Y - points[1].Y) * Canvas.ZoomFactor <= MaxPointerDistance ? MovementHandle.TopRight :
                                 Math.Abs(canvasPointer1Position.X - points[2].X) * Canvas.ZoomFactor <= MaxPointerDistance &&
                                 Math.Abs(canvasPointer1Position.Y - points[2].Y) * Canvas.ZoomFactor <= MaxPointerDistance ? MovementHandle.BottomRight :
                                 Math.Abs(canvasPointer1Position.X - points[3].X) * Canvas.ZoomFactor <= MaxPointerDistance &&
                                 Math.Abs(canvasPointer1Position.Y - points[3].Y) * Canvas.ZoomFactor <= MaxPointerDistance ? MovementHandle.BottomLeft :
                                 _currentEllipse.GetBoundingBox().Contains(canvasPointer1Position) ? MovementHandle.All : MovementHandle.None;

                    if (_movementHandle == MovementHandle.None) return;

                    UndoRedoService.ExecuteCommand(new UndoableActionCommand("Edit ellipse", o => Canvas.FireInvalidateCanvas(), o => Canvas.FireInvalidateCanvas()));
                }

                // capture variables for use in lambda
                var formerCenterX = _currentEllipse.CenterX;
                var formerCenterY = _currentEllipse.CenterY;
                var formerRadiusX = _currentEllipse.RadiusX;
                var formerRadiusY = _currentEllipse.RadiusY;

                switch (_movementHandle)
                {
                    case MovementHandle.BottomRight:
                        UndoRedoService.ExecuteCommand(new UndoableActionCommand("Move ellipse bottomright", o =>
                        {
                            var center = (relativeStart + relativeEnd) / 2;
                            var radius = (relativeEnd - relativeStart) / 2;
                            capturedCurrentEllipse.CenterX = new SvgUnit(SvgUnitType.Pixel, center.X);
                            capturedCurrentEllipse.CenterY = new SvgUnit(SvgUnitType.Pixel, center.Y);
                            capturedCurrentEllipse.RadiusX = new SvgUnit(SvgUnitType.Pixel, radius.X);
                            capturedCurrentEllipse.RadiusY = new SvgUnit(SvgUnitType.Pixel, radius.Y);
                        }, o =>
                        {
                            capturedCurrentEllipse.CenterX = formerCenterX;
                            capturedCurrentEllipse.CenterY = formerCenterY;
                            capturedCurrentEllipse.RadiusX = formerRadiusX;
                            capturedCurrentEllipse.RadiusY = formerRadiusY;
                        }), hasOwnUndoRedoScope: false);
                        break;
                    case MovementHandle.All:
                        var absoluteDeltaX = drag.Delta.Width / Canvas.ZoomFactor;
                        var absoluteDeltaY = drag.Delta.Height / Canvas.ZoomFactor;

                        // add translation to current line
                        var previousDelta = _offset ?? PointF.Create(0, 0);
                        var relativeDeltaX = absoluteDeltaX - previousDelta.X;
                        var relativeDeltaY = absoluteDeltaY - previousDelta.Y;

                        previousDelta.X = absoluteDeltaX;
                        previousDelta.Y = absoluteDeltaY;
                        _offset = previousDelta;

                        AddTranslate(_currentEllipse, relativeDeltaX, relativeDeltaY);
                        break;
                }

                Canvas.FireInvalidateCanvas();
            }
        }

        public override async Task OnDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
            await base.OnDraw(renderer, ws);

            if (_currentEllipse != null)
            {
                renderer.Graphics.Save();

                var radius = (int) (MaxPointerDistance / ws.ZoomFactor);
                var points = _currentEllipse.GetTransformedPoints();
                //renderer.DrawCircle(points[0].X - (radius >> 1), points[0].Y - (radius >> 1), radius, BluePen);
                renderer.DrawCircle(points[2].X - (radius >> 1), points[2].Y - (radius >> 1), radius, BluePen);

                renderer.Graphics.Restore();
            }
        }

        public override async Task Initialize(SvgDrawingCanvas ws)
        {
            await base.Initialize(ws);

            IsActive = false;

            SelectedLineStyle = LineStyles.FirstOrDefault();
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
            if (IsActive && args.RemovedChild == _currentEllipse)
            {
                _currentEllipse = null;
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

        private SvgEllipse CreateEllipse(PointF relativeStart)
        {
            var ellipse = new SvgEllipse
            {
                Stroke = new SvgColourServer(Color.Create(0, 0, 0)),
                Fill = SvgPaintServer.None,
                StrokeWidth = new SvgUnit(SvgUnitType.Pixel, 5),
                CenterX = new SvgUnit(SvgUnitType.Pixel, relativeStart.X),
                CenterY = new SvgUnit(SvgUnitType.Pixel, relativeStart.Y),
                CustomAttributes = { { NoFillCustomAttributeKey, "" } }
            };

            if (!string.IsNullOrWhiteSpace(SelectedLineStyle) && SelectedLineStyle != "none")
            {
                ellipse.StrokeDashArray = GenerateStrokeDashArray(SelectedLineStyle.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Select(s => Convert.ToInt32(s)));
            }

            return ellipse;
        }

        private static SvgUnitCollection GenerateStrokeDashArray(IEnumerable<int> pattern)
        {
            var svgUnitCollection = new SvgUnitCollection();
            svgUnitCollection.AddRange(pattern.Select(element => new SvgUnit(SvgUnitType.Pixel, element)));
            return svgUnitCollection;
        }

        private void AddTranslate(SvgVisualElement element, float deltaX, float deltaY)
        {
            // the movetool stores the last translation explicitly for each element
            // that way, if another tool manipulates the translation (e.g. the snapping tool)
            // the movetool is not interfered by that
            var b = element.GetBoundingBox();
            var translate = _translate ?? PointF.Create(b.X, b.Y);

            translate.X += deltaX;
            translate.Y += deltaY;

            _translate = translate;

            var dX = translate.X - b.X;
            var dY = translate.Y - b.Y;

            var m = element.CreateTranslation(dX, dY);
            var formerM = element.Transforms.GetMatrix().Clone();
            UndoRedoService.ExecuteCommand(new UndoableActionCommand("Move object", o =>
            {
                element.SetTransformationMatrix(m);
            }, o =>
            {
                element.SetTransformationMatrix(formerM);
            }), hasOwnUndoRedoScope: false);
        }

        private void SelectEllipse(SvgEllipse ellipse)
        {
            _currentEllipse = ellipse;
            Canvas.SelectedElements.Clear();
            Canvas.SelectedElements.Add(ellipse);
        }

        #endregion

        #region Inner types

        private enum MovementHandle { None, TopLeft, TopRight, BottomRight, BottomLeft, All }

        #endregion
    }
}
