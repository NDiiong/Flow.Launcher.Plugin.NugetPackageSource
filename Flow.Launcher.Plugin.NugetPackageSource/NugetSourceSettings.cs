namespace Flow.Launcher.Plugin.NugetPackageSource
{
    public class NugetSourceSettings
    {
        public string Title { get; set; } = null!;
        public string Path { get; set; } = null!;
        public bool Enabled { get; set; }
        public int Index { get; internal set; }
    }
}