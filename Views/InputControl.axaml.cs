namespace InklingAi.Views
{
    using Avalonia;
    using Avalonia.Controls;
    using InklingAi.ViewModels;

    public partial class InputControl : UserControl
    {
        public static readonly StyledProperty<ImageData> ItemDataProperty =
            AvaloniaProperty.Register<InputControl, ImageData>(nameof(ItemData));

        public ImageData ItemData
        {
            get { return GetValue(ItemDataProperty); }
            set { SetValue(ItemDataProperty, value); }
        }
        public InputControl()
        {
            InitializeComponent();
        }
    }
}
