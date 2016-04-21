using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.ComponentModel;

namespace EmbalagemPremedidos
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        private Produto produto;
        bool isCodBarra, isPesoEmbalagem;
        private int errorCount;

        public MainWindow()
        {
            InitializeComponent();
            produto = new Produto();
            produto.PropertyChanged += Produto_PropertyChanged;
            stackPanelProduto.DataContext = produto;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (var db = new DadosDataContext())
            {
                int qtdProdutos = db.Produto.Count();
                statusBarItensCadastrados.Content = "Produtos Cadastrados: " + qtdProdutos;
            }
            this.AddHandler(Validation.ErrorEvent, new RoutedEventHandler(OnErrorEvent));
        }

        private void OnErrorEvent(object sender, RoutedEventArgs e)
        {
            var validationEventArgs = e as ValidationErrorEventArgs;
            if (validationEventArgs == null)
                throw new Exception("Unexpected event args");
            switch (validationEventArgs.Action)
            {
                case ValidationErrorEventAction.Added:
                    {
                        errorCount++; break;
                    }
                case ValidationErrorEventAction.Removed:
                    {
                        errorCount--; break;
                    }
                default:
                    {
                        throw new Exception("Unknown action");
                    }
            }
            btnSalvar.IsEnabled = errorCount == 0;
        }

        private void Produto_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //Verifica se código de barra ja existe
            if(e.PropertyName.Equals("CodBarra"))
            {
                double temp = 0;
                isCodBarra = (double.TryParse(produto.CodBarra, out temp)) ? true : false;
                if (isCodBarra)
                {
                    using (var db = new DadosDataContext())
                    {
                        Produto p = (from s in db.Produto
                                     where s.CodBarra == produto.CodBarra
                                     select s).FirstOrDefault();
                        if (p != null)
                        {
                            produto.Id = p.Id;
                            produto.CodBarra = p.CodBarra;
                            produto.PesoEmbalagem = p.PesoEmbalagem;
                            produto.Descricao = p.Descricao;
                            btnSalvar.Content = "Atualizar";
                            statusBarItemInfo.Content = "Alterando produto";
                        }
                        else
                        {
                            btnSalvar.Content = "Salvar";
                            statusBarItemInfo.Content = "Inserindo novo produto";
                        }
                    } 
                }
            }
            else if(e.PropertyName.Equals("PesoEmbalagem"))
            {
                isPesoEmbalagem = (produto.PesoEmbalagem > 0) ? true : false;
            }

            btnSalvar.IsEnabled = isCodBarra && isPesoEmbalagem;
        }

        private void btnSalvar_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new DadosDataContext())
            {
                btnSalvar.IsEnabled = false;
                db.Produto.InsertOnSubmit(produto);
                db.SubmitChanges();

                int qtdProdutos = db.Produto.Count();
                statusBarItensCadastrados.Content = "Produtos Cadastrados: " + qtdProdutos;
                statusBarItemInfo.Content = "Produto Salvo";

                //Limpa produto
                produto.Id = 0;
                produto.CodBarra = null;
                produto.PesoEmbalagem = 0;
                produto.Descricao = null;

                btnSalvar.IsEnabled = true;
            }
        }


        #region Eventos de Usabilidade
        private void tbCodigoBarras_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int dummy;
            e.Handled = !int.TryParse(e.Text, out dummy);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox s = e.Source as TextBox;
            s.SelectAll();
        }

        private void btnListaProdutos_Click(object sender, RoutedEventArgs e)
        {
            ListaProdutosWindow listaWindows =  new ListaProdutosWindow();
            listaWindows.Show();
        }

        private void StackPanel_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox s = e.Source as TextBox;
                if (s != null)
                {
                    s.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }

                e.Handled = true;
            }
        }

        #endregion
    }
}
