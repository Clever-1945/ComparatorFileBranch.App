using ComparatorFileBranch.App.Controls;
using System.Windows;
using System.Windows.Controls;

namespace ComparatorFileBranch.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MaskControl maskControl;

        private int zIndexError = 100;

        public MainWindow()
        {
            InitializeComponent();
            Assistant.GetDispatcher = () => this.Dispatcher;
            Assistant.AsyncError = OnAsyncError;
            Assistant.ChangeMask = OnChangeMask;
            maskControl = new MaskControl();
        }

        private void OnAsyncError(Exception ex)
        {
            var c = new ErrorControl(ex, (control) => 
            {
                if(GridMainName.Children.Contains(control))
                {
                    GridMainName.Children.Remove(control);
                }
            });
            Panel.SetZIndex(c, zIndexError++);
            Grid.SetRow(c, 0);
            Grid.SetColumn(c, 1);

            GridMainName.Children.Add(c);
        }

        private void OnChangeMask(bool isMask)
        {
            if (!isMask)
            {
                if (GridMainName.Children.Contains(maskControl))
                {
                    GridMainName.Children.Remove(maskControl);
                }
            }
            else
            {
                if (!GridMainName.Children.Contains(maskControl))
                {
                    Panel.SetZIndex(maskControl, 100);
                    Grid.SetRow(maskControl, 0);
                    Grid.SetRowSpan(maskControl, 2);
                    Grid.SetColumn(maskControl, 0);
                    Grid.SetColumnSpan(maskControl, 3);
                    GridMainName.Children.Add(maskControl);
                }
            }
        }
    }
}