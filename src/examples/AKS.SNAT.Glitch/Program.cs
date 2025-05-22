using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SNAT.Glitch;

/// <summary>
/// This program simulates SNAT exhaustion by sending a large number of requests to a specified endpoint.
/// </summary>
/// <remarks>
/// The program uses HttpClient to send GET requests to the specified URL in a loop.
/// It can be used to test the behavior of a system under SNAT exhaustion conditions.
/// </remarks>
class Program
{
    static async Task Main(string[] args)
    {
        string url = Environment.GetEnvironmentVariable("PUBLIC_RESOURCE_URL")!;
        if (string.IsNullOrEmpty(url))
        {
            Console.WriteLine("Please set the PUBLIC_RESOURCE_URL environment variable.");
            return;
        }

        string fixedEnv = Environment.GetEnvironmentVariable("FIXED") ?? "0";
        bool useFixed = fixedEnv == "1";

        int delay = 1; // Delay in milliseconds between requests
        var i = 0;

        if (useFixed)
        {
            using (HttpClient client = new HttpClient())
            {
                while (true)
                {
                    i++;
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(url);
                        Console.WriteLine($"[FIXED] Request {i}: {response.StatusCode}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[FIXED] Request {i} failed: {ex.Message}");
                    }
                    await Task.Delay(delay);
                }
            }
        }
        else
        {
            while (true)
            {
                i++;
                HttpClient client = new HttpClient();
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    Console.WriteLine($"Request {i}: {response.StatusCode}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Request {i} failed: {ex.Message}");
                }
                await Task.Delay(delay);
            }
        }
    }
}