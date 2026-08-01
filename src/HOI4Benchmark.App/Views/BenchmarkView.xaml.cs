using System.Windows.Controls;
using HOI4Benchmark.App.ViewModels;

namespace HOI4Benchmark.App.Views;

public partial class BenchmarkView : UserControl
{
    public BenchmarkView(BenchmarkViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();
        DataContext = viewModel;
    }
}