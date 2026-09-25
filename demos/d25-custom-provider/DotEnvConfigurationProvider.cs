using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace D25.CustomProvider;

/// <summary>
/// Reads a .env-style file into configuration. A <c>__</c> in a key becomes a <c>:</c>, exactly
/// like the built-in environment variables provider.
/// </summary>
public sealed class DotEnvConfigurationProvider : ConfigurationProvider, IDisposable
{
    // The built-in FileConfigurationSource carries the same delay, for the same reason: the
    // file-changed event usually arrives before the writer has finished flushing.
    private static readonly TimeSpan ReloadDelay = TimeSpan.FromMilliseconds(250);

    private readonly string _path;
    private readonly PhysicalFileProvider? _files;
    private readonly IDisposable? _subscription;

    public DotEnvConfigurationProvider(DotEnvConfigurationSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        _path = System.IO.Path.GetFullPath(source.Path);

        if (!source.ReloadOnChange)
        {
            return;
        }

        var directory = System.IO.Path.GetDirectoryName(_path) ?? AppContext.BaseDirectory;
        _files = new PhysicalFileProvider(directory);

        _subscription = ChangeToken.OnChange(
            () => _files.Watch(System.IO.Path.GetFileName(_path)),
            () =>
            {
                Thread.Sleep(ReloadDelay);
                Load();

                // This is the line that makes IOptionsMonitor fire. Without it the new values sit
                // in Data and nothing downstream ever hears about them.
                OnReload();
            });
    }

    /// <summary>Runs once when the configuration root is built, and again on every reload.</summary>
    public override void Load()
    {
        // Data is the protected IDictionary<string, string?> on ConfigurationProvider. Replacing it
        // wholesale means a deleted key really disappears.
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        if (File.Exists(_path))
        {
            foreach (var line in File.ReadAllLines(_path))
            {
                var text = line.Trim();
                if (text.Length == 0 || text.StartsWith('#'))
                {
                    continue;
                }

                var separator = text.IndexOf('=', StringComparison.Ordinal);
                if (separator <= 0)
                {
                    continue;
                }

                var key = text[..separator].Trim().Replace("__", ":", StringComparison.Ordinal);
                data[key] = text[(separator + 1)..].Trim().Trim('"');
            }
        }

        Data = data;
    }

    /// <summary>What GetDebugView() prints in parentheses next to every key this provider supplied.</summary>
    public override string ToString() => $".env file: {System.IO.Path.GetFileName(_path)}";

    public void Dispose()
    {
        _subscription?.Dispose();
        _files?.Dispose();
    }
}
