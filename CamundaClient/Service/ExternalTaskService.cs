using CamundaClient.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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

        public async Task<bool> CompleteAsync(string workerId, string externalTaskId, Dictionary<string, object> variablesToPassToProcess)
        {
            var request = new CompleteRequest
            {
                WorkerId = workerId,
                Variables = CamundaClientHelper.ConvertVariables(variablesToPassToProcess)
            };

            var result = await helper.PostAsync<object>($"{ExternalTaskService.EXTERNAL_TASK_URI}/{externalTaskId}/complete", request);
            
            return true;
        }

        public async Task<bool> FailureAsync(string workerId, string externalTaskId, string errorMessage, int retries, long retryTimeout)
        {
            var request = new FailureRequest
            {
                WorkerId = workerId,
                ErrorMessage = errorMessage,
                Retries = retries,
                RetryTimeout = retryTimeout
            };

            var result = await helper.PostAsync<object>($"{ExternalTaskService.EXTERNAL_TASK_URI}/{externalTaskId}/failure", request);

            return true;
        }

        public async Task<bool> BpmnErrorAsync(string workerId, string externalTaskId, string errorCode)
        {
            var request = new
            {
                WorkerId = workerId,
                ErrorCode = errorCode
            };

            var result = await helper.PostAsync<object>($"{ExternalTaskService.EXTERNAL_TASK_URI}/{externalTaskId}/bpmnError", request);

            return true;
        }

        public async Task<ExternalTask> FetchTaskAsync(string externalTaskId)
        {
            var result = await helper.GetAsync<ExternalTask>($"{ExternalTaskService.EXTERNAL_TASK_URI}", null);

            return result;
        }

        public async Task<IEnumerable<ExternalTask>> FetchTaskListAsync(string workerId, string topicName, int maxResults)
        {
            var queryParams = new List<Tuple<string, string>>();
            if (!String.IsNullOrEmpty(workerId))
            {
                queryParams.Add(new Tuple<string, string>("workerId", workerId));
            }

            if (!String.IsNullOrEmpty(topicName))
            {
                queryParams.Add(new Tuple<string, string>("topicName", topicName));
            }

            if (maxResults > 0)
            {
                queryParams.Add(new Tuple<string, string>("maxResult", maxResults.ToString()));
            }

            var result = await helper.GetAsync<IEnumerable<ExternalTask>>($"{ExternalTaskService.EXTERNAL_TASK_URI}", queryParams.ToArray());

            return result;
        }

        public async Task<int> CountTasksAsync(string workerId, string topicName)
        {
            var queryParams = new List<Tuple<string, string>>();

            if (!String.IsNullOrEmpty(workerId))
            {
                queryParams.Add(new Tuple<string, string>("workerId", workerId));
            }

            if (!String.IsNullOrEmpty(topicName))
            {
                queryParams.Add(new Tuple<string, string>("topicName", topicName));
            }

            var result = await helper.GetAsync<CountResponse>($"{ExternalTaskService.EXTERNAL_TASK_URI}/count", queryParams.ToArray());

            return result?.Count ?? -1;
        }
    }
}
