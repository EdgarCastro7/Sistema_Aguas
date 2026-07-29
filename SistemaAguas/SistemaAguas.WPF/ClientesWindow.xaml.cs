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
using SistemaAguas.WPF.Models;
using System.Net.Http;

namespace SistemaAguas.WPF
{
    /// <summary>
    /// Interaction logic for ClientesWindow.xaml
    /// </summary>
    public partial class ClientesWindow : Window
    {

        private readonly HttpClient client = new HttpClient();

        public ClientesWindow()
        {
            InitializeComponent();
            client.BaseAddress = new Uri("https://localhost:44327/");

            CarregarClientes();
        }

        private async Task CarregarClientes()
        {
            HttpResponseMessage response = await client.GetAsync("api/clientes");

            if (response.IsSuccessStatusCode)
            {
                List<Cliente> clientes = await response.Content.ReadAsAsync<List<Cliente>>();

                dgClientes.ItemsSource = clientes;
            }
        }

        private void dgClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgClientes.SelectedItem == null)
            {
                return;
            }

            Cliente cliente = (Cliente)dgClientes.SelectedItem;

            txtNome.Text = cliente.Nome;
            txtMorada.Text = cliente.Morada;
            txtNif.Text = cliente.NIF;
            txtContacto.Text = cliente.Contacto;
            txtEmail.Text = cliente.Email;
            txtCodigoPostal.Text = cliente.CodigoPostal;
            chkAtivo.IsChecked = cliente.Ativo;
        }

        private async void btnAdicionar_Click(object sender, RoutedEventArgs e)
        {
            Cliente cliente = new Cliente();

            cliente.Nome = txtNome.Text;
            cliente.Morada = txtMorada.Text;
            cliente.NIF = txtNif.Text;
            cliente.Contacto = txtContacto.Text;
            cliente.Email = txtEmail.Text;
            cliente.CodigoPostal = txtCodigoPostal.Text;
            cliente.Ativo = chkAtivo.IsChecked ?? false;

            HttpResponseMessage response = await client.PostAsJsonAsync("api/clientes", cliente);

            if(response.IsSuccessStatusCode)
            {
                MessageBox.Show("Cliente adicionado!");

                await CarregarClientes();

                LimparCampos();
            }
            else
            {
                string erro = await response.Content.ReadAsStringAsync();

                MessageBox.Show(erro);
            }
        }

        private void LimparCampos()
        {
            txtCodigoPostal.Clear();
            txtContacto.Clear();
            txtEmail.Clear();
            txtMorada.Clear();
            txtNif.Clear();
            txtNome.Clear();
            chkAtivo.IsChecked = false;

            dgClientes.SelectedItem = null;
        }

        private void btnLimpar_Click(object sender, RoutedEventArgs e)
        {
            LimparCampos();
        }

        private async void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (dgClientes.SelectedItem == null)
            {
                MessageBox.Show("Selecione um cliente.");
                return;
            } 

            Cliente cliente = (Cliente)dgClientes.SelectedItem;

            cliente.Nome = txtNome.Text;
            cliente.Morada = txtMorada.Text;
            cliente.NIF = txtNif.Text;
            cliente.Contacto = txtContacto.Text;
            cliente.Email = txtEmail.Text;
            cliente.CodigoPostal = txtCodigoPostal.Text;
            cliente.Ativo = chkAtivo.IsChecked ?? false;

            HttpResponseMessage response = await client.PutAsJsonAsync($"api/clientes/{cliente.Id}", cliente);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Cliente atualizado!");

                await CarregarClientes();

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
            if (dgClientes.SelectedItem == null)
            {
                MessageBox.Show("Selecione um cliente.");
                return;
            }
             
            MessageBoxResult resultado = MessageBox.Show("Tem a certeza que pretende eliminar este cliente?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if(resultado == MessageBoxResult.No)
            {
                return;
            }

            Cliente cliente = (Cliente)dgClientes.SelectedItem;

            HttpResponseMessage response = await client.DeleteAsync($"api/clientes/{cliente.Id}");


            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Cliente apagado!");

                await CarregarClientes();

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
