using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WMSApp.Models.ResponseMonitor;

namespace WMSApp.Class
{
    public class HttpClientWapi : IDisposable
    {
        /// <summary>
        /// Inner declaration
        /// </summary>
        CancellationTokenSource cancelSource { get; set; } = null;
        CancellationToken cancelToken { get; set; } = default;
        Stopwatch stopWatch { get; set; } = default;        
        HttpStatusCode httpStatusCode { get; set; } = default;
        long size { get; set; } = default;

        /// <summary>
        /// Public declararion
        /// </summary>
        public string lastErrorDesc { get; set; } = string.Empty;
        public bool isSuccessStatusCode { get; set; } = default;

        /// <summary>
        /// The constructor
        /// </summary>
        public HttpClientWapi() { }

        /// <summary>
        /// Dispose code
        /// </summary>
        public void Dispose() { }

        /// <summary>
        /// Handler normal opr as sales, clock in and other
        /// </summary>
        /// <param name="cioRequest"></param>
        /// <returns></returns>
        public async Task<string> RequestSvrAsync(Cio cioRequest)
        {
            string repliedContent = string.Empty;
            try
            {
                ResetProperties();
                cancelSource = new CancellationTokenSource();
                cancelToken = cancelSource.Token;
                var shareUltilities = App.su;
                var httpClient = App.client; // reference to the single http client

                // added 20200311T1124
                // add in the bearer token for request resource
                httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", App.bearerToken.access_token);

                string json = JsonConvert.SerializeObject(cioRequest);
                StringContent stringContent = new StringContent(json, Encoding.UTF8, shareUltilities.APP_JSON);
                Uri uri = new Uri(shareUltilities.WEBAPI_HOST + shareUltilities.QUERY_VERIFIED_SAPUSER);

                cancelToken.ThrowIfCancellationRequested(); // <-- to detect any cancellation by the user                
                HttpResponseMessage response = await httpClient.PostAsync(uri, stringContent, cancelToken);
                isSuccessStatusCode = response.IsSuccessStatusCode;

                cancelToken.ThrowIfCancellationRequested();
                repliedContent = await response.Content.ReadAsStringAsync();
                RecordResponse(response, shareUltilities.QUERY_VERIFIED_SAPUSER, cioRequest.request);

                return repliedContent;
            }
            catch (Exception excep)
            {
                lastErrorDesc = excep.Message;
                return repliedContent;
            }
        }

        /// <summary>
        /// Handler dynamic query path of the server
        /// like super admin add, update company, users and group
        /// </summary>
        /// <param name="cioRequest"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<string> RequestSvrAsync_mgt(Cio cioRequest, string query)
        {
            
            string repliedContent = string.Empty;
            try
            {
                ResetProperties();
                cancelSource = new CancellationTokenSource();
                cancelToken = cancelSource.Token;
                var shareUltilities = App.su;
                var httpClient = App.client; // reference to the single http client

                // added 20200311T1124
                // add in the bearer token for request resource
                httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", App.bearerToken.access_token);

                string json = JsonConvert.SerializeObject(cioRequest);
                var stringContent = new StringContent(json, Encoding.UTF8, shareUltilities.APP_JSON);
                Uri uri = new Uri(shareUltilities.WEBAPI_HOST + query);

                cancelToken.ThrowIfCancellationRequested(); // <-- to detect any cancellation by the user                
                var response = await httpClient.PostAsync(uri, stringContent, cancelToken);
                isSuccessStatusCode = response.IsSuccessStatusCode;

                cancelToken.ThrowIfCancellationRequested();
                repliedContent = await response.Content.ReadAsStringAsync();
                RecordResponse(response, query, cioRequest.request);

                return repliedContent;
            }            
            catch (Exception excep)
            { 
                lastErrorDesc = excep.Message;
                return repliedContent;
            }
        }

        public async Task<string> RequestSvrAsync_mgt(dynamic cioRequest, string query)
        {
            string repliedContent = string.Empty;
            try
            {
                ResetProperties();
                cancelSource = new CancellationTokenSource();
                cancelToken = cancelSource.Token;
                var shareUltilities = App.su;
                var httpClient = App.client; // reference to the single http client

                // added 20200311T1124
                // add in the bearer token for request resource
                httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", App.bearerToken.access_token);

                string json = JsonConvert.SerializeObject(cioRequest);
                var stringContent = new StringContent(json, Encoding.UTF8, shareUltilities.APP_JSON);
                Uri uri = new Uri(shareUltilities.WEBAPI_HOST + query);

                cancelToken.ThrowIfCancellationRequested(); // <-- to detect any cancellation by the user                
                var response = await httpClient.PostAsync(uri, stringContent, cancelToken);
                isSuccessStatusCode = response.IsSuccessStatusCode;

                cancelToken.ThrowIfCancellationRequested();
                repliedContent = await response.Content.ReadAsStringAsync();

                RecordResponse(response, query, $"{cioRequest.request}");
                return repliedContent;
            }
            catch (Exception excep)
            {
                lastErrorDesc = excep.Message;
                return repliedContent;
            }
        }

        public async Task<string> RequestSvrAsync_NoAuthn(Cio cioRequest, string query)
        {
            string repliedContent = string.Empty;
            try
            {
                ResetProperties();
                cancelSource = new CancellationTokenSource();
                cancelToken = cancelSource.Token;
                var shareUltilities = App.su;
                var httpClient = App.client; // reference to the single http client

                // added 20200311T1124
                // add in the bearer token for request resource
                //httpClient.DefaultRequestHeaders.Authorization =
                //            new AuthenticationHeaderValue("Bearer", App.bearerToken.access_token);

                string json = JsonConvert.SerializeObject(cioRequest);
                var stringContent = new StringContent(json, Encoding.UTF8, shareUltilities.APP_JSON);
                Uri uri = new Uri(shareUltilities.WEBAPI_HOST + query);

                cancelToken.ThrowIfCancellationRequested(); // <-- to detect any cancellation by the user                
                var response = await httpClient.PostAsync(uri, stringContent, cancelToken);
                isSuccessStatusCode = response.IsSuccessStatusCode;

                cancelToken.ThrowIfCancellationRequested();
                repliedContent = await response.Content.ReadAsStringAsync();
                RecordResponse(response, query, cioRequest.request);
                return repliedContent;
            }
            catch (Exception excep)
            {
                lastErrorDesc = excep.Message;
                return repliedContent;
            }
        }

        public async Task<string> RequestSvrAsync_NoAuthn(dynamic content, string query, string request)
        {
            string repliedContent = string.Empty;
            try
            {
                ResetProperties();

                cancelSource = new CancellationTokenSource();
                cancelToken = cancelSource.Token;
                var shareUltilities = App.su;
                var httpClient = App.client; // reference to the single http client

                var json = JsonConvert.SerializeObject(content);
                var stringContent = new StringContent(json, Encoding.UTF8, shareUltilities.APP_JSON);
                Uri uri = new Uri(shareUltilities.WEBAPI_HOST + query);

                cancelToken.ThrowIfCancellationRequested(); // <-- to detect any cancellation by the user                
                var response = await httpClient.PostAsync(uri, stringContent, cancelToken);
                isSuccessStatusCode = response.IsSuccessStatusCode;

                cancelToken.ThrowIfCancellationRequested();
                repliedContent = await response.Content.ReadAsStringAsync();
                RecordResponse(response, query, request);

                return repliedContent;
            }
            catch (Exception excep)
            {
                lastErrorDesc = excep.Message;
                return repliedContent;
            }
        }

        /// <summary>
        /// Cancel the request
        /// </summary>
        public void CancelRequest()
        {
            if (!cancelSource.IsCancellationRequested)
            {
                cancelSource.Cancel();
            }   
        }

        // 20210122 response for monitoring 
        // 
        // 
        void RecordResponse(HttpResponseMessage response, string query, string request)
        {
            size = GetContentLength(response.Content.Headers.ContentLength);
            httpStatusCode = response.StatusCode;

            HandlerResponseMonitor(request, stopWatch.Elapsed, size, httpStatusCode, query);
        }

        void ResetProperties()
        {
            stopWatch = Stopwatch.StartNew();
            size = -1;
            httpStatusCode = default;
        }

        long GetContentLength(object len) => (len == null) ? 0 : (long)len;

        // update server response monitor
        // 20210121T0941
        async void HandlerResponseMonitor(string request, TimeSpan time, decimal size,
            HttpStatusCode statusCode, string endPoint)
        {
            try
            {
                var newInfor = new ResponseMonitor
                {
                    Request = request,
                    Duration = $"{time}",
                    SizeInByte = size,
                    HttpStatusCode = $"{statusCode}",
                    AppName = App.appName,
                    User = App.waUser,
                    TransDate = DateTime.Now,
                    EndPoint = endPoint,
                };

                // post to scr to save
                var shareUltilities = App.su;
                var httpClient = App.client; // reference to the single http client

                // added 20200311T1124
                // add in the bearer token for request resource
                httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", App.bearerToken.access_token);

                var json = JsonConvert.SerializeObject(newInfor);
                var stringContent = new StringContent(json, Encoding.UTF8, shareUltilities.APP_JSON);
                Uri uri = new Uri(App.su.WEBAPI_HOST + App.su.UPDATE_RESP_MONITOR);

                cancelToken.ThrowIfCancellationRequested(); // <-- to detect any cancellation by the user                
                var response = await httpClient.PostAsync(uri, stringContent, cancelToken);

                isSuccessStatusCode = response.IsSuccessStatusCode;

                cancelToken.ThrowIfCancellationRequested();
                var repliedContent = await response.Content.ReadAsStringAsync();
                
            }
            catch (Exception e)
            {
                lastErrorDesc = $"{e.Message}\n{e.StackTrace}";
            }

        }
    }
}
