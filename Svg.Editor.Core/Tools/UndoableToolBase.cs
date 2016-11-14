using System;
using System.Collections.Generic;
using Svg.Editor.Interfaces;
using Svg.Editor.UndoRedo;

namespace Svg.Editor.Tools
{
    public abstract class UndoableToolBase : ToolBase
    {
        protected UndoableToolBase(string name, IUndoRedoService undoRedoService) : this(name, null, undoRedoService) { }

        protected UndoableToolBase(string name, IDictionary<string,object> properties, IUndoRedoService undoRedoService) : base(name, properties)
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

        protected IUndoRedoService UndoRedoService { get; }

        public override void Dispose()
        {
            base.Dispose();

            UndoRedoService.ActionExecuted -= UndoRedoServiceOnActionExecuted;
            UndoRedoService.CanUndoChanged -= UndoRedoServiceOnCanUndoRedoChanged;
            UndoRedoService.CanRedoChanged -= UndoRedoServiceOnCanUndoRedoChanged;
        }
    }
}
