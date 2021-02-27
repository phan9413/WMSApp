using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WMSApp.Class.Helper
{
    public class PictureUploadHelper
    {
        public string LastErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// For APS net core file upload
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="headerGuid"></param>
        /// <returns></returns>
        async public Task<bool> ManageFileUpload(string filePath, string headerGuid)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo == null)
                {
                    LastErrorMessage = $"invalid file path\n{filePath}";
                    return false;
                }

                var client = App.client;
                client.DefaultRequestHeaders.Clear();
                //client.DefaultRequestHeaders.Add("token", App.waToken);
                client.DefaultRequestHeaders.Add("user", App.waUser);
                client.DefaultRequestHeaders.Add("guid", headerGuid);
                //client.DefaultRequestHeaders.Add("saveFileNamePrefix", App.waUser);

                // 20200311T1231 
                // add in access token for authentication
                client.DefaultRequestHeaders.Authorization =
                         new AuthenticationHeaderValue("Bearer", App.bearerToken.access_token);

                var filesStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var streamReader = new StreamReader(filesStream);

                var fileStreamContent = new StreamContent(streamReader.BaseStream);
                fileStreamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") 
                { 
                    Name = "file",  //<---- the controller pass in parameter
                    FileName = $"{headerGuid}{fileInfo.Extension}"
                };

                fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");                
                var webapiAddress = App.su.WEBAPI_HOST + "FilesUpload"; //App.su.QUERY_FILEUPLOAD;

                using (var formData = new MultipartFormDataContent())
                {
                    formData.Add(fileStreamContent);
                    var response = await client.PostAsync(webapiAddress, formData);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        
    }
}
