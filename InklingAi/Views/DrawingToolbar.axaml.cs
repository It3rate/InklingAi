namespace InklingAi.Views
{
    using System.Collections.Generic;
    using Avalonia.Controls;

    public partial class DrawingToolbar : ButtonSet
    {
        private List<Button> _buttons;
        public DrawingToolbar()
        {
            InitializeComponent();
            _buttons = new List<Button>() { btArrow, btErase, btPaint, btPen, btResize };
        }

        private void OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
        }
        public void SetMode(Button sel)
        {
            foreach (var button in _buttons)
            {
                if (button == sel)
                {
                    button.IsEnabled = true;
                }
                else
                {
                    button.IsEnabled = false;
                }
            }
        }
    }
}
