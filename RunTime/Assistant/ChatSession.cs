using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HT.Framework.AI
{
    /// <summary>
    /// AI会话
    /// </summary>
    [Serializable]
    public class ChatSession
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        public string ID;
        /// <summary>
        /// 会话名称
        /// </summary>
        public string Name;
        /// <summary>
        /// 远端大模型根地址
        /// </summary>
        public string BaseAddress = "http://localhost:11434";
        /// <summary>
        /// 远端大模型接口
        /// </summary>
        public string API = "/api/chat";
        /// <summary>
        /// 会话超时时长（秒）
        /// </summary>
        public int Timeout = 60;
        /// <summary>
        /// 多轮对话时，支持回传的最大轮数
        /// </summary>
        public int Round = 7;
        /// <summary>
        /// 是否打印相关日志（仅在编辑器中）
        /// </summary>
        public bool IsLogInEditor = true;
        /// <summary>
        /// 会话数据
        /// </summary>
        public ChatData Data = new ChatData();
        /// <summary>
        /// 提示词
        /// </summary>
        public ChatMessage PromptWords = new ChatMessage() { Role = "system" };
        /// <summary>
        /// 会话消息记录
        /// </summary>
        public List<ChatMessage> Messages = new List<ChatMessage>();

        /// <summary>
        /// AI会话
        /// </summary>
        public ChatSession()
        {
            
        }
        /// <summary>
        /// AI会话
        /// </summary>
        public ChatSession(string id, string name)
        {
            ID = id;
            Name = name;
        }

        /// <summary>
        /// 设置提示词
        /// </summary>
        /// <param name="content">提示词内容</param>
        public void SetPromptWords(string content)
        {
            PromptWords.Role = "system";
            PromptWords.Content = string.IsNullOrEmpty(content) ? content : content.Trim();
        }
        /// <summary>
        /// 用户说话
        /// </summary>
        /// <param name="content">说话内容</param>
        /// <param name="replyCallback">AI回话时回调（如果是流式请求，则重复回调此委托）</param>
        /// <param name="endCallback">AI回话结束（参数为本次请求是否成功）</param>
        public void UserSpeak(string content, HTFAction<string> replyCallback, HTFAction<bool> endCallback)
        {
            Messages.Add(new ChatMessage { Role = "user", Think = null, Content = content, Date = DateTime.Now.ToDefaultDateString() });

            ChatCompletionAsync(null, replyCallback, endCallback);
        }
        /// <summary>
        /// 用户说话
        /// </summary>
        /// <param name="content">说话内容</param>
        /// <param name="customData">自定义会话数据</param>
        /// <param name="replyCallback">AI回话时回调（如果是流式请求，则重复回调此委托）</param>
        /// <param name="endCallback">AI回话结束（参数为本次请求是否成功）</param>
        public void UserSpeak(string content, string customData, HTFAction<string> replyCallback, HTFAction<bool> endCallback)
        {
            Messages.Add(new ChatMessage { Role = "user", Think = null, Content = content, Date = DateTime.Now.ToDefaultDateString() });

            ChatCompletionAsync(customData, replyCallback, endCallback);
        }

        private async void ChatCompletionAsync(string customData, HTFAction<string> replyCallback, HTFAction<bool> endCallback)
        {
            using HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(BaseAddress);
            client.Timeout = TimeSpan.FromSeconds(Timeout);

            string jsonContent = null;
            if (string.IsNullOrEmpty(customData))
            {
                BuildChatMessages();
                jsonContent = JsonToolkit.JsonToString(Data);
            }
            else
            {
                jsonContent = customData;
            }
            using StringContent stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

#if UNITY_EDITOR
            if (IsLogInEditor) Log.Info("<b>[Send Request]:</b> " + jsonContent);
#endif

            StringBuilder buffer = new StringBuilder();
            bool isSuccess = true;

            void RequestError(string detail)
            {
                isSuccess = false;
                buffer.Clear();
                Log.Error("请求出错：" + detail);
            }

            try
            {
                using HttpResponseMessage response = await client.PostAsync(API, stringContent);

                if (Data.stream)
                {
                    using Stream stream = await response.Content.ReadAsStreamAsync();
                    using StreamReader reader = new StreamReader(stream);

                    while (!reader.EndOfStream)
                    {
                        string line = await reader.ReadLineAsync();
#if UNITY_EDITOR
                        if (IsLogInEditor) Log.Info("<b>[Receive Data (Stream)]:</b> " + line);
#endif
                        ChatCompletion_ReceiveData receiveData = JsonToolkit.StringToJson<ChatCompletion_ReceiveData>(line);
                        if (receiveData != null)
                        {
                            if (string.IsNullOrEmpty(receiveData.error) && receiveData.message != null)
                            {
                                replyCallback?.Invoke(receiveData.message.content);
                                buffer.Append(receiveData.message.content);
                            }
                            else
                            {
                                RequestError(receiveData.error);
                                break;
                            }
                        }
                        else
                        {
                            RequestError(line);
                            break;
                        }
                        await Task.Yield();
                    }
                }
                else
                {
                    response.EnsureSuccessStatusCode();

                    string result = await response.Content.ReadAsStringAsync();
#if UNITY_EDITOR
                    if (IsLogInEditor) Log.Info("<b>[Receive Data (Non Stream)]:</b> " + result);
#endif
                    ChatCompletion_ReceiveData receiveData = JsonToolkit.StringToJson<ChatCompletion_ReceiveData>(result);
                    if (receiveData != null)
                    {
                        if (string.IsNullOrEmpty(receiveData.error) && receiveData.message != null)
                        {
                            replyCallback?.Invoke(receiveData.message.content);
                            buffer.Append(receiveData.message.content);
                        }
                        else
                        {
                            RequestError(receiveData.error);
                        }
                    }
                    else
                    {
                        RequestError(result);
                    }
                    await Task.Yield();
                }
            }
            catch (HttpRequestException ex)
            {
                RequestError(ex.Message);
            }
            catch (Exception e)
            {
                RequestError(e.Message);
            }
            finally
            {
                string assistant = buffer.ToString();
                if (!string.IsNullOrEmpty(assistant))
                {
                    AssistantUtility.SplitContent(assistant, out string think, out string content);
                    Messages.Add(new ChatMessage { Role = "assistant", Think = think, Content = content, Date = DateTime.Now.ToDefaultDateString() });
                }
                buffer.Clear();

                endCallback?.Invoke(isSuccess);
            }
        }
        private void BuildChatMessages()
        {
            Data.messages.Clear();
            if (!string.IsNullOrEmpty(PromptWords.Content))
            {
                Data.messages.Add(new ChatData.Message() { role = PromptWords.Role, content = PromptWords.Content });
            }
            int i = Messages.Count - Round;
            if (i < 0) i = 0;
            for (; i < Messages.Count; i++)
            {
                Data.messages.Add(new ChatData.Message() { role = Messages[i].Role, content = Messages[i].Content });
            }
        }

        [Serializable]
        public class ChatData
        {
            public string model = "deepseek-coder-v2:16b";
            public List<Message> messages = new List<Message>();
            public bool stream = true;
            public Options options = new Options();

            [Serializable]
            public class Message
            {
                public string role;
                public string content;
            }
            [Serializable]
            public class Options
            {
                public float temperature = 0.8f;
            }
        }
        [Serializable]
        public class ChatMessage
        {
            public string Role;
            public string Think;
            public string Content;
            public string Images;
            public string Date;
            public bool IsFold;
        }

        private class ChatCompletion_ReceiveData
        {
            public Message message;
            public string error;

            public class Message
            {
                public string role;
                public string content;
            }
        }
    }
}