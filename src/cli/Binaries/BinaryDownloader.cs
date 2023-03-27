using System.Collections.ObjectModel;
using System.Formats.Tar;
using System.IO.Compression;

using Spectre.Console;

namespace Avalanche.Cli.Binaries;

public class BinaryDownloader
{
    public static readonly ReadOnlyDictionary<BinaryDependency, string> LatestReleaseUrls = new(
        new Dictionary<BinaryDependency, string>
        {
            { BinaryDependency.avalancheGo, "https://api.github.com/repos/ava-labs/avalanchego/releases/latest" },
            {
                BinaryDependency.avalancheNetworkRunner,
                "https://api.github.com/repos/ava-labs/avalanche-network-runner/releases/latest"
            }
        });

    private readonly AppConfig _config;
    private readonly HttpClient _http;

    public BinaryDownloader(AppConfig config)
    {
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Avalanche cli");
        _http = client;
        _config = config;
    }


    public async IAsyncEnumerable<BinaryDependencyData> DownloadLatestReleases()
    {
        foreach (KeyValuePair<BinaryDependency, string> url in LatestReleaseUrls)
        {
            yield return await DownloadLatestRelease(url);
        }
    }

    private async Task<BinaryDependencyData> DownloadLatestRelease(KeyValuePair<BinaryDependency, string> url)
    {
        string jsonFilePath = BinaryManager.GetLatestReleaseJsonPath(_config.BasePath, url.Key);
        Log.Debug("downloading " + url.Value);
        string json = await _http.GetStringAsync(url.Value);
        Log.Debug($"Saving latest release json file {jsonFilePath}");
        File.WriteAllText(jsonFilePath, json);
        return BinaryDependencyData.Parse(json, url.Key, _config.BasePath);
    }

    public static bool CheckDownloadAvailable(BinaryDependencyData item)
    {
        return !File.Exists(item.FileName);
    }

    public async Task DownloadLatestBinary(BinaryDependencyData item, ProgressTask task)
    {
        Console.WriteLine("item.fileName" + item.FileName);
        if (!File.Exists(item.ArchiveFileName))
        {
            await Download(item, task);
        }

        if (!File.Exists(item.FileName))
        {
            await Extract(item);
        }
    }

    private async Task Download(BinaryDependencyData item, ProgressTask task)
    {
        try
        {
            string url = item.DownloadUrl;
            Log.Debug("Downloading " + url);
            using HttpResponseMessage response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            // Set the max value of the progress task to the number of bytes
            task.MaxValue(response.Content.Headers.ContentLength ?? 0);
            // Start the progress task
            task.StartTask();

            string filename = item.ArchiveFileName;
            AnsiConsole.MarkupLine($"Starting download of [u]{filename}[/] ({task.MaxValue} bytes)");

            using Stream contentStream = await response.Content.ReadAsStreamAsync();
            await WriteStreamToFile(contentStream, filename, task);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error while downloading {item.DownloadUrl}");
            // An error occured
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex}");
        }
    }

    private static async Task WriteStreamToFile(Stream contentStream, string filename, ProgressTask task)
    {
        using FileStream fileStream =
            new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
        Memory<byte> buffer = new byte[8192].AsMemory();
        while (true)
        {
            int read = await contentStream.ReadAsync(buffer);
            if (read == 0)
            {
                AnsiConsole.MarkupLine($"Download of [u]{filename}[/] [green]completed![/]");
                break;
            }

            // Increment the number of read bytes for the progress task
            task.Increment(read);

            // Write the read bytes to the output stream
            await fileStream.WriteAsync(buffer[..read]);
        }
    }

    public static async Task ExtractTGZ(string gzArchiveName, string destFolder)
    {
        Log.Debug($"Extracting {gzArchiveName} to {destFolder}");
        MemoryStream tarStream = new MemoryStream();
        using GZipStream decompressor =
            new GZipStream(File.Open(gzArchiveName, FileMode.Open), CompressionMode.Decompress);
        await decompressor.CopyToAsync(tarStream);
        tarStream.Position = 0;
        await TarFile.ExtractToDirectoryAsync(tarStream, destFolder, true);
    }

    public static async Task Extract(BinaryDependencyData item)
    {
        Directory.CreateDirectory(item.PhysicalPath);
        await ExtractTGZ(item.ArchiveFileName, item.PhysicalPath);
        string destinationDirectory = item.FileName[..item.FileName.LastIndexOf('/')];
        Directory.CreateDirectory(destinationDirectory);
        Directory.Delete(destinationDirectory, true);
        Log.Debug(destinationDirectory);
        Directory.CreateSymbolicLink(destinationDirectory, item.PhysicalPath);
        CreateLatestBinSymbolicLink(item);
    }

    public static void CreateLatestBinSymbolicLink(BinaryDependencyData item)
    {
        string fileName = item.FileName.Replace(item.Version, "latest");
        string destinationDirectory = fileName[..fileName.LastIndexOf('/')];
        Log.Debug("creating folder " + destinationDirectory);
        Directory.CreateDirectory(destinationDirectory);
        Directory.Delete(destinationDirectory, true);
        Directory.CreateSymbolicLink(destinationDirectory, item.PhysicalPath);
    }

    private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        // Get information about the source directory
        DirectoryInfo dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
        }

        // Cache directories before we start copying
        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
}