using CliWrap;
using CliWrap.Buffered;
using System.Text.RegularExpressions;

namespace Flow.Launcher.Plugin.NugetPackageSource
{
    public interface INugetPackagesSource
    {
        Task<Result> EnableAsync(string title, CancellationToken cancellationToken = default);

        Task<Result> DisableAsync(string title, CancellationToken cancellationToken = default);

        Task<Result> RemoveAsync(string title, CancellationToken cancellationToken = default);

        Task<Result<List<NugetSourceSettings>>> ListAsync(CancellationToken cancellationToken = default);
    }

    public partial class NugetPackagesSource : INugetPackagesSource
    {
        [GeneratedRegex("(?'Index'\\d+)\\.\\s+(?'Title'.+)\\s+\\[(?'Status'\\w+)\\]")]
        private static partial Regex NugetSourcesRegex();

        private static readonly Regex _rnugetSources = NugetSourcesRegex();

        public async Task<Result<List<NugetSourceSettings>>> ListAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var output = await Cli.Wrap("dotnet")
                .WithArguments(new[] { "nuget", "list", "source" })
                .ExecuteBufferedAsync(cancellationToken);

                var nugetSources = Parse(output.StandardOutput);
                return Result<List<NugetSourceSettings>>.Ok(nugetSources);
            }
            catch (Exception)
            {
                return new Result<List<NugetSourceSettings>>();
            }
        }

        private static List<NugetSourceSettings> Parse(string standardOutput)
        {
            var lines = standardOutput.Trim().Replace("\r\n", "\n").Split('\n');
            if (lines.Length < 3)
                return new List<NugetSourceSettings>();

            lines = lines.Skip(1).ToArray();
            var nugetSources = new List<NugetSourceSettings>();
            for (var i = 0; i < lines.Length - 1; i += 2)
            {
                var match = _rnugetSources.Match(lines[i]);
                var index = match.Groups["Index"].Value;
                var title = match.Groups["Title"].Value;
                var status = match.Groups["Status"].Value;

                nugetSources.Add(new NugetSourceSettings
                {
                    Index = int.Parse(index),
                    Title = title,
                    Path = lines[i + 1].Trim(),
                    Enabled = status == "Enabled",
                });
            }

            return nugetSources;
        }

        public async Task<Result> EnableAsync(string title, CancellationToken cancellationToken = default)
        {
            try
            {
                await Cli.Wrap("dotnet")
              .WithArguments(new[] { "nuget", "enable", "source", $"{title}" })
              .ExecuteBufferedAsync(cancellationToken);

                return Result.Ok();
            }
            catch (Exception)
            {
                return Result.Ok();
            }
        }

        public async Task<Result> DisableAsync(string title, CancellationToken cancellationToken = default)
        {
            try
            {
                await Cli.Wrap("dotnet")
               .WithArguments(new[] { "nuget", "disable", "source", $"{title}" })
               .ExecuteBufferedAsync(cancellationToken);

                return Result.Ok();
            }
            catch (Exception)
            {
                return Result.Ok();
            }
        }

        public async Task<Result> RemoveAsync(string title, CancellationToken cancellationToken = default)
        {
            try
            {
                await Cli.Wrap("dotnet")
               .WithArguments(new[] { "nuget", "remove", "source", $"{title}" })
               .ExecuteBufferedAsync(cancellationToken);

                return Result.Ok();
            }
            catch (Exception)
            {
                return Result.Ok();
            }
        }
    }
}