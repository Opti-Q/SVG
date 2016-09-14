using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Core.Interfaces;
using Svg.Core.UndoRedo;
using Svg.Transforms;

namespace Svg.Core.Tools
{
    public interface ITextInputService
    {
        Task<TextTool.TextProperties> GetUserInput(string title, string textValue, IEnumerable<string> textSizeOptions, int textSizeSelected);
    }

    public class TextTool : UndoableToolBase
    {
        // if user moves cursor, she does not want to add/edit text
        private bool _moveEventWasRegistered;
        private ITool _activatedFrom;
        private bool _dialogShown;
        private ITextInputService TextInputService => Engine.Resolve<ITextInputService>();

        #region Public properties

        public override int InputOrder => 300;

        public float[] FontSizes
        {
            get
            {
                object fontSizes;
                if (!Properties.TryGetValue("fontsizes", out fontSizes))
                    fontSizes = Enumerable.Empty<float>();
                return (float[]) fontSizes;
            }
        }

        public float SelectedFontSize { get; set; }

        public string[] FontSizeNames
        {
            get
            {
                object fontSizeNames;
                if (!Properties.TryGetValue("fontsizenames", out fontSizeNames))
                    fontSizeNames = Enumerable.Empty<string>();
                return (string[]) fontSizeNames;
            }
        }

        #endregion

        public TextTool(IDictionary<string, object> properties, IUndoRedoService undoRedoService) : base("Text", properties, undoRedoService)
        {
            IconName = "ic_text_fields_white_48dp.png";
            ToolUsage = ToolUsage.Explicit;
            ToolType = ToolType.Create;
            object selectedFontSizeIndex;
            if (Properties.TryGetValue("selectedfontsizeindex", out selectedFontSizeIndex))
                SelectedFontSize = FontSizes[Convert.ToInt32(selectedFontSizeIndex)];
        }

        public override async Task Initialize(SvgDrawingCanvas ws)
        {
            await base.Initialize(ws);

            IsActive = false;
        }

        public override async Task OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            var p = @event as PointerEvent;
            if (ws.ActiveTool.ToolType == ToolType.Select && p?.EventType == EventType.PointerUp)
            {
                var pointerDiff = p.Pointer1Position - p.Pointer1Down;
                var pointerDistX = Math.Abs(pointerDiff.X);
                var pointerDistY = Math.Abs(pointerDiff.Y);
                // determine if active by searching through selection and determining whether pointer was put on selected element
                // if there are selected elements and pointer was put down on one of them, activate tool, otherwise deactivate
                var selectedTextBase = ws.GetElementsUnderPointer<SvgTextBase>(p.Pointer1Position, 20).FirstOrDefault();
                if (pointerDistX < 20.0f && pointerDistY < 20.0f &&                         // pointer didn't move
                    ws.SelectedElements.Count == 1 &&                                       // and there is just 1 element selected
                    selectedTextBase != null && selectedTextBase.ParentsAndSelf.Contains(ws.SelectedElements.First()))  // and the selected element is a parent of the text element
                {
                    // save the active tool for restoring later
                    _activatedFrom = ws.ActiveTool;
                    ws.ActiveTool = this;
                    ws.FireInvalidateCanvas();
                }
            }

            if (!IsActive)
                return;

            // if user moves cursor, she does not want to add/edit text
            var me = @event as MoveEvent;
            if (me != null)
            {
                // if user moves with thumb we do not want to add text on pointer-up
                var isMove = Math.Abs(me.AbsoluteDelta.X) + Math.Abs(me.AbsoluteDelta.Y) > 10d;
                if (isMove)
                    _moveEventWasRegistered = true;
            }

            var pe = @event as PointerEvent;
            if (pe != null && pe.EventType == EventType.PointerDown)
            {
                _moveEventWasRegistered = false;
            }
            else if (pe != null && pe.EventType == EventType.PointerUp)
            {
                if (_moveEventWasRegistered)
                {
                    return;
                }

                var pointerDiff = p.Pointer1Position - p.Pointer1Down;
                var pointerDistX = Math.Abs(pointerDiff.X);
                var pointerDistY = Math.Abs(pointerDiff.Y);

                // if Point-Down and Point-Up are merely the same
                if (pointerDistX < 20.0f && pointerDistY < 20.0f)
                {
                    // if there is text below the pointer, edit it
                    var e = ws.GetElementsUnderPointer<SvgTextBase>(pe.Pointer1Position, 20).FirstOrDefault();

                    if (e != null)
                    {
                        // primitive handling of text spans
                        //var span = e.Children.OfType<SvgTextSpan>().FirstOrDefault();
                        //if (span != null)
                        //    e = span;

                        // joining the spans as newlines
                        var text = !string.IsNullOrWhiteSpace(e.Text) ? e.Text : string.Join("\n", e.Children.OfType<SvgTextSpan>().Select(x => x.Text));

                        if (_dialogShown) return;
                        _dialogShown = true;
                        var txtProperties = await TextInputService.GetUserInput("Edit text", text, FontSizeNames, Array.IndexOf(FontSizes, (int) Math.Round(e.FontSize, 0)));
                        _dialogShown = false;
                        var txt = txtProperties.Text;
                        var fontSize = FontSizes[txtProperties.FontSizeIndex];
                        var lineHeight = txtProperties.LineHeight;

                        // make sure there is at least empty text in it so we actually still have a bounding box!!
                        if (string.IsNullOrEmpty(txt?.Trim()))
                            txt = "  ";

                        // if text was removed, and parent was document, remove element
                        // if parent was not the document, then this would be a text within another group and should not be removed
                        if (string.IsNullOrWhiteSpace(txt) && e.Parent is SvgDocument)
                        {
                            var parent = e.Parent;
                            UndoRedoService.ExecuteCommand(new UndoableActionCommand("Remove text", o =>
                            {
                                parent.Children.Remove(e);
                                Canvas.FireInvalidateCanvas();
                            }, o =>
                            {
                                parent.Children.Add(e);
                                Canvas.FireInvalidateCanvas();
                            }));
                        }
                        else if (text != txt || Math.Abs(e.FontSize.Value - fontSize) > 0.01f)
                        {
                            var formerText = e.Text;
                            var formerChildrenTexts = e.Children.OfType<SvgTextSpan>().Select(x => x.Text).ToArray();
                            var formerFontSize = e.FontSize;
                            UndoRedoService.ExecuteCommand(new UndoableActionCommand("Edit text", o =>
                            {
                                var lines = txt.Split('\n');
                                if (lines.Length > 1)
                                {
                                    e.Text = null;
                                    var spans = lines.Select((t, i) =>
                                        new SvgTextSpan
                                        {
                                            Nodes = { new SvgContentNode { Content = t } },
                                            X = e.X,
                                            Y =
                                                new SvgUnitCollection
                                                {
                                                    e.Y.FirstOrDefault() + fontSize * lineHeight * i
                                                }
                                        });

                                    // add spans as children
                                    e.Children.Clear();
                                    foreach (var span in spans)
                                    {
                                        e.Children.Add(span);
                                    }
                                }
                                else
                                {
                                    var span = e.Children.OfType<SvgTextSpan>().FirstOrDefault() ?? e;
                                    span.Text = lines.First();
                                }
                                e.FontSize = new SvgUnit(SvgUnitType.Pixel, fontSize);
                                Canvas.FireInvalidateCanvas();
                            }, o =>
                            {
                                e.Text = formerText;
                                e.Children.Clear();
                                for (var i = 0; i < formerChildrenTexts.Length; i++)
                                {
                                    e.Children.Add(new SvgTextSpan
                                    {
                                        Nodes = { new SvgContentNode { Content = formerChildrenTexts[i] } },
                                        X = new SvgUnitCollection { 0 },
                                        Y = new SvgUnitCollection { fontSize * lineHeight * i }
                                    });
                                }
                                e.FontSize = formerFontSize;
                                Canvas.FireInvalidateCanvas();
                            }));
                        }
                    }
                    // else add new text   
                    else
                    {
                        if (_dialogShown) return;
                        _dialogShown = true;
                        var txtProperties = await TextInputService.GetUserInput("Add text", null, FontSizeNames, Array.IndexOf(FontSizes, SelectedFontSize));
                        _dialogShown = false;
                        var txt = txtProperties.Text;
                        var fontSize = FontSizes[txtProperties.FontSizeIndex];
                        var lineHeight = txtProperties.LineHeight;

                        // only add if user really entered text.
                        if (!string.IsNullOrWhiteSpace(txt))
                        {
                            var svgText = new SvgText
                            {
                                FontSize = new SvgUnit(SvgUnitType.Pixel, fontSize),
                                Stroke = new SvgColourServer(Engine.Factory.CreateColorFromArgb(255, 0, 0, 0)),
                                Fill = new SvgColourServer(Engine.Factory.CreateColorFromArgb(255, 0, 0, 0))
                            };

                            var lines = txt.Split('\n');
                            if (lines.Length > 1)
                            {
                                var spans = lines.
                                    Select(
                                        (t, i) =>
                                            new SvgTextSpan
                                            {
                                                Nodes = {new SvgContentNode {Content = t}},
                                                X = new SvgUnitCollection {0},
                                                Y = new SvgUnitCollection {fontSize*lineHeight*i}
                                            });
                                foreach (var span in spans)
                                {
                                    svgText.Children.Add(span);
                                }
                            }
                            else
                            {
                                svgText.Text = lines.First();
                            }

                            var relativePointer = ws.ScreenToCanvas(pe.Pointer1Position);
                            var childBounds = svgText.Bounds;
                            var halfRelChildWidth = childBounds.Width / 2;
                            var halfRelChildHeight = childBounds.Height / 2;

                            var x = relativePointer.X - halfRelChildWidth;
                            var y = relativePointer.Y - halfRelChildHeight;
                            svgText.Transforms.Add(new SvgTranslate(x, y));

                            UndoRedoService.ExecuteCommand(new UndoableActionCommand("Add text", o =>
                            {
                                ws.Document.Children.Add(svgText);
                                ws.FireInvalidateCanvas();
                            }, o =>
                            {
                                ws.Document.Children.Remove(svgText);
                                ws.FireInvalidateCanvas();
                            }));
                        }
                    }

                    if (_activatedFrom != null)
                    {
                        ws.ActiveTool = _activatedFrom;
                        _activatedFrom = null;
                    }
                }
            }
        }

        public class TextProperties
        {
            public string Text { get; set; }
            public int FontSizeIndex { get; set; }
            public float LineHeight { get; set; } = 1.25f;
        }
    }

}
