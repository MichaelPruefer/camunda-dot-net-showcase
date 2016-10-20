using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamundaClient.Dto
{
    public class ProcessActivityInstanceResponse
    {
        public string Id { get; set; }
        public object ParentActivityInstanceId { get; set; }
        public string ActivityId { get; set; }
        public string ActivityType { get; set; }
        public string ProcessInstanceId { get; set; }
        public string ProcessDefinitionId { get; set; }
        public ChildActivityInstance[] ChildActivityInstances { get; set; }
        public string[] ExecutionIds { get; set; }
        public string ActivityName { get; set; }
        public string Name { get; set; }
    }

    public class ChildActivityInstance
    {
        public string Id { get; set; }
        public string ParentActivityInstanceId { get; set; }
        public string ActivityId { get; set; }
        public string ActivityType { get; set; }
        public string ProcessInstanceId { get; set; }
        public string ProcessDefinitionId { get; set; }
        public ChildActivityInstance[] ChildActivityInstances { get; set; }
        public string[] ExecutionIds { get; set; }
        public string ActivityName { get; set; }
        public string Name { get; set; }
    }
}



