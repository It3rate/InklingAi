using AIEnv.Clients.StableDiffusion;
using Avalonia.Controls;
using Avalonia.Interactivity;
using InklingAi.ViewModels;

namespace InklingAi.Views;

public partial class MainView : UserControl
{
    private InklingModel _model = InklingModel.Instance;
    public MainView()
    {
        InitializeComponent();
    }
}
