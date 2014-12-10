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
    /// 
    public sealed partial class Compare : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        // create httpClient
        private HttpClient httpClient;

        // Create car ID string for both cars primary key
        public string CarID1;
        public string CarID2;

        public Compare()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            // create an instance of httpClient
            httpClient = new HttpClient();
            // Limit the max buffer size for the response so we don't get overwhelmed
            httpClient.MaxResponseContentBufferSize = 256000;
            httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");

            // change the layout of the page so nothing is visable
            ContentRoot2.Opacity = 0;
            ContentRoot.Opacity = 0;
            HelpMessage.Opacity = 0;
            HelpMessage2.Opacity = 0;

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
            // Call ReadXML method when navigated to
             await ReadXML();
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

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(About));
        }

        private void view_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(View));
        }

        public async Task ReadXML()
        {
            // Create lists to store Car ID's
            List<Car_Specs.Specs.CarID> carID;
            // File Name always the same
            string fileName = "ID.XML";
            
            // Set the ID's to null
            string ID1 = null;
            string ID2 = null;

        
            // Create deserializer
            var deserializer = new DataContractSerializer(typeof(List<Car_Specs.Specs.CarID>));

            // Try Catch when reading from local storage
            try
            {
              
                // create stream
                var stream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(fileName);

                using (StreamReader reader = new StreamReader(stream))
                {

                    //content = await reader.ReadToEndAsync();
                    carID = ((List<Car_Specs.Specs.CarID>)deserializer.ReadObject(stream));

                }
               
                // Set the ID's for each car
                ID1 = carID[0].ID;
                ID2 = carID[1].ID;
             
            }
            catch (Exception ex)
            {
                // For debugging
                Debug.WriteLine(ex.ToString());
            }




            // if the 2 cars arnt set show the help message
            if (ID1 != null && ID2 != null)
            {
                // both are set properly so show the grid
                ContentRoot2.Opacity = 1;
                ContentRoot.Opacity = 0;
                // set public CarID's
                CarID1 = ID1;
                CarID2 = ID2;
                // run Display Data and send it id's
                await DisplayData(ID1, ID2);

            }
            else
            {
                if (ID1 == null && ID2 == null)
                {
                    // help message 1
                    ContentRoot2.Opacity = 0;
                    ContentRoot.Opacity = 1;
                    HelpMessage.Opacity = 1;
                    HelpMessage2.Opacity = 0;
                }

                if (ID1 != null && ID2 == null)
                {
                    // help message 2
                    ContentRoot2.Opacity = 0;
                    ContentRoot.Opacity = 1;
                    HelpMessage.Opacity = 0;
                    HelpMessage2.Opacity = 1;
                }

            }
    

        }

        private async Task DisplayData(string Car1, string Car2)
        {
            // try catch for making a call for json data         
            try
            {
                // string to hold responce data
                String responseBodyAsText;
                // Make the call with specific id
                HttpResponseMessage response = await httpClient.GetAsync("https://api.edmunds.com/api/vehicle/v2/styles/" + Car1 + "?view=full&fmt=json&api_key=w7te8pq2racmgdpgp5zxa3b3");
                response.EnsureSuccessStatusCode();
                responseBodyAsText = await response.Content.ReadAsStringAsync();
                responseBodyAsText = responseBodyAsText.Replace("<br>", Environment.NewLine); // Insert new lines
                // Parse the recieved data into a JObject
                JObject results = JObject.Parse(responseBodyAsText);             

                // Parse JObject 

                // All data we need for the key features
                string make = results["make"]["name"].ToString();
                string model = results["model"]["name"].ToString();
                string year = results["year"]["year"].ToString();
                string engineDisplacement = results["engine"]["size"].ToString();
                string engineCompresser = results["engine"]["compressorType"].ToString();
                string mpgHighway = results["MPG"]["highway"].ToString();
                string mpgCity = results["MPG"]["city"].ToString();
                string carType = results["categories"]["vehicleStyle"].ToString();
                string transmissionType = results["transmission"]["transmissionType"].ToString();
                string transmissionSpeeds = results["transmission"]["numberOfSpeeds"].ToString();
                string driveWheels = results["drivenWheels"].ToString();             
                string compressionRatio = results["engine"]["compressionRatio"].ToString();
                string fuelType = results["engine"]["fuelType"].ToString();
                string horsepower = results["engine"]["horsepower"].ToString();
                string torque = results["engine"]["torque"].ToString();
                string valves = results["engine"]["totalValves"].ToString();
                string configuration = results["engine"]["configuration"].ToString();
                string engineCyliners = results["engine"]["cylinder"].ToString();

                // display more data
                MakeTitle1.Text = make;
                CarTitle1.Text = model;
                Year1.Text = year;
                CompressionRatio1.Text =  compressionRatio;
                MPG1.Text = mpgCity + " / " + mpgHighway;
                FuelType1.Text = fuelType;
                Horsepower1.Text =  horsepower;
                Torque1.Text = torque;
                Valves1.Text = valves;
                CompressorType1.Text = engineCompresser;
                Drivewheels1.Text = driveWheels;
                Configuration1.Text = configuration;
                Cylinders1.Text = engineCyliners;
                CarType1.Text = carType;
                TransmissionType1.Text = transmissionType;
                Speeds1.Text = transmissionSpeeds;
                // Get Picture method, send id
                displayPicture(Car1);


            }
            catch (HttpRequestException hre)
            {
                Debug.WriteLine(hre);
            }
            catch (Exception ex)
            {

                Debug.WriteLine(ex);
            }

            try
            {
                // string to hold responce ata
                String responseBodyAsText;
                // Make the call
                HttpResponseMessage response = await httpClient.GetAsync("https://api.edmunds.com/api/vehicle/v2/styles/" + Car2 + "?view=full&fmt=json&api_key=w7te8pq2racmgdpgp5zxa3b3");
                response.EnsureSuccessStatusCode();
                responseBodyAsText = await response.Content.ReadAsStringAsync();
                responseBodyAsText = responseBodyAsText.Replace("<br>", Environment.NewLine); // Insert new lines
                // Parse response into JObject
                JObject results = JObject.Parse(responseBodyAsText);
                Debug.WriteLine(results);            

                // All data needed
                string make = results["make"]["name"].ToString();
                string model = results["model"]["name"].ToString();
                string year = results["year"]["year"].ToString();
                string engineDisplacement = results["engine"]["size"].ToString();
                string engineCompresser = results["engine"]["compressorType"].ToString();
                string mpgHighway = results["MPG"]["highway"].ToString();
                string mpgCity = results["MPG"]["city"].ToString();
                string carType = results["categories"]["vehicleStyle"].ToString();
                string transmissionType = results["transmission"]["transmissionType"].ToString();
                string transmissionSpeeds = results["transmission"]["numberOfSpeeds"].ToString();
                string driveWheels = results["drivenWheels"].ToString();               
                string compressionRatio = results["engine"]["compressionRatio"].ToString();
                string fuelType = results["engine"]["fuelType"].ToString();
                string horsepower = results["engine"]["horsepower"].ToString();
                string torque = results["engine"]["torque"].ToString();
                string valves = results["engine"]["totalValves"].ToString();
                string configuration = results["engine"]["configuration"].ToString();
                string engineCyliners = results["engine"]["cylinder"].ToString();

                // display data
                MakeTitle2.Text = make;
                CarTitle2.Text = model;
                Year2.Text = year;
                CompressionRatio2.Text = compressionRatio;
                MPG2.Text = mpgCity + " / " + mpgHighway;
                FuelType2.Text = fuelType;
                Horsepower2.Text = horsepower;
                Torque2.Text = torque;
                Valves2.Text = valves;
                CompressorType2.Text = engineCompresser;
                Drivewheels2.Text = driveWheels;
                Configuration2.Text = configuration;
                Cylinders2.Text = engineCyliners;
                CarType2.Text = carType;
                TransmissionType2.Text = transmissionType;
                Speeds2.Text = transmissionSpeeds;
                // Display picture, send id
                displayPicture(Car2);


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
            // Try catch calling for data
            try
            {
                // string to hold data
                String responseBodyAsTextImage;
                // make the call
                HttpResponseMessage response1 = await httpClient.GetAsync("https://api.edmunds.com/v1/api/vehiclephoto/service/findphotosbystyleid?styleId=" + id + "&fmt=json&api_key=w7te8pq2racmgdpgp5zxa3b3");
                response1.EnsureSuccessStatusCode();
                responseBodyAsTextImage = await response1.Content.ReadAsStringAsync();
                responseBodyAsTextImage = responseBodyAsTextImage.Replace("<br>", Environment.NewLine); // Insert new lines
                // parse responce into JArray
                JArray imageResults = JArray.Parse(responseBodyAsTextImage);             
                // String to hold image sourse
                string imageSource = imageResults[1]["photoSrcs"][1].ToString();          
                // create URI object 
                Uri uri = new Uri("http://media.ed.edmunds-media.com" + imageSource + "", UriKind.Absolute);

                // if the ID's match the cars set the photos
                if (id == CarID1)
                {
                    //diplay the photo
                    CarPic1.ImageSource = new BitmapImage(
                        new Uri("http://media.ed.edmunds-media.com" + imageSource + "", UriKind.Absolute)
                    );

                }

                if (id == CarID2)
                {
                    // display the photo
                    CarPic2.ImageSource = new BitmapImage(
                        new Uri("http://media.ed.edmunds-media.com" + imageSource + "", UriKind.Absolute)
                    );

                }

            }
            catch (HttpRequestException hre)
            {
                Debug.WriteLine(hre);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

        }

        private async void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            // Donations Button, Launch browser

            // String holding the url
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
