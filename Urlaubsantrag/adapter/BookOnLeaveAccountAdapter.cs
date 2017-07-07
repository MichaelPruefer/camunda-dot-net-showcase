using System;
using System.Collections.Generic;
using CamundaClient.Dto;
using CamundaClient.Worker;

namespace Urlaubsantrag.adapter
{
    [ExternalTaskTopic("leaveAccount")]
    [ExternalTaskVariableRequirements("name", "duration")]
    class BookOnLeaveAccountAdapter : IExternalTaskAdapter
    {

        public void Execute(ExternalTask externalTask, ref Dictionary<string, object> resultVariables)
        {
            Console.WriteLine("OK - Urlaubsanspruch von " + externalTask.Variables["name"].Value + " reduziert um " + externalTask.Variables["duration"].Value + " Tage");

            resultVariables.Add("SomeResult", "Irgendwas, was wir uns merken wollen");
        }

    }

}
