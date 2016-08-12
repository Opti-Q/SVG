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

    //public enum ToolType
    //{
    //    Undefined,
    //    Create,
    //    Modify
    //}

    public interface ITool : IDisposable
    {
        string Name { get; }
        ToolUsage ToolUsage { get; }
        //ToolType ToolType { get; }
        bool IsActive { get; set; }
        IEnumerable<IToolCommand> Commands { get; }
        /// <summary>
        /// Properties for the tool that can be configured in the designer. Key should be lower-case for consistency.
        /// </summary>
        IDictionary<string, object> Properties { get; }
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
        string GroupName { get; }
        string GroupIconName { get; }
    }

    public class ToolCommand : IToolCommand
    {
        private const int DEFAULT_SORT_VALUE = 1000;
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;
        private readonly Func<IToolCommand, int> _sortFunc;
        private string _groupName;
        private string _groupIcon;
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public ITool Tool { get; }
        public virtual string IconName { get; set; }

        public virtual int Sort
        {
            get
            {
                if (_sortFunc != null)
                    return _sortFunc(this);

                return DEFAULT_SORT_VALUE;
            }
        }

        public virtual string GroupName
        {
            get { return _groupName ?? Tool.Name; }
            set { _groupName = value; }
        }

        public virtual string GroupIconName
        {
            get { return _groupIcon ?? Tool.IconName; }
            set { _groupIcon = value; }
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