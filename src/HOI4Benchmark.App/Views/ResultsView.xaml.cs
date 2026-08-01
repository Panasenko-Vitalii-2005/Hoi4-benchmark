using System.Windows.Controls;
using HOI4Benchmark.App.ViewModels;

namespace HOI4Benchmark.App.Views;

public partial class ResultsView : UserControl
{
    public ResultsView(ResultsViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();

        DataContext = viewModel;
    }
}