namespace Natom.Extensions.MQ.WorkerUtilities.Config
{
    public class ProcessConfig
    {
        public int MinIntervalMS { get; set; }
        public int MsgReadingQuantity { get; set; }
        public int Threads { get; set; }
    }
}