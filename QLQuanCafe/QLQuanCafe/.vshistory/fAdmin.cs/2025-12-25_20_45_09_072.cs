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

        private void LoadGridView()
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
            LoadGridView();
        }

        private void btnShowAccount_Click(object sender, EventArgs e)
        {
            string connection_string = @"Data Source=THINKPADX1\SQLSEVER;Initial Catalog=QLQuanCafe;Integrated Security=True";
            SqlConnection connection = new SqlConnection(connection_string);

            string query = "SELECT userName, PassWord FROM Account";
            connection.Open();

            SqlCommand command = new SqlCommand(query, connection);

            DataTable data = new DataTable();

            SqlDataAdapter adapter = new SqlDataAdapter(command);

            adapter.Fill(data);
            connection.Close();

            dtgvAccount.DataSource = data;
        }

        private void dtgvAccount_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int i = e.RowIndex;
            txtUserName.Text = dtgvAccount.Rows[i].Cells[0].Value.ToString();
            txtDisplayName.Text = dtgvAccount.Rows[i].Cells[2].Value.ToString();
            txtPassword.Text = dtgvAccount.Rows[i].Cells[1].Value.ToString();
            cbTypeAccount.Text = dtgvAccount.Rows[i].Cells[3].Value.ToString();
        }

        private void btnAddAccount_Click(object sender, EventArgs e)
        {
            if (txtUserName.Text.Length != 5)
            {
                MessageBox.Show("Username phải đúng 5 ký tự");
                return;
            }
            else
            {
                string qSelect = "SELECT COUNT(*) FROM Account WHERE userName = '" + txtUserName.Text + "'";
                SqlCommand boLenh = new SqlCommand(qSelect, ketNoi);
                try
                {
                    ketNoi.Open();
                    int checkUserName = (int)boLenh.ExecuteScalar();

                    if (checkUserName != 0)
                    {
                        MessageBox.Show("Username đã tồn tại, vui lòng chọn username khác");
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
            
            if(txtUserName.Text == "") { 
                MessageBox.Show("Vui lòng nhập Username");
                return;
            }

            if(txtDisplayName.Text == "")
            {
                MessageBox.Show("Vui lòng nhập tên hiển thị");
                return;
            }

            string qAdd = "INSERT INTO Account (userName, PassWord, displayName, typeAccount) " +
                          "VALUES (@userName, @PassWord, @displayName, @typeAccount)";
            boDocGhi.InsertCommand
        }
    }
}
