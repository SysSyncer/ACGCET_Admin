using System.Windows;

namespace ACGCET_Admin.Services
{
    public partial class CustomMessageBox : Window
    {
        public CustomMessageBox(string message, string title = "Notification")
        {
            InitializeComponent();
            MessageText.Text = message;
            TitleText.Text = title;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public static void Show(string message, string title = "Notification")
        {
            var msgBox = new CustomMessageBox(message, title);
            msgBox.ShowDialog();
        }
    }
}
