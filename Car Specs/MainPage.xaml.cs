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


// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Car_Specs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        // create httpClient
        private HttpClient httpClient;

        public MainPage()
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
            if (e.PageState != null && e.PageState.ContainsKey("Year Input"))
            {
                // if the pagestate exists, load 
                year.Text = e.PageState["Year"].ToString();
                make.Text = e.PageState["Make"].ToString();
                model.Text = e.PageState["Model"].ToString();
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
            // save state of text boxes
            e.PageState["Year"] = year.Text;
            e.PageState["Make"] = make.Text;
            e.PageState["Model"] = model.Text;
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

       // basic navigation
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(About));
        }

        private void compareBTN_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Compare));
        }

        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(Model));
        }

        private void view_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(View));
        }

        private async void searchMake_Click(object sender, RoutedEventArgs e)
        {
            // strings holdng text from the 3 textboxes
            String makeString = make.Text;
            String yearString = year.Text;
            String modelString = model.Text;

            // Clear listview and title
            listView1.Items.Clear();
            listViewTitle.Items.Clear();

            // dynamicly make a textblock
            TextBlock newTitle = new TextBlock();
            //newTB.Name = userInput;
            newTitle.Text =  makeString;
            newTitle.FontSize = 20;
            // add the newly created TextBlock
            listViewTitle.Items.Add(newTitle);
            
            // try catch making a call for data
            try
            {
                // string to hold data
                String responseBodyAsText;
                // make call
                HttpResponseMessage response = await httpClient.GetAsync("https://api.edmunds.com/api/vehicle/v2/" + makeString + "/" + modelString + "?year=" + yearString + "&view=basic&fmt=json&api_key=w7te8pq2racmgdpgp5zxa3b3");
                response.EnsureSuccessStatusCode();
                responseBodyAsText = await response.Content.ReadAsStringAsync();
                responseBodyAsText = responseBodyAsText.Replace("<br>", Environment.NewLine); // Insert new lines
                // parse respone into JObject
                JObject results = JObject.Parse(responseBodyAsText);
                Debug.WriteLine(results);

                // Parse through the JObject
                int i = 0;
                foreach (var result in results["years"])
                {
                    string modelID = (string)result["id"];

                    int a = 0;
                    foreach (var styles in result["styles"])
                    {
                        var type1 = result["styles"][a];
                        string modelStrings = (string)type1["name"];
                        string carID = (string)type1["id"];                     
                        // dynamicly make a textblock
                        TextBlock newTB = new TextBlock();
                        // Add properties to new textblock
                        newTB.Text = "  " + modelStrings;
                        newTB.Tapped += Onb2Click;
                        newTB.Tag = carID;
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
            // Send Tag to models page to use for query
            Frame.Navigate(typeof(Specs), TB.Tag);
        }

        private async void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            // Donate Button

            // url
            string urlpayment = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=WZF87T9F6GY5L";
            // cretae uri
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
        // make textboxes empty when clicked on
        private void make_GotFocus(object sender, RoutedEventArgs e)
        {
            make.Text = string.Empty;
        }

        private void year_GotFocus(object sender, RoutedEventArgs e)
        {
            year.Text = string.Empty;
        }

        private void model_GotFocus(object sender, RoutedEventArgs e)
        {
            model.Text = string.Empty;
        }


    }
}
