using System.Windows.Controls;
using ACGCET_Admin.ViewModels.AdminControl;

namespace ACGCET_Admin.Views.AdminControl
{
    public partial class UserManagementView : UserControl
    {
        public UserManagementView()
        {
            InitializeComponent();
        }

        private void NewUserPassword_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is UserManagementViewModel vm)
                vm.NewPassword = ((PasswordBox)sender).Password;
        }
    }
}
