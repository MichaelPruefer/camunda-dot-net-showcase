using CamundaClient.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using CamundaClient.Requests;
using System.Threading.Tasks;

namespace CamundaClient.Service
{

    public class ExternalTaskService
    {
        private CamundaClientHelper helper;

        public ExternalTaskService(CamundaClientHelper client)
        {
            this.helper = client;
        }

        public IList<ExternalTask> FetchAndLockTasks(string workerId, int maxTasks, string topicName, long lockDurationInMilliseconds, IEnumerable<string> variablesToFetch)
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

            return FetchAndLockTasks(lockRequest);
        }

        public IList<ExternalTask> FetchAndLockTasks(FetchAndLockRequest fetchAndLockRequest)
        {
            var http = helper.HttpClient("external-task/fetchAndLock");
            try
            {
                var requestContent = new StringContent(JsonConvert.SerializeObject(fetchAndLockRequest), Encoding.UTF8, CamundaClientHelper.CONTENT_TYPE_JSON);
                var response = http.PostAsync("", requestContent).Result;
                if (response.IsSuccessStatusCode)
                {
                    var tasks = JsonConvert.DeserializeObject<IEnumerable<ExternalTask>>(response.Content.ReadAsStringAsync().Result);

                    http.Dispose();
                    return new List<ExternalTask>(tasks);
                }
                else
                {
                    http.Dispose();
                    throw new EngineException("Could not fetch and lock tasks: " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                http.Dispose();
                Console.WriteLine(ex.Message);
                // TODO: Handle Exception, add back off
                return new List<ExternalTask>();
            }
        }

        public void Complete(string workerId, string externalTaskId, Dictionary<string, object> variablesToPassToProcess)
        {
            var http = helper.HttpClient("external-task/" + externalTaskId + "/complete");

            var request = new CompleteRequest();
            request.WorkerId = workerId;
            request.Variables = CamundaClientHelper.ConvertVariables(variablesToPassToProcess);

            var requestContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, CamundaClientHelper.CONTENT_TYPE_JSON);
            var response = http.PostAsync("", requestContent).Result;
            http.Dispose();
            if (!response.IsSuccessStatusCode)
            {
                throw new EngineException("Could not complete external Task: " + response.ReasonPhrase);
            }
        }

        public void Failure(string workerId, string externalTaskId, string errorMessage, int retries, long retryTimeout)
        {
            var http = helper.HttpClient("external-task/" + externalTaskId + "/failure");

            var request = new FailureRequest();
            request.WorkerId = workerId;
            request.ErrorMessage = errorMessage;
            request.Retries = retries;
            request.RetryTimeout = retryTimeout;

            var requestContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, CamundaClientHelper.CONTENT_TYPE_JSON);
            var response = http.PostAsync("", requestContent).Result;
            http.Dispose();
            if (!response.IsSuccessStatusCode)
            {
                throw new EngineException("Could not report failure for external Task: " + response.ReasonPhrase);
            }
        }
    }
}
