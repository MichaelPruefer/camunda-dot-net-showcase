using System.Collections.Generic;
using System.Windows.Controls;
using CamundaClient.Dto;
using System.Windows;
using System;

namespace InsuranceApplicationWpfTasklist.TaskForms.EN
{
    /// <summary>
    /// Interaktionslogik für AntragPruefen.xaml
    /// </summary>
    public partial class DecideAboutApplication : Page, CamundaTaskForm
    {
        private TasklistWindow Tasklist;
        private HumanTask Task;
        public Dictionary<string, object> TaskVariables { get; set; }
        public Dictionary<string, object> NewVariables { get; set; }

        public DecideAboutApplication()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void initialize(TasklistWindow tasklist, HumanTask task)
        {
            this.Tasklist = tasklist;
            this.Task = task;
            TaskVariables = Tasklist.Camunda.HumanTaskService.LoadVariables(task.Id);
            NewVariables = new Dictionary<string, object>();
            NewVariables.Add("approved", false);
        }

        private void buttonCompleteTaskl_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try {
                Tasklist.Camunda.HumanTaskService.Complete(Task.Id, NewVariables);
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error completing the task: " + ex.Message + "\nPlease investigate and try again.", "Could not complete task", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Tasklist.HideDetails();
            Tasklist.ReloadTasks();
        }
    }
}
