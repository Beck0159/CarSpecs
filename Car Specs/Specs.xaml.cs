using Car_Specs.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.Serialization;
using Windows.Storage;
using System.Threading.Tasks;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Car_Specs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Specs : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        // string list
        private List<string> carID;   
        // string to hold id
        public string id;
        // create httpClient
        private HttpClient httpClient;

        public Specs()
        {
            this.InitializeComponent();

            // initialize data members
            carID = new List<string>();

            // create an instance of httpClient
            httpClient = new HttpClient();
            // Limit the max buffer size for the response so we don't get overwhelmed
            httpClient.MaxResponseContentBufferSize = 256000;
            httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            // get id sent from last page
            id = e.Parameter.ToString();

            try
            {
                // string to hold responce
                String responseBodyAsText;
                // make call
                HttpResponseMessage response = await httpClient.GetAsync("https://api.edmunds.com/api/vehicle/v2/styles/" + id + "?view=full&fmt=json&api_key=w7te8pq2racmgdpgp5zxa3b3");
                response.EnsureSuccessStatusCode();
                responseBodyAsText = await response.Content.ReadAsStringAsync();
                responseBodyAsText = responseBodyAsText.Replace("<br>", Environment.NewLine); // Insert new lines
                // parse responce into JObject
                JObject results = JObject.Parse(responseBodyAsText);

                // All data we need for the key features
                string make = results["make"]["name"].ToString();
                string model = results["model"]["name"].ToString();
                string engineDisplacement = results["engine"]["size"].ToString();
                string engineSize = results["engine"]["displacement"].ToString();
                string engineCompresser = results["engine"]["compressorType"].ToString();
                string mpgHighway = results["MPG"]["highway"].ToString();
                string mpgCity = results["MPG"]["city"].ToString();
                string carType = results["categories"]["vehicleStyle"].ToString();
                string transmissionType = results["transmission"]["transmissionType"].ToString();
                string transmissionSpeeds = results["transmission"]["numberOfSpeeds"].ToString();
                string driveWheels = results["drivenWheels"].ToString();

                // Display key features
                title.Text = make + " " + model;
                CarType.Text = "Car Type: "+carType+", "+driveWheels;
                Transmission.Text = "Transmission: "+transmissionSpeeds + " speed " + transmissionType;
                EngineInfo.Text = "Engine: "+engineDisplacement + "L " + engineCompresser;
                MPG.Text ="MPG City/Highway: "+ mpgCity + "/" + mpgHighway;
                Displacement.Text = "Displacement: " + engineSize;

                // Data needed for engine/transmission block
                string compressionRatio = results["engine"]["compressionRatio"].ToString();
                string fuelType = results["engine"]["fuelType"].ToString();
                string horsepower = results["engine"]["horsepower"].ToString();
                string torque = results["engine"]["torque"].ToString();
                string valves = results["engine"]["totalValves"].ToString();
                string configuration = results["engine"]["configuration"].ToString();
                string engineCyliners = results["engine"]["cylinder"].ToString();

                // display engine/transmission data
                CompressionRatio.Text = "Compression Ratio: " + compressionRatio;
                FuelType.Text = "Fuel: " + fuelType;
                Horsepower.Text = "Horsepower: " + horsepower;
                Torque.Text = "Torque: " + torque;
                Valves.Text = "Total Valves: " + valves;
                Configuration.Text = "Configuration: " + configuration;
                Cylinders.Text = "Cylinders: " + engineCyliners;
                Size.Text = "Size: " + engineDisplacement;
                // send id to displayPicture Method
                displayPicture(id);


            }
            catch (HttpRequestException hre)
            {
                Debug.WriteLine(hre);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

        public async void displayPicture(string id)
        {
           // try catch, recieving data
            try
            {
                // string to hold responce
                String responseBodyAsTextImage;
                // make call
                HttpResponseMessage response1 = await httpClient.GetAsync("https://api.edmunds.com/v1/api/vehiclephoto/service/findphotosbystyleid?styleId=" + id + "&fmt=json&api_key=w7te8pq2racmgdpgp5zxa3b3");
                response1.EnsureSuccessStatusCode();
                responseBodyAsTextImage = await response1.Content.ReadAsStringAsync();
                responseBodyAsTextImage = responseBodyAsTextImage.Replace("<br>", Environment.NewLine); // Insert new lines
                Debug.WriteLine(responseBodyAsTextImage);
                // parse responce into JArray
                JArray imageResults = JArray.Parse(responseBodyAsTextImage);
                // string to hold image source
                string imageSource = imageResults[0]["photoSrcs"][1].ToString();
                // URI of the picture
                Uri uri = new Uri("http://media.ed.edmunds-media.com"+imageSource+"", UriKind.Absolute);
                // diplay image
                imagePlace.ImageSource = new BitmapImage(
                    new Uri("http://media.ed.edmunds-media.com"+imageSource+"", UriKind.Absolute)
                );
                // stop the progress ring
                ProgressRing.IsActive = false;

            }
            catch (HttpRequestException hre)
            {
                Debug.WriteLine(hre);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        // Basic Navigation
        private void searchBTN_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void compareBTN_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Compare));
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(About));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // add car id to list, add list to database

            if (id != null)
            {
                // send ID to writeXML
                WriteXML(id);

                // display toast message
                    var toastNotifier = ToastNotificationManager.CreateToastNotifier();
                    var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
                    var toastText = toastXml.GetElementsByTagName("text");
                    (toastText[0] as XmlElement).InnerText = "Added To Compare Page";
                    var toast = new ToastNotification(toastXml);
                    toastNotifier.Show(toast);
                    Debug.WriteLine(id);
            }

        }

        public class CarID
        {
            // Class CarID which hold only a string ID
            public String ID;
        }

        public async static void WriteXML(string id)
        {
            // Create lists
            List<CarID> carID;
            List<CarID> test;
            string fileName = "ID.XML";
            string newID = null;
            // New Instance of a list
            carID = new List<CarID>();
      

            // Get current List, will fail if nothing is added
            // Add New ID To List
            var deserializer = new DataContractSerializer(typeof(List<CarID>));

            try
            {

                string content = string.Empty;

                var stream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(fileName);

                using (StreamReader reader = new StreamReader(stream))
                {

                    // Save it to the list
                    test = ((List<CarID>)deserializer.ReadObject(stream));
                    
                }             
                // set the new id
                newID = test[0].ID;

            }
            catch (Exception ex)
            {
                // For debugging
                Debug.WriteLine(ex.ToString());
            }

            // Create new lists with old and new id
            // this makes it so only 2 items will ever be stored at once
            CarID overview = new CarID();
            overview.ID = id;

            CarID testing = new CarID();
            testing.ID = newID;

            // add the CarIDs
            carID.Add(overview);
            carID.Add(testing);

            // Save List
            try
           {
                // Create a serializer
                var serializer = new DataContractSerializer(typeof(List<CarID>));

                using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync(fileName, CreationCollisionOption.ReplaceExisting))
                {
                    // Write object to memory
                    serializer.WriteObject(stream, carID);
                }



            }
            catch (Exception ex)
            {
                // For debugging
                Debug.WriteLine(ex.ToString());
            }

        }
        // navigation
        private void view_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(View));
        }

        private async void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            // Donate button

            // donate url
            string urlpayment = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=WZF87T9F6GY5L";
            // create uri
            var uri = new Uri(urlpayment);
            // Set the option to show a warning
            var options = new Windows.System.LauncherOptions();
            options.TreatAsUntrusted = true;

            // Launch the URI with a warning prompt
            var success = await Windows.System.Launcher.LaunchUriAsync(uri, options);

            if (success)
            {
                // URI launched
            }
            else
            {
                // URI launch failed
            }
        }
       
    }
}
