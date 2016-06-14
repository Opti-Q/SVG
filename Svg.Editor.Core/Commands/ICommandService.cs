namespace Svg.Core.Commands
{
    public interface ICommandService
    {
        bool Execute(IUndoRedoCommand command);
        bool Undo();
    }
}