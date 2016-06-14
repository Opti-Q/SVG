using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svg.Core.Commands
{
    public class CommandService : ICommandService
    {
        private readonly Stack<IUndoRedoCommand> _commands; 

        public CommandService()
        {
            _commands = new Stack<IUndoRedoCommand>();
        }

        public bool Execute(IUndoRedoCommand command)
        {
            try
            {
                _commands.Push(command);
                command.Execute(null);
                return true;
            }
            catch (Exception exception)
            {
                // ExceptionPolicy? :)
                return false;
            }
        }

        public bool Undo()
        {
            try
            {
                var command = _commands.Pop();
                command.Undo();
                return true;
            }
            catch (Exception exception)
            {
                // ExceptionPolicy? :)
                return false;
            }
        }
    }
}
