namespace Natom.Extensions.MQ.WorkerUtilities.Config
{
    public class WorkerConfig
    {
        public string Name { get; set; }
        public string InstanceName { get; set; }
        public ProcessConfig Process { get; set; }
    }
}