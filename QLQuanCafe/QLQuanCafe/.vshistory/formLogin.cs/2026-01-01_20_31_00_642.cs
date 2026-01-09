using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLQuanCafe
{
    public partial class formLogin : Form
    {
        public formLogin()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string user = Login(txtUserName.Text, txtPassword.Text);

            if (user != null)
            {
                formAccountProfile f = new formAccountProfile(user);
                this.Hide();
                f.ShowDialog();
                this.Show();
            }
            else
            {
                MessageBox.Show("Sai tài khoản hoặc mật khẩu");
                txtPassword.Clear();
                txtUserName.Focus();
            }
        }


        string Login(string userName, string password)
        {
            string connection_string =
                @"Data Source=THINKPADX1\SQLSEVER;
          Initial Catalog=QLQuanCafe;
          Integrated Security=True";

            string query = @"SELECT userName 
                     FROM Account 
                     WHERE userName = @userName AND PassWord = @password";

            using (SqlConnection connection = new SqlConnection(connection_string))
            {
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@userName", userName);
                cmd.Parameters.AddWithValue("@password", password);

                connection.Open();
                object result = cmd.ExecuteScalar();
                return result == null ? null : result.ToString();
            }
        }


        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void formLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(MessageBox.Show("Bạn có thực sự muốn thoát chương trình", "THÔNG BÁO", MessageBoxButtons.OKCancel) != System.Windows.Forms.DialogResult.OK)
            {
                e.Cancel = true;
            }
        }
    }
}
