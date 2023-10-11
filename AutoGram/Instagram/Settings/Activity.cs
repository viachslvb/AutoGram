using System;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Settings
{
    class Activity
    {
        [JsonProperty("Started at")]
        public readonly string StartedAt;

        [JsonProperty("Finished at")]
        public string FinishedAt;

        public string Label;

        public Proxy Proxy;

        public SentStats Sent;

        public ActionsStats Actions;

        public DirectStats Direct;

        public SessionStats Session;

        public string Status;

        public int TimeStamp;

        public Activity(Proxy proxy)
        {
            StartedAt = DateTime.Now.ToString();
            Proxy = proxy;
            Label = AutoGram.Settings.Basic.General.BindLabel;
            Sent = new SentStats();
            Actions = new ActionsStats();
            Direct = new DirectStats();
            Session = new SessionStats();
        }

        public void Update()
        {
            FinishedAt = DateTime.Now.ToString();
            UpdateTimeStamp();
        }

        private void UpdateTimeStamp()
        {
            this.TimeStamp = Utils.DateTimeNowTotalSeconds;
        }
    }

    class SessionStats
    {
        public int LoopCounter;
        public int CommentingTaskExecutionCounter;
    }

    class ActionsStats
    {
        public int Likes;
        public int Follows;
        public int ExploreProfiles;
    }

    class DirectStats
    {
        public int PendingRequests;
        public int UnseenMessages;
        public int StartedDialogs;
        public int FinishedDialogs;
    }

    class SentStats
    {
        public int Total;
        public int Success;
        public int Error;
    }
}
