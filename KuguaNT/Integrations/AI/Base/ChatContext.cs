using OpenAI.Chat;
using System.Text.RegularExpressions;

namespace Kugua.Integrations.AI.Base
{
    public class ChatContext
    {
        public List<dynamic> ChatNodeList;
        public MessageContext LastMessageContext;


        public static readonly string SystemRole = "system";
        public static readonly string UserRole = "user";
        public static readonly string AssistantRole = "assistant";
        public static readonly string ToolRole = "tool";


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
                ChatNodeList.Add(new { role = SystemRole, content = value });
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
                    if (node.role == UserRole)
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
                    if (node.role == SystemRole)
                    {
                        messages.Add(ChatMessage.CreateSystemMessage((string)node.content));
                    }
                    else if(node.role == AssistantRole)
                    {
                        messages.Add(ChatMessage.CreateAssistantMessage((string)node.content));
                    }
                    else if (node.role == UserRole)
                    {
                        messages.Add(ChatMessage.CreateUserMessage((string)node.content));
                    }
                    else if (node.role == ToolRole)
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
            ChatNodeList.Add(new { role = SystemRole, content = LLM.DefaultPrompt });
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
            ChatNodeList.Add(new { role = UserRole, content = context.Texts });
        }

        public void AddUserText(string text)
        {
            ChatNodeList.Add(new { role = UserRole, content = text });
        }

        public void AddAssistantText(string text)
        {
            ChatNodeList.Add(new { role = AssistantRole, content = text });
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
            if(this.ChatNodeList!=null && ChatNodeList.Last().role == AssistantRole)
            {
                LLM.Instance.ChatOutput(this);
            }
        }
    }
}
