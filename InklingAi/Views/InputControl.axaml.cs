namespace InklingAi.Views
{
    using Avalonia;
    using Avalonia.Controls;
    using InklingAi.ViewModels;
    using System.Diagnostics;

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
            _expander.Expanded += OnExpanded;
            _expander.Collapsed += OnCollapsed;
        }


        private void OnExpanded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ItemData.IsActive = true;
            var mvm = InklingModel.GetRootDataContext(this);
            if (mvm != null)
            {
                mvm.InfluenceData.ResetActiveItems();
            }
        }
        private void OnCollapsed(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ItemData.IsActive = false;
            var mvm = InklingModel.GetRootDataContext(this);
            if(mvm != null)
            {
                mvm.InfluenceData.ResetActiveItems();
            }
        }
    }
}
