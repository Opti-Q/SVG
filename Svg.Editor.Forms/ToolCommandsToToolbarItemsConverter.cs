using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Svg.Editor.Forms.Services;
using Svg.Editor.Tools;
using Xamarin.Forms;

namespace Svg.Editor.Forms
{
    public class ToolCommandsToToolbarItemsConverter : IValueConverter
    {
        private IImageSourceProvider GetImageProvider()
        {
            var isp = Engine.TryResolve<IImageSourceProvider>();

            return isp ?? new DefaultImageSourceProvider();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int shownActions;
            if (parameter is int)
                shownActions = (int) parameter;
            else
            {
                shownActions = 3;
            }

            var imageProvider = GetImageProvider();

            var commandLists = (value as IEnumerable<IEnumerable<IToolCommand>>);
            var items = new List<ToolbarItem>();
            foreach (var commands in commandLists.Where(l => l.Any()).OrderBy(l => l.Min(li => li.Sort)))
            {
                var cmds = commands.Where(c => c.CanExecute(null)).ToArray();
                if (cmds.Length == 0)
                    continue;

                var itemOrder = items.Count >= shownActions ? ToolbarItemOrder.Secondary : ToolbarItemOrder.Primary;

                // single command => show as toolbaritem
                if (cmds.Length == 1)
                {
                    var command = cmds.Single();
                    items.Add(new ToolbarItem
                    {
                        Text = command.Name,
                        AutomationId = command.Name,
                        Icon = imageProvider.GetImage(command.IconName),
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
                        Text = cmd.GroupName,
                        AutomationId = cmd.GroupName,
                        Icon = imageProvider.GetImage(cmd.GroupIconName),
                        Command = new Command(async () =>
                        {
                            var cs = cmds;
                            var tags = cs.Select(c => c.Name).ToArray();

                            var result = await Application.Current.MainPage.DisplayActionSheet(cmd.GroupName, "cancel", null, tags);

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
