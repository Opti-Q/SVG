using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Interfaces;
using Svg.Core.UndoRedo;

namespace Svg.Core.Tools
{
    public class ArrangeTool : UndoableToolBase
    {
        public ArrangeTool(IUndoRedoService undoRedoService) : base("Arrange", undoRedoService)
        {
            IconName = "ic_swap_vert_white_48dp.png";
        }

        public override async Task Initialize(SvgDrawingCanvas ws)
        {
            await base.Initialize(ws);

            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Move forward", o =>
                {
                    var children = Canvas.Document.Children;
                    var selected = Canvas.SelectedElements;
                    UndoRedoService.ExecuteCommand(new UndoableActionCommand("Move forward operation", o1 => Canvas.FireInvalidateCanvas(), o1 => Canvas.FireInvalidateCanvas()));
                    for (int i = selected.Count; i >= 0; i--)
                    {
                        var element = selected[i];
                        var cIndex = children.IndexOf(element);
                        var successor = children.ElementAtOrDefault(cIndex + 1) as SvgVisualElement;
                        if (successor != null && !selected.Contains(successor))
                        {
                            UndoRedoService.ExecuteCommand(new UndoableActionCommand("Move forward", o1 =>
                            {
                                children[cIndex] = successor;
                                children[cIndex + 1] = element;
                            }, o1 =>
                            {
                                children[cIndex] = element;
                                children[cIndex + 1] = successor;
                            }), hasOwnUndoRedoScope: false);
                        }
                    }
                }, o => Canvas.SelectedElements.Any(), iconName: "ic_arrow_upward_white_48dp.png"),
                new ToolCommand(this, "Move back", o =>
                {
                    var children = Canvas.Document.Children;
                    var selected = Canvas.SelectedElements;
                    UndoRedoService.ExecuteCommand(new UndoableActionCommand("Move back operation", o1 => Canvas.FireInvalidateCanvas(), o1 => Canvas.FireInvalidateCanvas()));
                    for (int i = 0; i < selected.Count; i++)
                    {
                        var element = selected[i];
                        var cIndex = children.IndexOf(element);
                        var precursor = children.ElementAtOrDefault(cIndex - 1) as SvgVisualElement;
                        if (precursor != null && !selected.Contains(precursor))
                        {
                            UndoRedoService.ExecuteCommand(new UndoableActionCommand("Move back", o1 =>
                            {
                                children[cIndex] = precursor;
                                children[cIndex - 1] = element;
                            }, o1 =>
                            {
                                children[cIndex] = element;
                                children[cIndex - 1] = precursor;
                            }), hasOwnUndoRedoScope: false);
                        }
                    }
                }, o => Canvas.SelectedElements.Any(), iconName: "ic_arrow_downward_white_48dp.png")
            };
        }
    }
}
