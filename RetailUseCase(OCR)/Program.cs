using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using RetailUseCase.NewFolder;
using System.Collections.Generic;

namespace CSHttpClientSample
{
    public class Program
    {
        // Replace <Subscription Key> with your valid subscription key.
        const string subscriptionKey = "e05f1a77f0b541caba2f803611b1fa01";

        // You must use the same Azure region in your REST API method as you used to
        // get your subscription keys. For example, if you got your subscription keys
        // from the West US region, replace "westcentralus" in the URL
        // below with "westus".
        //
        // Free trial subscription keys are generated in the "westus" region.
        // If you use a free trial subscription key, you shouldn't need to change
        // this region.
        const string uriBase = "https://retailocr.cognitiveservices.azure.com/vision/v1.0/ocr";
        static void Main()
        {
            var ch="1";
            // Get the path and filename to process from the user.
            Console.WriteLine("Optical Character Recognition:");
            while (ch == "1")
            {
                Console.Write("Enter the path to an image with text you wish to read: ");
                string imageFilePath = Console.ReadLine();

                if (File.Exists(imageFilePath))
                {
                    // Call the REST API method.
                    Console.WriteLine("\nWait a moment for the results to appear.\n");
                    MakeOCRRequest(imageFilePath).Wait();
                }
                else
                {
                    Console.WriteLine("\nInvalid file path");
                }
                Console.WriteLine("\nPress Enter 0 to exit and 1 to continue.......");
                ch = Console.ReadLine();
            }
        }

        /// <summary>
        /// Gets the text visible in the specified image file by using
        /// the Computer Vision REST API.
        /// </summary>
        /// <param name="imageFilePath">The image file with printed text.</param>
        static async Task MakeOCRRequest(string imageFilePath)
        {
            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                // Request parameters. 
                // The language parameter doesn't specify a language, so the 
                // method detects it automatically.
                // The detectOrientation parameter is set to true, so the method detects and
                // and corrects text orientation before detecting text.
                string requestParameters = "language=unk&detectOrientation=true";

                // Assemble the URI for the REST API method.
                string uri = uriBase + "?" + requestParameters;

                HttpResponseMessage response;

                // Read the contents of the specified local image
                // into a byte array.
                byte[] byteData = GetImageAsByteArray(imageFilePath);

                // Add the byte array as an octet stream to the request body.
                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses the "application/octet-stream" content type.
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");

                    // Asynchronously call the REST API method.
                    response = await client.PostAsync(uri, content);
                }

                // Asynchronously get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();
                //var items = JsonConvert.DeserializeObject<Retailmodel>(contentString);
                List<string> listDistinctWords = new List<string>();
                listDistinctWords = ExtractPrintedWords(contentString);
                // Display the JSON response.
                //Console.WriteLine("\nResponse:\n\n{0}\n",
                //  JToken.Parse(contentString).ToString());
        
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
        }
       
        /// <summary>
        /// Returns the contents of the specified file as a byte array.
        /// </summary>
        /// <param name="imageFilePath">The image file to read.</param>
        /// <returns>The byte array of the image data.</returns>
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            // Open a read-only file stream for the specified file.
            using (FileStream fileStream =
                new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                // Read the file's contents into a byte array.
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }
        static List<string> ExtractPrintedWords(string jsonResponse)
        {
            List<string> listDistinctWords = new List<string>();

            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonResponse);

            foreach (var region in jsonObj.regions)
            {
                foreach (var line in region.lines)
                {
                    foreach (var word in line.words)
                    {

                        listDistinctWords.Add(Convert.ToString(word.text));
                    }
                }
            }
            string a = "";
            if (listDistinctWords.Contains("Yogurt"))
            {
                string productname1 = listDistinctWords[0] + " " + listDistinctWords[1] + " " + listDistinctWords[2] + " " + listDistinctWords[3];
                string productname2 = listDistinctWords[10] + " " + listDistinctWords[11] + " " + listDistinctWords[12] + " " + listDistinctWords[13];
                string RetailPrice1 = listDistinctWords[9];
                string RetailPrice2 = listDistinctWords[19];
                Console.WriteLine("Product Name:" + " " + productname1);
                Console.WriteLine("Retail Price:" + " " + RetailPrice1);
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Product Name:" + " " + productname2);
                Console.WriteLine("Retail Price:" + " " + RetailPrice2);
            }
            else if (listDistinctWords.Contains("Beefsteak") && listDistinctWords.Contains("Tomatoes,"))
            {
                string productname1 = listDistinctWords[0] + " " + listDistinctWords[1] + " " + listDistinctWords[2] ;
                productname1 = productname1.Replace(",", "");
                string price = listDistinctWords[7]  ;
                string RetailPrice1 = listDistinctWords[6] + price;
                Console.WriteLine("Product Name:" + " " + productname1);
                Console.WriteLine("Retail Price:" + " " + RetailPrice1);
            }

            else if (listDistinctWords.Contains("Red") && listDistinctWords.Contains("Potatoes,"))
            {
                string productname1 = listDistinctWords[0] + " " + listDistinctWords[1] ;
                
                string RetailPrice1 = listDistinctWords[6] ;
                productname1 = productname1.Replace(",", "");
                Console.WriteLine("Product Name:" + " " + productname1);
                Console.WriteLine("Retail Price:" + " " + RetailPrice1);
            }

            //else if (listDistinctWords.Contains("Beefsteak") && listDistinctWords.Contains("Tomatoes,"))
            //{
            //    string productname1 = listDistinctWords[0] + " " + listDistinctWords[1] + " " + listDistinctWords[2];
            //    string price = listDistinctWords[7] == "LB" ? (listDistinctWords[7] + "(LB=Pound)") : listDistinctWords[7];
            //    string RetailPrice1 = listDistinctWords[6] + price;
            //    Console.WriteLine("Product Name:" + " " + productname1);
            //    Console.WriteLine("Retail Price:" + " " + RetailPrice1);
            //}

            //else if (listDistinctWords.Contains("Beefsteak") && listDistinctWords.Contains("Tomatoes,"))
            //{
            //    string productname1 = listDistinctWords[0] + " " + listDistinctWords[1] + " " + listDistinctWords[2];
            //    string price = listDistinctWords[7] == "LB" ? (listDistinctWords[7] + "(LB=Pound)") : listDistinctWords[7];
            //    string RetailPrice1 = listDistinctWords[6] + price;
            //    Console.WriteLine("Product Name:" + " " + productname1);
            //    Console.WriteLine("Retail Price:" + " " + RetailPrice1);
            //}

            return listDistinctWords;
         
        }
    }
}
