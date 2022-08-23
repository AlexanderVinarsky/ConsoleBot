using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Enums.Filters;
using VkNet.Model.Keyboard;
using System.Threading;
using System.IO;

namespace ConsoleApp
{
    class Program
    {

        public static List<string> l_questions = new List<string>();
        public static List<string> l_answers = new List<string>();
        public static string token = "";
        public static VkApi vk = new VkApi();
        public static long idOfConversation = 0;
        static void Main(string[] args)
        {
            string path1 = Environment.CurrentDirectory + "/questions.txt";
            string path2 = Environment.CurrentDirectory + "/answers.txt";
            string path3 = Environment.CurrentDirectory + "/APItoken.txt";
            string path4 = Environment.CurrentDirectory + "/ConversationID.txt";
            if (File.Exists(path1) && File.Exists(path2) && File.Exists(path3) && File.Exists(path4))
            {
                using (StreamReader sr1 = new StreamReader(path1))
                {

                    while (!sr1.EndOfStream)
                    {
                        string temp = sr1.ReadLine();
                        l_questions.Add(temp);
                    }

                }
                using (StreamReader sr2 = new StreamReader(path2))
                {
                    while (!sr2.EndOfStream)
                    {
                        string temp = sr2.ReadLine();
                        l_answers.Add(temp);
                    }
                    l_answers.Add(sr2.ReadLine());
                }
                using (StreamReader sr3 = new StreamReader(path3))
                {
                    token = sr3.ReadToEnd();
                }
                using (StreamReader sr4 = new StreamReader(path4))
                {
                    idOfConversation = long.Parse(sr4.ReadToEnd());
                }
            }
            vk.Authorize(new ApiAuthParams
            {
                AccessToken = token,
                Settings = Settings.All
            });
            while (true)
            {
                Thread.Sleep(1);
                Receive();
            }
        }

        public static bool Receive()
        {

            object[] minfo = GetMessage();
            long? userid = Convert.ToInt32(minfo[2]);
            if (minfo[0] == null)
                return false;
            KeyboardBuilder key = new KeyboardBuilder();
            string code = "";
            if (minfo[1].ToString() != "")
                code = minfo[1].ToString();
            else
                code = minfo[0].ToString();

            for (int i = 0; i < l_questions.Count; i++)
            {
                if (code.Contains(l_questions[i]))
                {
                    SendMessage(l_answers[i], userid, null);
                    break;
                }
            }

            return true;
        }
        public static void SendMessage(string message, long? userid, MessageKeyboard keyboard)
        {
            vk.Messages.Send(new MessagesSendParams
            {
                Message = message,
                PeerId = userid,
                RandomId = new Random().Next(),
                Keyboard = keyboard
            });
        }


        [Obsolete]
        public static object[] GetMessage()
        {
            string message = "";
            string keyname = "";
            long? userid = 0;
            long? conversationid = 0;
            var messages = vk.Messages.GetDialogs(new MessagesDialogsGetParams
            {
                Count = 100,
                Unread = true
            });
            if (messages.Messages.Count != 0)
            {
                if (messages.Messages[0].ChatId != null) if (messages.Messages[0].ChatId.Value == idOfConversation)
                    {
                        if (messages.Messages[0].Body.ToString() != "" && messages.Messages[0].Body.ToString() != null)
                            message = messages.Messages[0].Body.ToString();
                        else
                            message = "";

                        if (messages.Messages[0].Payload != null)
                            keyname = messages.Messages[0].Payload.ToString();
                        else
                            keyname = "";
                        userid = messages.Messages[0].UserId.Value;

                        if (messages.Messages[0].ChatId != null) conversationid = messages.Messages[0].ChatId.Value;



                        object[] keys = new object[3] { message, keyname, userid };
                        if (conversationid > 0) vk.Messages.MarkAsRead((conversationid + 2000000000).ToString());
                        return keys;
                    }
                    else
                        return new object[] { null, null, null };
                else return new object[] { null, null, null };
            }
            else
                return new object[] { null, null, null };

        }
    }
}