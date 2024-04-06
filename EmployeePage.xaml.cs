using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using TimesheetMobileApp.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;

namespace TimesheetMobileApp
{
    public partial class EmployeePage : ContentPage
    {
        ObservableCollection<Employee> dataa = new ObservableCollection<Employee>();

        public EmployeePage()
        {
            InitializeComponent();

            LoadDataFromRestAPI();         


            emp_lataus.Text = "Ladataan työntekijöitä...";

            async void LoadDataFromRestAPI()
            {
                try
                {
                    HttpClientHandler GetInsecureHandler()
                    {
                        HttpClientHandler handler = new HttpClientHandler();
                        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                        {
                            if (cert.Issuer.Equals("CN=localhost"))
                                return true;
                            return errors == System.Net.Security.SslPolicyErrors.None;
                        };
                        return handler;
                    }


#if DEBUG
                    HttpClientHandler insecureHandler = GetInsecureHandler();
                    HttpClient client = new HttpClient(insecureHandler);
#else
                    HttpClient client = new HttpClient();
#endif
                    client.BaseAddress = new Uri("https://10.0.2.2:5001/");
                    string json = await client.GetStringAsync("api/employees");

                    IEnumerable<Employee> employees = JsonConvert.DeserializeObject<Employee[]>(json);

                    ObservableCollection<Employee> dataa2 = new ObservableCollection<Employee>(employees);
                    dataa = dataa2;

                    if (!Preferences.ContainsKey("User"))
                    {
                        saveButton.IsVisible = true;
                        clearButton.IsVisible = false;
                        employeeList.ItemsSource = dataa;
                    }
                    
                   
                    emp_lataus.Text = "";

                    if (Preferences.ContainsKey("User"))
                    {
                        Hakukentta.Text = Preferences.Get("User", Hakukentta.Text);
                        employeeList.ItemsSource = dataa.Where(x => x.LastName == Preferences.Get("User", Hakukentta.Text));
                        saveButton.IsVisible = false;
                        clearButton.IsVisible = true;
                        Hakukentta.SetBinding(SearchBar.SearchCommandParameterProperty, binding: new Binding(source: Hakukentta, path: "Text"));
                    }
                }

                catch (Exception e)
                {
                    await DisplayAlert("Virhe", e.Message.ToString(), "SELVÄ!");
                }
                
            }
        }


        void OnSearchBarButtonPressed(object sender, EventArgs args)
        {
            SearchBar searchBar = (SearchBar)sender;
            string searchText = searchBar.Text;

            employeeList.ItemsSource = dataa.Where(x => x.LastName.ToLower().Contains(searchText.ToLower())
            || x.FirstName.ToLower().Contains(searchText.ToLower()));

        }

        async void päivitysButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EmployeePage());
        }

        async void navButton_Clicked(object sender, EventArgs e)
        {
            Employee emp = (Employee)employeeList.SelectedItem;

            if (emp == null)
            {
                await DisplayAlert("Valinta puuttuu", "Valitse työntekijä.", "OK");
                return;
            }
            else
            {
                int id = emp.IDEmployee;
                await Navigation.PushAsync(new WorkAssignmentPage(id));
            }
        }


        async void saveButton_Clicked(object sender, EventArgs e)
        {
            Employee emp = (Employee)employeeList.SelectedItem;

            if (emp == null)
            {
                await DisplayAlert("Valinta puuttuu", "Valitse työntekijä.", "OK");
                return;
            }
            else
            {
                employeeList.ItemsSource = dataa.Where(x => x.LastName == Preferences.Get("User", Hakukentta.Text));
                await DisplayAlert("Valinta muistetaan"+ " " + emp.FirstName + " " + emp.LastName, "MUISTA PÄIVITTÄÄ SIVU!", "OK");
                Hakukentta.Text = Preferences.Get("User", emp.LastName);
                Preferences.Set("User", Hakukentta.Text);
                saveButton.IsVisible = false;
                clearButton.IsVisible = true;
                Hakukentta.SetBinding(SearchBar.SearchCommandParameterProperty, binding: new Binding(source: Hakukentta, path: "Text"));
            }
        }

        async void clearButton_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Valinta unohdetaan", "Muista päivittää sivu! ", "OK");
            Preferences.Clear();
            clearButton.IsVisible = false;
            saveButton.IsVisible = true;
        }
    }
}
