namespace FaxTesting
{
    public class FaxStatus
    {
        public string Status { get; set; }
        public FaxResult Result { get; set; }
    }

    public class FaxResult
    {
        public string FileName { get; set; }
        public string SentStatus { get; set; }
        public string DateQueued { get; set; }
        public string DateSent { get; set; }
        public string EpochTime { get; set; }
        public string ToFaxNumber { get; set; }
        public string Pages { get; set; }
        public string Duration { get; set; }
        public string RemoteID { get; set; }
        public string ErrorCode { get; set; }
        public string Size { get; set; }
        public string AccountCode { get; set; }
    }
}
