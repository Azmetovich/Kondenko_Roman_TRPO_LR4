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
    /// Interaction logic for SalesWindow.xaml
    /// </summary>
    public partial class SalesWindow : Window
    {
        public SalesWindow()
        {
            InitializeComponent();
            LoadSales();
        }

        private void LoadSales()
        {
            try
            {
                using (AptekaEntities db = new AptekaEntities())
                {
                    var sales = (from sale in db.Prodazhis
                                 join product in db.Preparaties
                                 on sale.IdPreparata equals product.IdPreparata
                                 join employee in db.Sotrudnikis
                                 on sale.IdSotrudnika equals employee.IdSotrudnika
                                 orderby sale.DataProdazhi descending
                                 select new SaleRow
                                 {
                                     IdProdazhi = sale.IdProdazhi,
                                     Preparat = product.Nazvanie,
                                     Sotrudnik = employee.Familiya + " " + employee.Imya,
                                     DataProdazhi = sale.DataProdazhi,
                                     Kolichestvo = sale.Kolichestvo,
                                     CenaProdazhi = sale.CenaProdazhi
                                 }).ToList();

                    SalesGrid.ItemsSource = sales;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки продаж: " + ex.Message);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadSales();
        }
    }
}