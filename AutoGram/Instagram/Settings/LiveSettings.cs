namespace AutoGram.Instagram.Settings
{
    class LiveSettings
    {
        public FollowSettings Follow;
        public LikeSettings Like;
        public ExploreProfilesSettings ExploreProfiles;

        public LiveSettings()
        {
            Follow = new FollowSettings();
            Like = new LikeSettings();
            ExploreProfiles = new ExploreProfilesSettings();

            Follow.Limit = AutoGram.Settings.Advanced.Live.FollowSettings.RandomLimit.Use
                ? Utils.Random.Next(
                    AutoGram.Settings.Advanced.Live.FollowSettings.RandomLimit.From,
                    AutoGram.Settings.Advanced.Live.FollowSettings.RandomLimit.To)
                : AutoGram.Settings.Advanced.Live.FollowSettings.Limit;

            Like.Limit = AutoGram.Settings.Advanced.Live.LikeSettings.RandomLimit.Use
                ? Utils.Random.Next(
                    AutoGram.Settings.Advanced.Live.LikeSettings.RandomLimit.From,
                    AutoGram.Settings.Advanced.Live.LikeSettings.RandomLimit.To)
                : AutoGram.Settings.Advanced.Live.LikeSettings.Limit;

            ExploreProfiles.Limit = AutoGram.Settings.Advanced.Live.ExploreProfilesSettings.RandomLimit.Use
                ? Utils.Random.Next(
                    AutoGram.Settings.Advanced.Live.ExploreProfilesSettings.RandomLimit.From,
                    AutoGram.Settings.Advanced.Live.ExploreProfilesSettings.RandomLimit.To)
                : AutoGram.Settings.Advanced.Live.ExploreProfilesSettings.Limit;
        }
    }

    public class FollowSettings
    {
        public int Limit;
        public bool IsLimit;
    }

    public class LikeSettings
    {
        public int Limit;
        public bool IsLimit;
    }

    public class ExploreProfilesSettings
    {
        public int Limit;
        public bool IsLimit;
    }
}
