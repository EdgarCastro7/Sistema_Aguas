using SistemaAguas.WPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
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

            client.BaseAddress = new Uri("http://sistemaaguas107.somee.com/");

            _ = Inicializar();
        }

        private async Task Inicializar()
        {
            await CarregarClientes();
            await CarregarContadores();
            await CarregarConsumos();
            await CarregarFaturas();

            cbEstadoPagamento.SelectedIndex = 0;
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
                cbClientes.DisplayMemberPath = "Nome";
                cbClientes.SelectedValuePath = "Id";

                cbPesquisaCliente.ItemsSource = clientes;
                cbPesquisaCliente.DisplayMemberPath = "Nome";
                cbPesquisaCliente.SelectedValuePath = "Id";
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

            if (response.IsSuccessStatusCode)
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

            cbPesquisaCliente.SelectedItem = null;
            dpInicio.SelectedDate = null;
            dpFim.SelectedDate = null;
            cbEstadoPagamento.SelectedIndex = 0;
        }

        private async void dgFaturas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbContadores.SelectedItem == null)
            {
                return;
            }

            Contador contador = (Contador)cbContadores.SelectedItem;

            HttpResponseMessage response = await client.GetAsync($"api/clientes/{contador.ClienteId}");

            if (response.IsSuccessStatusCode)
            {
                Cliente cliente = await response.Content.ReadAsAsync<Cliente>();

                cbClientes.SelectedValue = cliente.Id;
            }

            await CarregarConsumosDoContador(contador.Id);

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
            chkAnulada.IsChecked = fatura.Anulado;
        }

        private async Task CarregarConsumosDoContador(int contadorId)
        {
            HttpResponseMessage response = await client.GetAsync($"api/consumos/contador/{contadorId}");

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

        private async void btnLimpar_Click(object sender, RoutedEventArgs e)
        {
            LimparCampos();
            await CarregarFaturas();
        }

        private async void btnAdicionar_Click(object sender, RoutedEventArgs e)
        {
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

            Contador contador = (Contador)cbContadores.SelectedItem;
            Consumo consumo = (Consumo)cbConsumos.SelectedItem;

            Fatura fatura = new Fatura();

            fatura.ClienteId = contador.ClienteId;
            fatura.ContadorId = contador.Id;
            fatura.ConsumoId = consumo.Id;
            fatura.DataFatura = dpDataFatura.SelectedDate.Value;
            fatura.Pago = chkPago.IsChecked ?? false;
            fatura.Anulado = chkAnulada.IsChecked ?? false;

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

            Fatura fatura = (Fatura)dgFaturas.SelectedItem;
            Contador contador = (Contador)cbContadores.SelectedItem;
            Consumo consumo = (Consumo)cbConsumos.SelectedItem;

            if (contador == null || consumo == null)
            {
                MessageBox.Show("Selecione um contador e um consumo.");
                return;
            }

            if (dpDataFatura.SelectedDate == null)
            {
                MessageBox.Show("Selecione a data da fatura.");
                return;
            }

            fatura.ContadorId = contador.Id;
            fatura.ConsumoId = consumo.Id;
            fatura.ClienteId = contador.ClienteId;
            fatura.DataFatura = dpDataFatura.SelectedDate.Value;
            fatura.Pago = chkPago.IsChecked ?? false;
            fatura.Anulado = chkAnulada.IsChecked ?? false;


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

        private async void cbContadores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbContadores.SelectedItem == null)
            {
                return;
            }
            Contador contador = (Contador)cbContadores.SelectedItem;

            cbClientes.SelectedValue = contador.ClienteId;

            await CarregarConsumosDoContador(contador.Id);
        }

        private void cbConsumos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbConsumos.SelectedItem == null)
            {
                return;
            }

            Consumo consumo = (Consumo)cbConsumos.SelectedItem;

            double tarifa;

            if (consumo.ValorConsumido <= 5)
            {
                tarifa = 0.3;
            }
            else if (consumo.ValorConsumido <= 15)
            {
                tarifa = 0.8;
            }
            else if (consumo.ValorConsumido <= 25)
            {
                tarifa = 1.2;
            }
            else
            {
                tarifa = 1.6;
            }
            txtValorTotal.Text = (consumo.ValorConsumido * tarifa).ToString("0.00");
        }

        private async void btnPesquisar_Click(object sender, RoutedEventArgs e)
        {
            await PesquisarFaturas();
        }

        private async Task PesquisarFaturas()
        {
            string cliente = "";
            string dataInicio = "";
            string dataFim = "";
            string pago = "";

            if (cbPesquisaCliente.SelectedValue != null)
            {
                cliente = "clienteId=" + cbPesquisaCliente.SelectedValue + "&";
            }

            if (dpInicio.SelectedDate != null)
            {
                dataInicio = "dataInicio=" + dpInicio.SelectedDate.Value.ToString("yyyy-MM-dd") + "&";
            }

            if (dpFim.SelectedDate != null)
            {
                dataFim = "dataFim=" + dpFim.SelectedDate.Value.ToString("yyyy-MM-dd") + "&";
            }

            if (cbEstadoPagamento.SelectedIndex == 1)
            {
                pago = "pago=true&";
            }
            else if (cbEstadoPagamento.SelectedIndex == 2)
            {
                pago = "pago=false&";
            }

            HttpResponseMessage response = await client.GetAsync($"api/faturas/pesquisar?{cliente}{dataInicio}{dataFim}{pago}");

            if (response.IsSuccessStatusCode)
            {
                dgFaturas.ItemsSource = await response.Content.ReadAsAsync<List<Fatura>>();
            }
        }
    }
}
    
    
