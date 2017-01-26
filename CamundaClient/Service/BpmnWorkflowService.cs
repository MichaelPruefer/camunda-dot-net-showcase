using CamundaClient.Dto;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using CamundaClient.Requests;
using System.Text;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;
using System.Linq;

namespace CamundaClient.Service
{

    public class BpmnWorkflowService
    {
        private CamundaClientHelper helper;

        public BpmnWorkflowService(CamundaClientHelper client)
        {
            this.helper = client;
        }

        public string StartProcessInstance(string processDefinitionKey, Dictionary<string, object> variables) => StartProcessInstance(processDefinitionKey, null, variables);

        public string StartProcessInstance(string processDefinitionKey, string businessKey, Dictionary<string, object> variables)
        {
            using (var http = helper.HttpClient($"process-definition/key/{processDefinitionKey}/start"))
            {

                var request = new CompleteRequest
                {
                    Variables = CamundaClientHelper.ConvertVariables(variables),
                    BusinessKey = businessKey
                };

                var requestContent = new StringContent(
                    JsonConvert.SerializeObject(
                        request,
                        Formatting.None,
                        new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }),
                    Encoding.UTF8,
                    CamundaClientHelper.CONTENT_TYPE_JSON
                );
                var response = http.PostAsync("", requestContent).Result;
                if (response.IsSuccessStatusCode)
                {
                    var processInstance = JsonConvert.DeserializeObject<ProcessInstance>(response.Content.ReadAsStringAsync().Result);

                    return processInstance.Id;
                }
                else
                {
                    //var errorMsg = response.Content.ReadAsStringAsync();
                    throw new EngineException(response.ReasonPhrase);
                }
            }
        }

        public async Task<IDictionary<string, object>> LoadVariablesAsync(string taskId)
        {
            using (var http = helper.HttpClient($"task/{taskId}/variables"))
            {

                var response = await http.GetAsync("");
                if (response.IsSuccessStatusCode)
                {
                    // Successful - parse the response body
                    var variableResponse = JsonConvert.DeserializeObject<Dictionary<string, Variable>>(await response.Content.ReadAsStringAsync());

                    var variables = new Dictionary<string, object>();
                    foreach (var variable in variableResponse)
                    {
                        variables.Add(variable.Key, variable.Value.Value);
                    }
                    return variables;
                }
                else
                {
                    throw new EngineException("Could not load variable: " + response.ReasonPhrase);
                }
            }
        }

        public IList<ProcessInstance> LoadProcessInstances(IDictionary<string, string> queryParameters)
        {
            var queryString = string.Join("&", queryParameters.Select(x => x.Key + "=" + x.Value));
            var http = helper.HttpClient("process-instance/?" + queryString);

            var response = http.GetAsync("").Result;
            if (response.IsSuccessStatusCode)
            {
                // Successful - parse the response body
                var instances = JsonConvert.DeserializeObject<IEnumerable<ProcessInstance>>(response.Content.ReadAsStringAsync().Result);
                http.Dispose();
                return new List<ProcessInstance>(instances);
            }
            else
            {
                //Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                http.Dispose();
                throw new EngineException("Could not load process instances: " + response.ReasonPhrase);
            }

        }

    }


}
