namespace Svg.Editor.Sample.Forms.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();

            SvgPlatform.Init();
            Editor.Init();
            Svg.Editor.Forms.FormsPlatform.Init();

            LoadApplication(new Forms.App());
        }
    }
}
