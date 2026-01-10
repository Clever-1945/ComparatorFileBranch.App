using ComparatorFileBranch.App.Definitions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ComparatorFileBranch.App.Controls
{
    /// <summary>
    /// Логика взаимодействия для LeftToolsControl.xaml
    /// </summary>
    public partial class LeftToolsControl : UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public GitBranchInfo[] ListBranch { set; get; }
        public GitBranchInfo[] ListFilteredBranch { set; get; }
        public GitBranchInfo CurrentBranch { set; get; }
        public string TextInfo { set; get; }
        public Action OnClickSave { set; get; }
        public Action<GitBranchInfo> OnSelectBranch { set; get; }


        public LeftToolsControl()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void SetListBranch(GitBranchInfo[] list)
        {
            Dispatcher.Invoke(() =>
            {
                ListBranch = list ?? new GitBranchInfo[0] { };

                CurrentBranch = ListBranch.FirstOrDefault(x => x.IsCurrent);
                OnSelectBranch?.Invoke(CurrentBranch);

                ApplyFilteredBranchs();
                OnPropertyChanged(nameof(ListBranch));
                OnPropertyChanged(nameof(CurrentBranch));
            });
        }

        public void SetTextInfo(string textInfo)
        {
            Dispatcher.Invoke(() =>
            {
                TextInfo = textInfo;
                OnPropertyChanged(nameof(TextInfo));
            });
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            OnClickSave?.Invoke();
        }

        private void OnChangeFilter(object sender, TextChangedEventArgs e)
        {
            ApplyFilteredBranchs();
        }

        private void OnSelectedBranch(object sender, SelectionChangedEventArgs e)
        {
            OnSelectBranch?.Invoke(CurrentBranch);
        }

        private void ApplyFilteredBranchs()
        {
            var filter = (NameTextFilter.Text ?? "").Trim();

            ListFilteredBranch = (ListBranch ?? Array.Empty<GitBranchInfo>()).Where(x =>
            {
                if (String.IsNullOrWhiteSpace(filter))
                    return true;

                return x.Name.Contains(filter, StringComparison.OrdinalIgnoreCase);
            }).ToArray();
            OnPropertyChanged(nameof(ListFilteredBranch));
        }
    }
}
