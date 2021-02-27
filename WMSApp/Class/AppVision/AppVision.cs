using Newtonsoft.Json;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace WMSApp.Class.AppVision
{
    public class AppVision
    {
        string subscriptionKey { get; set; } = "814ed98d79de45c48b9c63e9e6631fc5";
        string endpoint { get; set; } = "https://appvision.cognitiveservices.azure.com/";
        string uriBase { get; set; } = string.Empty;//endpoint + "vision/v2.1/ocr";
        string CallerAddress { get; set; } = string.Empty;

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="returnAddress"></param>
        public AppVision(string returnAddress)
        {
            uriBase = endpoint + "vision/v2.1/ocr";
            //uriBase = endpoint + "vision/v3.1/read/analyze";
            CallerAddress = returnAddress;
        }

        /// <summary>
        /// OCR execution
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteOCR()
        {
            // 1. Add camera logic.
            if (CrossMedia.Current == null) throw new Exception("Error, Crosss media object");

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                throw new Exception("Error, no camera available.");

            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                Directory = "TextOcr", //<---- save the photo into the android album
                Name = $"text{DateTime.Now:ddMMyyyymmss}.jpg",
                SaveToAlbum = true,
                CompressionQuality = 75,
                CustomPhotoSize = 50,
                PhotoSize = PhotoSize.MaxWidthHeight,
                MaxWidthHeight = 2000,
                DefaultCamera = CameraDevice.Rear
            });

            if (file == null) throw new Exception("File capture is empty");

            // Get the path and filename to process from the user.            
            string imageFilePath = file.Path;

            if (!File.Exists(imageFilePath)) throw new Exception("File no exist");

            // Call the REST API method.
            Console.WriteLine("\nWait a moment for the results to appear.\n");
            await MakeOCRRequest(imageFilePath);
        }

        /// <summary>
        /// Gets the text visible in the specified image file by using
        /// the Computer Vision REST API.
        /// </summary>
        /// <param name="imageFilePath">The image file with printed text.</param>
        async Task MakeOCRRequest(string imageFilePath)
        {
            var client = App.client;

            // Request headers.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            // Request parameters. 
            // The language parameter doesn't specify a language, so the 
            // method detects it automatically.
            // The detectOrientation parameter is set to true, so the method detects and
            // and corrects text orientation before detecting text.
            string requestParameters = "language=en&detectOrientation=true";
            //string requestParameters = "detectOrientation=true";

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
            if (string.IsNullOrWhiteSpace(contentString)) RepliedMessage("empty");

            var text = ReadText(contentString);
            if (string.IsNullOrWhiteSpace(text)) RepliedMessage("Empty");            

            RepliedMessage(text);
        }

        

        /// <summary>
        /// Returns the contents of the specified file as a byte array.
        /// </summary>
        /// <param name="imageFilePath">The image file to read.</param>
        /// <returns>The byte array of the image data.</returns>
        byte[] GetImageAsByteArray(string imageFilePath)
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


        /// <summary>
        ///  check the text capture and return the text
        /// </summary>
        /// <param name="textCapture"></param>
        /// <returns></returns>
        string ReadText(string textCapture)
        {
            var root = JsonConvert.DeserializeObject<OcrRoot>(textCapture);
            if (root == null) throw new Exception("The reading is null");
            if (root.regions == null) throw new Exception("Region is null");

            string reading = string.Empty;
            root.regions.ForEach(region =>
            {
                region.lines.ForEach(line =>
                {
                    line.words.ForEach(word =>
                    {
                        if (!string.IsNullOrWhiteSpace(word.text))
                        {
                            reading += $"{word.text} ";
                        }
                    });
                });
            });
            return reading;
        }

        void RepliedMessage(string readText) => MessagingCenter.Send(readText, CallerAddress);

    }
}
