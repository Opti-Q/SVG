namespace Svg.Editor.Sample.Forms.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();

            Editor.Init();

            LoadApplication(new Forms.App());
        }
    }
}
