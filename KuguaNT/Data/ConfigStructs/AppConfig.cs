namespace Kugua.Data.ConfigStructs
{

    public class AppConfig
    {
        public string Version { get; set; }
        public DateTime LateUpdated { get; set; }
        public string ResoucePath { get; set; }

        public AvatarConfig Avatar { get; set; }

        public AIConfig AI {  get; set; }
        public NetConfig Net { get; set; }

        public Dictionary<string, ConfigResouce> Resouces { get; set; }
    }
}
