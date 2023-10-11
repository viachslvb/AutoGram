using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AutoGram
{
    class RandomUserData
    {
        public string FirstName;
        public string LastName;
        public string UserName;
        public string Password;
        public Email Email;

        private static readonly List<string> FirstNameList = new List<string>();
        private static readonly List<string> LastNameList = new List<string>();

        private static readonly string FileLastNames;
        private static readonly string FileFirstNames;

        private static readonly List<string> EmailSuffix = new List<string>
        {
            "@gmail.com", "@yahoo.com", "@hotmail.com", "@icloud.com", "@outlook.com",
            "@ukr.net", "@i.ua"
        };

        static RandomUserData()
        {
            FileLastNames = Variables.FolderRegisterModule + "/" + Variables.FileNames;
            FileFirstNames = Variables.FolderRegisterModule + "/" + Variables.FileFirstNames;
            ImportNames();
        }

        private static void ImportNames()
        {
            string[] lastNames = File.ReadAllLines(FileLastNames);
            foreach (var lastName in lastNames.Where(x => x != string.Empty))
            {
                LastNameList.Add(lastName);
            }

            var firstNames = File.ReadAllLines(FileFirstNames);
            foreach (var firstName in firstNames.Where(x => x != string.Empty))
            {
                FirstNameList.Add(firstName);
            }
        }

        public static RandomUserData Get(bool usernameForSuggestions = false)
        {
            var firstName = Settings.IsAdvanced &&
                            !Settings.Advanced.Register.RandomName
                ? Settings.Advanced.Register.RegistrationName
                : FirstNameList[Utils.Random.Next(FirstNameList.Count)];

            var randomUser = new RandomUserData
            {
                FirstName = firstName,
                LastName = LastNameList[Utils.Random.Next(LastNameList.Count)],
                Password = Utils.CreatePassword()
            };

            var email = $"{randomUser.FirstName.ToLower()}.{randomUser.LastName.ToLower()}{Utils.GetRandomNumber(5, 6)}";
            var emailSuffix = Settings.IsAdvanced && Settings.Advanced.Register.RandomizeEmails
                ? EmailSuffix[Utils.Random.Next(EmailSuffix.Count)]
                : "@gmail.com";

            /* Old method 
            randomUser.UserName = Settings.Advanced.Register.RandomizeUsernames
                ? CreateUsername(randomUser)
                : Settings.Advanced.Register.UseRealUsernames
                    ? CreateRealUsername(randomUser)
                    : email;
            */

            randomUser.UserName = GenerateUniqueUsername(firstName);


            if (usernameForSuggestions)
            {
                randomUser.UserName = $"{randomUser.FirstName}_{randomUser.LastName}";
            }

            randomUser.Email = new Email(
                $"{email}{emailSuffix}",
                $"{Utils.GetRandomNumber(6, 8)}"
                );

            return randomUser;
        }

        private static string GenerateUniqueUsername(string firstName)
        {
            Random random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            string randomizedUsername = new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return firstName + randomizedUsername;
        }

        private static string CreateRealUsername(RandomUserData userData)
        {
            string username = string.Empty;

            username = userData.FirstName.ToLower();

            int letterIndexForRepeating = Utils.Random.Next(0, username.Length);
            string letterForRepeating = username[letterIndexForRepeating].ToString();

            string pre = username.Substring(0, letterIndexForRepeating);
            string post = username.Substring(letterIndexForRepeating);

            int r = Utils.Random.Next(1, 4);

            username = pre;
            for (var i = 0; i < r; i++)
                username += letterForRepeating;
            username += post;

            var consonantCharacters = new string[] { "b", "m", "n", "t", "k", "l", "c", "z", "p", "s", "r" };
            var vowelCharacters = new string[] { "a", "i", "e", "o" };

            string consonantCharacter = consonantCharacters[Utils.Random.Next(consonantCharacters.Length)];
            string cnsch = consonantCharacter;
            r = Utils.Random.Next(1, 3);

            for (var i = 0; i < r; i++)
            {
                consonantCharacter += cnsch;
            }

            string secondPart = vowelCharacters[Utils.Random.Next(vowelCharacters.Length)] +
                                consonantCharacter +
                                vowelCharacters[Utils.Random.Next(vowelCharacters.Length)];

            username = $"{username}.{secondPart}";

            return username;
        }

        private static string CreateUsername(RandomUserData userData)
        {
            int r = Utils.Random.Next(0, 6);

            string username = string.Empty;

            switch (r)
            {
                case 0:
                    username = $"{userData.FirstName.ToLower()}.{userData.LastName.ToLower()}{Utils.GetRandomNumber(3, 5)}";
                    break;
                case 1:
                    username = $"{userData.LastName.ToLower()}_{Utils.GetRandomNumber(3, 5)}";
                    break;
                case 2:
                    username = $"{Utils.WordsList[Utils.Random.Next(Utils.WordsList.Count)]}_{Utils.GetRandomNumber(4, 5)}";
                    break;
                case 3:
                    username = $"{Utils.WordsList[Utils.Random.Next(Utils.WordsList.Count)]}.{Utils.GetRandomNumber(4, 5)}";
                    break;
                case 4:
                    username = $"{userData.LastName.ToLower()}.{Utils.GetRandomNumber(3, 6)}";
                    break;
                case 5:
                    username = $"{userData.FirstName.ToLower()}_{userData.LastName.ToLower()}_{Utils.GetRandomNumber(3, 5)}";
                    break;
            }

            return username;
        }

        public static List<RandomContact> GetRandomContacts()
        {
            List<RandomContact> contacts = new List<RandomContact>();

            int max = Utils.Random.Next(12, 48);

            for (var i = 0; i < max; i++)
            {
                RandomUserData randomUserData = Get();
                RandomContact randomContact = new RandomContact()
                {
                    email_addresses = new List<string>() { Utils.GeneratePhoneNumber() },
                    phone_numbers = new List<string> { randomUserData.Email.Username },
                    first_name = randomUserData.FirstName
                };

                contacts.Add(randomContact);
            }

            contacts.Clear();

            return contacts;
        }
    }

    class RandomContact
    {
        public List<string> phone_numbers;
        public List<string> email_addresses;
        public string first_name;
    }
}
