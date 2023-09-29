namespace InklingAi.Views
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Svg.Skia;
    using Avalonia.VisualTree;
    using InklingAi.ViewModels;

    public partial class AspectButtons : UserControl
    {
        private List<Button> _buttons = new();
        private List<Svg> _images = new();
        private Dictionary<Svg, string> _onStates = new();
        private Dictionary<Svg, string> _offStates = new();
        public AspectButtons()
        {
            InitializeComponent();
            this.AttachedToVisualTree += OnAttachedToVisualTree;
        }

        private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                _buttons = this.GetVisualDescendants().OfType<Button>().ToList();
                _images = this.GetVisualDescendants().OfType<Svg>().ToList();
                foreach (var button in _buttons)
                {
                    if (button.Content is Svg svg)
                    {
                        _images.Add(svg);
                        var path = svg.Path!;
                        _onStates[svg] = path;
                        var disabledPath = path.Substring(0, path.Length - 4) + "_d.svg";
                        _offStates[svg] = disabledPath;
                    }
                }
                ResetButtons(_buttons[0]);
            });
        }

        private void OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ResetButtons(sender as Button);
        }

        private void ResetButtons(Button? selected)
        {
            if (selected != null)
            {
                foreach (var button in _buttons)
                {
                    if (button.Content is Svg svg)
                    {
                        if (button == selected)
                        {
                            button.IsEnabled = false;
                            svg.Path = _onStates[svg];
                        }
                        else
                        {
                            button.IsEnabled = true;
                            svg.Path = _offStates[svg];
                        }
                    }
                }
            }
        }
    }
}
