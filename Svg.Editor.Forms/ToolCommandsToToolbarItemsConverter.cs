using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Svg.Editor.Tools;
using Xamarin.Forms;

namespace Svg.Editor.Forms
{
    public class ToolCommandsToToolbarItemsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int shownActions;
            if (parameter is int)
                shownActions = (int) parameter;
            else
            {
                shownActions = 3;
            }

            var commandLists = (value as IEnumerable<IEnumerable<IToolCommand>>);
            var items = new List<ToolbarItem>();
            foreach (var commands in commandLists.Where(l => l.Any()).OrderBy(l => l.Min(li => li.Sort)))
            {
                var cmds = commands.Where(c => c.CanExecute(null)).ToArray();
                if (cmds.Length == 0)
                    continue;

                var itemOrder = cmds.Length >= shownActions ? ToolbarItemOrder.Primary : ToolbarItemOrder.Secondary;

                // single command => show as toolbaritem
                if (cmds.Length == 1)
                {
                    var command = cmds.Single();
                    items.Add(new ToolbarItem
                    {
                        Text = command.Name,
                        AutomationId = command.Name,
                        Icon = command.IconName,
                        Command = new Command(() => command.Execute(null), () => command.CanExecute(null)),
                        Order = itemOrder
                    });
                }
                // multiple commands => create action menu
                else
                {
                    var cmd = cmds.First();
                    items.Add(new ToolbarItem
                    {
                        Text = cmd.Tool.Name,
                        AutomationId = cmd.Tool.Name,
                        Icon = cmd.Tool.IconName,
                        Command = new Command(async () =>
                        {
                            var cs = cmds;
                            var tags = cs.Select(c => c.Name).ToArray();

                            var result = await Application.Current.MainPage.DisplayActionSheet(cmd.Tool.Name, "cancel", null, tags);

                            var selectedCommand = cs.FirstOrDefault(c => c.Name == result);
                            selectedCommand?.Execute(null);
                        }),
                        Order = itemOrder
                    });

                }
            }

            return items;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
