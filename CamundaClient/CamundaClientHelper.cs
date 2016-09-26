using CamundaClient.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace CamundaClient
{

    public class CamundaClientHelper
    {
        public Uri RestUrl { get; }
        public const string CONTENT_TYPE_JSON = "application/json";
        public string RestUsername { get; }
        public string RestPassword { get; }

        public CamundaClientHelper(Uri restUrl, string username, string password)
        {
            this.RestUrl = restUrl;
            this.RestUsername = username;
            this.RestPassword = password;
        }

        public HttpClient HttpClient(string path)
        {
            HttpClient client = null;
            if (RestUsername != null)
            {
                var credentials = new NetworkCredential(RestUsername, RestPassword);
                client = new HttpClient(new HttpClientHandler() { Credentials = credentials });
            }
            else
            {
                client = new HttpClient();
            }
            client.BaseAddress = new Uri(RestUrl + path);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(CONTENT_TYPE_JSON));

            return client;
        }

        public static Dictionary<string, Variable> ConvertVariables(Dictionary<string, object> variables)
        {
            // report successful execution
            var result = new Dictionary<string, Variable>();
            if (variables == null)
            {
                return result;
            }
            foreach (var variable in variables)
            {
                Variable camundaVariable = new Variable
                {
                    Value = variable.Value
                };
                result.Add(variable.Key, camundaVariable);
            }
            return result;
        }

        public async Task<T> PostAsync<T>(string uri, object content)
        {
            var result = default(T);
            using (var http = HttpClient(uri))
            {
                try
                {
                    var requestContent = new StringContent(
                        JsonConvert.SerializeObject(
                            content,
                            Formatting.None,
                            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }),
                        Encoding.UTF8,
                        CamundaClientHelper.CONTENT_TYPE_JSON
                    );
                    var response = await http.PostAsync("", requestContent);
                    if (response.IsSuccessStatusCode)
                    {
                        result = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        throw new EngineException($"Post '{uri}' failed with: {response.ReasonPhrase}");
                    }
                }
                finally
                {
                    http.Dispose();
                }
            }

            return result;
        }

        public async Task PostAsync(string uri, object content)
        {
            using (var http = HttpClient(uri))
            {
                try
                {
                    var requestContent = new StringContent(
                        JsonConvert.SerializeObject(
                            content,
                            Formatting.None,
                            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }),
                        Encoding.UTF8,
                        CamundaClientHelper.CONTENT_TYPE_JSON
                    );
                    var response = await http.PostAsync("", requestContent);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new EngineException($"Post '{uri}' failed with: {response.ReasonPhrase}");
                    }
                }
                finally
                {
                    http.Dispose();
                }
            }
        }

        public async Task<T> GetAsync<T>(string uri, IDictionary<string, string> queryParams)
        {
            string queryParam = BuildQueryString(queryParams);

            var result = default(T);

            using (var http = HttpClient(uri + queryParam))
            {
                var response = await http.GetAsync("");
                if (response.IsSuccessStatusCode)
                {
                    result = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    throw new EngineException($"Get '{uri}' failed with: {response.ReasonPhrase}");
                }
            }

            return result;
        }

        public async Task GetAsync(string uri, IDictionary<string, string> queryParams)
        {
            string queryParam = BuildQueryString(queryParams);

            using (var http = HttpClient(uri + queryParam))
            {
                var response = await http.GetAsync("");
                if (!response.IsSuccessStatusCode)
                {
                    throw new EngineException($"Get '{uri}' failed with: {response.ReasonPhrase}");
                }
            }
        }

        private static string BuildQueryString(IDictionary<string, string> queryParams)
        {
            var queryParam = String.Join("&", queryParams?.Select(pair => $"{pair.Key}={pair.Value}") ?? new string[] { });

            if (!string.IsNullOrEmpty(queryParam))
            {
                queryParam = "?" + queryParam;
            }

            return queryParam;
        }
    }
}
