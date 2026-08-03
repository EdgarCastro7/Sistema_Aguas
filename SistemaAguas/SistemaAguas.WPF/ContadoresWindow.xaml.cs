using SistemaAguas.WPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SistemaAguas.WPF
{
    /// <summary>
    /// Interaction logic for ContadoresWindow.xaml
    /// </summary>
    public partial class ContadoresWindow : Window
    {
        private readonly HttpClient client = new HttpClient();
 
        public ContadoresWindow()
        {
            InitializeComponent();
            client.BaseAddress = new Uri("http://sistemaaguas107.somee.com/");

            CarregarContadores();
            CarregarClientes();
        }

        private async Task CarregarClientes()
        {
            HttpResponseMessage response = await client.GetAsync("api/clientes");

            if (response.IsSuccessStatusCode)
            {
                List<Cliente> clientes = await response.Content.ReadAsAsync<List<Cliente>>();

                cbClientes.ItemsSource = clientes;
            }
            else
            {
                string erro = await response.Content.ReadAsStringAsync();
                MessageBox.Show(erro);
            }
        }

        private async Task CarregarContadores()
        {
            HttpResponseMessage response = await client.GetAsync("api/contadores");

            if (response.IsSuccessStatusCode)
            {
                List<Contador> contadores = await response.Content.ReadAsAsync<List<Contador>>();

                dgContadores.ItemsSource = contadores;             
            }
            else
            {
                string erro = await response.Content.ReadAsStringAsync();
                MessageBox.Show(erro);
            }
        }

        private void dgContadores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (dgContadores.SelectedItem == null)
            {
                return;
            }

            Contador contador = (Contador)dgContadores.SelectedItem;

            txtNumeroContador.Text = contador.NumeroContador;
            cbClientes.SelectedValue = contador.ClienteId;
            dpDataInstalacao.SelectedDate = contador.DataInstalacao;
            chkAtivo.IsChecked = contador.Ativo;
        }

        private void LimparCampos()
        {
            txtNumeroContador.Clear();
            chkAtivo.IsChecked = false;
            dpDataInstalacao.SelectedDate = null;
            cbClientes.SelectedItem = null;
            dgContadores.SelectedItem = null;
        }

        private async void btnAdicionar_Click(object sender, RoutedEventArgs e)
        {
            if(cbClientes.SelectedItem == null)
            {
                MessageBox.Show("Selecione um cliente");
                return;
            }

            Cliente cliente = (Cliente)cbClientes.SelectedItem;
            Contador contador = new Contador();
            {
                contador.NumeroContador = txtNumeroContador.Text;
                contador.ClienteId = cliente.Id;
                contador.DataInstalacao = (DateTime)dpDataInstalacao.SelectedDate;
                contador.Ativo = chkAtivo.IsChecked ?? false;
            }


            if (string.IsNullOrWhiteSpace(txtNumeroContador.Text))
            {
                MessageBox.Show("Introduza o número do contador.");
                return;
            }

            if (cbClientes.SelectedItem == null)
            {
                MessageBox.Show("Selecione um cliente.");
                return;
            }

            if (dpDataInstalacao.SelectedDate == null)
            {
                MessageBox.Show("Selecione a data de instalação.");
                return;
            }

            HttpResponseMessage response = await client.PostAsJsonAsync($"api/contadores", contador);

            if(response.IsSuccessStatusCode)
            {
                MessageBox.Show("Controlador adicionado!");

                await CarregarContadores();
                LimparCampos();
            }
            else
            {
                string erro = await response.Content.ReadAsStringAsync();
                MessageBox.Show(erro);
            }
        }

        private void btnLimpar_Click(object sender, RoutedEventArgs e)
        {
            LimparCampos();
        }

        private async void btnEditar_Click(object sender, RoutedEventArgs e)
        {

            if (dgContadores.SelectedItem == null)
            {
                MessageBox.Show("Selecione o contador");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNumeroContador.Text))
            {
                MessageBox.Show("Introduza o número do contador.");
                return;
            }

            if (cbClientes.SelectedItem == null)
            {
                MessageBox.Show("Selecione um cliente.");
                return;
            }

            if (dpDataInstalacao.SelectedDate == null)
            {
                MessageBox.Show("Selecione a data de instalação.");
                return;
            }

            Contador contador = (Contador)dgContadores.SelectedItem;
            Cliente cliente = (Cliente)cbClientes.SelectedItem;

            contador.NumeroContador = txtNumeroContador.Text;
            contador.ClienteId = cliente.Id;
            contador.DataInstalacao = (DateTime)dpDataInstalacao.SelectedDate;
            contador.Ativo = chkAtivo.IsChecked ?? false;

            HttpResponseMessage response = await client.PutAsJsonAsync($"api/contadores/{contador.Id}", contador);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Contador atualizado!");
                await CarregarContadores();

                LimparCampos();
            }
            else
            {
                string erro = await response.Content.ReadAsStringAsync();
                MessageBox.Show(erro);
            }
        }

        private async void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if(dgContadores.SelectedItem == null)
            {
                MessageBox.Show("Selecione o contador");
                return;
            }

            MessageBoxResult resultado = MessageBox.Show("Tem a certeza que pretende eliminar este contador?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resultado == MessageBoxResult.No)
            {
                return;
            }

            Contador contador = (Contador)dgContadores.SelectedItem;

            HttpResponseMessage response = await client.DeleteAsync($"api/contadores/{contador.Id}");

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Contador eliminado!");
                await CarregarContadores();

                LimparCampos();
            }
            else
            {
                string erro = await response.Content.ReadAsStringAsync();
                MessageBox.Show(erro);
            }
        }

        private void cbClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
