﻿using Car_Specs.Common;
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

        // create httpClient
        private HttpClient httpClient;

        public Specs()
        {
            this.InitializeComponent();

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
            String id = e.Parameter.ToString();
            Debug.WriteLine(id);

            try
            {
                //
                String responseBodyAsText;

                HttpResponseMessage response = await httpClient.GetAsync("https://api.edmunds.com/api/vehicle/v2/styles/" + id + "?view=full&fmt=json&api_key=w7te8pq2racmgdpgp5zxa3b3");
                response.EnsureSuccessStatusCode();
                responseBodyAsText = await response.Content.ReadAsStringAsync();
                responseBodyAsText = responseBodyAsText.Replace("<br>", Environment.NewLine); // Insert new lines

                JObject results = JObject.Parse(responseBodyAsText);
                Debug.WriteLine(results);


                //Debug.WriteLine("Make:" + results["make"]["name"]);

                // All data we need for the key features
                string make = results["make"]["name"].ToString();
                string model = results["model"]["name"].ToString();
                string engineDisplacement = results["engine"]["size"].ToString();
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
                Debug.WriteLine("Coooool"+make+model+engineCompresser+engineCyliners+mpgCity+mpgHighway+carType);

            }
            catch (HttpRequestException hre)
            {
                //StatusText.Text = hre.ToString();
            }
            catch (Exception ex)
            {
                // For debugging
                //StatusText.Text = ex.ToString();
            }

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

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
            
            var toastNotifier = ToastNotificationManager.CreateToastNotifier();
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            var toastText = toastXml.GetElementsByTagName("text");
            (toastText[0] as XmlElement).InnerText = "Added To Compare Page";
            var toast = new ToastNotification(toastXml);
            toastNotifier.Show(toast);

        }

        private void view_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(View));
        }

       
    }
}
