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
    public partial class fAdmin : Form
    {
        SqlConnection ketNoi;
        SqlDataAdapter boDocGhi;
        DataSet dsAccount;
        public fAdmin()
        {
            InitializeComponent();
        }        
        void LoadTypeAccount()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Value", typeof(bool));
            dt.Columns.Add("Text", typeof(string));

            dt.Rows.Add(false, "Nhân viên");
            dt.Rows.Add(true, "Admin");

            cbTypeAccount.DataSource = dt;
            cbTypeAccount.DisplayMember = "Text";
            cbTypeAccount.ValueMember = "Value";
        }

        void clearInformation()
        {
            txtUserName.ReadOnly = false;
            txtUserName.Text = "";
            txtDisplayName.Text = "";
            txtPassword.Text = "";
            cbTypeAccount.SelectedIndex = 0;
        }
        private void LoadGridViewAccount()
        {
            string connection_string = @"Data Source=THINKPADX1\SQLSEVER;Initial Catalog=QLQuanCafe;Integrated Security=True";
            ketNoi = new SqlConnection(connection_string);
            string sql = "SELECT * FROM Account";
            boDocGhi = new SqlDataAdapter(sql, ketNoi);
            dsAccount = new DataSet("DSAccount");
            boDocGhi.Fill(dsAccount, "Account");
            dtgvAccount.DataSource = dsAccount.Tables["Account"];
        }

        private void fAdmin_Load(object sender, EventArgs e)
        {
            LoadGridViewAccount();
            LoadTypeAccount();
        }

        private void dtgvAccount_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dtgvAccount.Rows[e.RowIndex];

            txtUserName.Text = row.Cells["userName"].Value.ToString();
            txtDisplayName.Text = row.Cells["displayName"].Value.ToString();
            txtPassword.Text = row.Cells["PassWord"].Value.ToString();
            cbTypeAccount.SelectedValue = Convert.ToBoolean(row.Cells["Type"].Value);

            txtUserName.ReadOnly = true;
        }
        private void btnAddAccount_Click(object sender, EventArgs e)
        {

            if (txtUserName.Text.Length <= 1)
            {
                MessageBox.Show("Username phải nhiều hơn 1 ký tự");
                return;
            }
            else
            {
                string qSelect = "SELECT COUNT(*) FROM Account WHERE userName = @userName";
                SqlCommand boLenh = new SqlCommand(qSelect, ketNoi);
                boLenh.Parameters.AddWithValue("@userName", txtUserName.Text);

                try
                {
                    ketNoi.Open();
                    int checkUserName = (int)boLenh.ExecuteScalar();

                    if (checkUserName != 0)
                    {
                        MessageBox.Show("Username đã tồn tại, vui lòng chọn username khác");
                        clearInformation();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi kiểm tra username: " + ex.Message);
                    return;
                }
                finally
                {
                    if (ketNoi.State == ConnectionState.Open)
                        ketNoi.Close();
                }
            }

            if (txtUserName.Text == "")
            {
                MessageBox.Show("Vui lòng nhập Username");
                return;
            }

            if (txtDisplayName.Text == "")
            {
                MessageBox.Show("Vui lòng nhập tên hiển thị");
                return;
            }

            string qAdd = "INSERT INTO Account (userName, PassWord, displayName, type) " +
                          "VALUES (@userName, @PassWord, @displayName, @Type)";
            boDocGhi.InsertCommand = new SqlCommand(qAdd, ketNoi);
            boDocGhi.InsertCommand.Parameters.AddWithValue("@userName", txtUserName.Text);
            boDocGhi.InsertCommand.Parameters.AddWithValue("@PassWord", txtPassword.Text);
            boDocGhi.InsertCommand.Parameters.AddWithValue("@displayName", txtDisplayName.Text);
            boDocGhi.InsertCommand.Parameters.AddWithValue("@Type", cbTypeAccount.SelectedValue);
            try
            {
                ketNoi.Open();
                boDocGhi.InsertCommand.ExecuteNonQuery();
                MessageBox.Show("Thêm tài khoản thành công");
                LoadGridView();
                clearInformation();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm tài khoản: " + ex.Message);
            }
            finally
            {
                if (ketNoi.State == ConnectionState.Open)
                    ketNoi.Close();
            }
        }

        private void btnDelAccount_Click(object sender, EventArgs e)
        {
            if (txtUserName.Text == "")
            {
                MessageBox.Show("Vui lòng chọn tài khoản cần xóa");
                return;
            }

            DialogResult rs = MessageBox.Show(
                $"Bạn có chắc muốn xóa tài khoản [{txtUserName.Text}]?",
                "Xác nhận",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

            if (rs == DialogResult.No) return;

            string qDel = "DELETE FROM Account WHERE userName = @userName";
            SqlCommand cmdDel = new SqlCommand(qDel, ketNoi);
            cmdDel.Parameters.AddWithValue("@userName", txtUserName.Text);

            try
            {
                ketNoi.Open();
                int kq = cmdDel.ExecuteNonQuery();

                if (kq > 0)
                {
                    MessageBox.Show("Xóa tài khoản thành công");
                    LoadGridView();
                    clearInformation();
                    txtUserName.ReadOnly = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kiểm tra tài khoản: " + ex.Message);
                return;
            }
            finally
            {
                if (ketNoi.State == ConnectionState.Open)
                    ketNoi.Close();
            }
        }

        private void btnEditAccount_Click(object sender, EventArgs e)
        {
            if(txtUserName.Text == "")
            {
                MessageBox.Show("Vui lòng chọn tài khoản cần sửa");
                return;
            }

            if(txtDisplayName.Text == "")
            {
                MessageBox.Show("Vui lòng nhập tên hiển thị");
                return;
            }

            DialogResult rs = MessageBox.Show(
                $"Bạn có chắc muốn sửa thông tin tài khoản [{txtUserName.Text}]?",
                "Xác nhận",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

            if (rs == DialogResult.No) return;

            string qUpdate = @"UPDATE Account SET 
                                displayName = @displayName,
                                PassWord = @PassWord,
                                Type = @Type WHERE userName = @userName";
            
            SqlCommand cmdUpdate = new SqlCommand(qUpdate, ketNoi);
            cmdUpdate.Parameters.AddWithValue("@displayName", txtDisplayName.Text);
            cmdUpdate.Parameters.AddWithValue("@PassWord", txtPassword.Text);
            cmdUpdate.Parameters.AddWithValue("@Type", cbTypeAccount.SelectedValue);
            cmdUpdate.Parameters.AddWithValue("@userName", txtUserName.Text);

            try
            {
                ketNoi.Open();
                int kq = cmdUpdate.ExecuteNonQuery();

                if (kq > 0)
                {
                    MessageBox.Show("Cập nhật tài khoản thành công");
                    LoadGridView();
                    clearInformation();
                    txtUserName.ReadOnly = false;
                }
                else
                {
                    MessageBox.Show("Không tìm thấy tài khoản để cập nhật");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật tài khoản: " + ex.Message);
                return;
            }
            finally
            {
                if (ketNoi.State == ConnectionState.Open)
                    ketNoi.Close();
            }
        }
    }
}
