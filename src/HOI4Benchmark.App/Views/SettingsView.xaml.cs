using System.Windows.Controls;
using HOI4Benchmark.App.ViewModels;

namespace HOI4Benchmark.App.Views;

public partial class SettingsView : UserControl
{
    public SettingsView(SettingsViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();

        DataContext = viewModel;
    }
}