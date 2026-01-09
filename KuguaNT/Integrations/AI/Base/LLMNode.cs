namespace Kugua.Integrations.AI.Base
{

    public enum LLMRole
    {
        assistant,
        user,
        system,
        tool
    }


    public class LLMNode
    {
        public LLMRole role;
        public string tag;
        public string content;
        
        public object ToApi
        {
            get
            {
                return new { role = role.ToString(), content = content };
            }
        }
    }




}