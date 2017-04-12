using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace Barcode2
{
    public partial class Form1 : Form
    {
        string str_api = "http://hyg.xinlvs.com";
        string str_PrintData = "/api/print.php?act=getPrintData";
        string Token;
        Printer printer;

        public Form1(string token)
        {
            Token = token;
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            LoadDevice();
        }

        private void LoadDevice()
        {
            #region 打印机
            PrintDocument print = new PrintDocument();
            string sDefault = print.PrinterSettings.PrinterName;//默认打印机名

            foreach (string sPrint in PrinterSettings.InstalledPrinters)//获取所有打印机名称
            {
                var i = ddlPrinter.Items.Add(sPrint);
                if (sPrint == sDefault)
                    ddlPrinter.SelectedIndex = i;
            }
            #endregion
        }

        private PrintResult GetPrintData()
        {
            var htmlstr = Html.Post(str_api + str_PrintData, string.Format("hyg_token={0}", Token));
            htmlstr = UniconToString(htmlstr);
            return JsonConvert.DeserializeObject<PrintResult>(htmlstr);
        }

        /// <summary>
        /// 将Unicon字符串转成汉字String
        /// </summary>
        /// <param name="str">Unicon字符串</param>
        /// <returns>汉字字符串</returns>
        public string UniconToString(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                System.Text.RegularExpressions.MatchCollection mc = System.Text.RegularExpressions.Regex.Matches(str, "\\\\u([\\w]{4})");
                for (int i = 0; i < mc.Count; i++)
                {
                    var s = ((char)int.Parse(mc[i].Value.Replace("\\u", ""), System.Globalization.NumberStyles.HexNumber)).ToString();
                    str = str.Replace(mc[i].Value, s);
                }
            }
            return str;
        }        

        private void Run()
        {
            var data = GetPrintData();
            if(data.status == 40000)
            {
                return;
            }
            if (printer == null)
            {
                printer = new Printer();
                Thread th = new Thread(printer.Run);
                th.Start();
            }
            printer.Enqueue(data);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(button2.Text == "开始")
            {
                button2.Text = "停止";
                textBox1.Enabled = false;                
                timer1.Enabled = true;
            }
            else
            {
                button2.Text = "开始";
                textBox1.Enabled = true;
                timer1.Enabled = false;
            }
        }        

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            int i = 3;
            int.TryParse(textBox1.Text, out i);
            timer1.Interval = i * 1000;
            Run();
            if(button2.Text == "停止")
                timer1.Enabled = true;
        }
    }

    public class Printer
    {
        Queue<PrintResult> jobList = new Queue<PrintResult>();
        bool isbusy = false;

        public void Enqueue(PrintResult aj)
        {
            isbusy = true;
            jobList.Enqueue(aj);
            isbusy = false;
        }

        public void Run()
        {
            while (true)
            {
                if (!isbusy)
                {
                    if (jobList.Count > 0)
                    {
                        try
                        {
                            PrintContent(jobList.Dequeue());
                        }
                        catch { }
                    }
                }
            }
        }

        private void PrintContent(PrintResult data)
        {
            StringBuilder sb = new StringBuilder();
            var maxLength = 16;
            foreach (var item in data.data)
            {
                sb.Clear();
                sb.AppendFormat("店铺名称({0})\n", item.shop_info.shop_name);
                sb.Append("\n");
                sb.Append("订  单  号：" + item.order_info.order_sn + "\n");
                sb.Append("下单时间：" + item.order_info.add_time + "\n");
                sb.Append("\n");
                sb.Append("品名         数量         单价         金额\n");
                sb.Append("\n");
                foreach (var good in item.goods_list)
                {
                    if (good.goods_name.Length > maxLength)
                    {
                        sb.Append(good.goods_name.Substring(0, maxLength) + "\n");
                        var stra = good.goods_name.Substring(maxLength);
                        if (stra.Length > maxLength)
                        {
                            sb.Append(stra.Substring(0, maxLength) + "\n");
                            stra = stra.Substring(maxLength);
                            sb.Append(stra + "\n");
                        }
                        else
                        {
                            sb.Append(stra + "\n");
                        }
                    }
                    else
                    {
                        sb.Append(good.goods_name + "\n");
                    }
                    var number = "x" + good.goods_number.ToString();
                    var price_padLeft = 21;
                    var price = good.goods_price.ToString();
                    var total_padLeft = 18;
                    var total = good.subtotal.ToString();
                    sb.AppendFormat("                 {0}{1}{2}\n", number, price.PadLeft(price_padLeft - number.Length, ' '), total.PadLeft(total_padLeft - price.Length, ' '));
                }
                sb.Append("\n");
                sb.AppendFormat("配送费{0}\n", item.order_info.shipping_fee.ToString().PadLeft(42, ' '));
                sb.AppendFormat("合计{0}元\n", item.order_info.total.ToString().PadLeft(43, ' '));
                sb.Append("\n");
                sb.Append("备注信息:\n");
                sb.Append(item.order_info.remark);
                for (int i = 0; i < item.shop_info.print_time; i++)
                    Print(sb.ToString());
            }
        }

        private StringReader sr;

        public bool Print(string str)
        {
            bool result = true;
            try
            {
                sr = new StringReader(str);
                PrintDocument pd = new PrintDocument();
                pd.PrintController = new System.Drawing.Printing.StandardPrintController();

                PaperSize pageSize = new PaperSize("First custom size", getYc(70), 2000);
                pd.DefaultPageSettings.PaperSize = pageSize;
                pd.DefaultPageSettings.Margins.Top = 10;
                pd.DefaultPageSettings.Margins.Left = 0;
                pd.PrinterSettings.PrinterName = pd.DefaultPageSettings.PrinterSettings.PrinterName;//默认打印机
                pd.PrintPage += new PrintPageEventHandler(this.pd_PrintPage);
                pd.Print();
            }
            catch (Exception ex)
            {
                result = false;
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }
            return result;
        }

        private void pd_PrintPage(object sender, PrintPageEventArgs ev)
        {
            Font titleFont = new Font("Arial", 14, FontStyle.Bold);//打印字体
            Font printFont = new Font("Arial", 8);//打印字体
            float linesPerPage = 0;
            float yPos = 0;
            float count = 0;
            float leftMargin = ev.MarginBounds.Left;
            float topMargin = ev.MarginBounds.Top;
            String line = "";
            linesPerPage = ev.MarginBounds.Height / printFont.GetHeight(ev.Graphics);

            yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
            ev.Graphics.DrawString("           好宜购\n", titleFont, Brushes.Black,
               leftMargin, yPos, new StringFormat());
            count += 2;

            while (count < linesPerPage && ((line = sr.ReadLine()) != null))
            {
                yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
                ev.Graphics.DrawString(line, printFont, Brushes.Black,
                   leftMargin, yPos, new StringFormat());
                count += 1;
            }
            if (line != null)
                ev.HasMorePages = true;
            else
                ev.HasMorePages = false;
        }

        private int getYc(double cm)
        {
            return (int)(cm / 25.4) * 100;
        }        
    }
}
