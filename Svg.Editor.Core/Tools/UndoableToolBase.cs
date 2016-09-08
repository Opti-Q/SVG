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
            UndoRedoService.CanUndoChanged += UndoRedoServiceOnCanUndoRedoChanged;
            UndoRedoService.CanRedoChanged += UndoRedoServiceOnCanUndoRedoChanged;
        }

        private void UndoRedoServiceOnActionExecuted(object sender, CommandEventArgs commandEventArgs)
        {
            // clear selection when undoing
            if (commandEventArgs.ExecuteAction == ExecuteAction.Undo)
                Canvas?.SelectedElements.Clear();
        }

        private void UndoRedoServiceOnCanUndoRedoChanged(object sender, EventArgs eventArgs)
        {
            Canvas?.FireToolCommandsChanged();
        }

        ~UndoableToolBase()
        {
            UndoRedoService.ActionExecuted -= UndoRedoServiceOnActionExecuted;
            UndoRedoService.CanUndoChanged -= UndoRedoServiceOnCanUndoRedoChanged;
            UndoRedoService.CanRedoChanged -= UndoRedoServiceOnCanUndoRedoChanged;
        }

        protected IUndoRedoService UndoRedoService { get; }
    }
}
