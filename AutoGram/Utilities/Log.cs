using System;
using System.IO;

namespace AutoGram
{
    public enum LogResource
    {
        General,
        JsonReader,
        Register,
        Login,
        ChangeProfilePicture,
        SuggestedFriend,
        Post,
        Comment,
        Live,
        Direct,
        BulkCommenting,
        Special
    }

    static class Log
    {
        private static readonly object LockAccess = new object();

        public static void Write(string data, LogResource logResource = LogResource.General)
        {
            lock (LockAccess)
            {
                try
                {
                    using (TextWriter textWriter = new StreamWriter(Variables.FileLog, true))
                    {
                        textWriter.WriteLine($"[{DateTime.Now}] [{logResource}] {data}");
                    }
                }
                catch (Exception e)
                {
                    //throw new Exception();
                }
            }
        }
    }
}
