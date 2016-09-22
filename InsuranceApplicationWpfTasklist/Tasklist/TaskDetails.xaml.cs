using CamundaClient;
using CamundaClient.Dto;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace InsuranceApplicationWpfTasklist.Tasklist
{
    /// <summary>
    /// Interaktionslogik für TaskDetails.xaml
    /// </summary>
    public partial class TaskDetails : Page
    {

        private string cockpiturl;

        public TaskDetails(HumanTask task)
        {
            InitializeComponent();

            cockpiturl = CamundaEngineClient.COCKPIT_URL + "#/process-instance/" + task.ProcessInstanceId + "/runtime";
            CockpitUrlText.Text = cockpiturl;

            taskDetailsListView.Items.Add(new TaskProperty("Process Instance Id", task.ProcessInstanceId));
            taskDetailsListView.Items.Add(new TaskProperty("Process Definition Id", task.ProcessDefinitionId));
            taskDetailsListView.Items.Add(new TaskProperty("Task Assignee", task.Assignee));
            taskDetailsListView.Items.Add(new TaskProperty("Task Creation Date", String.Format("{0:dd/MM/yyyy HH:mm:ss}", task.Created)));
            taskDetailsListView.Items.Add(new TaskProperty("Task Definition Key", task.TaskDefinitionKey));
            taskDetailsListView.Items.Add(new TaskProperty("Task Description", task.Description));
            taskDetailsListView.Items.Add(new TaskProperty("Task Due Date", String.Format("{0:dd/MM/yyyy HH:mm:ss}", task.Due)));
            taskDetailsListView.Items.Add(new TaskProperty("Task Follow Up Date", String.Format("{0:dd/MM/yyyy HH:mm:ss}", task.FollowUp)));
            taskDetailsListView.Items.Add(new TaskProperty("Task Form Key", task.FormKey));
            taskDetailsListView.Items.Add(new TaskProperty("Task Id", task.Id));
            taskDetailsListView.Items.Add(new TaskProperty("Task Name", task.Name));
            taskDetailsListView.Items.Add(new TaskProperty("Task Owner", task.Owner));
            taskDetailsListView.Items.Add(new TaskProperty("Task Priority", task.Priority));
        }

        private void NavigateToCockpit(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(cockpiturl);
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(cockpiturl);
        }
    }

    public class TaskProperty {
        public string Property { get;  }
        public string Value { get; }
        public TaskProperty(string property, string value)
        {
            this.Property = property;
            this.Value = value;
        }
    }
}
