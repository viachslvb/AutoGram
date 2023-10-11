using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Database;
using Newtonsoft.Json;

namespace AutoGram.Services
{
    public static class InstagramAppRepository
    {
        public static List<InstagramApp> Apps;

        private static readonly Random Random = new Random();

        static InstagramAppRepository()
        {
            Apps = new List<InstagramApp>();

            var dataInstagramApps = File.ReadAllText(Variables.FileInstagramApps);
            InstagramApp[] instagramApps = JsonConvert.DeserializeObject<InstagramApp[]>(dataInstagramApps);

            foreach (var instagramApp in instagramApps)
            {
                Apps.Add(instagramApp);
            }
        }

        public static InstagramApp Get()
        {
            return Apps[Random.Next(0, Apps.Count)];
        }

        public static InstagramApp GetByVersionCode(string versionCode)
        {
            return Apps.FirstOrDefault(a => a.Code == versionCode);
        }
    }
}
