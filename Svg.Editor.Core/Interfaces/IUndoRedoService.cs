using System;

namespace Svg.Core.Interfaces
{
    public interface IUndoRedoService
    {
        bool CanRedo();
        bool CanUndo();
        //void ExecuteCommand(IUndoableCommand command, object state = null);
        void ExecuteCommand(IUndoableCommand command, object state = null, bool hasOwnUndoRedoScope = true);
        void Redo();
        void Undo();
        void Clear();
        //void RemoveCommand(IUndoableCommand command);
        //void AddCommand(IUndoableCommand command);
        //IEnumerable<IUndoableCommand> UndoStack { get; }
        //IEnumerable<IUndoableCommand> RedoStack { get; }
        //int RedoBufferSize { get; set; }
        //int UndoBufferSize { get; set; }
        event EventHandler CanRedoChanged;
        event EventHandler CanUndoChanged;

    }
}