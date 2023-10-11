using Database.DirectSender;

namespace AutoGram.Instagram.Response.Model
{
    public class UserDirectData : UserDirect
    {
        public string SourcePk { get; set; }

        public string SourceUsername { get; set; }
    }
}