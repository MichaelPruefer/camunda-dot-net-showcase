using System;
using System.Collections.Generic;
using CamundaClient.Dto;
using CamundaClient.Worker;

namespace Urlaubsantrag.adapter
{
    [ExternalTaskTopic("informColleagues")]
    [ExternalTaskVariableRequirements("name", "startDate", "duration")]
    class InformColleaguesAdapter : IExternalTaskAdapter
    {

        public void Execute(ExternalTask externalTask, ref Dictionary<string, object> resultVariables)
        {
            Console.WriteLine(externalTask.Variables["name"].Value + " geht für " + externalTask.Variables["duration"].Value + " Tage in Urlaub");
        }

    }

}
