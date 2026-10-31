using System;
using System.Collections.Generic;
using System.Linq;
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
using WpfApp1.Entity;
using WpfApp1.Win;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordInput.Password;

            if (login == "" || password == "")
            {
                MessageBox.Show("Введите логин и пароль.");
                return;
            }

            try
            {
                using (AptekaEntities db = new AptekaEntities())
                {
                    var user = db.Users.FirstOrDefault(x => x.Login == login && x.Password == password);
                    if (user == null)
                    {
                        MessageBox.Show("Неверный логин или пароль.");
                        return;
                    }

                    CatalogWindow window = new CatalogWindow(user.IdUser, user.IdRole, user.Login);
                    window.Show();
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка базы данных: " + ex.GetBaseException().Message);
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow window = new RegistrationWindow();
            window.Owner = this;
            window.ShowDialog();
        }
    }
}
