using System;
using System.IO;
using System.Text;
using System.Windows;
using AutoGram.Storage.Model;
using Newtonsoft.Json;

namespace AutoGram
{
    class Settings
    {
        public static BasicSettings Basic { get; private set; }
        public static AdvancedSettings Advanced { get; private set; }
        public static bool IsAdvanced { get; private set; }

        static Settings()
        {
            Load();
        }

        private static void Load()
        {
            try
            {
                Basic = JsonConvert.DeserializeObject<BasicSettings>(File.ReadAllText(Variables.FileSettings));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            try
            {
                Advanced = JsonConvert.DeserializeObject<AdvancedSettings>(File.ReadAllText(Variables.FileAdvancedSettings));
                IsAdvanced = true;
            }
            catch (Exception)
            {
                IsAdvanced = false;
            }
        }

        public static void Save()
        {
            var serialize = JsonConvert.SerializeObject(Basic, Formatting.Indented);
            File.WriteAllText(Variables.FileSettings, serialize, Encoding.UTF8);
        }
    }
}
