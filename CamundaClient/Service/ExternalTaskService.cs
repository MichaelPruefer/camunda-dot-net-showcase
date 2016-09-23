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
                var requestContent = new StringContent(
                    JsonConvert.SerializeObject(
                        fetchAndLockRequest,
                        Formatting.None,
                        new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }),
                    Encoding.UTF8,
                    CamundaClientHelper.CONTENT_TYPE_JSON
                );
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

        public async Task<bool> CompleteAsync(string workerId, string externalTaskId, Dictionary<string, object> variablesToPassToProcess)
        {
            using (var httpClient = helper.HttpClient($"external-task/{externalTaskId}/complete"))
            {
                //new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }
                var request = new CompleteRequest
                {
                    WorkerId = workerId,
                    Variables = CamundaClientHelper.ConvertVariables(variablesToPassToProcess)
                };

                var requestContent = new StringContent(
                    JsonConvert.SerializeObject(
                        request,
                        Formatting.None,
                        new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }),
                    Encoding.UTF8,
                    CamundaClientHelper.CONTENT_TYPE_JSON
                );
                var response = await httpClient.PostAsync("", requestContent);
                if (!response.IsSuccessStatusCode)
                {
                    throw new EngineException("Could not complete external Task: " + response.ReasonPhrase);
                }
            }

            return true;
        }

        public async Task<bool> FailureAsync(string workerId, string externalTaskId, string errorMessage, int retries, long retryTimeout)
        {
            using (var http = helper.HttpClient("external-task/" + externalTaskId + "/failure"))
            {
                var request = new FailureRequest();
                request.WorkerId = workerId;
                request.ErrorMessage = errorMessage;
                request.Retries = retries;
                request.RetryTimeout = retryTimeout;

                var requestContent = new StringContent(
                    JsonConvert.SerializeObject(
                        request,
                        Formatting.None,
                        new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }),
                    Encoding.UTF8,
                    CamundaClientHelper.CONTENT_TYPE_JSON
                );
                var response = await http.PostAsync("", requestContent);
                if (!response.IsSuccessStatusCode)
                {
                    throw new EngineException("Could not report failure for external Task: " + response.ReasonPhrase);
                }
            }

            return true;
        }

        public async Task<bool> BpmnErrorAsync(string workerId, string externalTaskId, string errorCode)
        {
            using (var http = helper.HttpClient($"external-task/{externalTaskId}/bpmnError"))
            {
                var request = new
                {
                    WorkerId = workerId,
                    ErrorCode = errorCode
                };
                var requestContent = new StringContent(
                    JsonConvert.SerializeObject(
                        request,
                        Formatting.None,
                        new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }),
                    Encoding.UTF8,
                    CamundaClientHelper.CONTENT_TYPE_JSON
                );
                var response = await http.PostAsync("", requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    throw new EngineException("Could not report BPMN error for external Task: " + response.ReasonPhrase);
                }
            }

            return true;
        }
    }
}
