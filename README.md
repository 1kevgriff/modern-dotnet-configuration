# Modern .NET Configuration

Talk materials — slides, demos, and reference links — for **Modern .NET Configuration**.

Configuration in .NET is one of those things every app has and almost nobody revisits.
This talk covers what the configuration system actually does, where the sharp edges are,
and the patterns that hold up in production: providers and precedence, options binding
and validation, secrets that never touch source control, and configuration that changes
without a redeploy.

## Talk

| | |
| --- | --- |
| **Presented at** | [Cloud & AI Summit](https://www.cloudandaisummit.com/) (debut) |
| **Speaker** | Kevin Griffin — [consultwithgriff.com/bio](https://consultwithgriff.com/bio) |
| **Target** | .NET 10 / C# 14 |

## Topics

- `IConfiguration`, the provider chain, and how precedence really resolves
- `appsettings.json`, environment variables, command line, and user secrets
- The options pattern: `IOptions`, `IOptionsSnapshot`, `IOptionsMonitor` — and when each one is wrong
- Binding, validation, and failing fast at startup instead of at 3 a.m.
- Keeping secrets out of source control (User Secrets locally, Key Vault / managed identity in Azure)
- Centralized and dynamic configuration with Azure App Configuration, including feature flags
- Reload-on-change: what actually reloads, what doesn't, and why
- Writing a custom configuration provider
- Testing configuration-dependent code

## Repository layout

```text
/slides     Slide deck and exported PDF
/demos      Runnable demo projects, numbered in presentation order
/README.md  You are here
```

## Running the demos

```bash
dotnet --version   # 10.0.x expected
cd demos
dotnet build
```

Each demo folder has its own README with the setup steps and the point it's making.
Demos that need secrets read them from User Secrets or environment variables — no
credentials are committed to this repo, and `appsettings.Local.json` / `.env` are ignored.

## References

- [Configuration in .NET](https://learn.microsoft.com/dotnet/core/extensions/configuration)
- [Options pattern in .NET](https://learn.microsoft.com/dotnet/core/extensions/options)
- [Safe storage of app secrets in development](https://learn.microsoft.com/aspnet/core/security/app-secrets)
- [Azure App Configuration](https://learn.microsoft.com/azure/azure-app-configuration/)

## Speaker

**Kevin Griffin** — software consultant and Microsoft MVP.
Full bio: [consultwithgriff.com/bio](https://consultwithgriff.com/bio)

- Web: [consultwithgriff.com](https://consultwithgriff.com)
- X: [@1kevgriff](https://x.com/1kevgriff)
- Bluesky: [@consultwithgriff.com](https://bsky.app/profile/consultwithgriff.com)
- LinkedIn: [in/1kevgriff](https://www.linkedin.com/in/1kevgriff/)
- GitHub: [@1kevgriff](https://github.com/1kevgriff)
- YouTube: [@consultwithgriff](https://youtube.com/@consultwithgriff)
- Speaking: [sessionize.com/kevingriffin](https://sessionize.com/kevingriffin/)

## License

Slides and demo code are provided for educational use. Feel free to borrow ideas
for your own applications; please credit the source if you reuse the material in a talk.
