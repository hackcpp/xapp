using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;

namespace test.Controllers
{
    public class HttpController : Controller
    {
        private readonly ILogger<HttpController> _logger;
        private readonly IHttpClientFactory _clientFactory;

        public HttpController(ILogger<HttpController> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> TestHttpClient()
        {
            var results = new List<string>();
            try
            {
                // 创建自定义HttpClientHandler忽略SSL验证
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
                
                // 使用自定义handler创建HttpClient
                var client = new HttpClient(handler);
                
                // GET 请求测试
                var response = await client.GetAsync("https://postman-echo.com/get");
                results.Add($"GET请求状态: {response.StatusCode}");
                var content = await response.Content.ReadAsStringAsync();
                results.Add($"GET响应内容: {content}\n");

                // POST 请求测试
                var postData = new StringContent(
                    "{ \"test\": \"Hello from HttpClient\" }", 
                    Encoding.UTF8, 
                    "application/json");
                response = await client.PostAsync("https://postman-echo.com/post", postData);
                results.Add($"POST请求状态: {response.StatusCode}");
                content = await response.Content.ReadAsStringAsync();
                results.Add($"POST响应内容: {content}\n");

                // Headers 测试
                var request = new HttpRequestMessage(HttpMethod.Get, "https://postman-echo.com/headers");
                request.Headers.Add("X-Custom-Header", "Test Value");
                response = await client.SendAsync(request);
                results.Add($"Headers测试状态: {response.StatusCode}");
                content = await response.Content.ReadAsStringAsync();
                results.Add($"Headers响应内容: {content}");
            }
            catch (Exception ex)
            {
                results.Add($"Error: {ex.Message}");
                _logger.LogError(ex, "Error in HttpClient test");
            }

            ViewBag.Results = results;
            return View("TestResults");
        }

        public async Task<IActionResult> TestWebRequest()
        {
            var results = new List<string>();
            try
            {
                // GET 请求测试
                var request = WebRequest.Create("https://postman-echo.com/get") as HttpWebRequest;
                if (request is null)
                {
                    results.Add("Error: Failed to create HttpWebRequest for GET.");
                    ViewBag.Results = results;
                    return View("TestResults");
                }
                request.Method = "GET";
                
                using (var webResponse = await request.GetResponseAsync() as HttpWebResponse)
                {
                    if (webResponse != null)
                    {
                        results.Add($"GET请求状态: {webResponse.StatusCode}");
                        var responseStream = webResponse.GetResponseStream();
                        if (responseStream != null)
                        {
                            using (var reader = new StreamReader(responseStream))
                            {
                                var content = await reader.ReadToEndAsync();
                                results.Add($"GET响应内容: {content}\n");
                            }
                        }
                    }
                }

                // POST 请求测试
                request = WebRequest.Create("https://postman-echo.com/post") as HttpWebRequest;
                if (request is null)
                {
                    results.Add("Error: Failed to create HttpWebRequest for POST.");
                    ViewBag.Results = results;
                    return View("TestResults");
                }
                request.Method = "POST";
                request.ContentType = "application/json";
                
                var postData = "{ \"test\": \"Hello from HttpWebRequest\" }";
                var bytes = Encoding.UTF8.GetBytes(postData);
                
                using (var stream = await request.GetRequestStreamAsync())
                {
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                }

                using (var response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    if (response != null)
                    {
                        results.Add($"POST请求状态: {response.StatusCode}");
                        var postStream = response.GetResponseStream();
                        if (postStream == null)
                        {
                            results.Add("Error: Response stream is null");
                        }
                        else
                        {
                            using (var reader = new StreamReader(postStream))
                            {
                                var content = await reader.ReadToEndAsync();
                                results.Add($"POST响应内容: {content}\n");
                            }
                        }
                    }
                    else
                    {
                        results.Add("Error: POST response is null.");
                    }
                }

                // Headers 测试
                request = WebRequest.Create("https://postman-echo.com/headers") as HttpWebRequest;
                if (request is null)
                {
                    results.Add("Error: Failed to create HttpWebRequest for Headers test.");
                    ViewBag.Results = results;
                    return View("TestResults");
                }
                request.Method = "GET";
                request.Headers.Add("X-Custom-Header: Test Value");

                using (var response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    if (response != null)
                    {
                        results.Add($"Headers测试状态: {response.StatusCode}");
                        var headersStream = response.GetResponseStream();
                        if (headersStream == null)
                        {
                            results.Add("Error: Response stream is null");
                        }
                        else
                        {
                            using (var reader = new StreamReader(headersStream))
                            {
                                var content = await reader.ReadToEndAsync();
                                results.Add($"Headers响应内容: {content}");
                            }
                        }
                    }
                    else
                    {
                        results.Add("Error: Headers response is null.");
                    }
                }
            }
            catch (Exception ex)
            {
                results.Add($"Error: {ex.Message}");
                _logger.LogError(ex, "Error in HttpWebRequest test");
            }

            ViewBag.Results = results;
            return View("TestResults");
        }
    }
}
