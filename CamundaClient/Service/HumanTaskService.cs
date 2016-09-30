using CamundaClient.Dto;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using CamundaClient.Requests;
using System.Threading.Tasks;

namespace CamundaClient.Service
{

    public class HumanTaskService
    {
        private CamundaClientHelper helper;

        public HumanTaskService(CamundaClientHelper client)
        {
            this.helper = client;
        }

        public async Task<IEnumerable<HumanTask>> LoadTasksAsync() => await LoadTasksAsync(new Dictionary<string, string>());

        public async Task<IEnumerable<HumanTask>> LoadTasksAsync(IDictionary<string, string> queryParameters)
        {
            var result = await helper.GetAsync<IEnumerable<HumanTask>>("task/", queryParameters);

            return result;
        }

        public async Task<Dictionary<string, object>> LoadVariablesAsync(string taskId)
        {
            var variableResponse = await helper.GetAsync<Dictionary<string, Variable>>($"task/{taskId}/variables", null);

            Dictionary<string, object> variables = new Dictionary<string, object>();
            foreach (var variable in variableResponse)
            {
                if (variable.Value.Type == "object")
                {
                    string stringValue = (string)variable.Value.Value;
                    // lets assume we only work with JSON serialized values 
                    stringValue = stringValue.Remove(stringValue.Length - 1).Remove(0, 1); // remove one bracket from {{ and }}
                    var jsonObject = JContainer.Parse(stringValue);

                    variables.Add(variable.Key, jsonObject);
                }
                else
                {
                    variables.Add(variable.Key, variable.Value.Value);
                }
            }

            return variables;
        }

        public async Task CompleteAsync(string taskId, Dictionary<string, object> variables)
        {
            var request = new CompleteRequest
            {
                Variables = CamundaClientHelper.ConvertVariables(variables)
            };
            await helper.PostAsync($"task/{taskId}/complete", request);
        }

        public async Task ResolveAsync(string taskId, Dictionary<string, object> variables)
        {
            var request = new CompleteRequest
            {
                Variables = CamundaClientHelper.ConvertVariables(variables)
            };
            await helper.PostAsync($"task/{taskId}/resolve", request);
        }

        public async Task ClaimAsync(string taskId, string userId)
        {
            var request = new
            {
                UserId = userId
            };
            await helper.PostAsync($"task/{taskId}/claim", request);
        }

        public async Task UnclaimAsync(string taskId)
        {
            await helper.PostAsync($"task/{taskId}/unclaim", null);
        }
    }
}
