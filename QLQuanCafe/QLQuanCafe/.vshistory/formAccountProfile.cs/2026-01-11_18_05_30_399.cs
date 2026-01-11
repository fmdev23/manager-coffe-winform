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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace QLQuanCafe
{
    public partial class formAccountProfile : Form
    {

        string currentUserName;
        string connection_string_sql = @"Data Source=THINKPADX1\SQLSEVER;Initial Catalog=QLQuanCafe;Integrated Security=True";
        public formAccountProfile(string userName)
        {
            InitializeComponent();
            currentUserName = userName;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void formAccountProfile_Load(object sender, EventArgs e)
        {
            string connection_string = connection_string_sql;

            string query = @"SELECT userName, displayName
                            FROM Account
                            WHERE userName = @userName";

            using (SqlConnection connection = new SqlConnection(connection_string))
            {
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@userName", currentUserName);

                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    txtUserName1.Text = reader["userName"].ToString();
                    txtDisplayName.Text = reader["displayName"].ToString();
                }
            }

            txtUserName1.ReadOnly = true;

            // clear password fields
            txtPassword.Clear();
            txtNewPass.Clear();
            txtReEnterPass.Clear();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (txtPassword.Text == "")
            {
                MessageBox.Show("Vui lòng nhập mật khẩu hiện tại");
                return;
            }

            if (txtDisplayName.Text == "")
            {
                MessageBox.Show("Tên hiển thị không được để trống");
                return;
            }

            string connection_string = connection_string_sql;

            using (SqlConnection connection = new SqlConnection(connection_string))
            {
                connection.Open();

                //Kiểm tra mật khẩu hiện tại
                string checkPassQuery =@"SELECT COUNT(*) 
                                        FROM Account 
                                        WHERE userName = @userName AND PassWord = @password";

                SqlCommand checkCmd = new SqlCommand(checkPassQuery, connection);
                checkCmd.Parameters.AddWithValue("@userName", currentUserName);
                checkCmd.Parameters.AddWithValue("@password", txtPassword.Text);

                int check = (int)checkCmd.ExecuteScalar();

                if (check == 0)
                {
                    MessageBox.Show("Mật khẩu hiện tại không đúng");
                    return;
                }

                //Kiểm tra đổi mật khẩu
                bool changePassword =
                    txtNewPass.Text != "" || txtReEnterPass.Text != "";

                if (changePassword && txtNewPass.Text != txtReEnterPass.Text)
                {
                    MessageBox.Show("Mật khẩu mới nhập lại không khớp");
                    return;
                }

                //Update
                string updateQuery = changePassword
                    ? @"UPDATE Account 
                        SET displayName = @displayName, PassWord = @newPassword 
                        WHERE userName = @userName"
                    : @"UPDATE Account 
                        SET displayName = @displayName 
                        WHERE userName = @userName";

                SqlCommand updateCmd = new SqlCommand(updateQuery, connection);
                updateCmd.Parameters.AddWithValue("@displayName", txtDisplayName.Text);
                updateCmd.Parameters.AddWithValue("@userName", currentUserName);

                if (changePassword)
                    updateCmd.Parameters.AddWithValue("@newPassword", txtNewPass.Text);

                int kq = updateCmd.ExecuteNonQuery();

                if (kq > 0)
                {
                    MessageBox.Show("Cập nhật thông tin thành công");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Cập nhật thất bại");
                }
            }
        }

    }
}
