using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows.Controls;


namespace YARDT
{
    class Utils
    {
        public static void PrintDeckList(JObject deck, JArray set, List<string> order, StackPanel sp, ref bool labelsDrawn, string mainDirName)
        {
            foreach (string cardCode in order)
            {
                string amount = deck["CardsInDeck"].Value<string>(cardCode);
                foreach (var item in set)
                {
                    if (item.Value<string>("cardCode") == cardCode)
                    {
                        //Create button
                        ControlUtils.CreateButton(sp, item, amount, !labelsDrawn, mainDirName);
                        //top += button.Height + 2;
                        Console.WriteLine(string.Format("{0,-3}{1,-25}{2}", item.Value<string>("cost"), item.Value<string>("name"), amount));
                        break;
                    }
                }
            }
            labelsDrawn = true;
        }

        public static string HttpReq(string URL)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var response = client.GetAsync(URL).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content;

                        // by calling .Result you are synchronously reading the result
                        string responseString = responseContent.ReadAsStringAsync().Result;

                        return responseString;
                    }

                    return null;
                }
                catch
                {
                    return null;
                }
            }
        }

    }
}
