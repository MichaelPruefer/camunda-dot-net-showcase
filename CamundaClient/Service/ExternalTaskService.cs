using CamundaClient.Dto;
using System;
using System.Collections.Generic;
using CamundaClient.Requests;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace CamundaClient.Service
{

    public class ExternalTaskService
    {
        private CamundaClientHelper helper;
        private const string EXTERNAL_TASK_URI = "external-task";

        public ExternalTaskService(CamundaClientHelper client)
        {
            this.helper = client;
        }

        public async Task<IEnumerable<ExternalTask>> FetchAndLockTasksAsync(string workerId, int maxTasks, string topicName, long lockDurationInMilliseconds, IEnumerable<string> variablesToFetch)
        {
            var lockRequest = new FetchAndLockRequest
            {
                WorkerId = workerId,
                MaxTasks = maxTasks
            };
            var lockTopic = new FetchAndLockTopic
            {
                TopicName = topicName,
                LockDuration = lockDurationInMilliseconds,
                Variables = variablesToFetch
            };
            lockRequest.Topics.Add(lockTopic);

            return await FetchAndLockTasksAsync(lockRequest);
        }

        public async Task<IEnumerable<ExternalTask>> FetchAndLockTasksAsync(FetchAndLockRequest fetchAndLockRequest)
        {
             return await helper.PostAsync<IList<ExternalTask>>($"{ExternalTaskService.EXTERNAL_TASK_URI}/fetchAndLock", fetchAndLockRequest);
        }

        public async Task CompleteAsync(string workerId, string externalTaskId, Dictionary<string, object> variablesToPassToProcess)
        {
            var request = new CompleteRequest
            {
                WorkerId = workerId,
                Variables = CamundaClientHelper.ConvertVariables(variablesToPassToProcess)
            };

            await helper.PostAsync($"{ExternalTaskService.EXTERNAL_TASK_URI}/{externalTaskId}/complete", request);
        }

        public async Task FailureAsync(string workerId, string externalTaskId, string errorMessage, int retries, long retryTimeout)
        {
            var request = new FailureRequest
            {
                WorkerId = workerId,
                ErrorMessage = errorMessage,
                Retries = retries,
                RetryTimeout = retryTimeout
            };

            await helper.PostAsync($"{ExternalTaskService.EXTERNAL_TASK_URI}/{externalTaskId}/failure", request);
        }

        public async Task BpmnErrorAsync(string workerId, string externalTaskId, string errorCode)
        {
            var request = new
            {
                WorkerId = workerId,
                ErrorCode = errorCode
            };

            await helper.PostAsync($"{ExternalTaskService.EXTERNAL_TASK_URI}/{externalTaskId}/bpmnError", request);
        }

        public async Task<ExternalTask> FetchTaskAsync(string externalTaskId)
        {
            var result = await helper.GetAsync<ExternalTask>($"{ExternalTaskService.EXTERNAL_TASK_URI}", null);

            return result;
        }

        public async Task SetRetriesAsync(string externalTaskId, int retries)
        {
            await helper.PutAsync($"{ExternalTaskService.EXTERNAL_TASK_URI}/{externalTaskId}/retries", new { Retries = retries });
        }

        public async Task<IEnumerable<ExternalTask>> FetchTaskListAsync(string workerId, string topicName, int maxResults)
        {
            var queryParams = new Dictionary<string, string>();
            if (!String.IsNullOrEmpty(workerId))
            {
                queryParams.Add("workerId", workerId);
            }

            if (!String.IsNullOrEmpty(topicName))
            {
                queryParams.Add("topicName", topicName);
            }

            if (maxResults > 0)
            {
                queryParams.Add("maxResult", maxResults.ToString());
            }

            var result = await helper.GetAsync<IEnumerable<ExternalTask>>($"{ExternalTaskService.EXTERNAL_TASK_URI}", queryParams);

            return result;
        }

        public async Task<IEnumerable<ExternalTask>> ListAsync(string rawFilter)
        {
            return await helper.GetAsync<IEnumerable<ExternalTask>>($"{ExternalTaskService.EXTERNAL_TASK_URI}?{rawFilter}", null);
        }

        public async Task<int> CountTasksAsync(string workerId, string topicName)
        {
            var queryParams = new Dictionary<string, string>();
            if (!String.IsNullOrEmpty(workerId))
            {
                queryParams.Add("workerId", workerId);
            }

            if (!String.IsNullOrEmpty(topicName))
            {
                queryParams.Add("topicName", topicName);
            }

            var result = await helper.GetAsync<CountResponse>($"{ExternalTaskService.EXTERNAL_TASK_URI}/count", queryParams);

            return result?.Count ?? -1;
        }
    }
}
