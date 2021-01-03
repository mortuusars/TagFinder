using System;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Test
{
    public static class Program
    {
        static void Main(string[] args)
        {
            GetContent();

            Console.ReadKey();
        }

        private static async void GetContent()
        {
            var client = new HttpClient();

            var result = await client.GetAsync("https://github.com/mortuusars/TagFinder/releases/latest");

            var content = await result.Content.ReadAsStringAsync();

            var matches = Regex.Matches(content, "/mortuusars/TagFinder/releases/tag/\\d+\\.\\d+\\.\\d+");
            var ver = Regex.Match(matches[0].ToString(), "\\d+\\.\\d+\\.\\d+").ToString();
        }
    }
}
