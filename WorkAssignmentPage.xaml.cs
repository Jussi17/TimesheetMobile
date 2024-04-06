using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TimesheetMobileApp.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

namespace TimesheetMobileApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WorkAssignmentPage : ContentPage
    {
        int eId;
        string lat;
        string lon;

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

        public WorkAssignmentPage(int id)
        {
            InitializeComponent();
            eId = id;

            lon_label.Text = "Sijaintia haetaan...";
            wa_lataus.Text = "Ladataan työtehtäviä...";

            GetCurrentLocation();


            async void GetCurrentLocation()
            {
                try
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

                    var location = await Geolocation.GetLocationAsync(request);

                    if (location != null)
                    {
                        lon_label.Text = "Longitude: " + Math.Round(location.Longitude,2);
                        lat_label.Text = "Latitude: " + Math.Round(location.Latitude,2);

                        lat = Math.Round(location.Latitude,2).ToString();
                        lon = Math.Round(location.Longitude,2).ToString();
                    }
                }
                catch (FeatureNotSupportedException fnsEx)
                {
                    await DisplayAlert("Virhe", fnsEx.ToString(), "ok");
                }
                catch (FeatureNotEnabledException fneEx)
                {
                    await DisplayAlert("Virhe", fneEx.ToString(), "ok");
                }
                catch (PermissionException pEx)
                {
                    await DisplayAlert("Virhe", pEx.ToString(), "ok");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Virhe", ex.ToString(), "ok");
                }
        }



            LoadDataFromRestAPI();

            async void LoadDataFromRestAPI()
            {
                try
                {
#if DEBUG
                    HttpClientHandler InsecureHandler = GetInsecureHandler();
                    HttpClient client = new HttpClient(InsecureHandler);
#else
                    HttpClient client = new HttpClient();
#endif
                    client.BaseAddress = new Uri("https://10.0.2.2:5001/");
                    string json = await client.GetStringAsync("api/workassignments");

                    IEnumerable<Workassignments> wa = JsonConvert.DeserializeObject<Workassignments[]>(json);
                    ObservableCollection<Workassignments> dataa = new ObservableCollection<Workassignments>(wa);
                    //dataa = dataa;
                    waList.ItemsSource = dataa;

                    wa_lataus.Text = "";

                }
                catch (Exception e)
                {
                    await DisplayAlert("Virhe", e.Message.ToString(), "SELVÄ!");
                }
            }
        }

        async void startbutton_Clicked(object sender, EventArgs e)
        {
            Workassignments wa = (Workassignments)waList.SelectedItem;

            if (wa == null)
            {
                await DisplayAlert("Valinta puuttuu", "Valitse työtehtävä", "OK");
                return;
            }

            try
            {
                Operation op = new Operation
                {
                    EmployeeID = eId,
                    WorkAssignmentID = wa.IdWorkAssignment,
                    CustomerID = wa.IdCustomer,
                    OperationType = "start",
                    Comment = "Aloitettu",
                    Latitude = lat,
                    Longitude = lon
                };


#if DEBUG
                HttpClientHandler InsecureHandler = GetInsecureHandler();
                HttpClient client = new HttpClient(InsecureHandler);
#else
                HttpClient client = new HttpClient();
#endif
                client.BaseAddress = new Uri("https://10.0.2.2:5001/");

                string input = JsonConvert.SerializeObject(op);
                StringContent content = new StringContent(input, Encoding.UTF8, "application/json");

                HttpResponseMessage message = await client.PostAsync("/api/workassignments", content);

                string reply = await message.Content.ReadAsStringAsync();

                bool success = JsonConvert.DeserializeObject<bool>(reply);

                if (success == false)
                {
                    await DisplayAlert("Ei voida aloittaa", "Työ on jo käynnissä", "OK");
                    return;
                }
                else if (success)
                {
                    await DisplayAlert("Työ aloitettu", "Työ on aloitettu", "OK");
                }
            }

            catch (Exception ex)
            {
                await DisplayAlert(ex.GetType().Name, ex.Message, "OK");
            }
        }

        async void stopbutton_Clicked(object sender, EventArgs e)
        {
            Workassignments wa = (Workassignments)waList.SelectedItem;

            if (wa == null)
            {
                await DisplayAlert("Valinta puuttuu", "Valitse työtehtävä", "Ok");
                return;
            }

            string result = await DisplayPromptAsync("Kommentti","Kirjoita kommentti");

            try
            {
                Operation op = new Operation
                {
                    EmployeeID = eId,
                    WorkAssignmentID = wa.IdWorkAssignment,
                    CustomerID = wa.IdCustomer,
                    OperationType = "stop",
                    Comment = result,
                    Latitude = lat,
                    Longitude = lon
                };
#if DEBUG
                HttpClientHandler InsecureHandler = GetInsecureHandler();
                HttpClient client = new HttpClient(InsecureHandler);
#else
                HttpClient client = new HttpClient();
#endif
                client.BaseAddress = new Uri("https://10.0.2.2:5001/");

                string input = JsonConvert.SerializeObject(op);
                StringContent content = new StringContent(input, Encoding.UTF8, "application/json");

                HttpResponseMessage message = await client.PostAsync("/api/workassignments", content);

                string reply = await message.Content.ReadAsStringAsync();

                bool success = JsonConvert.DeserializeObject<bool>(reply);

                if (success == false)
                {
                    await DisplayAlert("Ei voida lopettaa", "Työtä ei ole aloitettu", "OK");
                    return;
                }
                else if (success)
                {
                    await DisplayAlert("Työn päättyminen", "Työ on päättynyt", "OK");
                    await Navigation.PushAsync(new WorkAssignmentPage(eId));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert(ex.GetType().Name, ex.Message,"OK");
            }
        }

        private void btnCam_Clicked(object sender, EventArgs e)
        {

        }
    }
}