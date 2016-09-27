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
        public void TestHappyPath()
        {
            // Engine client should point to a dedicated Camunda instance for test, preferrably locally available
            var camunda = new CamundaEngineClient(new System.Uri("http://localhost:8080/engine-rest/engine/default/"), null, null);

            // Deploy the process under test
            string deploymentId = camunda.RepositoryService.DeployAsync("testcase", new List<object> {
                FileParameter.FromManifestResource(Assembly.GetExecutingAssembly(), "SimpleCalculationProcess.calculation.bpmn") }).Result;

            string processInstanceId = camunda.BpmnWorkflowService.StartProcessInstance("calculate", new Dictionary<string, object>()
            {
                {"x", 5 },
                {"y", 10 }
            });

            var externalTasks = camunda.ExternalTaskService.FetchAndLockTasksAsync("testcase", 100, "calculate", 1000, new List<string>() { "x", "y" }).Result;
            Assert.AreEqual(1, externalTasks.Count());
            Assert.AreEqual("ServiceTaskCalculate", externalTasks.First().ActivityId);

            camunda.ExternalTaskService.CompleteAsync("testcase", externalTasks.First().Id, new Dictionary<string, object>() { { "result", 15 } }).Wait();

            var tasks = camunda.HumanTaskService.LoadTasksAsync(new Dictionary<string, string>() {
                { "processInstanceId", processInstanceId }
            }).Result;
            Assert.AreEqual(1, tasks.Count());
            Assert.AreEqual("UserTaskReviewResult", tasks.First().TaskDefinitionKey);

            camunda.HumanTaskService.CompleteAsync(tasks.First().Id, new Dictionary<string, object>()).Wait();

            // not the process instance has ended, TODO: Check state with History

            // cleanup after test case
            camunda.RepositoryService.DeleteDeploymentAsync(deploymentId).Wait();
        }
    }
}
