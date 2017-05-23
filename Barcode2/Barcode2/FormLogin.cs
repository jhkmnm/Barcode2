using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Barcode2
{
    public partial class FormLogin : Form
    {
        string str_api = "http://hygdata.xinlvs.com";
        string str_login = "/api/print.php?act=login";
        public string Token = "";

        public void Read()
        {
            if (File.Exists("url.txt"))
            {
                StreamReader sr = new StreamReader("url.txt", Encoding.Default);
                str_api = sr.ReadLine();
                sr.Close();
            }
        }

        public FormLogin()
        {
            InitializeComponent();
            Read();
            LoadPw();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (chkR.Checked)
                SavePw();

            var htmlstr = Html.Post(str_api + str_login, string.Format("user_name={0}&password={1}", txtUserName.Text, txtPassword.Text));
            var loginresult = JsonConvert.DeserializeObject<LoginResult>(htmlstr);
            if(loginresult == null)
            {
                MessageBox.Show("登录失败，检查配置文件或服务器");
                return;
            }
            if (loginresult.status == 40000)
            {
                MessageBox.Show(loginresult.message);
                return;
            }
            Token = loginresult.data.token;
            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// 本地保存登录的账号和密码
        /// </summary>
        public void SavePw()
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings["UserName"].Value = txtUserName.Text;
            configuration.AppSettings.Settings["PassWord"].Value = txtPassword.Text;            
            configuration.AppSettings.Settings["chkSavePass"].Value = "True";
            configuration.Save();
        }

        /// <summary>
        /// 读取本地的账号和密码
        /// </summary>
        public void LoadPw()
        {
            if (ConfigurationManager.AppSettings["chkSavePass"] == "True")
            {
                txtUserName.Text = ConfigurationManager.AppSettings["UserName"];
                txtPassword.Text = ConfigurationManager.AppSettings["PassWord"];                
                chkR.Checked = true;
            }
        }
    }
}
