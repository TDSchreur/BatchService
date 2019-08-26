using System;
using System.Configuration;
using Worker.Jobs;

namespace Worker.Core
{
    public class JobSchedule
    {
        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                switch (value)
                {
                    case "SubmitCommandsJob":
                        Type = typeof(SubmitCommandsJob);
                        break;
                    case "GetCommandsJob":
                        Type = typeof(GetCommandsJob);
                        break;
                    case "UpdateTriggerJob":
                        Type = typeof(UpdateTriggerJob);
                        break;
                    default:
                        throw new ConfigurationErrorsException("Unknow Jobname");
                }

                _name = value;
            }
        }

        public string Cron { get; set; }

        public Type Type { get; set; }
    }
}
