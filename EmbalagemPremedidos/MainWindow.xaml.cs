using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace EmbalagemPremedidos
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        private Produto produto;
        long ID;
        bool isCodBarra, isPesoEmbalagem;
        private int errorCount;

        public MainWindow()
        {
            //new TesteBanco().Clear();
            InitializeComponent();
            produto = new Produto();
            produto.PropertyChanged += Produto_PropertyChanged;
            stackPanelProduto.DataContext = produto;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (var db = new Dados())
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
                        Mensagem("Peso da Embalagem inválido", Brushes.Red);
                        errorCount++; break;
                    }
                case ValidationErrorEventAction.Removed:
                    {
                        Mensagem("", Brushes.Gray);
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
                isCodBarra = (ValidaCodBarra()) ? true : false;
                if (isCodBarra)
                {
                    using (var db = new Dados())
                    {
                        Produto p = (from s in db.Produto
                                     where s.CodBarra == produto.CodBarra
                                     select s).FirstOrDefault();
                        if (p != null)
                        {
                            ID = p.Id;
                            produto.CodBarra = p.CodBarra;
                            produto.PesoEmbalagem = p.PesoEmbalagem;
                            produto.Descricao = p.Descricao;
                            btnSalvar.Content = "Atualizar";
                            Mensagem("Alterando produto", Brushes.Green);
                        }
                        else
                        {
                            btnSalvar.Content = "Salvar";
                            Mensagem("Inserindo novo Produto", Brushes.Green);
                        }
                    } 
                }
                else
                {
                    Mensagem("Código de Barras Inválido", Brushes.Red);
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
            btnSalvar.IsEnabled = false;
            using (var db = new Dados())
            {
                if (ID == 0)
                {
                    InsereNovoProduto(db);
                }
                else
                {
                    AtualizaProduto(db);
                }
                int qtdProdutos = db.Produto.Count();
                statusBarItensCadastrados.Content = "Produtos Cadastrados: " + qtdProdutos;
            }
                        
            Mensagem("Produto Salvo", Brushes.Green);

            //Limpa produto
            ID = 0;
            produto.PesoEmbalagem = 0;
            produto.Descricao = null;
            btnSalvar.Content = "Salvo";
        }

        private void InsereNovoProduto(Dados db)
        {
            db.Produto.InsertOnSubmit(produto);
            db.SubmitChanges();
        }
        private void AtualizaProduto(Dados db)
        {
            Produto p = (from q in db.Produto
                        where q.Id == ID
                        select q).SingleOrDefault();
            p.PesoEmbalagem = produto.PesoEmbalagem;
            p.Descricao = produto.Descricao;
            db.SubmitChanges();
        }

        private bool ValidaCodBarra()
        {
            Regex regex = new Regex("^[0-9]{13}$");
            if (string.IsNullOrEmpty(produto.CodBarra) || !regex.IsMatch(produto.CodBarra))
            {
                return false;
            }
            int[] numeros = Array.ConvertAll(produto.CodBarra.ToCharArray(), c => (int)Char.GetNumericValue(c));
            int somaPares = numeros[1] + numeros[3] + numeros[5] + numeros[7] + numeros[9] + numeros[11];
            int somaImpares = numeros[0] + numeros[2] + numeros[4] + numeros[6] + numeros[8] + numeros[10];
            int resultado = somaImpares + somaPares * 3;
            int digitoVerificador = 10 - resultado % 10;
            return digitoVerificador == numeros[12];
        }

        private void Mensagem(string msg, Brush color)
        {
            tbMensagem.Text = msg;
            tbMensagem.Foreground = color;
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
