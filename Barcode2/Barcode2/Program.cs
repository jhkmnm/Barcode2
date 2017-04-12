using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Windows.Forms;

namespace Barcode2
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            FormLogin login = new FormLogin();
            if (login.ShowDialog() == DialogResult.OK)
            { 
                Application.Run(new Form1(login.Token));
            }
        }
    }

    public class Html
    {
        public static string Post(string url, string postdata)
        {
            Encoding myEncoding = Encoding.UTF8;
            string sContentType = "application/x-www-form-urlencoded";
            HttpWebRequest req;

            try
            {
                req = WebRequest.Create(url) as HttpWebRequest;
                req.Method = "POST";
                req.Accept = "*/*";
                req.KeepAlive = false;
                req.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

                byte[] bufPost = myEncoding.GetBytes(postdata);
                req.ContentType = sContentType;
                req.ContentLength = bufPost.Length;
                Stream newStream = req.GetRequestStream();
                newStream.Write(bufPost, 0, bufPost.Length);
                newStream.Close();

                HttpWebResponse res = req.GetResponse() as HttpWebResponse;
                try
                {
                    Encoding encoding = Encoding.UTF8;
                    System.Diagnostics.Debug.WriteLine(encoding);

                    using (Stream resStream = res.GetResponseStream())
                    {
                        using (StreamReader resStreamReader = new StreamReader(resStream, encoding))
                        {
                            return resStreamReader.ReadToEnd();
                        }
                    }
                }
                finally
                {
                    res.Close();
                }
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }

    public class PostResult
    {
        public int status { get; set; }
        public string message { get; set; }
    }

    public class LoginToken { public string token { get; set; } }

    public class LoginResult : PostResult
    {
        public LoginToken data { get; set; }
    }

    public class PrintResult : PostResult
    {
        public List<PrintData> data { get; set; }
    }

    public class PrintData
    {
        public List<Goods> goods_list { get; set; }
        public Order order_info { get; set; }
        public Shop shop_info { get; set; }
    }

    public class Goods
    {
        public string goods_name { get; set; }
        public float goods_price { get; set; }
        public int goods_number { get; set; }
        public float subtotal { get; set; }
    }

    public class Order
    {
        public string order_sn { get; set; }
        public string add_time { get; set; }
        public float shipping_fee { get; set; }
        public float total { get; set; }
        public string remark { get; set; }
    }

    public class Shop
    {
        public string shop_name { get; set; }
        public int print_time { get; set; }
    }
}
