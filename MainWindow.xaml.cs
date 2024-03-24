using CurrencyConverterSimple.Models;
using System.Windows;
using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Newtonsoft.Json;
using System.Data;
using System.Text.RegularExpressions;


namespace CurrencyConverterSimple
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Root root = new Root();
            SetRoot(root);
        }

        private async void SetRoot(Root root)
        {
            // Insert your app_id below
            root = await GetData<Root>("https://openexchangerates.org/api/latest.json?app_id=");
            PopulateComboBox(cmbFromCurrency, root);
            PopulateComboBox(cmbToCurrency, root);
        }

        public static async Task<Root> GetData<T>(string url)
        {
            Root tempRoot = new Root();
            try
            {
                HttpClient client = new HttpClient();
                using (client)
                {
                    client.Timeout = TimeSpan.FromMinutes(1);
                    HttpResponseMessage responseMessage = await client.GetAsync(url);
                    if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string response = await responseMessage.Content.ReadAsStringAsync();
                        Root getResponceObject = JsonConvert.DeserializeObject<Root>(response);
                        return getResponceObject;
                    }
                    return tempRoot;
                }
            }
            catch
            {
                return tempRoot;
            }
        }

        private void PopulateComboBox(ComboBox inputComboBox, Root root)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Name");
            dataTable.Columns.Add("Value");

            Dictionary<string, double> dict = root.rates.GetType().GetProperties()
                                                .ToDictionary(prop => prop.Name, prop => (double)prop.GetValue(root.rates, null));


            dataTable.Rows.Add("---SELECT---", 0);
            foreach (KeyValuePair<string, double> entry in dict)
            {
                dataTable.Rows.Add(entry.Key.ToString(), entry.Value);
            }

            inputComboBox.DisplayMemberPath = "Name";
            inputComboBox.SelectedValuePath = "Value";
            inputComboBox.ItemsSource = dataTable.DefaultView;
            inputComboBox.SelectedIndex = 0;
        }

        private void CheckInput(bool isNum)
        {
            if (Regex.IsMatch(txtCurrency.Text, @"^\s+$") || txtCurrency.Text == "")
            {
                txtCurrency.Text = string.Empty;
                throw new Exception("Wrong input! Input is empty.");
            }
            else if (!isNum)
            {
                txtCurrency.Text = string.Empty;
                throw new Exception("Wrong input! Input must be a number.");
            }
        }
        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            bool isNum = Int32.TryParse(txtCurrency.Text, out int number);
            try
            {
                CheckInput(isNum);

                if (cmbFromCurrency.SelectedIndex == 0 || cmbToCurrency.SelectedIndex == 0)
                {
                    MessageBox.Show("Please input currency type field.", "Currency converter", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    double amount = Convert.ToDouble(txtCurrency.Text);

                    DataRowView dataFrom = (DataRowView)cmbFromCurrency.SelectedItem;
                    double valueFrom = Convert.ToDouble(dataFrom["Value"]);

                    DataRowView dataTo = (DataRowView)cmbToCurrency.SelectedItem;
                    double valueTo = Convert.ToDouble(dataTo["Value"]);

                    if (cmbFromCurrency.Text.Equals("USD"))
                    {
                        string formatted = String.Format("{0:0.00}", amount * valueTo); 

                        lblCurrency.Content = $"{formatted} {dataTo["Name"]}";
                    }
                    else
                    {
                        string formatted = String.Format("{0:0.00}", amount / valueFrom * valueTo);
                        lblCurrency.Content = $"{formatted} {dataTo["Name"]}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " \nPlease try again.", "Currency converter", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearAll();
        }

        private void ClearAll()
        {
            lblCurrency.Content = string.Empty;
            txtCurrency.Text = string.Empty;
            cmbFromCurrency.SelectedIndex = 0;
            cmbToCurrency.SelectedIndex = 0;
        }

        private void Swap_Button_Click(object sender, RoutedEventArgs e)
        {
            // swap currency selections 
            (cmbFromCurrency.SelectedIndex, cmbToCurrency.SelectedIndex) = (cmbToCurrency.SelectedIndex, cmbFromCurrency.SelectedIndex);
        }
    }
}
