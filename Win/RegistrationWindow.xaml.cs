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
using System.Windows.Shapes;
using WpfApp1.Entity;

namespace WpfApp1.Win
{
    /// <summary>
    /// Interaction logic for RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string surname = SurnameBox.Text.Trim();
            string name = NameBox.Text.Trim();
            string middle = MiddleBox.Text.Trim();
            string phone = PhoneBox.Text.Trim();
            string login = LoginBox.Text.Trim();
            string password = PasswordInput.Password;

            if (surname == "" || name == "" || phone == "" || login == "" || password == "")
            {
                MessageBox.Show("Заполните все обязательные поля.");
                return;
            }
            if (password.Length < 4)
            {
                MessageBox.Show("Пароль должен содержать хотя бы 4 символа.");
                return;
            }

            try
            {
                using (AptekaEntities db = new AptekaEntities())
                {
                    if (db.Users.Any(x => x.Login == login))
                    {
                        MessageBox.Show("Такой логин уже существует.");
                        return;
                    }

                    using (var transaction = db.Database.BeginTransaction())
                    {
                        User user = new User();
                        user.Login = login;
                        user.Password = password;
                        user.IdRole = 2;
                        db.Users.Add(user);
                        db.SaveChanges();

                        Sotrudniki employee = new Sotrudniki();
                        employee.Familiya = surname;
                        employee.Imya = name;
                        if (middle == "")
                        {
                            employee.Otchestvo = null;
                        }
                        else
                        {
                            employee.Otchestvo = middle;
                        }
                        employee.Telefon = phone;
                        employee.IdUser = user.IdUser;
                        db.Sotrudnikis.Add(employee);
                        db.SaveChanges();
                        transaction.Commit();
                    }
                }

                MessageBox.Show("Регистрация выполнена. Теперь можно войти.");
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка регистрации: " + ex.GetBaseException().Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}