namespace Kugua.Integrations.AI.Base
{
    public delegate string ChatOutputHandler(ChatContext context);


    public abstract class LLMBase
    {
        public string Name;

        //public abstract void Init();

        public abstract Task<string> ChatAsync(ChatContext context);

        //public abstract string Image(MessageContext context);

        //public abstract string ChatWithImage(string text, MagickImageCollection img, MessageContext context);

    }
}
