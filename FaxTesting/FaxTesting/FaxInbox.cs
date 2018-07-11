namespace FaxTesting
{
    public class FaxInbox
    {
        public string Status { get; set; }
        public FaxDetails[] Result { get; set; }
    }

    public class FaxDetails
    {
        public string FileName { get; set; }
        public string ReceiveStatus { get; set; }
        public string Date { get; set; }
        public string EpochTime { get; set; }
        public string CallerID { get; set; }
        public string RemoteID { get; set; }
        public int Pages { get; set; }
        public int Size { get; set; }
        public string ViewedStatus { get; set; }
    }
}
