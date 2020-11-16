using System;
using System.Configuration;
using Worker.Jobs;

namespace Worker.Core
{
#nullable disable
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
                    case "RequestBreedingValues":
                        Type = typeof(RequestBreedingValuesJob);
                        break;
                    //case "UpdateAnimalData":
                    //    Type = typeof(UpdateAnimalDataJob);
                    //    break;
                    //case "UpdateTriggers":
                    //    Type = typeof(UpdateTriggersJob);
                    //    break;
                    default:
                        throw new ConfigurationErrorsException("Unknow Jobname");
                }

                _name = value;
            }
        }

        public string Cron { get; set; }

        public Type Type { get; private set; }
    }
#nullable restore
}
