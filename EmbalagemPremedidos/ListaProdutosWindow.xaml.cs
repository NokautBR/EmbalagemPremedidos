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

namespace EmbalagemPremedidos
{
    /// <summary>
    /// Lógica interna para ListaProdutosWindow.xaml
    /// </summary>
    public partial class ListaProdutosWindow : Window
    {
        public ListaProdutosWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (var db = new DadosDataContext())
            {
               dataGrid.ItemsSource = db.Produto;
            }
        }
    }
}
