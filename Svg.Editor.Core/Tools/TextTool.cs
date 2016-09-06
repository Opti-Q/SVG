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

        public TextTool(string jsonProperties, IUndoRedoService undoRedoService) : base("Text", jsonProperties, undoRedoService)
        {
            IconName = "ic_text_fields_white_48dp.png";
            ToolUsage = ToolUsage.Explicit;
            ToolType = ToolType.Create;
            object selectedFontSizeIndex;
            if (Properties.TryGetValue("selectedfontsizeindex", out selectedFontSizeIndex))
                SelectedFontSize = FontSizes[Convert.ToInt32(selectedFontSizeIndex)];
        }

        private ITextInputService TextInputService => Engine.Resolve<ITextInputService>();

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            IsActive = false;

            return base.Initialize(ws);
        }

        public int[] FontSizes
        {
            get
            {
                object fontSizes;
                if (!Properties.TryGetValue("fontsizes", out fontSizes))
                    fontSizes = Enumerable.Empty<int>();
                return (int[]) fontSizes;
            }
        }

        public int SelectedFontSize { get; set; }

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

        public override async Task OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            if (!IsActive)
                return;

            // if user moves cursor, she does not want to add/edit text
            var me = @event as MoveEvent;
            if (me != null)
            {
                // if user moves with thumb we do not want to add text on pointer-up
                var isMove = Math.Sqrt(Math.Pow(me.AbsoluteDelta.X, 2) + Math.Pow(me.AbsoluteDelta.Y, 2)) > 20d;
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

                var dX = pe.Pointer1Position.X - pe.Pointer1Down.X;
                var dY = pe.Pointer1Position.Y - pe.Pointer1Down.Y;

                // if Point-Down and Point-Up are merely the same
                if (dX < 20 && dY < 20)
                {
                    // if there is text below the pointer, edit it
                    var e = ws.GetElementsUnderPointer<SvgTextBase>(pe.Pointer1Position, 20).FirstOrDefault();

                    if (e != null)
                    {
                        // primitive handling of text spans
                        var span = e.Children.OfType<SvgTextSpan>().FirstOrDefault();
                        if (span != null)
                            e = span;

                        var txtProperties = await TextInputService.GetUserInput("Edit text", e.Text?.Trim(), FontSizeNames, Array.IndexOf(FontSizes, (int) e.FontSize));
                        var txt = txtProperties.Text;
                        var fontSize = FontSizes[txtProperties.FontSizeIndex];

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
                        else if (!string.Equals(e.Text, txt))
                        {
                            var formerText = e.Text;
                            var formerFontSize = e.FontSize;
                            UndoRedoService.ExecuteCommand(new UndoableActionCommand("Edit text", o =>
                            {
                                e.Text = txt;
                                e.FontSize = new SvgUnit(SvgUnitType.Pixel, fontSize);
                                Canvas.FireInvalidateCanvas();
                            }, o =>
                            {
                                e.Text = formerText;
                                e.FontSize = formerFontSize;
                                Canvas.FireInvalidateCanvas();
                            }));
                        }
                    }
                    // else add new text   
                    else
                    {
                        var txtProperties = await TextInputService.GetUserInput("Add text", null, FontSizeNames, Array.IndexOf(FontSizes, SelectedFontSize));
                        var txt = txtProperties.Text;
                        var fontSize = FontSizes[txtProperties.FontSizeIndex];

                        // only add if user really entered text.
                        if (!string.IsNullOrWhiteSpace(txt))
                        {
                            var t = new SvgText(txt)
                            {
                                FontSize = new SvgUnit(SvgUnitType.Pixel, fontSize),
                                Stroke = new SvgColourServer(Engine.Factory.CreateColorFromArgb(255, 0, 0, 0)),
                                Fill = new SvgColourServer(Engine.Factory.CreateColorFromArgb(255, 0, 0, 0))
                            };

                            var relativePointer = ws.ScreenToCanvas(pe.Pointer1Position);
                            var childBounds = t.Bounds;
                            var halfRelChildWidth = childBounds.Width / 2;
                            var halfRelChildHeight = childBounds.Height / 2;

                            var x = relativePointer.X - halfRelChildWidth;
                            var y = relativePointer.Y - halfRelChildHeight;
                            //t.X = new SvgUnitCollection {new SvgUnit(SvgUnitType.Pixel, x)};
                            //t.Y = new SvgUnitCollection { new SvgUnit(SvgUnitType.Pixel, y) };
                            t.Transforms.Add(new SvgTranslate(x, y));

                            UndoRedoService.ExecuteCommand(new UndoableActionCommand("Add text", o =>
                            {
                                ws.Document.Children.Add(t);
                                ws.FireInvalidateCanvas();
                            }, o =>
                            {
                                ws.Document.Children.Remove(t);
                                ws.FireInvalidateCanvas();
                            }));
                        }
                    }
                }
            }
        }

        public class TextProperties
        {
            public string Text { get; set; }
            public int FontSizeIndex { get; set; }
        }
    }

}
