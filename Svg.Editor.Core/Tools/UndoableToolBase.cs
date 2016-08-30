using System;
using Svg.Core.Interfaces;
using Svg.Core.UndoRedo;

namespace Svg.Core.Tools
{
    public abstract class UndoableToolBase : ToolBase
    {
        protected UndoableToolBase(string name, IUndoRedoService undoRedoService) : this(name, "", undoRedoService) { }

        protected UndoableToolBase(string name, string jsonProperties, IUndoRedoService undoRedoService) : base(name, jsonProperties)
        {
            UndoRedoService = undoRedoService;
            UndoRedoService.ActionExecuted += UndoRedoServiceOnActionExecuted;
        }

        ~UndoableToolBase()
        {
            UndoRedoService.ActionExecuted -= UndoRedoServiceOnActionExecuted;
        }

        private void UndoRedoServiceOnActionExecuted(object sender, CommandEventArgs commandEventArgs)
        {
            Canvas?.FireToolCommandsChanged();
        }

        protected IUndoRedoService UndoRedoService { get; }
    }
}
