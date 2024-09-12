namespace Flow.Launcher.Plugin.NugetPackageSource
{
    public class Main : IAsyncPlugin, IContextMenu, IAsyncReloadable
    {
        private const string ICON_PATH = @"Images\icon.png";
        private const string ICON_ENABLE = @"Images\icon_enable.png";
        private const string ICON_DISABLE = @"Images\icon_disable.png";

        private PluginInitContext? _context;
        private INugetPackagesSource _nugetPackage = null!;

        public Task InitAsync(PluginInitContext context)
        {
            _context = context;
            _nugetPackage = new NugetPackagesSource();

            return Task.CompletedTask;
        }

        public async Task<List<Plugin.Result>> QueryAsync(Query query, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return null;

            var nugetPackages = await _nugetPackage.ListAsync(token);
            if (!nugetPackages.Data.Any())
            {
                return new List<Plugin.Result>
                {
                    new() {
                        Title = "No recent items found",
                        IcoPath = ICON_ENABLE,
                    }
                };
            }

            var selectedItems = query.Search switch
            {
                string search when string.IsNullOrEmpty(search) => nugetPackages.Data,
                _ => nugetPackages.Data.Where(e => FuzzySearch(e, query.Search)),
            };

            return selectedItems
                .OrderBy(item => item.Index)
                .Select(CreateResult).ToList();
        }

        private Plugin.Result CreateResult(NugetSourceSettings nuget)
        {
            return new Plugin.Result
            {
                Title = nuget.Title,
                ContextData = nuget,
                IcoPath = nuget.Enabled ? ICON_ENABLE : ICON_DISABLE,
                Action = c => true
            };
        }

        public List<Plugin.Result> LoadContextMenus(Plugin.Result selectedResult)
        {
            if (selectedResult.ContextData is NugetSourceSettings nuget)
            {
                var result = new List<Plugin.Result>();
                if (nuget.Enabled)
                {
                    result.Add(new()
                    {
                        Title = $"Disable source \"{selectedResult.Title}\".",
                        SubTitle = nuget.Path,
                        IcoPath = nuget.Enabled ? ICON_ENABLE : ICON_DISABLE,
                        AsyncAction = async c =>
                        {
                            await _nugetPackage!.DisableAsync(nuget.Title);
                            _context!.API.ShowMsg("Nuget Package Source", $"Disable \"{nuget.Title}\" successfully!", ICON_PATH);
                            return true;
                        }
                    });
                }
                else
                {
                    result.Add(new()
                    {
                        Title = $"Enable source \"{selectedResult.Title}\".",
                        SubTitle = nuget.Path,
                        IcoPath = nuget.Enabled ? ICON_ENABLE : ICON_DISABLE,
                        AsyncAction = async c =>
                        {
                            await _nugetPackage!.EnableAsync(nuget.Title);
                            _context!.API.ShowMsg("Nuget Package Source", $"Enable \"{nuget.Title}\" successfully!", ICON_PATH);
                            return true;
                        }
                    });
                }

                result.Add(new()
                {
                    Title = $"Remove source \"{selectedResult.Title}\".",
                    SubTitle = selectedResult.SubTitle,
                    IcoPath = nuget.Enabled ? ICON_ENABLE : ICON_DISABLE,
                    AsyncAction = async c =>
                    {
                        await _nugetPackage!.RemoveAsync(nuget.Title);
                        _context!.API.ShowMsg("Nuget Package Source", $"Remove \"{nuget.Title}\" successfully!", ICON_PATH);

                        await Task.Delay(100);
                        return true;
                    }
                });

                return result;
            }

            return null;
        }

        private bool FuzzySearch(NugetSourceSettings nuget, string search)
        {
            var matchResult = _context!.API.FuzzySearch(search, nuget.Title);
            return matchResult.IsSearchPrecisionScoreMet();
        }

        public async Task ReloadDataAsync()
        {
            await _nugetPackage.ListAsync();
        }
    }
}