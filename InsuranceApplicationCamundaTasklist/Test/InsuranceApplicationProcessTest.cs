using CamundaClient;
using CamundaClient.Service;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CamundaClient.Dto;

namespace SimpleCalculationProcess
{
    [TestFixture]
    class CalculationProcessTest
    {
        [Test]
        public void TestInsuranceApplicationManuallyApproved()
        {
            // Engine client should point to a dedicated Camunda instance for test, preferrably locally available

            var camunda = new CamundaEngineClient(new System.Uri("http://localhost:8080/engine-rest/engine/default/"), null, null);

            // Deploy the models under test
            string deploymentId = camunda.RepositoryService.Deploy("testcase", new List<object> {
                    FileParameter.FromManifestResource(Assembly.GetExecutingAssembly(), "InsuranceApplicationCamundaTasklist.CamundaModels.DE.InsuranceApplication.bpmn"),
                    FileParameter.FromManifestResource(Assembly.GetExecutingAssembly(), "InsuranceApplicationCamundaTasklist.CamundaModels.DE.RiskAssessment.dmn")});

            try
            {
                // Start Instance
                string processInstanceId = camunda.BpmnWorkflowService.StartProcessInstance("insuranceApplication.NET.DE", new Dictionary<string, object>()
                {
                    {"age", 36 },
                    {"carManufacturer", "Porsche" },
                    {"carType", "911" }
                });

                // Check User Task
                var tasks = camunda.HumanTaskService.LoadTasks(new Dictionary<string, string>() {
                    { "processInstanceId", processInstanceId }
                });
                Assert.AreEqual(1, tasks.Count);
                Assert.AreEqual("userTaskAntragEntscheiden", tasks.First().TaskDefinitionKey);

                // Complete User Task, approve application
                camunda.HumanTaskService.Complete(tasks.First().Id, new Dictionary<string, object>() {
                    {"approved", true }
                });

                // Check that External Task for policy created is there
                var externalTasks = camunda.ExternalTaskService.FetchAndLockTasks("testcase", 100, "issuePolicy", 1000, new List<string>());
                Assert.AreEqual(1, externalTasks.Count);
                Assert.AreEqual("ServiceTaskPoliceAusstellen", externalTasks.First().ActivityId);
                camunda.ExternalTaskService.Complete("testcase", externalTasks.First().Id, new Dictionary<string, object>());

                // Check that External Task for sending the policy is there
                externalTasks = camunda.ExternalTaskService.FetchAndLockTasks("testcase", 100, "sendEmail", 1000, new List<string>());
                Assert.AreEqual(1, externalTasks.Count);
                Assert.AreEqual("ServiceTaskSendPolicy", externalTasks.First().ActivityId);
                camunda.ExternalTaskService.Complete("testcase", externalTasks.First().Id, new Dictionary<string, object>());

                // now the process instance has ended, TODO: Check state with History
            }
            finally
            {
                // cleanup after test case
                camunda.RepositoryService.DeleteDeployment(deploymentId);
            }
        }
    }
}
