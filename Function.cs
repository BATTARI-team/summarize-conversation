using CloudNative.CloudEvents;
using Google.Cloud.Functions.Framework;
using Google.Events.Protobuf.Cloud.Storage.V1;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

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
            Console.WriteLine("MP4 file detected. Converting to MP3...");

            try
            {
                // ffmpegの実行パスを設定します。
                // 環境に合わせてffmpegのパスを修正してください。
                string ffmpegPath = "/usr/bin/ffmpeg"; // 例: Linux環境でのffmpegのパス

                // Cloud Storageからファイルをダウンロードします。
                string inputFilePath = $"/tmp/{data.Name}";
                string outputFilePath = $"/tmp/{Path.GetFileNameWithoutExtension(data.Name)}.mp3";

                await DownloadFileFromStorage(data.Bucket, data.Name, inputFilePath);

                // mp4 -> mp3
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = $"-i \"{inputFilePath}\" \"{outputFilePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine("Conversion successful.");
                        Console.WriteLine($"Output: {output}");

                        // mp3をアップロード
                        await UploadFileToStorage(data.Bucket, Path.GetFileName(outputFilePath), outputFilePath);

                        Console.WriteLine("MP3 file uploaded to Cloud Storage.");
                    }
                    else
                    {
                        Console.WriteLine($"Conversion failed. Exit code: {process.ExitCode}");
                        Console.WriteLine($"Error: {error}");
                    }
                }

                // 一時ファイルを削除します。
                File.Delete(inputFilePath);
                File.Delete(outputFilePath);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during conversion: {ex.Message}");
            }
        }
    }

    // Cloud Storageからファイルをダウンロードするメソッド（実装が必要）
    private async Task DownloadFileFromStorage(string bucketName, string objectName, string destinationFilePath)
    {
        // Google Cloud Storageクライアントライブラリを使用してファイルをダウンロードする処理を実装します。
        // 例:
        // var storage = StorageClient.Create();
        // using (var outputFile = File.OpenWrite(destinationFilePath))
        // {
        //     await storage.DownloadObjectAsync(bucketName, objectName, outputFile);
        // }
        Console.WriteLine($"Downloading {objectName} from {bucketName} to {destinationFilePath}");
        // 実際のダウンロード処理をここに実装してください。
        // 例として、空のファイルを作成するだけにしておきます。
        File.Create(destinationFilePath).Dispose();
    }

    // Cloud Storageにファイルをアップロードするメソッド（実装が必要）
    private async Task UploadFileToStorage(string bucketName, string objectName, string sourceFilePath)
    {
        // Google Cloud Storageクライアントライブラリを使用してファイルをアップロードする処理を実装します。
        // 例:
        // var storage = StorageClient.Create();
        // using (var fileStream = File.OpenRead(sourceFilePath))
        // {
        //     await storage.UploadObjectAsync(bucketName, objectName, null, fileStream);
        // }
        Console.WriteLine($"Uploading {sourceFilePath} to {bucketName}/{objectName}");
        // 実際のアップロード処理をここに実装してください。
    }
}
