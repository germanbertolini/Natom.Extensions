namespace Natom.Extensions.MQ.WorkerUtilities.Config
{
    public class QueueConfig
    {
        public string QueueName { get; set; }
        public string Exchange { get; set; }
        public string RoutingKey { get; set; }
    }
}