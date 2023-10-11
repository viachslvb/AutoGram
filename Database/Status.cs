namespace Database
{
    public class Status
    {
        public int Id { get; set; }
        public int Accounts { get; set; }
        public int Success { get; set; }
        public int Failure { get; set; }

        public AndroidDevice AndroidDevice { get; set; }
    }
}
