using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace AutoGram
{
    class MediaObject
    {
        public string Path;
        public string Caption;
        public string Comment;

        public int Width;
        public int Height;

        public byte[] Image;
        public byte[] Video;

        public static List<string> SmilesList;

        private static Dictionary<String, String> _symbolsCyrilDictionary;
        private static List<string> _separatorsList;
        private static Queue<string> _tagsQueue = new Queue<string>();
        private static Queue<string> _randomTagsQueue = new Queue<string>();

        private static readonly object TagLock = new object();

        private bool _isCaptionForProfileUrl;

        static MediaObject()
        {
            FillSymbolsCyrilDictionary();

            FillSmilesList();

            FillSeparatorsList();

            FillRandomTags();
        }

        public MediaObject(string path, bool isProfileUrl = false)
        {
            this.Path = path;
            _isCaptionForProfileUrl = isProfileUrl;
        }

        private static void FillRandomTags()
        {
            var randomTags = Settings.Basic.Hashtags.AddFromFileAdditionalTags
                ? File.ReadAllText(Variables.FileAdditionalTags).Split(' ').Distinct().ToArray()
                : Settings.Basic.User.AdditionalHashtags.Split(' ').Distinct().ToArray();

            randomTags.Shuffle();

            _randomTagsQueue = new Queue<string>(randomTags);
        }

        private static void FillSymbolsCyrilDictionary()
        {
            _symbolsCyrilDictionary = new Dictionary<string, string>
            {
                {"a", "а"},
                {"e", "е"},
                {"o", "о"},
                {"i", "і"},
                {"c", "с"},
                {"p", "р"},
                {"h", "н"},
                {"m", "м"},
                {"x", "х"},
                {"K", "К"},
                {"I", "1"}
            };
        }

        private static void FillSeparatorsList()
        {
            _separatorsList = new List<string> { "-", "=", ",", ".", "_", "*", ".", "+" };
        }

        private static void FillSmilesList()
        {
            SmilesList = Settings.Basic.Text.Smiles.Split(' ').ToList();
        }

        private static bool UseIt(int range = 2)
        {
            return Utils.Random.Next(0, range) == 0;
        }

        private static Queue<string> GetTags(bool unicMode, int count, int randomCount, Queue<string> currentQueue = null)
        {
            Queue<string> tagsQueue = currentQueue ?? new Queue<string>();

            if(!_randomTagsQueue.Any())
                FillRandomTags();

            if (unicMode)
            {
                if (!_tagsQueue.Any())
                {
                    var tags = Settings.Basic.User.Hashtags.Split(' ').Distinct().ToArray();
                    tags.Shuffle();

                    _tagsQueue = new Queue<string>(tags);
                }

                while (_tagsQueue.Any())
                {
                    if (tagsQueue.Count >= count) break;

                    tagsQueue.Enqueue(_tagsQueue.Dequeue());
                }

                if (tagsQueue.Count < count)
                {
                    GetTags(unicMode, count, randomCount, tagsQueue);
                }
            }
            else
            {
                var tags = Settings.Basic.User.Hashtags.Split(' ').Distinct().ToArray();
                tags.Shuffle();

                _tagsQueue = new Queue<string>(tags);

                while (_tagsQueue.Any() && tagsQueue.Count < count)
                {
                    tagsQueue.Enqueue(_tagsQueue.Dequeue());
                }
            }

            if (randomCount > 0)
            {
                int addedRandomTags = 0;

                while (addedRandomTags < randomCount)
                {
                    string randomTag = Settings.Basic.Hashtags.AddFromAdditionalHashtags || Settings.Basic.Hashtags.AddFromFileAdditionalTags
                        ? _randomTagsQueue.Any()
                            ? _randomTagsQueue.Dequeue()
                            : "#" + Utils.WordsList[Utils.Random.Next(Utils.WordsList.Count)]
                        : "#" + Utils.WordsList[Utils.Random.Next(Utils.WordsList.Count)];

                    tagsQueue.Enqueue(randomTag);
                    addedRandomTags++;
                }

                var tagsArray = tagsQueue.ToArray();
                tagsArray.Shuffle();

                tagsQueue = new Queue<string>(tagsArray);
            }

            return tagsQueue;
        }

        private (Queue<string> Tags, int MaxTags, int TagsInSentence) GetTags()
        {
            int maxTags = Settings.Basic.Debug.RandomHashtagsCount
                ? Utils.Random.Next(18, 26)
                : Settings.Basic.Limit.Hashtags;

            if (Settings.IsAdvanced
                && Settings.Advanced.Hashtag.UseRandomLimit)
            {
                maxTags = Utils.Random.Next(Settings.Advanced.Hashtag.From, Settings.Advanced.Hashtag.To);
            }

            int tagsInSentence = maxTags > 20
                ? Utils.Random.Next(3, 5)
                : Utils.Random.Next(2, 4);

            int percentageOfRandomhHashtags = Settings.Basic.Hashtags.PercentageOfRandomHashtags > 100 ||
                                              Settings.Basic.Hashtags.PercentageOfRandomHashtags < 0
                ? 10
                : Settings.Basic.Hashtags.PercentageOfRandomHashtags;

            int randomHashtagsCount = Settings.Basic.Hashtags.AddRandomHashtags
                ? Utils.GetPercentage(maxTags, percentageOfRandomhHashtags)
                : 0;

            Queue<string> tagsQueue;

            lock (TagLock) 
                tagsQueue = GetTags(false, maxTags - randomHashtagsCount, randomHashtagsCount);

            return (tagsQueue, maxTags, tagsInSentence);
        }

        private static string ChangeSymbols(string str)
        {
            string strStepOne = string.Empty;

            // Step #1 | Replace latin symbols to cyril
            foreach (var c in str.ToCharArray())
            {
                string s = c.ToString();

                if (!UseIt() || s == Variables.MessengerTemplate)
                {
                    strStepOne += s;
                    continue;
                }

                string n = string.Empty;

                if (_symbolsCyrilDictionary.TryGetValue(s, out n))
                    strStepOne += n;
                else
                    strStepOne += s;
            }

            // Step #2 | Random duplicate characters
            string strStepTwo = string.Empty;

            if (Settings.Basic.Text.DuplicateCharacters)
            {
                foreach (var w in strStepOne.Split(' '))
                {
                    var r = 0;

                    foreach (var c in w.ToCharArray())
                    {
                        string s = c.ToString();

                        if (s == Variables.MessengerTemplate || s == "I" || s == "1")
                        {
                            strStepTwo += s;
                            continue;
                        }

                        strStepTwo += s;

                        if (UseIt(8) && r < 1)
                        {
                            strStepTwo += s;
                            r++;
                        }
                    }

                    strStepTwo += " ";
                }
            }
            else
            {
                strStepTwo = strStepOne;
            }

            // Step #3 | Push smiles between words
            string strStepThree = string.Empty;
            int smilesCount = 0;

            SmilesList.Shuffle();
            Queue<string> smilesQueue = new Queue<string>(SmilesList);

            if (Settings.Basic.Text.SmileInPhotoDesc)
            {
                foreach (var word in strStepTwo.Split(' '))
                {
                    if (UseIt(3) && smilesCount < 2)
                    {
                        strStepThree += smilesQueue.Dequeue() + " ";
                        smilesCount++;
                    }

                    strStepThree += word + " ";
                }

                if (smilesCount < 2)
                {
                    while (smilesCount < 2)
                    {
                        strStepThree += smilesQueue.Peek();
                        smilesCount++;
                    }
                }
                else
                {
                    strStepThree += smilesQueue.Peek();
                    if (UseIt()) strStepThree += smilesQueue.Peek();
                    if (UseIt(5)) strStepThree += smilesQueue.Peek();
                }

                if (strStepThree.Substring(strStepThree.Length - 1, 1) == " ")
                {
                    strStepThree = strStepThree.Remove(strStepThree.Length - 1);
                }
            }
            else
            {
                strStepThree = strStepTwo;

                if (UseIt()) strStepThree = smilesQueue.Dequeue() + " " + strStepThree;
                else strStepThree = strStepThree + " " + smilesQueue.Dequeue();
            }

            return strStepThree;
        }

        protected void UpdateSize()
        {
            using (var ms = new MemoryStream(this.Image))
            {
                var bmp = new Bitmap(ms);
                this.Width = bmp.Width;
                this.Height = bmp.Height;
            }
        }

        public static string GetMessengerLogin()
        {
            var messengers = Settings.Basic.General.Messenger;

            if (messengers.Length == 2)
                return Utils.Random.Next(0, 2) == 0 ? messengers[0] : messengers[1];

            return messengers.Length > 2 ? messengers[Utils.Random.Next(0, messengers.Length)] : messengers[0];
        }

        private string GetTargetCaption()
        {
            var photoDescription = Settings.IsAdvanced
                                   && Settings.Advanced.Profile.UseAdvancedDescriptions
                ? !_isCaptionForProfileUrl
                    ? Settings.Advanced.Profile.PhotoDescriptions[0]
                    : Settings.Advanced.Profile.PhotoDescriptions[1]
                : Settings.Basic.User.PhotoDescription;

            var captions = photoDescription.Split('|');
            var caption = captions[Utils.Random.Next(0, captions.Length - 1)];

            if (Settings.Basic.Text.ChangeSymbols)
                caption = ChangeSymbols(caption);

            caption = caption.Replace(Variables.MessengerTemplate, GetMessengerLogin());

            return caption;
        }

        private string GetSeparator()
        {
            string separator = string.Empty;

            var separatorsCount =
                    Settings.Advanced.Post.Content.Caption.SeparatorsRandomLimit.UseRandomLimit
                        ? Utils.Random.Next(Settings.Advanced.Post.Content.Caption.SeparatorsRandomLimit.From,
                            Settings.Advanced.Post.Content.Caption.SeparatorsRandomLimit.To)
                        : Settings.Advanced.Post.Content.Caption.SeparatorsLimit;

            for (var i = 0; i < separatorsCount; i++)
            {
                separator += ".\n";
            }

            return separator;
        }

        private string GetTagsSegment(Queue<string> tags, int maxTags, int tagsInSentence, bool randomizeWithText)
        {
            int tagCounter = 0;
            string tagsSegment = string.Empty;

            if (Settings.Basic.Hashtags.AddMarkTag
                && !string.IsNullOrEmpty(Settings.Basic.User.MarkTag))
            {
                if (tags.Count < 30)
                {
                    tags.Enqueue(Settings.Basic.User.MarkTag);
                    tagCounter = -1;
                }
            }

            while (tags.Any() && tagCounter < maxTags)
            {
                string sentence = Utils.SentencesList[Utils.Random.Next(0, Utils.SentencesList.Count - 1)];

                string[] words = sentence.Split(' ');

                string modifiedSentence = string.Empty;

                var i = 0;
                var s = 0;

                foreach (var word in words)
                {
                    if (s != 0) modifiedSentence += " ";

                    if (UseIt() && tags.Any() && i < tagsInSentence && s != 0 && tagCounter < maxTags)
                    {
                        modifiedSentence += tags.Dequeue() + " ";
                        tagCounter++;
                        i++; 
                    }

                    if (!randomizeWithText)
                        continue;

                    modifiedSentence += word;

                    s++;
                }

                if (i < tagsInSentence)
                {
                    while (i < tagsInSentence && tags.Any() && tagCounter < maxTags)
                    {
                        modifiedSentence += " " + tags.Dequeue();
                        tagCounter++;
                        i++;
                    }
                }

                if (randomizeWithText)
                    modifiedSentence += ". ";

                tagsSegment += modifiedSentence;
            }

            return tagsSegment;
        }

        private string GetRandomText()
        {
            string randomText = string.Empty;

            for (var i = 0; i < Utils.Random.Next(1, 5); i++)
            {
                string sentence = Utils.SentencesList[Utils.Random.Next(0, Utils.SentencesList.Count - 1)];

                randomText += sentence + ". ";
            }

            return randomText;
        }

        protected void UpdateCaption()
        {
            if (!Settings.Advanced.Post.Content.UseAdvancedSettings)
            {
                UpdateCaptionObsolete();
                return;
            }

            /* CAPTION SETTINGS */

            var caption = new StringBuilder();

            // Target caption
            if (Settings.Advanced.Post.Content.Caption.WithTargetCaption)
            {
                caption.Append(GetTargetCaption());
                caption.Append("\n");
            }

            // Separator
            if (Settings.Advanced.Post.Content.Caption.UseSeparator)
            {
                caption.Append(GetSeparator());
            }

            // Tags settings
            (Queue<string> Tags, int MaxTags, int TagsInSentence) tagsSettings = GetTags();

            if (Settings.Advanced.Post.Content.Caption.AddTags)
            {
                int tagsInCaption = tagsSettings.MaxTags;

                if (Settings.Advanced.Post.Content.AddComment)
                {
                    tagsInCaption =
                        Settings.Advanced.Post.Content.Caption.TagsRandomLimit.UseRandomLimit
                            ? Utils.Random.Next(Settings.Advanced.Post.Content.Caption.TagsRandomLimit.From,
                                Settings.Advanced.Post.Content.Caption.TagsRandomLimit.To)
                            : Settings.Advanced.Post.Content.Caption.TagsLimit;

                    tagsSettings.MaxTags = tagsSettings.MaxTags - tagsInCaption;
                }

                string tagsSegment = GetTagsSegment(tagsSettings.Tags, tagsInCaption, tagsSettings.TagsInSentence, 
                    Settings.Advanced.Post.Content.Caption.TagsWithRandomText);

                caption.Append(tagsSegment);
            }

            if (Settings.Advanced.Post.Content.Caption.OnlyRandomText)
            {
                caption.Clear();
                caption.Append(GetRandomText());
            }

            if (Settings.Advanced.Post.Content.Caption.EmptyCaption)
            {
                caption.Clear();
            }

            this.Caption = caption.ToString();

            /* COMMENT SETTINGS */

            if (Settings.Advanced.Post.Content.AddComment)
            {
                this.Comment = GetTagsSegment(tagsSettings.Tags, tagsSettings.MaxTags, tagsSettings.TagsInSentence,
                    Settings.Advanced.Post.Content.Comment.TagsWithRandomText);
            }
        }

        protected void UpdateCaptionObsolete()
        {
            // Settings
            //bool separateDescByNewLine = true;
            bool simpleSeparate = false;
            bool dotSeparator = true;
            bool sameSeparator = false;

            var photoDescription = Settings.IsAdvanced
                                   && Settings.Advanced.Profile.UseAdvancedDescriptions
                ? !_isCaptionForProfileUrl
                    ? Settings.Advanced.Profile.PhotoDescriptions[0]
                    : Settings.Advanced.Profile.PhotoDescriptions[1]
                : Settings.Basic.User.PhotoDescription;


            var captions = photoDescription.Split('|');
            var caption = captions[Utils.Random.Next(0, captions.Length - 1)];

            if (Settings.Basic.Text.ChangeSymbols)
                caption = ChangeSymbols(caption);

            // Set Messenger Login
            caption = caption.Replace(Variables.MessengerTemplate, GetMessengerLogin());
            caption += "\n";

            string captionHeadDesc = string.Empty;

            if (Settings.Basic.Debug.OnlyHashtagCaption || Settings.Basic.Debug.DisablePhotoDesc)
                caption = "";

            if (Settings.Advanced.Post.Content.Comment.HideComment)
                captionHeadDesc = caption;

            if (Settings.Basic.Debug.EmptyCaption)
            {
                this.Caption = "";
                return;
            }

            if (Settings.Basic.Debug.OnlyPhotoDescCaption)
            {
                this.Caption = caption;
                return;
            }

            // Add random separators to desc
            var countDotes = Utils.CreateRandomNumb(10, 14);

            countDotes = simpleSeparate ? 1 : countDotes;

            // With unique sep
            string separator = _separatorsList[Utils.Random.Next(0, _separatorsList.Count - 1)];
            for (var i = 0; i < countDotes; i++)
            {
                caption += dotSeparator ?
                    ".\n" :
                    sameSeparator ?
                        separator + "\n" :
                        _separatorsList[Utils.Random.Next(0, _separatorsList.Count - 1)] + "\n";
            }

            if (Settings.Advanced.Post.Content.Comment.HideComment)
                caption = string.Empty;

            if (Settings.Basic.Debug.OnlyHashtagCaption || Settings.Basic.Debug.DisablePhotoDesc)
                caption = "";

            this.Caption = caption;

            // Tags counter
            var t = 0;

            // Smiles counter
            var sc = 0;

            int maximumHashtags = Settings.Basic.Debug.RandomHashtagsCount
                ? Utils.Random.Next(18, 26)
                : Settings.Basic.Limit.Hashtags;

            if (Settings.IsAdvanced
                && Settings.Advanced.Hashtag.UseRandomLimit)
            {
                maximumHashtags = Utils.Random.Next(Settings.Advanced.Hashtag.From, Settings.Advanced.Hashtag.To);
            }

            int hashtagsInSentence = maximumHashtags > 20
                ? Utils.Random.Next(3, 5)
                : Utils.Random.Next(2, 4);

            int percentageOfRandomhHashtags = Settings.Basic.Hashtags.PercentageOfRandomHashtags > 100 ||
                                              Settings.Basic.Hashtags.PercentageOfRandomHashtags < 0
                ? 10
                : Settings.Basic.Hashtags.PercentageOfRandomHashtags;

            int randomHashtagsCount = Settings.Basic.Hashtags.AddRandomHashtags
                ? Utils.GetPercentage(maximumHashtags, percentageOfRandomhHashtags)
                : 0;

            Queue<string> tagsQueue;

            if (Settings.IsAdvanced)
            {
                lock (TagLock) tagsQueue = GetTags(true, maximumHashtags - randomHashtagsCount, randomHashtagsCount);
            }
            else
            {
                tagsQueue = GetTags(false, maximumHashtags - randomHashtagsCount, randomHashtagsCount);
            }

            // Random Settings
            if (Settings.Basic.Debug.UseRandomSettings)
            {
                if (Settings.Advanced.Post.Content.Comment.HideComment)
                {
                    if (Settings.Basic.Debug.SmilesInCaptionHidingMode)
                    {
                        this.Caption = "";

                        for (var i = 0; i < Utils.Random.Next(1, 5); i++)
                            this.Caption += SmilesList[Utils.Random.Next(0, SmilesList.Count - 1)];
                    }

                    if (Settings.Basic.Debug.RandomSettingsEmptyCaptionHidingMode)
                    {
                        this.Caption = "";
                    }
                }

                string hashtagCaption = "";

                int randomMode = Utils.Random.Next(6);
                int s;

                switch (randomMode)
                {
                    case 0:
                        while (tagsQueue.Any() && t < maximumHashtags)
                        {
                            hashtagCaption += tagsQueue.Dequeue() + " " + Utils.CreateRandomText(6, 7, true, true) + "\n";

                            t++;
                        }
                        break;
                    case 1:
                        while (tagsQueue.Any() && t < maximumHashtags)
                        {
                            hashtagCaption += tagsQueue.Dequeue() + " " + Utils.CreateRandomText(6, 7, true, true) + " ";

                            if (UseIt()) hashtagCaption += "\n";

                            t++;
                        }
                        break;
                    case 2:
                        while (tagsQueue.Any() && t < maximumHashtags)
                        {
                            hashtagCaption += tagsQueue.Dequeue() + " " + Utils.CreateRandomText(6, 7, true, true) + " ";
                            t++;
                        }
                        break;
                    case 3:
                        s = maximumHashtags / 3;

                        while (tagsQueue.Any() && t < maximumHashtags)
                        {
                            hashtagCaption += tagsQueue.Dequeue() + " " + Utils.CreateRandomText(6, 7, true, true) + " ";

                            if (s == t)
                            {
                                s = s + s;
                                hashtagCaption += "\n";
                                if (UseIt()) hashtagCaption += "\n";
                            }

                            t++;
                        }
                        break;
                    case 4:
                        s = maximumHashtags / 2;

                        while (tagsQueue.Any() && t < maximumHashtags)
                        {
                            hashtagCaption += tagsQueue.Dequeue() + " " + Utils.CreateRandomText(6, 7, true, true) + " ";

                            if (UseIt(5)) hashtagCaption += "\n";

                            if (s == t) hashtagCaption += "\n";

                            t++;
                        }
                        break;
                    case 5:
                        while (tagsQueue.Any() && t < maximumHashtags)
                        {
                            hashtagCaption += tagsQueue.Dequeue() + " " + Utils.CreateRandomText(6, 7, true, true) + " ";

                            t++;
                        }
                        break;
                }


                if (Settings.Advanced.Post.Content.Comment.HideComment)
                    this.Comment = hashtagCaption;
                else
                    this.Caption = hashtagCaption;
            }
            else
            {
                if (Settings.Basic.Text.UseRandomCharacters)
                {
                    string randomizingHashtagsCaption = string.Empty;

                    while (tagsQueue.Any() && t < maximumHashtags)
                    {
                        randomizingHashtagsCaption += tagsQueue.Dequeue() + " " + Utils.CreateRandomText(6, 7, true, true) + " ";
                        t++;
                    }

                    this.Caption += randomizingHashtagsCaption;
                }
                else
                {
                    while (tagsQueue.Any() && t < maximumHashtags)
                    {
                        string sentence = Utils.SentencesList[Utils.Random.Next(0, Utils.SentencesList.Count - 1)];

                        string[] words = sentence.Split(' ');

                        string modifiedSentence = string.Empty;

                        var i = 0;

                        var s = 0;
                        foreach (var word in words)
                        {
                            if (s != 0) modifiedSentence += " ";

                            if (UseIt() && tagsQueue.Any() && i < hashtagsInSentence && s != 0 && t < maximumHashtags)
                            {
                                modifiedSentence += tagsQueue.Dequeue() + " ";
                                t++;
                                i++;
                            }

                            if (Settings.Basic.Debug.OnlyHashtagCaption
                                || Settings.Basic.Debug.HashtagDescWithoutRandomizing) continue;

                            if (Settings.Basic.Text.UseSmiles && Settings.Basic.Text.SmileInPhotoRandomText && UseIt(10) && sc < 3 && s != 0)
                            {
                                modifiedSentence += SmilesList[Utils.Random.Next(0, SmilesList.Count - 1)] + " ";
                                sc++;
                            }

                            modifiedSentence += word;

                            s++;
                        }

                        if (i < hashtagsInSentence)
                        {
                            while (i < hashtagsInSentence && tagsQueue.Any() && t < maximumHashtags)
                            {
                                modifiedSentence += " " + tagsQueue.Dequeue();
                                t++;
                                i++;
                            }
                        }

                        if (!Settings.Basic.Debug.OnlyHashtagCaption
                                && !Settings.Basic.Debug.HashtagDescWithoutRandomizing)
                            modifiedSentence += ". ";

                        this.Caption += modifiedSentence;
                    }
                }
            }


            // Settings for postAfterRegistration mode
            bool isOnlyRandomText = Settings.IsAdvanced && Settings.Advanced.PostAfterRegistration.Use
                && Settings.Advanced.PostAfterRegistration.IsOnlyRandomTextCaption
                || Settings.Basic.Debug.OnlyRandomTextCaption;

            if (isOnlyRandomText)
            {
                this.Caption = "";

                for (var i = 0; i < Utils.Random.Next(1, 5); i++)
                {
                    string sentence = Utils.SentencesList[Utils.Random.Next(0, Utils.SentencesList.Count - 1)];

                    this.Caption += sentence + ". ";
                }
            }

            if (Settings.Basic.Debug.OnlySmilesCaption)
            {
                this.Caption = "";

                for (var i = 0; i < Utils.Random.Next(1, 5); i++)
                    this.Caption += SmilesList[Utils.Random.Next(0, SmilesList.Count - 1)];
            }

            if (Settings.Advanced.Post.Content.Comment.HideComment && !Settings.Basic.Debug.UseRandomSettings)
            {
                this.Comment = this.Caption;
                this.Caption = captionHeadDesc;
            }

            if (Settings.Basic.Debug.RandomTextInComment)
            {
                this.Comment = "";

                for (var i = 0; i < Utils.Random.Next(1, 5); i++)
                {
                    string sentence = Utils.SentencesList[Utils.Random.Next(0, Utils.SentencesList.Count - 1)];

                    this.Comment += sentence + ". ";
                }

                this.Caption = "";
            }
        }
    }
}
