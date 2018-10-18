using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using Blog_IT.Models;
using System.Net;
namespace Blog_IT.Models
{
    public class App_Code
    {
        public static async Task GuiMail(List<string> dsmail, string subject, string displayname, string body)
        {
            BlogITEntities db = new BlogITEntities();
            var smtp = new SmtpClient("smtp.gmail.com", 587);
            var creds = new NetworkCredential("blogit.net@gmail.com", "nguyenaituan181995");

            smtp.UseDefaultCredentials = false;
            smtp.Credentials = creds;
            smtp.EnableSsl = true;
            foreach (string mail in dsmail)
            {
                var to = new MailAddress(mail);
                var from = new MailAddress("blogit.net@gmail.com", displayname);
                var msg = new MailMessage();
                msg.To.Add(to);
                msg.From = from;
                msg.IsBodyHtml = true;
                msg.Subject = subject;
                msg.Body = body;
                await smtp.SendMailAsync(msg);
            }
        }
        public static bool IsValidEmail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        public static string XoaKhoangTrangThua(string s)
        {
            for (int i = 0; i < s.Length - 1; i++)
            {
                if (s[i] == ' ' && s[i + 1] == ' ')
                {
                    s = s.Remove(i, 1);
                    i--;
                }
            }

            return s.Trim();
        }
        public static string VietnameseSigns(string str)
        {
            str = str.Trim();
            var charsToRemove = new string[] { "@", ",", ".", ";", "'", "/", "\\", "\"", "[", "]","#","+","?","-" };
            foreach (var c in charsToRemove)
            {
                str = str.Replace(c, string.Empty);
            }
            const string FindText = "áàảãạâấầẩẫậăắằẳẵặđéèẻẽẹêếềểễệíìỉĩịóòỏõọôốồổỗộơớờởỡợúùủũụưứừửữựýỳỷỹỵÁÀẢÃẠÂẤẦẨẪẬĂẮẰẲẴẶĐÉÈẺẼẸÊẾỀỂỄỆÍÌỈĨỊÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢÚÙỦŨỤƯỨỪỬỮỰÝỲỶỸỴ ";
            const string ReplText = "aaaaaaaaaaaaaaaaadeeeeeeeeeeeiiiiiooooooooooooooooouuuuuuuuuuuyyyyyAAAAAAAAAAAAAAAAADEEEEEEEEEEEIIIIIOOOOOOOOOOOOOOOOOUUUUUUUUUUUYYYYY-";
            int index = -1;
            char[] arrChar = FindText.ToCharArray();
            while ((index = str.IndexOfAny(arrChar)) != -1)
            {
                int index2 = FindText.IndexOf(str[index]);
                str = str.Replace(str[index], ReplText[index2]);
            }

            return str;
        }
        public static string ToUpperFirstCharacter(string s)
        {
            s = s.Trim();
            s = s.ToLower();

            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(s[0]);
            s = new string(a);
            return s;
        }
     
    }
}