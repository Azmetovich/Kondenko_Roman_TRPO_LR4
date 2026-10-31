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
    /// Interaction logic for ProductWindow.xaml
    /// </summary>
    public partial class ProductWindow : Window
    {
        private int productId;

        public ProductWindow(int id)
        {
            InitializeComponent();
            productId = id;
            if (id != 0) TitleText.Text = "Редактирование препарата";
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (AptekaEntities db = new AptekaEntities())
                {
                    ManufacturerBox.ItemsSource = db.Proizvoditelis.OrderBy(x => x.Nazvanie).ToList();
                    CategoryBox.ItemsSource = db.Kategoriis.OrderBy(x => x.NazvanieKategorii).ToList();

                    if (productId != 0)
                    {
                        Preparaty product = db.Preparaties.Find(productId);
                        if (product == null)
                        {
                            MessageBox.Show("Препарат не найден.");
                            Close();
                            return;
                        }
                        NameBox.Text = product.Nazvanie;
                        DescriptionBox.Text = product.Opisanie;
                        PriceBox.Text = product.Cena.ToString("0.00");
                        ManufacturerBox.SelectedValue = product.IdProizvoditelya;
                        CategoryBox.SelectedValue = product.IdKategorii;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.GetBaseException().Message);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string name = NameBox.Text.Trim();
            string description = DescriptionBox.Text.Trim();
            decimal price;

            if (name == "" || ManufacturerBox.SelectedValue == null || CategoryBox.SelectedValue == null)
            {
                MessageBox.Show("Заполните название, производителя и категорию.");
                return;
            }
            if (!decimal.TryParse(PriceBox.Text.Trim().Replace('.', ','), out price) || price <= 0)
            {
                MessageBox.Show("Введите правильную цену больше нуля.");
                return;
            }

            try
            {
                using (AptekaEntities db = new AptekaEntities())
                {
                    Preparaty product;
                    if (productId == 0)
                    {
                        product = new Preparaty();
                        db.Preparaties.Add(product);
                    }
                    else
                    {
                        product = db.Preparaties.Find(productId);
                        if (product == null)
                        {
                            MessageBox.Show("Препарат не найден.");
                            return;
                        }
                    }

                    product.Nazvanie = name;
                    if (description == "")
                    {
                        product.Opisanie = null;
                    }
                    else
                    {
                        product.Opisanie = description;
                    }
                    product.Cena = price;
                    product.IdProizvoditelya = (int)ManufacturerBox.SelectedValue;
                    product.IdKategorii = (int)CategoryBox.SelectedValue;
                    db.SaveChanges();
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.GetBaseException().Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
