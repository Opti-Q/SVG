using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Svg.Core.Tools
{
    public class UndoRedoTool : ToolBase
    {
        public UndoRedoTool() : base("Undo/Redo")
        {
            IconName = "ic_undo_white_48dp.png";
            ToolType = ToolType.Modify;
        }

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            // add tool commands
            Commands = new List<IToolCommand>
            {
                //new ChangeStrokeStyleCommand(ws, this, "Change stroke")
                new ToolCommand(this, "Undo", o => { }, iconName: "ic_undo_white_48dp.png"),
                new ToolCommand(this, "Redo", o => { }, o => false, iconName: "ic_redo_white_48dp.png")
            };

            return Task.FromResult(true);
        }
    }
}