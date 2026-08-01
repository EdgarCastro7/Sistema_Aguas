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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SistemaAguas.WPF
{
    /// <summary>
    /// Interaction logic for ConsumosWindow.xaml
    /// </summary>
    public partial class ConsumosWindow : Window
    {

        private readonly HttpClient client = new HttpClient();

        public ConsumosWindow()
        {
            InitializeComponent();
            client.BaseAddress = new Uri("https://localhost:44327/");

            CarregarConsumos();
            CarregarContadores();
        }

        private async void btnAdicionar_Click(object sender, RoutedEventArgs e)
        {
            if (cbContadores.SelectedItem == null)
            {
                MessageBox.Show("Selecione um contador.");
                return;
            }

            if (dpDataLeitura.SelectedDate == null)
            {
                MessageBox.Show("Selecione a data de leitura.");
                return;
            }

            if (!double.TryParse(txtLeituraAtual.Text, out double leituraAtual))
            {
                MessageBox.Show("Introduza uma leitura válida.");
                return;
            }

            Contador contador = (Contador)cbContadores.SelectedItem;

            Consumo consumo = new Consumo();
            {
                consumo.LeituraAtual = leituraAtual;
                consumo.DataLeitura = (DateTime)dpDataLeitura.SelectedDate;
                consumo.ContadorId = contador.Id;
            }

            HttpResponseMessage response = await client.PostAsJsonAsync($"api/consumos", consumo);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Consumo adicionado!");

                await CarregarConsumos();
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
            if (dgConsumos.SelectedItem == null)
            {
                MessageBox.Show("Selecione um consumo.");
                return;
            }

            if (cbContadores.SelectedItem == null)
            {
                MessageBox.Show("Selecione um contador.");
                return;
            }

            if (dpDataLeitura.SelectedDate == null)
            {
                MessageBox.Show("Selecione a data de leitura.");
                return;
            }

            if (!double.TryParse(txtLeituraAtual.Text, out double leituraAtual))
            {
                MessageBox.Show("Introduza uma leitura válida.");
                return;
            }

            Contador contador = (Contador)cbContadores.SelectedItem;
            Consumo consumo = (Consumo)dgConsumos.SelectedItem;

            consumo.DataLeitura = (DateTime)dpDataLeitura.SelectedDate;
            consumo.LeituraAtual = leituraAtual;
            consumo.ContadorId = contador.Id;


            HttpResponseMessage response = await client.PutAsJsonAsync($"api/consumos/{consumo.Id}", consumo);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Consumo atualizado!");
                await CarregarConsumos();

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
            if (dgConsumos.SelectedItem == null)
            {
                MessageBox.Show("Selecione um consumo");
                return;
            }

            MessageBoxResult resultado = MessageBox.Show("Tem a certeza que pretende eliminar este consumo?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resultado == MessageBoxResult.No)
            {
                return;
            }

            Consumo consumo = (Consumo)dgConsumos.SelectedItem;

            HttpResponseMessage response = await client.DeleteAsync($"api/consumos/{consumo.Id}");

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Consumo eliminado!");
                await CarregarConsumos();

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

        private void dgConsumos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(dgConsumos.SelectedItem == null)
            {
                return;
            }

            Consumo consumo = (Consumo)dgConsumos.SelectedItem;

            txtLeituraAnterior.Text = consumo.LeituraAnterior.ToString();
            txtLeituraAtual.Text = consumo.LeituraAtual.ToString();
            txtValorConsumido.Text = consumo.ValorConsumido.ToString();
            dpDataLeitura.SelectedDate = consumo.DataLeitura;
            cbContadores.SelectedValue = consumo.ContadorId;

        }

        private async Task CarregarConsumos()
        {
            HttpResponseMessage response = await client.GetAsync("api/consumos");

            if(response.IsSuccessStatusCode)
            {
                List<Consumo> consumos = await response.Content.ReadAsAsync<List<Consumo>>();
            
                dgConsumos.ItemsSource = consumos;

                LimparCampos();
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

        private void LimparCampos()
        {
            txtLeituraAnterior.Clear();
            txtValorConsumido.Clear();
            txtLeituraAtual.Clear();
            cbContadores.SelectedItem = null;
            dpDataLeitura.SelectedDate = null;
        }

        private void txtLeituraAtual_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(txtLeituraAtual.Text, out double leituraAtual) &&
                double.TryParse(txtLeituraAnterior.Text, out double leituraAnterior))
            {
                txtValorConsumido.Text = (leituraAtual - leituraAnterior).ToString();
            }
            else
            {
                txtValorConsumido.Clear();
            }
            AtualizarConsumo();
        }

        private void AtualizarConsumo()
        {
            if (double.TryParse(txtLeituraAnterior.Text, out double leituraAnterior) &&
                double.TryParse(txtLeituraAtual.Text, out double leituraAtual))
            {
                txtValorConsumido.Text = (leituraAtual - leituraAnterior).ToString();
            }
            else
            {
                txtValorConsumido.Clear();
            }
        }

        private async void cbContadores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbContadores.SelectedItem == null)
            {
                return;
            }

            Contador contador = (Contador)cbContadores.SelectedItem;

            HttpResponseMessage response = await client.GetAsync("api/consumos");

            if (response.IsSuccessStatusCode)
            {
                List<Consumo> consumos = await response.Content.ReadAsAsync<List<Consumo>>();

                Consumo ultimoConsumo = consumos.Where(c => c.ContadorId == contador.Id).OrderByDescending(c => c.DataLeitura).FirstOrDefault();

                if (ultimoConsumo == null)
                {
                    txtLeituraAnterior.Text = "100";
                }
                else
                {
                    txtLeituraAnterior.Text = ultimoConsumo.LeituraAtual.ToString();
                }
                AtualizarConsumo();
            }
        }
    }
}
