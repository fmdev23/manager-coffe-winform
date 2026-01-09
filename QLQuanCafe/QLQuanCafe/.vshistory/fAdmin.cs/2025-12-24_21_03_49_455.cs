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
        public fAdmin()
        {
            InitializeComponent();
            InitUI();
        }

        private void fAdmin_Load(object sender, EventArgs e)
        {
            string connection_string = @"Data Source=THINKPADX1\SQLSEVER;Initial Catalog=QLQuanCafe;Integrated Security=True";
            SqlConnection connection = new SqlConnection(connection_string);

            string query = "SELECT * FROM Account";
            connection.Open();

            SqlCommand command = new SqlCommand(query, connection);

            DataTable data = new DataTable();

            SqlDataAdapter adapter = new SqlDataAdapter(command);

            adapter.Fill(data);
            connection.Close();

            dtgvAccount.DataSource = data;
        }

        void InitUI()
        {
            this.Text = "Quản lý tài khoản";
            this.Size = new Size(800, 500);

            Label lblTitle = new Label();
            lblTitle.Text = "DANH SÁCH TÀI KHOẢN";
            lblTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 20);

            DataGridView dgv = new DataGridView();
            dgv.Name = "dtgvAccount";
            dgv.Location = new Point(20, 70);
            dgv.Size = new Size(740, 350);
            dgv.ReadOnly = true;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            this.Controls.Add(lblTitle);
            this.Controls.Add(dgv);
        }
    }
}
