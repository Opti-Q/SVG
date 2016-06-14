using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Svg.Core.Commands
{
    public interface IUndoRedoCommand : ICommand
    {
        void Undo();
    }
}
