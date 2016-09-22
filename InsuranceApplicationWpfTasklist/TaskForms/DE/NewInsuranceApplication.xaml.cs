using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CamundaClient.Dto;
using System;

namespace InsuranceApplicationWpfTasklist.TaskForms.DE
{
    /// <summary>
    /// Interaktionslogik für NewInsuranceApplication.xaml
    /// </summary>
    public partial class NewInsuranceApplication : Page, CamundaStartForm
    {
        private TasklistWindow Tasklist;
        private ProcessDefinition ProcessDefinition;
        public Dictionary<string, object> Variables { get; set;}

        public NewInsuranceApplication()
        {
            Variables = new Dictionary<string, object>();
            InitializeComponent();
            DataContext = this;
        }

        public void initialize(TasklistWindow tasklist, ProcessDefinition ProcessDefinition)
        {
            this.Tasklist = tasklist;
            this.ProcessDefinition = ProcessDefinition;
        }

        private void buttonStartProcessInstance_Click(object sender, RoutedEventArgs e)
        {
            try {
                Tasklist.Camunda.BpmnWorkflowService.StartProcessInstance(ProcessDefinition.Key, Variables);
                Tasklist.HideDetails();
                Tasklist.ReloadTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error starting the process instance: " + ex.Message + "\nPlease investigate and try again.", "Could not start process instance", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
