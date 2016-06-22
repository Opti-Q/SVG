using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Svg.Core.Events;
using Svg.Core.Interfaces;

namespace Svg.Core.Tools
{
    public enum ToolUsage
    {
        Implicit,
        Explicit
    }

    public interface ITool : IDisposable
    {
        string Name { get; }
        ToolUsage ToolUsage { get; }
        bool IsActive { get; set; }
        IEnumerable<IToolCommand> Commands { get; }
        string IconName { get; }
        Task Initialize(SvgDrawingCanvas ws);
        Task OnDraw(IRenderer renderer, SvgDrawingCanvas ws);
        Task OnPreDraw(IRenderer renderer, SvgDrawingCanvas ws);
        Task OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws);
        void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument);
        void Reset();
    }

    public interface IToolCommand : ICommand
    {
        string Name { get; }
        string Description { get; }
        ITool Tool { get; }
        string IconName { get; }
        int Sort { get; }
    }

    public class ToolCommand : IToolCommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;
        private readonly Func<IToolCommand, int> _sortFunc;
        public string Name { get; set; }
        public string Description { get; set; }
        public ITool Tool { get; }
        public string IconName { get; set; }

        public int Sort
        {
            get
            {
                if (_sortFunc != null)
                    return _sortFunc(this);

                return 100;
            }
        }
        
        public ToolCommand(ITool tool, string name, Action<object> execute, Func<object, bool> canExecute = null, string description = null, string iconName = null, Func<IToolCommand, int> sortFunc = null)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            _execute = execute;
            _canExecute = canExecute;
            _sortFunc = sortFunc;
            Tool = tool;
            Name = name;
            Description = description;
            IconName = iconName;
        }

        public virtual bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public virtual void Execute(object parameter)
        {
            _execute.Invoke(parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}