using CloudNative.CloudEvents;
using Google.Cloud.Functions.Framework;
using Google.Events.Protobuf.Cloud.Storage.V1;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace HelloGcs;

public class Function : ICloudEventFunction<StorageObjectData>
{
    public async Task HandleAsync(CloudEvent cloudEvent, StorageObjectData data, CancellationToken cancellationToken)
    {
        Console.WriteLine("Storage object information:");
        Console.WriteLine($"  Name: {data.Name}");
        Console.WriteLine($"  Bucket: {data.Bucket}");
        Console.WriteLine($"  Size: {data.Size}");
        Console.WriteLine($"  Content type: {data.ContentType}");
        Console.WriteLine("CloudEvent information:");
        Console.WriteLine($"  ID: {cloudEvent.Id}");
        Console.WriteLine($"  Source: {cloudEvent.Source}");
        Console.WriteLine($"  Type: {cloudEvent.Type}");
        Console.WriteLine($"  Subject: {cloudEvent.Subject}");
        Console.WriteLine($"  DataSchema: {cloudEvent.DataSchema}");
        Console.WriteLine($"  DataContentType: {cloudEvent.DataContentType}");
        Console.WriteLine($"  Time: {cloudEvent.Time?.ToUniversalTime():yyyy-MM-dd'T'HH:mm:ss.fff'Z'}");
        Console.WriteLine($"  SpecVersion: {cloudEvent.SpecVersion}");

        if (data.Name.ToLower().EndsWith(".mp4"))
        {
            Console.WriteLine("MP4 file detected. Summarizing conversation...");
            HttpClient client = new HttpClient();
            using StringContent jsonContent = new(
                JsonSerializer.Serialize(new
                {
                    fileName = data.Name
                }),
                Encoding.UTF8,
                "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://summarize-conversation.takutk.shop/Home/StartSummarize"); //Replace "YOUR_ENDPOINT_HERE"
            request.Content = jsonContent;
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Conversion successful.");
            }
            else
            {
                Console.WriteLine("Conversion failed.");
            }

        }
    }
}
