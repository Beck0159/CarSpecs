using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
using System.Runtime.Serialization;
using Windows.Storage;



namespace Car_Specs
{
    class LocalStorage
    {

        public class CarID
        {
            public String ID;
        }

        public async static void WriteXML(string id)
        {
   
             List<CarID> carID;
             string fileName = "ID.XML";

             carID = new List<CarID>();

            CarID overview = new CarID();
            overview.ID = id.ToString();

            carID.Add(overview);
           

            try
            {
                var serializer = new DataContractSerializer(typeof(List<CarID>));

                using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync(fileName, CreationCollisionOption.ReplaceExisting))
                {

                    serializer.WriteObject(stream, carID);
                }

         

            }
            catch (Exception ex)
            {
                // For debugging
                Debug.WriteLine(ex.ToString());
            }
          
        }       

    }
}
