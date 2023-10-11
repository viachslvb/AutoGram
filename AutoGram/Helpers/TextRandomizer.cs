using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoGram.Helpers
{
    static class TextRandomizer
    {
        public static List<string> SmilesList;
        private static Dictionary<String, String> _symbolsCyrilDictionary;

        static TextRandomizer()
        {
            FillSymbolsCyrilDictionary();
            FillSmilesList();
        }

        public static string Randomize(string text)
        {
            text = ChangeSymbols(text);
            text = text.Replace(Variables.MessengerTemplate, GetMessengerLogin());

            return text;
        }

        public static string GetMessengerLogin()
        {
            var messengers = Settings.Basic.General.Messenger;

            if (messengers.Length == 2)
                return Utils.Random.Next(0, 2) == 0 ? messengers[0] : messengers[1];

            return messengers.Length > 2 ? messengers[Utils.Random.Next(0, messengers.Length)] : messengers[0];
        }

        private static void FillSmilesList()
        {
            SmilesList = Settings.Basic.Text.Smiles.Split(' ').ToList();
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

        private static string ChangeSymbols(string str)
        {
            string strStepOne = string.Empty;

            // Step #1 | Replace latin symbols to cyril
            if (Settings.Basic.Text.ChangeSymbols)
            {
                foreach (var c in str.ToCharArray())
                {
                    string s = c.ToString();

                    if (!Utils.UseIt() || s == Variables.MessengerTemplate)
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
            }
            else
            {
                strStepOne = str;
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

                        if (Utils.UseIt(8) && r < 1)
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

            if (Settings.Basic.Text.UseSmiles)
            {
                foreach (var word in strStepTwo.Split(' '))
                {
                    if (Utils.UseIt(3) && smilesCount < 2)
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
                    if (Utils.UseIt()) strStepThree += smilesQueue.Peek();
                    if (Utils.UseIt(5)) strStepThree += smilesQueue.Peek();
                }

                if (strStepThree.Substring(strStepThree.Length - 1, 1) == " ")
                {
                    strStepThree = strStepThree.Remove(strStepThree.Length - 1);
                }
            }
            else
            {
                strStepThree = strStepTwo;

                if (Utils.UseIt()) strStepThree = smilesQueue.Dequeue() + " " + strStepThree;
                else strStepThree = strStepThree + " " + smilesQueue.Dequeue();
            }

            return strStepThree;
        }
    }
}
