using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QwatchTSWRLP
{
    public partial class FormMain : Form
    {
        public static readonly MySecretParameter SEC = new MySecretParameter();

        public FormMain()
        {
            InitializeComponent();
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            var http = WebRequest.Create($"{SEC.Url}/snapshot.jpg");
            http.Method = "GET";
            http.ContentType = "image/jpeg";
            http.Credentials = new NetworkCredential(SEC.UserName, SEC.Password);

            var res = http.GetResponse();
            pictureBox1.Image = Image.FromStream(res.GetResponseStream(), true);
        }
    }
}
