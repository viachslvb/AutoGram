namespace AutoGram.Task
{
    class Live
    {
        public static void Do(Instagram.Instagram user)
        {
            if (Settings.Advanced.Live.SuggestedFriends.Use)
                SubTask.SuggestedFriends.Explore(user);

            if (Settings.Advanced.Live.FollowUsersOnStarting.Use)
                SubTask.FollowUsersList.Do(user);
        }
    }
}
