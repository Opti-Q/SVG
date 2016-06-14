using System;
using System.Collections.Generic;
using System.Windows.Input;
using Svg.Core.Events;
using Svg.Core.Interfaces;

namespace Svg.Core.Tools
{
    public interface ITool : IDisposable
    {
        void OnDraw(IRenderer renderer, SvgDrawingCanvas svgWorkspace);
        void OnTouch(InputEvent @event, SvgDrawingCanvas svgWorkspace);
        void Reset();

        IEnumerable<IToolCommand> Commands { get; }

        bool IsActive { get; set; }

        int DrawOrder { get; }
        int CommandOrder { get; }
        string Name { get; }
    }

    public interface IToolCommand : ICommand
    {
        string Name { get; }
        string Description { get; }
        ITool Tool { get; }
    }

    public class ToolCommand : IToolCommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;
        public string Name { get; set; }
        public string Description { get; set; }
        public ITool Tool { get; }

        public ToolCommand(ITool tool, string name, Action<object> execute, Func<object, bool> canExecute = null, string description = null)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            _execute = execute;
            _canExecute = canExecute;
            Tool = tool;
            Name = name;
            Description = description;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            _execute.Invoke(parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}