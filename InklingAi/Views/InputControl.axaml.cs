namespace InklingAi.Views
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Input;
    using Avalonia.Interactivity;
    using Avalonia.VisualTree;
    using HarfBuzzSharp;
    using InklingAi.ViewModels;
    using System.Diagnostics;

    public partial class InputControl : UserControl
    {
        public static readonly StyledProperty<ProcessedImageData> ItemDataProperty =
            AvaloniaProperty.Register<InputControl, ProcessedImageData>(nameof(ItemData));

        public ProcessedImageData? ItemData
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

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            if (ItemData != null)
            {
                ItemData.InputControl = this;
            }
        }


        private void OnExpanded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ItemData != null)
            {
                ItemData.IsActive = true;
                var mvm = InklingModel.GetRootDataContext(this);
                if (mvm != null)
                {
                    SetFocus();
                    mvm.InfluenceData.ResetActiveItems();
                }
            }
        }
        private void OnCollapsed(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ItemData != null)
            {
                ItemData.IsActive = false;
                ClearFocus();
            }
        }
        public  ListBox? FindParentListBox()
        {
            Visual? parent = this;

            while (parent != null)
            {
                if (parent is ListBox listBox)
                {
                    return listBox;
                }

                parent = parent.GetVisualParent();
            }

            return null;
        }
        private void ClearFocus()
        {
            var mvm = InklingModel.GetRootDataContext(this);
            if (mvm != null)
            {
                var infData = mvm.InfluenceData;
                infData.ResetActiveItems();
                if (infData.FocusedItem == ItemData)
                {
                    var targ = infData.ActiveItems.Count > 0 ? infData.ActiveItems[0] : null;
                    // select the first remaining image input
                    if (targ != null && targ.InputControl != null && !targ.InputControl.IsFocused)
                    {
                        targ.InputControl.SetFocus();
                    }
                    infData.FocusedItem = targ;
                    var lb = FindParentListBox();
                    if(lb != null)
                    {
                        lb.SelectedItem = targ;
                    }
                }
            }
            InvalidateVisual();
        }
        private void SetFocus()
        {
            if (ItemData != null && ItemData.IsActive)
            {
                var mvm = InklingModel.GetRootDataContext(this);
                if (mvm != null)
                {
                    mvm.InfluenceData.FocusedItem = ItemData;
                    var lb = FindParentListBox();
                    if (lb != null)
                    {
                        lb.SelectedItem = this.DataContext;
                    }
                }
            }
            InvalidateVisual();
        }
        private void OnGotFocus(object? sender, PointerPressedEventArgs e)
        {
            SetFocus();
        }
    }
}
