using System.Windows;
using System.Windows.Controls;

namespace ComparatorFileBranch.App.Controls
{
    /// <summary>
    /// Логика взаимодействия для ErrorControl.xaml
    /// </summary>
    public partial class ErrorControl : UserControl
    {
        private Action<ErrorControl> _onClose;

        public ErrorControl(Exception ex, Action<ErrorControl> onClose)
        {
            InitializeComponent();
            _onClose = onClose;

            TextNameException.Text = ex.Message;
            TextNameSteck.Text = ex.StackTrace;
        }

        private void OnClose(object sender, RoutedEventArgs e)
        {
            _onClose?.Invoke(this);
        }
    }
}
