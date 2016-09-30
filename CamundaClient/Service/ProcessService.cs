using System.Collections.Generic;
using System.Threading.Tasks;
using CamundaClient.Dto;

namespace CamundaClient.Service
{
    public class ProcessService
    {
        private CamundaClientHelper helper;

        private const string PROCESS_INSTANCE_URI = "process-instance";

        public ProcessService(CamundaClientHelper client)
        {
            helper = client;
        }

        public async Task DeleteProcessInstanceAsync(string processInstanceId)
        {
            await helper.DeleteAsync($"{ProcessService.PROCESS_INSTANCE_URI}/{processInstanceId}");
        }

        public async Task GetProcessCountAsync()
        {
            var result = await helper.PostAsync<CountResponse>($"{ProcessService.PROCESS_INSTANCE_URI}/", null);
        }

        public async Task<IDictionary<string, Variable>> GetProcessVariables(string processInstanceId)
        {
            var result = await helper.GetAsync<IDictionary<string, Variable>>($"{ProcessService.PROCESS_INSTANCE_URI}/{processInstanceId}/variables", null);

            return result;
        }
    }
}