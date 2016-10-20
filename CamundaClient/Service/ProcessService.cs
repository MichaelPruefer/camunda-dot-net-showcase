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

        public async Task<ProcessActivityInstanceResponse> ActivityInstances(string processInstanceId, string activityName)
        {
            var result = await helper.GetAsync<ProcessActivityInstanceResponse>($"{PROCESS_INSTANCE_URI}/{processInstanceId}/activity-instances", new Dictionary<string, string> { { "activityName", activityName } });

            return result;
        }

        public async Task<ProcessInstance[]> List()
        {
            var result = await helper.GetAsync<ProcessInstance[]>($"{PROCESS_INSTANCE_URI}", null);

            return result;
        }

        public async Task Modification(string processInsanceId, string targetActivityInstanceId, string currentActivityInstanceId)
        {


            var target = new
            {
                Type = "startBeforeActivity",
                ActivityId = targetActivityInstanceId,
                Variable = new object()
            };
            var source = new
            {
                Type = "cancel",
                ActivityInstanceId = currentActivityInstanceId
            };
            var instructions = new object[] { target, source };

            var content = new
            {
                SkipCustomListeners = true,
                SkipIoMappings = true,
                Instructions = instructions,

            };

            await helper.PostAsync($"{PROCESS_INSTANCE_URI}/{processInsanceId}/modification", content);

        }
    }
}