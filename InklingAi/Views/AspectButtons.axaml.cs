namespace InklingAi.Views;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using InklingAi.ViewModels;

public partial class AspectButtons : ButtonSet
{
    private InklingModel _model = InklingModel.Instance;

    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<AspectButtons, int>(nameof(SelectedIndex), defaultBindingMode: BindingMode.TwoWay);

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }
    public AspectButtons()
    {
        InitializeComponent();
    }
    private void OnButtonClicked(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        switch (button.Name)
        {
            case "_portrait":
                SelectedIndex = 0;
                break;
            case "_square":
                SelectedIndex = 1;
                break;
            case "_landscape":
                SelectedIndex = 2;
                break;
        }
        _model.InvokeAspectChanged((AspectKind)SelectedIndex);
    }
}
