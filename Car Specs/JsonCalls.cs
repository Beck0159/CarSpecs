using Car_Specs.Common;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace Car_Specs
{

    class JsonCalls
    {
        // create httpClient
        private HttpClient httpClient;

        public JsonCalls()
        {
            // create an instance of httpClient
            httpClient = new HttpClient();
            // Limit the max buffer size for the response so we don't get overwhelmed
            httpClient.MaxResponseContentBufferSize = 256000;
            httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");

        }

        internal async void viewAllCars(String userInput)
        {
            try
            {
                String responseBodyAsText;
              

                HttpResponseMessage response = await httpClient.GetAsync("https://api.edmunds.com/api/vehicle/v2/makes?year=" + userInput + "&view=basic&fmt=json&api_key=w7te8pq2racmgdpgp5zxa3b3");
                response.EnsureSuccessStatusCode();
                responseBodyAsText = await response.Content.ReadAsStringAsync();
                responseBodyAsText = responseBodyAsText.Replace("<br>", Environment.NewLine); // Insert new lines


                ReturnJObject(responseBodyAsText);
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

        private string ReturnJObject(string responseBodyAsText)
        {
 	       // throw new NotImplementedException();

            return (responseBodyAsText);
        }

       
    }
}
