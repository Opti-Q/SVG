using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Svg.Editor.Tools;
using Xamarin.Forms;

namespace Svg.Editor.Forms
{
    public class ToolCommandsToToolbarItemsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var commands = value as IEnumerable<IToolCommand>;

            return commands?
                .OrderBy(a => a.Sort)
                .Select
                (
                    command => new ToolbarItem
                    {
                        Text = command.Name,
                        AutomationId = command.Name,
                        Icon = command.IconName,
                        Command = new Command(() => command.Execute(null), () => command.CanExecute(null))
                    }
                )
                .ToList();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
