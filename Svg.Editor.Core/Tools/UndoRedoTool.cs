using System.Collections.Generic;
using System.Threading.Tasks;
using Svg.Editor.Interfaces;

namespace Svg.Editor.Tools
{
    public class UndoRedoTool : UndoableToolBase
    {
        public UndoRedoTool(IUndoRedoService undoRedoService) : base("Undo/Redo", undoRedoService)
        {
            IconName = "ic_undo_white_48dp.png";
        }

        public override async Task Initialize(SvgDrawingCanvas ws)
        {
            await base.Initialize(ws);
            // add tool commands
            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Undo", o => UndoRedoService.Undo(), o => UndoRedoService.CanUndo(), iconName: "ic_undo_white_48dp.png"),
                new ToolCommand(this, "Redo", o => UndoRedoService.Redo(), o => UndoRedoService.CanRedo(), iconName: "ic_redo_white_48dp.png")
            };
        }

        public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            UndoRedoService.Clear();
        }
    }
}