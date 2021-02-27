using BotCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testss
{
    public static class Program
    {
        public static Vk.VkLongPoolClient vkclient;
        public static void Main(string[] args)
        {
            vkclient = new Vk.VkLongPoolClient("", "", MSG);
        }
        public static void MSG(Vk.VkLongPoolClient.Update update)
        {
            if (update.@object.peer_id == 244428396)
            {
                BotUser botuser = GetUserInfo(update);
                if (botuser != null)
                {
                    Console.WriteLine(botuser.needcaptcha);
                    if (update.@object.text == "1" && botuser.needcaptcha == false)
                    {
                        vkclient.Messages.Send.Text(update.@object.peer_id, "Сообщение обработано сервером");
                        UpdateUserInfo(update);
                        Console.WriteLine(botuser.needcaptcha);
                    }
                    else if (update.@object.text == "2")
                    {
                        vkclient.Messages.Send.Text(update.@object.peer_id, "Сообщение обработано сервером");
                    }
                    else if (botuser.needcaptcha == true)
                    {
                        CalcCaptcha(update);
                        Console.WriteLine(botuser.needcaptcha);
                        if (botuser.needcaptcha == false)
                        {
                            vkclient.Messages.Send.Text(update.@object.peer_id, "Капча пройдена");
                        }
                        else
                        {
                            botuser.captcha.GenerateType();
                            Console.WriteLine(botuser.captcha.type);
                            if (botuser.captcha.type == "math")
                            {
                                botuser.captcha.GenerateMath();
                                vkclient.Messages.Send.Text(update.@object.peer_id, "Решите капчу: " + botuser.captcha.math.Replace("+", "%2B"));
                            }
                            else if (botuser.captcha.type == "image")
                            {
                                if (File.Exists("" + botuser.vkid + ".png"))
                                    File.Delete("" + botuser.vkid + ".png");
                                botuser.captcha.GenerateImage(50, 25);
                                botuser.captcha.image.Save("" + botuser.vkid + ".jpg");
                                vkclient.Messages.Send.TextAndDocument(update.@object.peer_id, "Решите капчу: " + botuser.captcha.math.Replace("+", "%2B"), "" + botuser.vkid + ".jpg", "Captcha");
                            }
                        }
                    }
                }
            }
        }
        public static List<BotUser> BotUsers = new List<BotUser>();
        public static void UpdateUserInfo(Vk.VkLongPoolClient.Update update)
        {
            BotUser botuser = GetUserInfo(update);
            if (botuser != null)
            {
                Console.WriteLine(botuser.last);
                if (botuser.security.current >= botuser.security.maximum)
                {
                    update.@object.text = update.@object.text.Replace("[club190175842|@megacraftbot] ", "");
                    update.@object.text = update.@object.text.Replace("[club190175842|MegaCraft Manager] ", "");
                    update.@object.text = update.@object.text.Replace("[club190175842|SkillShop Technology] ", "");
                    if (update.@object.text != botuser.captcha.otvet.ToString())
                    {
                        if (DateTime.Now >= botuser.last.AddMilliseconds(botuser.resettime))
                        {
                            botuser.security.current = 1;
                            botuser.last = DateTime.Now;
                            botuser.needcaptcha = false;
                        }
                        else
                        {
                            botuser.needcaptcha = true;
                        }
                    }
                    else
                    {
                        botuser.needcaptcha = false;
                        botuser.security.current = 1;
                        botuser.last = DateTime.Now;

                    }
                }
                else
                {
                    botuser.last = DateTime.Now;
                    botuser.security.current++;
                }
            }
            else
            {
                botuser.last = DateTime.Now;
                botuser.security.current++;
            }
        }
        public static bool CalcCaptcha(Vk.VkLongPoolClient.Update update)
        {
            BotUser botuser = GetUserInfo(update);
            if (botuser != null)
            {
                if (botuser.security.current >= botuser.security.maximum)
                {
                    update.@object.text = update.@object.text.Replace("[club190175842|@megacraftbot] ", "");
                    update.@object.text = update.@object.text.Replace("[club190175842|MegaCraft Manager] ", "");
                    update.@object.text = update.@object.text.Replace("[club190175842|SkillShop Technology] ", "");
                    if (update.@object.text != botuser.captcha.otvet.ToString())
                    {
                        if (DateTime.Now >= botuser.last.AddMilliseconds(botuser.resettime))
                        {
                            botuser.security.current = 1;
                            botuser.last = DateTime.Now;
                            botuser.needcaptcha = false;
                            return false;
                        }
                        else
                        {
                            botuser.needcaptcha = true;
                            return true;
                        }
                    }
                    else
                    {
                        botuser.needcaptcha = false;
                        botuser.security.current = 1;
                        botuser.last = DateTime.Now;
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public static BotUser GetUserInfo(Vk.VkLongPoolClient.Update update)
        {
            if (update.type == "message_new")
            {
                bool find = false;
                foreach (BotUser user in BotUsers)
                {
                    if (user.vkid == update.@object.from_id)
                    {
                        find = true;
                        return user;
                    }
                }
                if (find == false)
                {
                    BotUser newuser = new BotUser();
                    newuser.vkid = update.@object.from_id;
                    newuser.last = DateTime.Now;
                    BotUser.BotSecurity newsecurity = new BotUser.BotSecurity();
                    newsecurity.current = 1;
                    newsecurity.maximum = 3;
                    newuser.security = newsecurity;
                    BotUser.Captcha captcha = new BotUser.Captcha();
                    captcha.GenerateMath();
                    newuser.captcha = captcha;
                    newuser.resettime = 1000 * 60;
                    newuser.needcaptcha = false;
                    BotUsers.Add(newuser);
                    return newuser;
                }
            }
            return null;
        }
    }
    public static class Calculator
    {
        // Создадим статический экземпляр DataTable, чтобы каждый раз не инициализировать его заново
        private static DataTable Table = new DataTable();

        // Наш метод подсчета
        // Добавьте отлов ошибок по вкусу)
        public static double Calc(string Expression)
        {
            return Convert.ToDouble(Table.Compute(Expression, string.Empty));;
        }
    }
    public class BotUser
    {
        public Captcha captcha;
        public BotSecurity security;
        public int? vkid;
        public DateTime last;
        public int resettime;
        public bool needcaptcha = false;

        public class Captcha
        {
            Random rnd = new Random();
            public string type = "math";
            public void GenerateType()
            {
                int typen = rnd.Next(1, 2 + 1);
                if (typen == 1)
                {
                    type = "math";
                }
                else if (typen == 2)
                {
                    type = "image";
                }
            }
            public void GenerateMath()
            {
                int typen = rnd.Next(1, 6);
                switch (typen)
                {
                    case 1:
                        math = "" + rnd.Next(1, 10) + " * " + rnd.Next(1, 10) + rnd.Next(1, 10);
                        break;
                    case 2:
                        math = "" + rnd.Next(1, 10) + rnd.Next(1, 10) + " + " + rnd.Next(1, 10) + rnd.Next(1, 10);
                        break;
                    case 3:
                        math = "" + rnd.Next(1, 10) + rnd.Next(1, 10) + " - " + rnd.Next(1, 10) + rnd.Next(1, 10);
                        break;
                    case 4:
                        math = "" + rnd.Next(1, 10) + rnd.Next(1, 10) + " / " + rnd.Next(1, 10) + rnd.Next(1, 10);
                        break;
                    case 5:
                        math = "" + rnd.Next(1, 10) + " * " + rnd.Next(1, 10);
                        break;
                    default:
                        math = "";
                        break;
                }
                int oo = Convert.ToInt32(Calculator.Calc(math));
                if (oo <= 4)
                    GenerateMath();
                else
                {
                    type = "math";
                    otvet = oo;
                }
            }
            public void GenerateImage(int Width, int Height)
            {
                Random rnd = new Random();

                //Создадим изображение
                Bitmap result = new Bitmap(Width, Height);

                //Вычислим позицию текста
                int Xpos = 0;
                int Ypos = 0;

                //Добавим различные цвета
                Brush[] colors = { Brushes.Black,
                     Brushes.Red,
                     Brushes.RoyalBlue,
                     Brushes.Green };

                //Укажем где рисовать
                Graphics g = Graphics.FromImage((Image)result);

                //Пусть фон картинки будет серым
                g.Clear(Color.Gray);

                //Сгенерируем текст
                otvet = 0;
                string oot = "" + rnd.Next(1, 10) + "" + rnd.Next(1, 10) + "" + rnd.Next(1, 10) + "" + rnd.Next(1, 10);
                otvet = int.Parse(oot);

                //Нарисуем сгенирируемый текст
                g.DrawString(otvet.ToString(),
                             new Font("Arial", 15),
                             colors[rnd.Next(colors.Length)],
                             new PointF(Xpos, Ypos));

                //Добавим немного помех
                /////Линии из углов
                g.DrawLine(Pens.Black,
                           new Point(0, 0),
                           new Point(Width - 1, Height - 1));
                g.DrawLine(Pens.Black,
                           new Point(0, Height - 1),
                           new Point(Width - 1, 0));
                ////Белые точки
                for (int i = 0; i < Width; ++i)
                    for (int j = 0; j < Height; ++j)
                        if (rnd.Next() % 20 == 0)
                            result.SetPixel(i, j, Color.White);

                image = result;
            }
            public Bitmap image;
            public string math = "";
            public int otvet;
        }
        public class BotSecurity
        {
            public int maximum;
            public int current;
        }
    }
}
