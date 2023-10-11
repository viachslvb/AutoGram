using System.IO;
using System.Text;
using System.Windows;
using AutoGram.Instagram.Response;
using Newtonsoft.Json;
using xNet;

namespace AutoGram
{
    internal static class Serializer
    {
        internal static void Serialize<T>(this T arg, string fileName)
        {
            string res = JsonConvert.SerializeObject(arg, Formatting.Indented);
            File.WriteAllText(fileName, res, Encoding.UTF8);
        }

        internal static T Deserialize<T>(this string json)
        {
            T res = JsonConvert.DeserializeObject<T>(json);
            return res;
        }

        internal static T ToResponse<T>(this HttpResponse httpResponse)
        {
            string httpResponseString = httpResponse.ToString();

            try
            {
                var response = JsonConvert.DeserializeObject<T>(httpResponseString);
                var httpResponseProperty = response.GetType().GetProperty("HttpResponse");

                if (httpResponseProperty != null)
                {
                    httpResponseProperty.SetValue(response, httpResponse);
                }

                return response;
            }
            catch (JsonReaderException)
            {
                using (TextWriter textWriter = new StreamWriter("json_errors.txt", true))
                {
                    textWriter.WriteLine($"[{System.DateTime.Now}] {httpResponseString}");
                }
                throw new JsonReaderException();
            }
        }
    }

    class InstagramResponse<T>
    {
        public T Response;
        public HttpResponse HttpResponse;
    }
}
