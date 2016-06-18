using System;
using System.Collections.Generic;
using System.Windows.Input;
using Svg.Core.Events;
using Svg.Core.Interfaces;

namespace Svg.Core.Tools
{
    public interface ITool : IDisposable
    {
        string Name { get; }
        bool IsActive { get; set; }
        IEnumerable<IToolCommand> Commands { get; }
        void Initialize(SvgDrawingCanvas ws);
        void OnDraw(IRenderer renderer, SvgDrawingCanvas ws);
        void OnPreDraw(IRenderer renderer, SvgDrawingCanvas ws);
        void OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws);
        void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument);
        void Reset();
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