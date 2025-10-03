namespace Kugua.Mods
{
    public class KuguaOlCommand
    {
        public string Name;
        public string[] Params;
        public MessageContext context;
        public HandleKuguaOlCommandEvent Callback;

        public bool DealReact(MessageContext reactContext)
        {
            try
            {
                string callbackResult = Callback(context, Params, reactContext);
                if (callbackResult == null)
                {
                    return true;
                }

            }catch(Exception ex)
            {
                Logger.Log(ex);
                
            }
            return false;
        }
    }
}
