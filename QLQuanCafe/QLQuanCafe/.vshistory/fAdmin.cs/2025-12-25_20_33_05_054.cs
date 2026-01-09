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
            ketNoi.Close();
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
    }
}
