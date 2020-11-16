namespace Worker.Controllers.Messages {
    public class JobModel
    {
        public JobModel(string name, string cron)
        {
            Name = name;
            Cron = cron;
        }

        public string Name { get; }

        public string Cron { get; }
    }
}
