using System.Collections.Generic;
using System.Windows.Controls;
using CamundaClient.Dto;

namespace InsuranceApplicationWpfTasklist.TaskForms
{
    /// <summary>
    /// Interaktionslogik für AntragPruefen.xaml
    /// </summary>
    public partial class Escalate : Page, CamundaTaskForm
    {
        private TasklistWindow Tasklist;
        private HumanTask Task;
        public Dictionary<string, object> Variables { get; set; }

        public Escalate()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void initialize(TasklistWindow tasklist, HumanTask task)
        {
            this.Tasklist = tasklist;
            this.Task = task;

            Variables = Tasklist.Camunda.HumanTaskService.LoadVariables(task.Id);
        }

    }
}
