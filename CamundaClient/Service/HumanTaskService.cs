using CamundaClient.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using CamundaClient.Requests;
using Newtonsoft.Json.Serialization;
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

        public async Task<bool> CompleteAsync(string taskId, Dictionary<string, object> variables)
        {
            var request = new CompleteRequest
            {
                Variables = CamundaClientHelper.ConvertVariables(variables)
            };
            var result = await helper.PostAsync<bool>($"task/{taskId}/complete", request);

            return true;
        }

        public async Task<bool> ResolveAsync(string taskId, Dictionary<string, object> variables)
        {
            var request = new CompleteRequest
            {
                Variables = CamundaClientHelper.ConvertVariables(variables)
            };
            var result = await helper.PostAsync<bool>($"task/{taskId}/resolve", request);

            return true;
        }

        public async Task<bool> ClaimAsync(string taskId, string userId)
        {
            var request = new
            {
                UserId = userId
            };
            var result = await helper.PostAsync<bool>($"task/{taskId}/claim", request);

            return true;
        }

        public async Task<bool> UnclaimAsync(string taskId)
        {
            var result = await helper.PostAsync<bool>($"task/{taskId}/unclaim", null);

            return true;
        }
    }
}
