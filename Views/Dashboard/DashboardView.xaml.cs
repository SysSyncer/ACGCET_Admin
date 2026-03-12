using System.Windows.Controls;
using ACGCET_Admin.ViewModels.Dashboard;

namespace ACGCET_Admin.Views.Dashboard
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
