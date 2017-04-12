using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Barcode2
{
    public partial class FormLogin : Form
    {
        string str_api = "http://hyg.xinlvs.com";
        string str_login = "/api/print.php?act=login";
        public string Token = "";

        public FormLogin()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            var htmlstr = Html.Post(str_api + str_login, string.Format("user_name={0}&password={1}", txtUserName.Text, txtPassword.Text));
            var loginresult = JsonConvert.DeserializeObject<LoginResult>(htmlstr);
            if (loginresult.status == 40000)
            {
                MessageBox.Show(loginresult.message);
                return;
            }
            Token = loginresult.data.token;
            this.DialogResult = DialogResult.OK;
        }
    }
}
