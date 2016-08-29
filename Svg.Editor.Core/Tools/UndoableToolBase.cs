using Svg.Core.Interfaces;

namespace Svg.Core.Tools
{
    public abstract class UndoableToolBase : ToolBase
    {
        protected UndoableToolBase(string name, IUndoRedoService undoRedoService) : base(name)
        {
            UndoRedoService = undoRedoService;
        }

        protected UndoableToolBase(string name, string jsonProperties, IUndoRedoService undoRedoService) : base(name, jsonProperties)
        {
            UndoRedoService = undoRedoService;
        }

        protected IUndoRedoService UndoRedoService { get; }
    }
}
