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
    public partial class fTableManager : Form
    {
        string currentUserName;
        string connection_string_sql = @"Data Source=THINKPADX1\SQLSEVER;Initial Catalog=QLQuanCafe;Integrated Security=True";

        public fTableManager(string userName)
        {
            InitializeComponent();
            currentUserName = userName;
        }

        private void fTableManager_Load(object sender, EventArgs e)
        {
            LoadTable();
        }

        void LoadTable()
        {
            flpTable.Controls.Clear();

            string connection_string = connection_string_sql;

            string query = "SELECT tableID, tableName, tableStatus FROM FoodTable";

            using (SqlConnection connection = new SqlConnection(connection_string))
            {
                SqlCommand cmd = new SqlCommand(query, connection);
                connection.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string tableName = reader["tableName"].ToString();
                    bool tableStatus = Convert.ToBoolean(reader["tableStatus"]);
                    int tableID = (int)reader["tableID"];

                    Button btn = new Button();
                    btn.Width = 80;
                    btn.Height = 80;
                    btn.Margin = new Padding(10);

                    btn.Tag = tableID; // Lưu trữ tableID trong thuộc tính Tag của nút

                    // Text hiển thị
                    if (tableStatus == false) // bàn trống
                    {
                        btn.Text = tableName + Environment.NewLine + "(Trống)";
                        btn.BackColor = Color.LightGreen;
                    }
                    else // bàn có người
                    {
                        btn.Text = tableName + Environment.NewLine + "(Có người)";
                        btn.BackColor = Color.LightGray;
                    }

                    btn.FlatStyle = FlatStyle.Flat;
                    btn.TextAlign = ContentAlignment.MiddleCenter;

                    flpTable.Controls.Add(btn);
                }
            }
        }

        private void adminToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fAdmin f = new fAdmin();
            f.ShowDialog();
        }

        private void smnThongTinCaNhan_Click(object sender, EventArgs e)
        {
            formAccountProfile f = new formAccountProfile(currentUserName);
            f.ShowDialog();
        }

        private void tsmnDangXuat_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lsvBill_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
