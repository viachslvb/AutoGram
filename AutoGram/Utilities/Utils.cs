using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Sodium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace AutoGram
{
    static class Utils
    {
        public static readonly Random Random = new Random();

        public static List<string> WordsList;
        public static List<string> SentencesList;
        
        static Utils()
        {
            WordsList = File.ReadAllLines(Variables.FileWords).Where(l => l != string.Empty).ToList();
            SentencesList = File.ReadAllLines(Variables.FileSentences).Where(l => l != string.Empty).ToList();
        }

        public static int DateTimeNowTotalSeconds => (int) (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        public static string DateTimeNowTotalSecondsWithMs => (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds.ToString("#.###", CultureInfo.GetCultureInfo("en-US"));

        public static string GenerateUUID(bool type)
        {
            object[] objects = new object[8];
            for (var i = 0; i < objects.Length; i++)
                objects[i] = Random.Next(0x3fff, 0xffff);

            var uuid = string.Format("{0,4:x}{1,4:x}-{2,4:x}-{3,4:x}-{4,4:x}-{5,4:x}{6,4:x}{7,4:x}", objects);

            return type ? uuid : uuid.Replace("-", "");
        }

        public static string GeneratePigeonSession()
        {
            object[] objects = new object[8];
            for (var i = 0; i < objects.Length; i++)
                objects[i] = Random.Next(0x3fff, 0xffff);

            var uuid = string.Format("{0,4:x}{1,4:x}-{2,4:x}-{3,4:x}-{4,4:x}-{5,4:x}{6,4:x}{7,4:x}", objects);

            return $"UFS-{uuid}-0";
        }

        public static string GenerateUUIDExtended(bool type)
        {
            string symbols = "qwertyuiopasdfghjklzxcvbnm1234567890";

            string guid = string.Empty;

            for (var i = 0; i < 8; i++)
                guid += symbols[Random.Next(0, symbols.Length)];

            guid += "-";

            for (var i = 0; i < 4; i++)
                guid += symbols[Random.Next(0, symbols.Length)];

            guid += "-";

            for (var i = 0; i < 4; i++)
                guid += symbols[Random.Next(0, symbols.Length)];

            guid += "-";

            for (var i = 0; i < 12; i++)
                guid += symbols[Random.Next(0, symbols.Length)];

            return type ? guid : guid.Replace("-", "");
        }

        public static string SecondsToHourMinutes(int secs)
        {
            int s = secs % 60;
            secs /= 60;
            int mins = secs % 60;
            int hours = secs / 60;
            return $"{hours:D2} h {mins:D2} m";
        }

        public static string GenerateDeviceID(string seed)
        {
            var deviceID = "";
            using (MD5 md5Hash = MD5.Create())
            {
                deviceID = GetMd5Hash(md5Hash, seed + DateTime.Now);
            }

            return "android-" + deviceID.Substring(0, 16);
        }

        private static string GetMd5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            StringBuilder sBuilder = new StringBuilder();

            foreach (byte t in data)
                sBuilder.Append(t.ToString("x2"));

            return sBuilder.ToString();
        }

        public static void CreateNewFile(string filePath)
        {
            if ((File.Exists(filePath)))
            {
                var fileInfo = new FileInfo(filePath);
                fileInfo.Delete();
            }

            var newFile = new FileStream(filePath, FileMode.CreateNew);
            newFile.Close();
        }

        public static void WriteToFile(string filePath, string value)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            {
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.WriteLine(value);
                }
            }
        }

        public static string GetSHA256(string data, string secret, bool request = false)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] keyBytes = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(data);
            HMACSHA256 cryptographer = new HMACSHA256(keyBytes);

            byte[] bytes = cryptographer.ComputeHash(messageBytes);

            string sha256 = BitConverter.ToString(bytes).Replace("-", "").ToLower();

            return request ? sha256 + "." + data : sha256;
        }

        public static string CreateRandomText(int lengthMin, int lengthMax, bool sub = false, bool subOned = false)
        {
            var rndVar = "";
            var count = Random.Next(lengthMin, lengthMax);
            string abc = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890";

            if (sub)
                abc = "qwertyuiopasdfghjklzxcvbnm";

            string digit = "0123456789";

            for (var i = 0; i < count; i++)
                rndVar += abc[Random.Next(abc.Length)];

            if (subOned)
                rndVar += digit[Random.Next(digit.Length)];

            return rndVar;
        }

        public static int CreateRandomNumb(int from, int to)
        {
            return Random.Next(from, to);
        }

        public static string GetRandomNumber(int lengthMin, int lengthMax)
        {
            var count = Random.Next(lengthMin, lengthMax);
            string randomNumber = "";
            for (var i = 0; i < count; i++)
                randomNumber += Random.Next(0, 9);

            return randomNumber;
        }

        public static string CreateBoundary()
        {
            return CreateRandomText(30, 30);
        }

        public static string GenerateBoundary()
        {
            string abc = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM";

            string Randomize(int length)
            {
                string r = string.Empty;

                for (var i = 0; i < length; i++)
                    r += abc[Random.Next(abc.Length)];

                return r;
            }

            return Randomize(5) + "_" + Randomize(2) + "_" + Randomize(21);
        }

        public static string GenerateUploadId()
        {
            var time = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

            return time + CreateRandomNumb(100, 999).ToString();
        }

        public static string EncodeNonAsciiCharacters(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                if (c > 127)
                {
                    // This character is too big for ASCII
                    string encodedValue = "\\u" + ((int)c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Encode(byte[] plain)
        {
            return System.Convert.ToBase64String(plain);
        }

        public static string Base64Decode(string encodedString)
        {
            byte[] data = Convert.FromBase64String(encodedString);
            string decodedString = Encoding.UTF8.GetString(data);

            return decodedString;
        }

        public static byte[] CreateSecureRandom()
        {
            using (var provider = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[24];
                provider.GetNonZeroBytes(bytes);

                return bytes;
            }
        }

        public static string Sha1(string input)
        {
            System.Security.Cryptography.SHA1 hash = System.Security.Cryptography.SHA1CryptoServiceProvider.Create();
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = hash.ComputeHash(plainTextBytes);

            return Base64Encode(hashBytes);
        }

        public static string GenerateUserBreadcrumb(double length)
        {
            var key = "iN4$aGr0m";
            var date = Math.Round((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds * 1000);

            var term = Random.Next(2, 3) * 1000 + length * Random.Next(15, 20) * 100;

            var textChangeEventCount = Math.Round(length / Random.Next(2, 3));

            if (textChangeEventCount == 0)
                textChangeEventCount = 1;

            var data = length + " " + term + " " + textChangeEventCount + " " + date;

            return data;
        }

        public static double RandomNumberBetween(double minValue, double maxValue)
        {
            var next = Random.NextDouble();

            return minValue + (next * (maxValue - minValue));
        }

        public static string TryParse(string input, string pattern)
        {
            Match match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
            return match.Value;
        }

        public static void FillData(ICollection<string> collection, string filePath)
        {
            foreach (var l in File.ReadAllLines(filePath).Where(l => l != string.Empty))
                collection.Add(l);
        }

        public static void exec(string input, string output)
        {
            var ffmpeg = new Process();

            ffmpeg.StartInfo.Arguments = " -i " + input + " -vframes 1 " + output;
            ffmpeg.StartInfo.FileName = Variables.FileVideoConvertor;
            ffmpeg.StartInfo.UseShellExecute = false;
            ffmpeg.StartInfo.RedirectStandardOutput = true;
            ffmpeg.StartInfo.RedirectStandardError = true;
            ffmpeg.StartInfo.CreateNoWindow = true;

            ffmpeg.Start();
            ffmpeg.WaitForExit(50);
            ffmpeg.Close();
        }

        public static void GetThumbnail(string video, string image)
        {
            exec(video, image);
        }

        public static string CreatePassword()
        {
            string lowerCaseChars = "qwertyuiopasdfghjklzxcvbnm";
            string upperCaseChars = "QWERTYUIOPASDFGHJKLZXCVBNM";
            string numericChars = "1234567890";

            string password = string.Empty;

            for (var i = 0; i < 3; i++)
                password += lowerCaseChars[Random.Next(lowerCaseChars.Length)];

            for (var i = 0; i < 2; i++)
                password += numericChars[Random.Next(numericChars.Length)];

            for (var i = 0; i < 3; i++)
                password += upperCaseChars[Random.Next(upperCaseChars.Length)];

            return password;
        }

        public static void EncodeFormat(string input, string output)
        {
            var ffmpeg = new Process
            {
                StartInfo =
                {
                    Arguments = "-i \"" + input + "\" -c:a aac -b:a 128k -c:v libx264 -crf 23 \"" + output + "\"",
                    FileName = Variables.FileVideoConvertor,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            ffmpeg.Start();
            ffmpeg.WaitForExit();
        }

        public static void RandomSleep(int from, int to)
        {
            Thread.Sleep(Random.Next(from, to));
        }

        public static bool UseIt(int range = 2)
        {
            return Utils.Random.Next(0, range) == 0;
        }

        public static string GeneratePhoneNumber()
        {
            string phoneNumber = UseIt() ? "38099" : "38097";
            phoneNumber += Random.Next(1000000, 9999999);

            return phoneNumber;
        }

        public static int GetPercentage(int all, int percent)
        {
            return percent * all / 100;
        }

        public static string GetEncryptedPassword(string pubKey, string pubKeyId, string password)
        {
            SecureRandom secureRandom = new SecureRandom();
            
            byte[] randKey = new byte[32];
            byte[] iv = new byte[12];
            secureRandom.NextBytes(randKey, 0, randKey.Length);
            secureRandom.NextBytes(iv, 0, iv.Length);
            long time = DateTimeNowTotalSeconds;
            byte[] associatedData = Encoding.UTF8.GetBytes(time.ToString());
            var pubKEY = Encoding.UTF8.GetString(Convert.FromBase64String(pubKey));
            byte[] encryptedKey;
            using (var rdr = PemKeyUtils.GetRSAProviderFromPemString(pubKEY.Trim()))
                encryptedKey = rdr.Encrypt(randKey, false);

            byte[] plaintext = Encoding.UTF8.GetBytes(password);

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(randKey), 128, iv, associatedData);
            cipher.Init(true, parameters);

            var ciphertext = new byte[cipher.GetOutputSize(plaintext.Length)];
            var len = cipher.ProcessBytes(plaintext, 0, plaintext.Length, ciphertext, 0);
            cipher.DoFinal(ciphertext, len);

            var con = new byte[plaintext.Length];
            Buffer.BlockCopy(ciphertext, 0, con, 0, plaintext.Length);
            ciphertext = con;
            var tag = cipher.GetMac();

            byte[] buffersSize = BitConverter.GetBytes(Convert.ToInt16(encryptedKey.Length));

            byte[] encKeyIdBytes = BitConverter.GetBytes(Convert.ToUInt16(pubKeyId));
            if (BitConverter.IsLittleEndian)
                Array.Reverse(encKeyIdBytes);
            encKeyIdBytes[0] = 1;
            var payload = Convert.ToBase64String(encKeyIdBytes.Concat(iv).Concat(buffersSize).Concat(encryptedKey).Concat(tag).Concat(ciphertext).ToArray());

            return $"#PWD_INSTAGRAM:4:{time}:{payload}";
        }

        public static string GenerateClientContextToken()
        {
            var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            Random random = new Random();
            var v = (long)Math.Floor(random.NextDouble() * 4294967295);

            var str = ("0000000000000000000000" + Convert.ToString(v, 2));
            str = str.Substring(str.Length - 22);
            var msgs = Convert.ToString(timestamp, 2) + str;

            return Convert.ToInt64(msgs, 2).ToString();
        }
    }
}
