namespace InklingAi.Views
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Avalonia.Controls;
    using Avalonia.Svg.Skia;

    public partial class AspectButtons : UserControl
    {
        private List<Button> _buttons = new();
        private List<Svg> _images = new();
        private Dictionary<Svg, string> _onStates = new();
        private Dictionary<Svg, string> _offStates = new();
        public AspectButtons()
        {
            InitializeComponent();
            _buttons.AddRange(new Button[] { btPortrait, btSquare, btLandscape });
            _images.AddRange(new Svg[] { svgPortrait, svgSquare, svgLandscape });
            foreach (var button in _buttons)
            {
                if (button.Content is Svg svg)
                {
                    _images.Add(svg);
                    var path = svg.Path;
                    _onStates[svg] = path;
                    var disabledPath = path.Substring(0, path.Length - 4) + "_d.svg";
                    _offStates[svg] = disabledPath;
                }
            }

            ResetButtons(btSquare);
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
