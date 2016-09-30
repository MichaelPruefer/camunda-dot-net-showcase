using System.Collections.Generic;
using System.Threading.Tasks;
using CamundaClient.Dto;

namespace CamundaClient.Service
{
    public class HistoryService
    {
        private CamundaClientHelper helper;
        private const string HISTORY_BASE_URI = "history";
        private const string HISTORY_VARIABLE_URI = HISTORY_BASE_URI + "/variable-instance";
        private const string HISTORY_TASK_URI = HISTORY_BASE_URI + "/task";
        public HistoryService(CamundaClientHelper client)
        {
            helper = client;
        }

        public async Task<int> GetVariableCountAsync(string processInstanceId)
        {
            var filter = new
            {
                ProcessInstanceId = processInstanceId
            };
            var result = await helper.PostAsync<CountResponse>($"{HistoryService.HISTORY_VARIABLE_URI}/count", filter);

            return result?.Count ?? -1;
        }

        public async Task<IEnumerable<Variable>> GetVariablesAsync(string processInstanceId)
        {
            var filter = new
            {
                ProcessInstanceId = processInstanceId
            };
            var result = await helper.PostAsync<IEnumerable<Variable>>($"{HistoryService.HISTORY_VARIABLE_URI}", filter);
            
            return result;
        }
    }
}