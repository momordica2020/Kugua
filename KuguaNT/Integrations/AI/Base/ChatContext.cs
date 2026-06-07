using OpenAI.Chat;
using System.Text.RegularExpressions;

namespace Kugua.Integrations.AI.Base
{
    public class ChatContext
    {
        public List<dynamic> ChatNodeList;
        public MessageContext LastMessageContext;


        public static readonly string RoleSystem = "system";
        public static readonly string RoleUser = "user";
        public static readonly string RoleAssistant = "assistant";
        public static readonly string RoleTool = "tool";


        public ChatContext()
        {
            this.ChatNodeList = new List<dynamic>();
            this.LastMessageContext = null;
            InitPrompt();
        }
        public string Id
        {
            get
            {
                if (LastMessageContext == null) return String.Empty;
                return LLM.GetChatId(LastMessageContext);
            }
        }

        public string Prompt
        {
            get
            {
                if (LastMessageContext == null || ChatNodeList == null || ChatNodeList.Count<1) return LLM.DefaultPrompt;
                return $"{ChatNodeList.First().content}";
            }
            set
            {
                if(ChatNodeList==null) ChatNodeList = new List<dynamic>();
                ChatNodeList.Clear();
                ChatNodeList.Add(new { role = RoleSystem, content = $"{LLM.DefaultPromptBegore}{value}" });
            }
        }

        public string LastUserAsk
        {
            get
            {
                if (ChatNodeList == null || ChatNodeList.Count < 1) return String.Empty;
                for(int i=ChatNodeList.Count-1;i>=0;i--)
                {
                    var node = ChatNodeList[i];
                    if (node.role == RoleUser)
                    {
                        return node.content;
                    }
                }
                return String.Empty;
            }
        }

        public List<ChatMessage> GptMessageList
        {
            get
            {
                List<ChatMessage> messages = new List<ChatMessage>();
                if (ChatNodeList == null) ChatNodeList = new List<dynamic>();
                foreach(var node in ChatNodeList)
                {
                    if (node.role == RoleSystem)
                    {
                        messages.Add(ChatMessage.CreateSystemMessage((string)node.content));
                    }
                    else if(node.role == RoleAssistant)
                    {
                        messages.Add(ChatMessage.CreateAssistantMessage((string)node.content));
                    }
                    else if (node.role == RoleUser)
                    {
                        messages.Add(ChatMessage.CreateUserMessage((string)node.content));
                    }
                    else if (node.role == RoleTool)
                    {
                        messages.Add(ChatMessage.CreateToolMessage((string)node.content));
                    }
                }
                //if(messages.Count>0 && messages.Last().Role == UserRole)
                //{
                //    messages.RemoveAt(messages.Count - 1);
                //}
                return messages;
            }
        }

        public void InitPrompt()
        {
            if (ChatNodeList == null) ChatNodeList = new List<dynamic>();
            ChatNodeList.Clear();
            ChatNodeList.Add(new { role = RoleSystem, content = LLM.DefaultPrompt });
        }

        public void InitWithJson(string json)
        {
            if (ChatNodeList == null) ChatNodeList = new List<dynamic>();
            ChatNodeList.Clear();
            ChatNodeList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(json);
            if (ChatNodeList == null) InitPrompt();
        }

        public void AddUserText(MessageContext context)
        {
            LastMessageContext = context;
            ChatNodeList.Add(new { role = RoleUser, content = context.Texts });
        }

        public void AddUserText(string text)
        {
            ChatNodeList.Add(new { role = RoleUser, content = text });
        }

        public void AddAssistantText(string text)
        {
            ChatNodeList.Add(new { role = RoleAssistant, content = text });
        }

        public void DeleteLast()
        {
            if(ChatNodeList!=null && ChatNodeList.Count>1)
            {
                ChatNodeList.RemoveAt(ChatNodeList.Count - 1);
            }
        }

        public void Output()
        {
            if(this.ChatNodeList!=null && ChatNodeList.Last().role == RoleAssistant)
            {
                LLM.Instance.ChatOutput(this);
            }
        }
    }
}
