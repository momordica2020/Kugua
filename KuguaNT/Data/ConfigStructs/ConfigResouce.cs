namespace Kugua.Data.ConfigStructs
{
    public enum ConfigResouceType
    {
        File,
        Path,
    }
    public class ConfigResouce
    {
        //public string Name { get; set; }
        public ConfigResouceType Type { get; set; }
        public string Path { get; set; }
    }

}
