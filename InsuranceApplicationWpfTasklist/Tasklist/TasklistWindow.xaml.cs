using System;
using System.Windows;
using System.Windows.Controls;
using CamundaClient;
using CamundaClient.Dto;
using System.ComponentModel;
using InsuranceApplicationWpfTasklist.Tasklist;
using System.Linq;

namespace InsuranceApplicationWpfTasklist
{
    /// <summary>
    /// Interaktionslogik für TasklistWindow.xaml
    /// </summary>
    public partial class TasklistWindow : Window
    {

        public CamundaEngineClient Camunda { get; }

        public TasklistWindow()
        {
            InitializeComponent();
            DataContext = this;
            Closing += OnWindowClosing;
            this.Camunda = new CamundaEngineClient();

            Startup();
        }

        /**
        * TODO: Make more robust to allow startup if Camunda is not available
        */
        public void Startup()
        {
            Camunda.Startup();

            ReloadTasks();
            LoadProcessDefinitions();
        }

        private void LoadProcessDefinitions()
        {
            var processDefinitions = Camunda.RepositoryService.LoadProcessDefinitions(true);
            processDefinitionListBox.Items.Clear();
            processDefinitionListBox.ItemsSource = processDefinitions.OrderBy(pd => pd.Name).ToList(); // add them sorted by name
            processDefinitionListBox.SelectedIndex = 0;

            processDefinitionListBox.DisplayMemberPath = "name";
        }

        public void ReloadTasks()
        {
            var tasks = Camunda.HumanTaskService.LoadTasks();
            taskListView.ItemsSource = tasks.OrderByDescending(task => task.Created).ToList(); // add them ordered by creation date
            /*
            Assembly thisExe = Assembly.GetEntryAssembly();
            var htmlStream = thisExe.GetManifestResourceStream("InsuranceApplicationWpfTasklist.Tasklist.diagram.html");
            DiagramBrowser.NavigateToStream(htmlStream);
            */
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            Camunda.Shutdown();
        }

        private void buttonReload_Click(object sender, RoutedEventArgs e)
        {
            ReloadTasks();
        }

        public void ShowDetails(string name, object content, Boolean firstTab) 
        {
            if (firstTab)
            {
                taskFormTabControl.Items.Clear();
            }

            var frame = new Frame();
            frame.Content = content;

            var tab = new TabItem();
            tab.FontSize = 16;
            tab.Header = name;
            tab.Content = frame;

            taskFormTabControl.Items.Add(tab);
            taskFormTabControl.Visibility = Visibility.Visible;
            if (firstTab)
            {
                taskFormTabControl.SelectedIndex = 0;
            }
        }

        public void HideDetails()
        {
            taskFormTabControl.Items.Clear();
            taskFormTabControl.Visibility = Visibility.Hidden;
        }

        private void ShowSelectedTask()
        {
            HumanTask task = (HumanTask)taskListView.SelectedItem;
            if (taskListView.SelectedIndex == -1 || task == null || task.FormKey == null)
            {
                HideDetails();
                return;
            }
            try
            {
                CamundaTaskForm taskFormPage = (CamundaTaskForm)Activator.CreateInstance(Type.GetType(task.FormKey));
                taskFormPage.initialize(this, task);
                ShowDetails("Task Form", taskFormPage, true);
                ShowDetails("Task Details", new TaskDetails(task), false);
            }
            catch (Exception ex)
            {
                // Could not load form - maybe no task for .NET tasklist!
                HideDetails();
            }
        }

        private void taskListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowSelectedTask();
        }

        private void buttonStartInsuranceApplication_Click(object sender, RoutedEventArgs e)
        {
            ProcessDefinition processDefinition = (ProcessDefinition)processDefinitionListBox.SelectedValue;
            if (processDefinitionListBox.SelectedIndex == -1 || processDefinition == null || processDefinition.StartFormKey == null)
            {
                HideDetails();
                return;
            }
            try
            {
                CamundaStartForm startFormPage = (CamundaStartForm)Activator.CreateInstance(Type.GetType(processDefinition.StartFormKey));
                startFormPage.initialize(this, processDefinition);
                ShowDetails("Start New '" + processDefinition.Name + "'", startFormPage, true);
            }
            catch (Exception ex)
            {
                // Could not load form - maybe no form key defined for .NET tasklist!
                HideDetails();
            }
        }

        private void taskListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowSelectedTask();
        }
    }
}
