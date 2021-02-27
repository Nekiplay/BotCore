using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BotCore
{
	public class Vk
	{
		public class VkLongPoolClient
		{
			/* VK Client*/
			public VkLongPoolClient(string token, string botCommunityId, Action<Update> onMessageReceivedCallback, IWebProxy webProxy = null)
			{
				Token = token;
				BotCommunityId = botCommunityId;
				OnMessageReceivedCallback = onMessageReceivedCallback;
				ReceiverWebClient = new WebClient() { Proxy = webProxy, Encoding = Encoding.UTF8 };
				SenderWebClient = new WebClient() { Proxy = webProxy, Encoding = Encoding.UTF8 };

				Messages = new _Messages_(this);

				Init();
				StartLongPoolAsync();
			}
			private static WebClient ReceiverWebClient { get; set; }
			private static WebClient SenderWebClient { get; set; }
			private static string Token { get; set; }
			private int LastTs { get; set; }
			private string Server { get; set; }
			private string Key { get; set; }
			private Action<Update> OnMessageReceivedCallback { get; set; }
			private static string BotCommunityId { get; set; }
			private static Random rnd = new Random();
			/* Methods */

			public _Utils_ Utils = new _Utils_();
			public _Docs_ Docs = new _Docs_();
			public _Messages_ Messages;
			public _Groups_ Groups = new _Groups_();
			public class _Utils_
			{
				/* Utils */
				public string GetShortLink(string url)
				{
					if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
					{
						string json = CallVkMethod("utils.getShortLink", "url=" + url);
						var j = JsonConvert.DeserializeObject(json) as JObject;
						var link = j["response"]["short_url"].ToString();
						if (link == "")
						{
							return GetShortLink(url);
						}
						else
						{
							return link;
						}
					}
					else
					{
						return "Invalid link format";
					}
				}
			}
			public class _Docs_
			{
				/* Docs */
				public string GetMessagesUploadServer(string peer_id, string type, string file)
				{
					string string1 = CallVkMethod("docs.getMessagesUploadServer", "peer_id=" + peer_id + "&type=" + type);
					if (string1 != "")
					{
						string uploadurl = Regex.Match(string1, "\"upload_url\":\"(.*)\"").Groups[1].Value.Replace(@"\/", "/");
						return uploadurl;
					}
					else
					{
						return "";
					}
				}
				public string GetMessagesUploadServer(int? peer_id, string type, string file)
				{
					return GetMessagesUploadServer(peer_id.ToString(), type, file);
				}
				public string Upload(string url, string file)
				{
					var c = new WebClient();
					var r2 = Encoding.UTF8.GetString(c.UploadFile(url, "POST", file));
					if (r2 != "")
					{
						return r2;
					}
					else
					{
						return Upload(url, file);
					}
				}
				public string Save(string file, string title)
				{
					var j2 = JsonConvert.DeserializeObject(file) as JObject;
					string json = CallVkMethod("docs.save", "&file=" + j2["file"].ToString() + "&title=" + title);
					if (json != "")
					{
						return json;
					}
					else
					{
						return Save(file, title);
					}
				}
				public string Get_Send_Attachment(string file)
				{
					var j3 = JsonConvert.DeserializeObject(file) as JObject;
					var at = "doc" + j3["response"]["doc"]["owner_id"].ToString() + "_" + j3["response"]["doc"]["id"].ToString();
					return at;
				}
			}
			public class _Messages_
			{
				VkLongPoolClient vk;
				public _Messages_(VkLongPoolClient vk)
				{
					this.vk = vk;

					Send = new _Send_(vk);
					Kick = new _Kick_();
					Get = new _Get_();
				}
				public _Send_ Send;
				public _Kick_ Kick;
				public _Get_ Get;
				public class _Get_
                {
					public string InviteLink(string chatId, bool reset)
					{
						try
						{
							string json = CallVkMethod("messages.getInviteLink", "peer_id=" + chatId + "&group_id=" + BotCommunityId);
							if (json != "")
							{
								var j = JsonConvert.DeserializeObject(json) as JObject;
								var link = j["response"]["link"].ToString();
								if (link != "")
								{
									return link;
								}
								else
                                {
									return "";
                                }
							}
                            else
                            {
								return "";
                            }
						}
						catch { return ""; }
					}
					public string InviteLink(int? chatId, bool reset)
					{
						return InviteLink(chatId.ToString(), reset);
					}
				}
				public class _Send_
				{
					VkLongPoolClient vk;
					public _Send_(VkLongPoolClient vk)
					{
						this.vk = vk;
					}
					/* Messages Send */
					public void Text(string chatId, string text, int mentos = 1)
					{
						string reply = CallVkMethod("messages.send", "peer_id=" + chatId + "&random_id=" + rnd.Next() + "&message=" + text + "&disable_mentions=" + mentos);
					}
					public void Text(int? chatId, string text, int mentos = 1)
					{
						Text(chatId.ToString(), text, mentos);
					}

					public void Keyboard(string chatId, Keyboard keyboard)
					{
						string kb = keyboard.GetKeyboard();
						string reply = CallVkMethod("messages.send", "peer_id=" + chatId + "&random_id=" + rnd.Next() + "&keyboard=" + kb);
					}
					public void Keyboard(int? chatId, Keyboard keyboard)
					{
						Keyboard(chatId.ToString(), keyboard);
					}

					public void TextAndKeyboard(string chatId, string text, Keyboard keyboard)
					{
						if (keyboard.buttons.Count() > 0)
						{
							string kb = keyboard.GetKeyboard();
							string reply = CallVkMethod("messages.send", "peer_id=" + chatId + "&random_id=" + rnd.Next() + "&message=" + text + "&keyboard=" + kb);
						}
						else
                        {
							Text(chatId, text);
                        }
					}
					public void TextAndKeyboard(int? chatId, string text, Keyboard keyboard)
					{
						TextAndKeyboard(chatId.ToString(), text, keyboard);
					}

					public void Sticker(string chatId, int sticker_id)
					{
						string reply = CallVkMethod("messages.send", "peer_id=" + chatId + "&random_id=" + rnd.Next() + "&sticker_id=" + sticker_id);
					}
					public void Sticker(int? chatId, int? sticker_id)
					{
						Sticker(chatId.ToString(), (int)sticker_id);
					}

					public void TextAndDocument(string chatId, string text, string file, string title)
					{
						string u2 = vk.Docs.GetMessagesUploadServer(chatId, "doc", file);
						string r2 = vk.Docs.Upload(u2, file);
						string r3 = vk.Docs.Save(r2, title);
						string at = vk.Docs.Get_Send_Attachment(r3);
						string reply = CallVkMethod("messages.send", "peer_id=" + chatId + "&random_id=" + rnd.Next() + "&message=" + text + "&attachment=" + at);
					}
					public void TextAndDocument(int? chatId, string text, string file, string title)
					{
						TextAndDocument(chatId.ToString(), text, file, title);
					}

					public void Custom(string chatId, string custom)
					{
						string reply = CallVkMethod("messages.send", "peer_id=" + chatId + "&random_id=" + rnd.Next() + custom);
					}
					public void Custom(int? chatId, string custom)
					{
						Custom(chatId.ToString(), custom);
					}
				}
				public class _Kick_
				{
					public void Group(string chat_id, string user_id)
					{
						if (user_id != BotCommunityId)
						{
							string json = CallVkMethod("messages.removeChatUser", "chat_id=" + chat_id + "&member_id=" + "-" + user_id);
						}
					}
					public void Group(int? chat_id, int user_id)
					{
						Group(chat_id.ToString(), user_id.ToString());
					}

					public void User(string chat_id, string user_id)
					{
						string json = CallVkMethod("messages.removeChatUser", "chat_id=" + chat_id + "&user_id=" + user_id + "&member_id=" + user_id);
					}
					public void User(int? chat_id, int? user_id)
					{
						User(chat_id.ToString(), user_id.ToString());
					}
				}
				public void SetActivity(string chatId, string type = "typing")
				{
					string id3 = chatId;
					id3 = id3.Substring(1);
					int ind = Convert.ToInt32(id3);
					CallVkMethod("messages.setActivity", "user_id=" + BotCommunityId + "&peer_id=" + chatId + "&group_id=" + "&type=" + type + "&group_id=" + BotCommunityId);
					CallVkMethod("messages.setActivity", "user_id=" + BotCommunityId + "&peer_id=" + ind + "&type=" + type + "&group_id=" + BotCommunityId);
				}
				public void SetActivity(int? chatId, string type = "typing")
				{
					SetActivity(chatId.ToString(), type);
				}

			}
			public class _Groups_
			{
				/* Groups */
				public void Online(bool enable = true)
				{
					if (enable)
					{
						CallVkMethod("groups.enableOnline", "group_id=" + BotCommunityId);
					}
					else
					{
						CallVkMethod("groups.disableOnline", "group_id=" + BotCommunityId);
					}
				}

				public string GetName_ByID(int? group_id)
				{
					try
					{
						string js = CallVkMethod("groups.getById", "group_id=" + group_id);
                        if (js != "")
                        {
                            var j3 = JsonConvert.DeserializeObject(js) as JObject;
                            string name = j3["response"][0]["name"].ToString();
                            return name;
                        }
                        else
                        {
							return "";
                        }
					}
					catch { return ""; }
				}
				public string GetName_ByID(string group_id)
				{
					return GetName_ByID(int.Parse(group_id));
				}
			}
			public class _Users_
			{
				/* Users */
				public string Get_FirstName_ByID(int? user_id)
				{
					try
					{
						string js = CallVkMethod("users.get", "user_ids=" + user_id);
						if (js != "")
						{
							var j3 = JsonConvert.DeserializeObject(js) as JObject;
							string name = j3["response"][0]["first_name"].ToString();
							return name;
						}
						else
						{
							return "";
						}
					}
					catch { return ""; }
				}
				public string Get_FirstName_ByID(string user_id)
				{
					return Get_FirstName_ByID(int.Parse(user_id));
				}
			}

			public class Keyboard
			{
				public bool one_time = false;
				public List<List<object>> buttons = new List<List<object>>();
				public bool inline = false;
				public Keyboard(bool one_time2, bool line = false)
				{
					if (line == true && one_time2 == true)
						one_time2 = false;

					one_time = one_time2;
					inline = line;
				}

				public void AddButton(string label, string payload, string color)
				{
					Buttons button = new Buttons(label, payload, color);
					buttons.Add(new List<object>() { button });
				}
				public string GetKeyboard()
				{
					return JsonConvert.SerializeObject(this, Formatting.Indented); ;
				}
				public class Buttons
				{
					public Action action;
					public string color;
					public Buttons(string labe11, string payload1, string color2)
					{
						action = new Action(labe11, payload1);
						color = color2;
					}

					public class Action
					{
						public string type;
						public string payload;
						public string label;
						public Action(string label3, string payload3)
						{
							type = "text";
							payload = "{\"button\": \"" + payload3 + "\"}";
							label = label3;
						}
					}
				}
			}

			/* LoongPool */
			private void Init()
			{
				var jsonResult = CallVkMethod("groups.getLongPollServer", "group_id=" + BotCommunityId);
				var j = JsonConvert.DeserializeObject(jsonResult) as JObject;

				Key = j["response"]["key"].ToString();
				Server = j["response"]["server"].ToString();
				LastTs = Convert.ToInt32(j["response"]["ts"].ToString());
			}
			private void StartLongPoolAsync()
			{
				Task.Factory.StartNew(() =>
				{
					while (true)
					{
						try
						{
							string baseUrl = String.Format("{0}?act=a_check&version=2&wait=25&key={1}&ts=", Server, Key);
							var data = ReceiverWebClient.DownloadString(baseUrl + LastTs);
							var messages = ProcessResponse(data);
							foreach (Update update in messages)
							{
								OnMessageReceivedCallback(update);
							}
						}
						catch { }
					}
				});
			}
			/* LongPool Data */
			public class AudioMsg
			{
				public int duration { get; set; }
				public string link_mp3 { get; set; }
				public string link_ogg { get; set; }
				public List<int> waveform { get; set; }
			}

			public class Preview
			{
				public AudioMsg audio_msg { get; set; }
			}
			public class Image
			{
				public string url { get; set; }
				public int width { get; set; }
				public int height { get; set; }
			}

			public class FwdMessage
			{
				public int date { get; set; }
				public int from_id { get; set; }
				public string text { get; set; }
				public List<object> attachments { get; set; }
			}
			public class ImagesWithBackground
			{
				public string url { get; set; }
				public int width { get; set; }
				public int height { get; set; }
			}

			public class Sticker
			{
				public int product_id { get; set; }
				public int sticker_id { get; set; }
				public List<Image> images { get; set; }
				public List<ImagesWithBackground> images_with_background { get; set; }
			}
			public class Doc
			{
				public int id { get; set; }
				public int owner_id { get; set; }
				public string title { get; set; }
				public int size { get; set; }
				public string ext { get; set; }
				public int date { get; set; }
				public int type { get; set; }
				public string url { get; set; }
				public Preview preview { get; set; }
				public string access_key { get; set; }
			}
			public class Audio
			{
				public string artist { get; set; }
				public int id { get; set; }
				public int owner_id { get; set; }
				public string title { get; set; }
				public int duration { get; set; }
				public string url { get; set; }
				public int date { get; set; }
				public bool is_hq { get; set; }
				public bool short_videos_allowed { get; set; }
				public bool stories_allowed { get; set; }
				public bool stories_cover_allowed { get; set; }
			}
			public class Size
			{
				public int height { get; set; }
				public string url { get; set; }
				public string type { get; set; }
				public int width { get; set; }
			}

			public class Photo
			{
				public int album_id { get; set; }
				public int date { get; set; }
				public int id { get; set; }
				public int owner_id { get; set; }
				public bool has_tags { get; set; }
				public int post_id { get; set; }
				public List<Size> sizes { get; set; }
				public string text { get; set; }
			}
			public class Attachment
			{
				public string type { get; set; }
				public Sticker sticker { get; set; }
				public Doc doc { get; set; }
				public Audio audio { get; set; }
				public Photo photo { get; set; }
			}
			public class Object
			{
				public string state { get; set; }
				public int from_id { get; set; }
				public int to_id { get; set; }
				public int? date { get; set; }
				public int? id { get; set; }
				public int? @out { get; set; }
				public int? peer_id { get; set; }
				public string text { get; set; }
				public int? conversation_message_id { get; set; }
				public Action action { get; set; }
				public List<FwdMessage> fwd_messages { get; set; }
				public bool? important { get; set; }
				public int? random_id { get; set; }
				public List<Attachment> attachments { get; set; }
				public string payload { get; set; }
				public bool? is_hidden { get; set; }
			}
			public class Action
			{
				/* Types 
				1. chat_invite_user
				2. chat_kick_user

				*/
				public string type { get; set; }
				public int member_id { get; set; }
			}

			public class Update
			{
				public string type { get; set; }
				public Object @object { get; set; }
				public int group_id { get; set; }
				public string event_id { get; set; }
			}

			public class Root
			{
				public int? failed { get; set; }
				public string ts { get; set; }
				public List<Update> updates { get; set; }
			}


			private List<Update> ProcessResponse(string jsonData)
			{
				if (jsonData == "{\"failed\":1}" || jsonData == "{\"failed\":2}" || jsonData == "{\"failed\":3}")
				{
					Init();
				}
				Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(jsonData);
				if (myDeserializedClass == null || myDeserializedClass.failed != null)
				{
					Init();
				}
				LastTs = Convert.ToInt32(myDeserializedClass.ts);
				return myDeserializedClass.updates;
			}

			public static string CallVkMethod(string methodName, string data)
			{
				try
				{
					var url = String.Format("https://api.vk.com/method/{0}?v=5.122&access_token={1}&{2}", methodName, Token, data);
					var jsonResult = SenderWebClient.DownloadString(url);

					return jsonResult;
				}
				catch { return String.Empty; }
			}
		}
	}
}