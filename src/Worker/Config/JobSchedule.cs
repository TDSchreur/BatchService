using System;
using System.Configuration;
using Worker.Jobs;

namespace Worker.Config
{
    public class JobSchedule
    {
        public string Name { get; set; }
        public string Cron { get; set; }

        public Type Type
        {
            get
            {
                switch (Name)
                {
                    case "HelloWorldJob":
                        return typeof(HelloWorldJob);
                    case "SecondJob":
                        return typeof(SecondJob);
                    case "UpdateTriggerJob":
                        return typeof(UpdateTriggerJob);
                    default:
                        throw new ConfigurationErrorsException("Unknow Jobname");
                }
            }
        }
    }
}
