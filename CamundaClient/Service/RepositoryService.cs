using CamundaClient.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using CamundaClient.Requests;
using System.Threading.Tasks;

namespace CamundaClient.Service
{

    public class RepositoryService
    {
        private CamundaClientHelper helper;

        public RepositoryService(CamundaClientHelper helper)
        {
            this.helper = helper;
        }


        public async Task<IEnumerable<ProcessDefinition>> LoadProcessDefinitionsAsync(bool onlyLatest)
        {
            IEnumerable<ProcessDefinition> result = null;
            using (var http = helper.HttpClient("process-definition/"))
            {
                var response = await http.GetAsync("?latestVersion=" + (onlyLatest ? "true" : "false"));
                if (response.IsSuccessStatusCode)
                {
                    result = JsonConvert.DeserializeObject<IEnumerable<ProcessDefinition>>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    return new List<ProcessDefinition>();
                }
            }

            // Could be extracted into separate method call if you run a lot of process definitions and want to optimize performance
            foreach (ProcessDefinition pd in result)
            {
                using (var http = helper.HttpClient($"process-definition/{pd.Id}/startForm"))
                {
                    var response = await http.GetAsync("");
                    var startForm = JsonConvert.DeserializeObject<StartForm>(await response.Content.ReadAsStringAsync());

                    pd.StartFormKey = startForm.Key;
                }
            }
            return new List<ProcessDefinition>(result);

        }
        public async Task<string> LoadProcessDefinitionXmlAsync(String processDefinitionId)
        {
            using (var http = helper.HttpClient("process-definition/" + processDefinitionId + "/xml"))
            {
                var response = await http.GetAsync("");
                if (response.IsSuccessStatusCode)
                {
                    ProcessDefinitionXml processDefinitionXml = JsonConvert.DeserializeObject<ProcessDefinitionXml>(response.Content.ReadAsStringAsync().Result);
                    return processDefinitionXml.Bpmn20Xml;
                }
                else
                {
                    return null;
                }
            }
        }
        public async Task DeleteDeploymentAsync(string deploymentId)
        {
            using (var http = helper.HttpClient($"deployment/{deploymentId}?cascade=true"))
            {
                var response = await http.DeleteAsync("");
                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    throw new EngineException($"{response.ReasonPhrase}: {errorMsg}");
                }
            }
        }

        public async Task<string> DeployAsync(string deploymentName, List<object> files)
        {
            var postParameters = new Dictionary<string, object> {
                { "deployment-name", deploymentName },
                { "deployment-source", "C# Process Application" },
                { "enable-duplicate-filtering", "true" },
                { "data", files }
            };

            // Create request and receive response
            var postURL = helper.RestUrl + "deployment/create";
            var webResponse = FormUpload.MultipartFormDataPost(postURL, helper.RestUsername, helper.RestPassword, postParameters);

            using (var reader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
            {
                var deployment = JsonConvert.DeserializeObject<Deployment>(await reader.ReadToEndAsync());
                return deployment.Id;
            }
        }

        public async Task AutoDeployAsync()
        {
            var thisExe = Assembly.GetEntryAssembly();
            var resources = thisExe.GetManifestResourceNames();

            if (resources.Length == 0)
            {
                return;
            }

            var files = new List<object>();
            foreach (string resource in resources)
            {
                // TODO Check if Camunda relevant (BPMN, DMN, HTML Forms)

                // Read and add to Form for Deployment                
                files.Add(FileParameter.FromManifestResource(thisExe, resource));

                Console.WriteLine("Adding resource to deployment: " + resource);
            }

            await DeployAsync(thisExe.GetName().Name, files);

            Console.WriteLine("Deployment to Camunda BPM succeeded.");

        }

    }
}
