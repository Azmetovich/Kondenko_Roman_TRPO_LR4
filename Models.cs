using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    internal class Models
    {
    }
    public class ProductCard
    {
        public int IdPreparata { get; set; }
        public string Nazvanie { get; set; }
        public string Opisanie { get; set; }
        public decimal Cena { get; set; }
        public string Proizvoditel { get; set; }
        public string Kategoriya { get; set; }
    }

    public class CartItem
    {
        public int IdPreparata { get; set; }
        public string Nazvanie { get; set; }
        public decimal Cena { get; set; }
        public int Kolichestvo { get; set; }
        public decimal Summa { get { return Cena * Kolichestvo; } }
    }

    public class SaleRow
    {
        public int IdProdazhi { get; set; }
        public string Preparat { get; set; }
        public string Sotrudnik { get; set; }
        public System.DateTime DataProdazhi { get; set; }
        public int Kolichestvo { get; set; }
        public decimal CenaProdazhi { get; set; }
        public decimal Summa { get { return CenaProdazhi * Kolichestvo; } }
    }
}
