namespace Kugua.Integrations.VTubeStudio
{
    public class VTSHeader
    {
        public string apiName;
        public string apiVersion;
        public string requestID;
        public string messageType;
    }

    public class VTSResponse
    {
        public string apiName;
        public string apiVersion;
        public string requestID;
        public string messageType;
        public long timestamp;
        public object data;
    }

   // public 
}
