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

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Car_Specs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class View : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        // create httpClient
        private HttpClient httpClient;
       

        public View()
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
            // loadstate if exists
            if (e.PageState != null && e.PageState.ContainsKey("Year Input"))
            {
                yearInput.Text = e.PageState["Year Input"].ToString();
            }
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
          // save year text
            e.PageState["Year Input"] = yearInput.Text;

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
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);

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

        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(About));
        }

        private async void searchMake_Click(object sender, RoutedEventArgs e)
        {
            // Clear Listviews
            listView1.Items.Clear();
            String userInput = yearInput.Text;

            try
            {
                // string responce
                String responseBodyAsText;
                // make the call
                HttpResponseMessage response = await httpClient.GetAsync("https://api.edmunds.com/api/vehicle/v2/makes?year=" + userInput + "&view=basic&fmt=json&api_key=w7te8pq2racmgdpgp5zxa3b3");
                response.EnsureSuccessStatusCode();
                responseBodyAsText = await response.Content.ReadAsStringAsync();
                responseBodyAsText = responseBodyAsText.Replace("<br>", Environment.NewLine); // Insert new lines
                // Parse responce into JObject
                JObject results = JObject.Parse(responseBodyAsText);

                // Parse JObject
                int i = 0;
                foreach (var result in results["makes"])
                {
                    // Create and Display makes
                    string makes = (string)result["name"];
                    // dynamicly make a textblock
                    TextBlock newTB1 = new TextBlock();
                    // Add properties
                    newTB1.Name = makes;
                    newTB1.Text = makes;
                    newTB1.FontSize = 20;
                    // add the newly created TextBlock
                    listView1.Items.Add(newTB1);

                    int a = 0;
                    foreach (var models in result["models"])
                    {
                        // Create and Display Models for each make
                        var type1 = result["models"][a];
                        string modelStrings = (string)type1["name"];
                        var years = type1["years"][0];

                        string modelID = (string)years["id"];
                        // dynamicly make a textblock
                        TextBlock newTB = new TextBlock();
                        // Add porperties
                        newTB.Name = userInput;
                        newTB.Text = "  " + modelStrings;
                        newTB.Tapped += Onb2Click;
                        newTB.Tag = makes;
                        // add the newly created TextBlock
                        listView1.Items.Add(newTB);

                        a++;
                    }

                    i++;
                }

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

        private void Onb2Click(object sender, TappedRoutedEventArgs e)
        {
            // get specific TextBlock 
            TextBlock TB = sender as TextBlock;
            // Create string names to hold tag text and name to be parsed on models page
            String names = ""+TB.Tag+","+TB.Text+","+TB.Name;


            // Send Tag to models page to use for query
            Frame.Navigate(typeof(Model), names);

        }

        private async void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            // string url
            string urlpayment = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=WZF87T9F6GY5L";
            // create a uri
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
        // make textboxes empty when clicked
        private void yearInput_GotFocus(object sender, RoutedEventArgs e)
        {
            yearInput.Text = string.Empty;
        }

        
    }
}
