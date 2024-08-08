using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAI.Chat;

namespace MasterThesisHelper.model
{
    internal class ChatGPTChat: IChatModel
    {
        static ChatClient? client;
        private List<ChatMessage> messages = new List<ChatMessage>();
        static void Init()
        {
            if(client==null)
            {
                client = new(model: "gpt-4o", System.IO.File.ReadAllText(@"D:\data_clump_solver\tokens\CHATGPT_TOKEN"));

            }

            //  ChatCompletion completion = client.CompleteChat(ChatMessage.CreateUserMessage();

            //Console.WriteLine($"[ASSISTANT]: {completion}");
        }
        public string SendAndWait(string prompt, IEnumerable<LatexBlock> context)
        {
            Init();
            messages.Add(prompt);
            messages.Add(string.Join("\n", context.Select((x) => x.GetCompleteString())));
            string appendix = messages[messages.Count - 1].ToString();

            ChatCompletion completion = client.CompleteChat(messages);
            string answer = completion.Content[0].Text;
            System.IO.File.WriteAllText("answer.txt", answer);
            messages.Add(ChatMessage.CreateAssistantMessage(answer));
            return answer;


        }
    }
}
