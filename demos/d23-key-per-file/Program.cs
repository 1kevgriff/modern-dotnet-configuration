using Microsoft.Extensions.Configuration;

// Docker secrets and Kubernetes mounted secrets are one file per value: the file
// NAME is the configuration key, the file CONTENTS are the value, and "__" is the
// section delimiter.
//
// Under `docker compose up` the mount really is /run/secrets. With no container, fall
// back to the copy of ./secrets next to the built assembly — the provider requires an
// ABSOLUTE path, and /run/secrets does not exist on a laptop.
var configuredPath = Environment.GetEnvironmentVariable("SECRETS_PATH");

var mountPath = string.IsNullOrWhiteSpace(configuredPath)
    ? Path.Combine(AppContext.BaseDirectory, "secrets")
    : configuredPath;

var displayPath = string.IsNullOrWhiteSpace(configuredPath)
    ? "./secrets   (standing in for /run/secrets)"
    : configuredPath;

var configuration = new ConfigurationBuilder()
    .AddKeyPerFile(directoryPath: mountPath, optional: false)
    .Build();

var mountedFiles = new DirectoryInfo(mountPath)
    .GetFiles()
    .Select(file => file.Name)
    .Order(StringComparer.Ordinal)
    .ToList();

Console.WriteLine();
Console.WriteLine($"  MOUNT  {displayPath}   —   {mountedFiles.Count} files, one value each");
Console.WriteLine();

foreach (var fileName in mountedFiles)
{
    var key = fileName.Replace("__", ":", StringComparison.Ordinal);

    Console.WriteLine($"  {fileName}");
    Console.WriteLine($"      ->  {key}  =  {configuration[key]}");
    Console.WriteLine();
}

var provider = configuration.Providers.Single().GetType().Name;

Console.WriteLine($"  every key above came from   {provider}");
Console.WriteLine();
