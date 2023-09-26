using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

class Program
{
    static async Task Main(string[] args)
    {
        static string GetApiKey()
        {
            string[] lines = File.ReadAllLines(".env");
            foreach (string line in lines)
            {
                string[] parts = line.Split('=');
                if (parts.Length == 2 && parts[0] == "OPENAI_API_KEY")
                {
                    return parts[1];
                }
            }
            throw new Exception("OPENAI_API_KEY not found in .env file.");
        }

        string apiKey = GetApiKey();
        string apiUrl = "https://api.openai.com/v1/chat/completions";

        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        string conversationId = null;

        while (true)
        {
            Console.Write("User: ");
            string userMessage = Console.ReadLine();

            var requestPayload = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant." },
                    new { role = "user", content = userMessage },
                },
                max_tokens = 150,
                temperature = 0.6
            };

            string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(requestPayload);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                dynamic responseData = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);

                var message = responseData.choices[0].message;
                Console.WriteLine($"{message.role}: {message.content}");

                if (message.role == "GPT")
                {
                    conversationId = message.id;
                }
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }
        }
    }
}
