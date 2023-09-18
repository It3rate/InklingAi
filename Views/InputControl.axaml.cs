namespace InklingAi.Views
{
    using Avalonia;
    using Avalonia.Controls;
    using InklingAi.ViewModels;

    public partial class InputControl : UserControl
    {
        public static readonly StyledProperty<InputItem> ItemDataProperty =
            AvaloniaProperty.Register<InputControl, InputItem>(nameof(ItemData));

        public InputItem ItemData
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
