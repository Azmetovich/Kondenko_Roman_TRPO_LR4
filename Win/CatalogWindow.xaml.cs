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
    /// Interaction logic for CatalogWindow.xaml
    /// </summary>
    public partial class CatalogWindow : Window
    {
        private int userId;
        private int roleId;
        private List<ProductCard> products = new List<ProductCard>();
        private List<CartItem> cart = new List<CartItem>();

        // Используется в XAML для кнопок администратора в шаблоне карточки.
        public Visibility AdminVisibility { get; set; }

        public CatalogWindow(int currentUserId, int currentRoleId, string login)
        {
            userId = currentUserId;
            roleId = currentRoleId;
            if (roleId == 1)
            {
                AdminVisibility = Visibility.Visible;
            }
            else
            {
                AdminVisibility = Visibility.Collapsed;
            }

            InitializeComponent();
            DataContext = this;
            if (roleId == 1)
            {
                UserText.Text = login + " (администратор)";
            }
            else
            {
                UserText.Text = login + " (сотрудник)";
            }
            AdminPanel.Visibility = AdminVisibility;
            LoadProducts();
            UpdateCart();
        }

        private void LoadProducts()
        {
            try
            {
                using (AptekaEntities db = new AptekaEntities())
                {
                    products = (from p in db.Preparaties
                                join pr in db.Proizvoditelis on p.IdProizvoditelya equals pr.IdProizvoditelya
                                join k in db.Kategoriis on p.IdKategorii equals k.IdKategorii
                                orderby p.Nazvanie
                                select new ProductCard
                                {
                                    IdPreparata = p.IdPreparata,
                                    Nazvanie = p.Nazvanie,
                                    Opisanie = p.Opisanie,
                                    Cena = p.Cena,
                                    Proizvoditel = pr.Nazvanie,
                                    Kategoriya = k.NazvanieKategorii
                                }).ToList();
                }
                ShowProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки каталога: " + ex.GetBaseException().Message);
            }
        }

        private void ShowProducts()
        {
            if (ProductsList == null || SearchBox == null) return;
            string search = SearchBox.Text.Trim().ToLower();
            ProductsList.ItemsSource = products.Where(x => x.Nazvanie.ToLower().Contains(search)).ToList();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowProducts();
        }

        private void UpdateCart()
        {
            CartGrid.ItemsSource = null;
            CartGrid.ItemsSource = cart.ToList();
            TotalText.Text = "Итого: " + cart.Sum(x => x.Summa).ToString("N2") + " руб.";
        }

        private void AddCart_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            ProductCard product = products.FirstOrDefault(x => x.IdPreparata == id);
            if (product == null) return;

            CartItem item = cart.FirstOrDefault(x => x.IdPreparata == id);
            if (item == null)
            {
                cart.Add(new CartItem
                {
                    IdPreparata = id,
                    Nazvanie = product.Nazvanie,
                    Cena = product.Cena,
                    Kolichestvo = 1
                });
            }
            else if (item.Kolichestvo < 100)
            {
                item.Kolichestvo++;
            }
            UpdateCart();
        }

        private void RemoveCart_Click(object sender, RoutedEventArgs e)
        {
            CartItem item = CartGrid.SelectedItem as CartItem;
            if (item == null)
            {
                MessageBox.Show("Выберите позицию в корзине.");
                return;
            }
            cart.Remove(item);
            UpdateCart();
        }

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            if (cart.Count == 0)
            {
                MessageBox.Show("Корзина пустая.");
                return;
            }

            try
            {
                using (AptekaEntities db = new AptekaEntities())
                {
                    Sotrudniki employee = db.Sotrudnikis.FirstOrDefault(x => x.IdUser == userId);
                    if (employee == null)
                    {
                        MessageBox.Show("Учётная запись не связана с сотрудником. Нужно заполнить IdUser в таблице Sotrudniki.");
                        return;
                    }

                    foreach (CartItem item in cart)
                    {
                        Preparaty product = db.Preparaties.Find(item.IdPreparata);
                        if (product == null)
                        {
                            MessageBox.Show("Один из препаратов был удалён. Обновите каталог.");
                            return;
                        }

                        Prodazhi sale = new Prodazhi();
                        sale.IdPreparata = item.IdPreparata;
                        sale.IdSotrudnika = employee.IdSotrudnika;
                        sale.DataProdazhi = DateTime.Now;
                        sale.Kolichestvo = item.Kolichestvo;
                        sale.CenaProdazhi = product.Cena;
                        db.Prodazhis.Add(sale);
                    }
                    db.SaveChanges();
                }
                cart.Clear();
                UpdateCart();
                MessageBox.Show("Продажа оформлена.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка оформления продажи: " + ex.GetBaseException().Message);
            }
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (roleId != 1) return;
            ProductWindow window = new ProductWindow(0);
            window.Owner = this;
            if (window.ShowDialog() == true) LoadProducts();
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (roleId != 1) return;
            int id = (int)((Button)sender).Tag;
            ProductWindow window = new ProductWindow(id);
            window.Owner = this;
            if (window.ShowDialog() == true) LoadProducts();
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (roleId != 1) return;
            int id = (int)((Button)sender).Tag;
            if (MessageBox.Show("Удалить этот препарат?", "Подтверждение",
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            try
            {
                using (AptekaEntities db = new AptekaEntities())
                {
                    Preparaty product = db.Preparaties.Find(id);
                    if (product == null) return;
                    if (db.Prodazhis.Any(x => x.IdPreparata == id))
                    {
                        MessageBox.Show("Нельзя удалить препарат, который уже есть в истории продаж.");
                        return;
                    }
                    db.Preparaties.Remove(product);
                    db.SaveChanges();
                }
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.GetBaseException().Message);
            }
        }

        private void Sales_Click(object sender, RoutedEventArgs e)
        {
            SalesWindow window = new SalesWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.Show();
            Close();
        }
    }
}
