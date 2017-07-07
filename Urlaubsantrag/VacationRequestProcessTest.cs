using CamundaClient;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CamundaClient.Dto;

namespace Urlaubsantrag
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
            string deploymentId = camunda.RepositoryService.Deploy("testcase", new List<object> {
                FileParameter.FromManifestResource(Assembly.GetExecutingAssembly(), "Urlaubsantrag.Models.Urlaubsantrag.bpmn") });
            try
            {

                string processInstanceId = camunda.BpmnWorkflowService.StartProcessInstance("Urlaubsantrag", new Dictionary<string, object>()
                {
                    {"name", "Bernd Rücker" },
                    {"vacationStartDate", 10 },
                    {"duration", 5 }
                });

                var tasks = camunda.HumanTaskService.LoadTasks(new Dictionary<string, string>() {
                    { "processInstanceId", processInstanceId }
                });
                Assert.AreEqual(1, tasks.Count);
                Assert.AreEqual("UserTask_UrlaubGenehmigen", tasks.First().TaskDefinitionKey);

                camunda.HumanTaskService.Complete(tasks.First().Id, new Dictionary<string, object>()
                {
                    {"approved", true }
                });


                var externalTasks = camunda.ExternalTaskService.FetchAndLockTasks(
                    "worker1", 
                    100, 
                    "leaveAccount", 
                    1000, 
                    new List<string>() { "name" });

                // Do the real service logic / calls

                camunda.ExternalTaskService.Complete("worker1", externalTasks.First().Id, new Dictionary<string, object>() {
                    { "someResult", 42 }
                });

                // TODO: Simulate timer
                //externalTasks = camunda.ExternalTaskService.FetchAndLockTasks("testcase", 100, "informColleagues", 1000, new List<string>());
                //Assert.AreEqual(1, externalTasks.Count);
                //Assert.AreEqual("ServiceTask_KollegenInformieren", externalTasks.First().ActivityId);
                //camunda.ExternalTaskService.Complete("testcase", externalTasks.First().Id, new Dictionary<string, object>());

                // not the process instance has ended, TODO: Check state with History
            }
            finally
            {
                // cleanup after test case
                camunda.RepositoryService.DeleteDeployment(deploymentId);
            }
        }
    }
}
