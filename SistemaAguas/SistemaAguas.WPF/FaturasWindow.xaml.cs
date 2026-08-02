using SistemaAguas.WPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting;
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

namespace SistemaAguas.WPF
{
    /// <summary>
    /// Interaction logic for FaturasWindow.xaml
    /// </summary>
    public partial class FaturasWindow : Window
    {

        HttpClient client = new HttpClient();

        public FaturasWindow()
        {
            InitializeComponent();

            client.BaseAddress = new Uri("https://localhost:44327/");

            CarregarFaturas();
            CarregarClientes();
            CarregarContadores();
            CarregarConsumos();
        }

        private async Task CarregarConsumos()
        {
            HttpResponseMessage response = await client.GetAsync("api/consumos");

            if (response.IsSuccessStatusCode)
            {
                List<Consumo> consumos = await response.Content.ReadAsAsync<List<Consumo>>();

                cbConsumos.ItemsSource = consumos;
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

                cbContadores.ItemsSource = contadores;
            }
            else
            {
                string erro = await response.Content.ReadAsStringAsync();
                MessageBox.Show(erro);
            }
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

        private async Task CarregarFaturas()
        {
            HttpResponseMessage response = await client.GetAsync("api/faturas");

            if(response.IsSuccessStatusCode)
            {
                List<Fatura> faturas = await response.Content.ReadAsAsync<List<Fatura>>();

                dgFaturas.ItemsSource = faturas;
            }
            else
            {
                string erro = await response.Content.ReadAsStringAsync();

                MessageBox.Show(erro);
            }
        }

        private void LimparCampos()
        {
            cbClientes.SelectedItem = null;
            cbContadores.SelectedItem = null;
            cbConsumos.SelectedItem = null;

            dpDataFatura.SelectedDate = null;

            txtValorTotal.Clear();

            chkPago.IsChecked = false;
            chkAnulada.IsChecked = false;

            dgFaturas.SelectedItem = null;
        }

        private void dgFaturas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgFaturas.SelectedItem == null)
            {
                return;
            }

            Fatura fatura = (Fatura)dgFaturas.SelectedItem;

            cbClientes.SelectedValue = fatura.ClienteId;
            cbContadores.SelectedValue = fatura.ContadorId;
            cbConsumos.SelectedValue = fatura.ConsumoId;

            dpDataFatura.SelectedDate = fatura.DataFatura;

            txtValorTotal.Text = fatura.ValorTotal.ToString();

            chkPago.IsChecked = fatura.Pago;
            chkAnulada.IsChecked = fatura.Anulada;
        }

        private void btnLimpar_Click(object sender, RoutedEventArgs e)
        {
            LimparCampos();
        }

        private async void btnAdicionar_Click(object sender, RoutedEventArgs e)
        {
            if (cbClientes.SelectedItem == null)
            {
                MessageBox.Show("Selecione um cliente.");
                return;
            }

            if (cbContadores.SelectedItem == null)
            {
                MessageBox.Show("Selecione um contador.");
                return;
            }

            if (cbConsumos.SelectedItem == null)
            {
                MessageBox.Show("Selecione um consumo.");
                return;
            }

            if (dpDataFatura.SelectedDate == null)
            {
                MessageBox.Show("Selecione a data da fatura.");
                return;
            }

            Cliente cliente = (Cliente)cbClientes.SelectedItem;
            Contador contador = (Contador)cbContadores.SelectedItem;
            Consumo consumo = (Consumo)cbConsumos.SelectedItem;

            Fatura fatura = new Fatura();

            fatura.ClienteId = cliente.Id;
            fatura.ContadorId = contador.Id;
            fatura.ConsumoId = consumo.Id;
            fatura.DataFatura = (DateTime)dpDataFatura.SelectedDate;
            fatura.Pago = chkPago.IsChecked ?? false;
            fatura.Anulada = chkAnulada.IsChecked ?? false;

            HttpResponseMessage response = await client.PostAsJsonAsync("api/faturas", fatura);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Fatura adicionada!");

                await CarregarFaturas();

                LimparCampos();
            }
            else
            {
                string erro = await response.Content.ReadAsStringAsync();
                MessageBox.Show(erro);
            }
        }

        private async void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (dgFaturas.SelectedItem == null)
            {
                MessageBox.Show("Selecione uma fatura.");
                return;
            }

            Cliente cliente = (Cliente)cbClientes.SelectedItem;
            Contador contador = (Contador)cbContadores.SelectedItem;
            Consumo consumo = (Consumo)cbConsumos.SelectedItem;

            Fatura fatura = (Fatura)dgFaturas.SelectedItem;

            fatura.ClienteId = cliente.Id;
            fatura.ContadorId = contador.Id;
            fatura.ConsumoId = consumo.Id;
            fatura.DataFatura = (DateTime)dpDataFatura.SelectedDate;
            fatura.Pago = chkPago.IsChecked ?? false;
            fatura.Anulada = chkAnulada.IsChecked ?? false;

            HttpResponseMessage response = await client.PutAsJsonAsync($"api/faturas/{fatura.Id}", fatura);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Fatura atualizada!");

                await CarregarFaturas();

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
            if (dgFaturas.SelectedItem == null)
            {
                MessageBox.Show("Selecione uma fatura.");
                return;
            }

            MessageBoxResult resultado = MessageBox.Show("Tem a certeza que pretende eliminar esta fatura?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resultado == MessageBoxResult.No)
            {
                return;
            }

            Fatura fatura = (Fatura)dgFaturas.SelectedItem;

            HttpResponseMessage response = await client.DeleteAsync($"api/faturas/{fatura.Id}");

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Fatura eliminada!");

                await CarregarFaturas();

                LimparCampos();
            }
            else
            {
                string erro = await response.Content.ReadAsStringAsync();
                MessageBox.Show(erro);
            }
        }
    }
}
