using System.Collections.Generic;
using System.Threading.Tasks;
using Svg.Core.Interfaces;

namespace Svg.Core.Tools
{
    public class UndoRedoTool : UndoableToolBase
    {
        public UndoRedoTool(IUndoRedoService undoRedoService) : base("Undo/Redo", undoRedoService)
        {
            IconName = "ic_undo_white_48dp.png";
        }

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            // add tool commands
            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Undo", o => UndoRedoService.Undo(), o => UndoRedoService.CanUndo(), iconName: "ic_undo_white_48dp.png"),
                new ToolCommand(this, "Redo", o => UndoRedoService.Redo(), o => UndoRedoService.CanRedo(), iconName: "ic_redo_white_48dp.png")
            };

            return base.Initialize(ws);
        }
    }
}