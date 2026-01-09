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
        }

        private void fAdmin_Load(object sender, EventArgs e)
        {

            string connection_string = @"Data Source=THINKPADX1\SQLSEVER;Initial Catalog=QLQuanCafe;Integrated Security=True";
            SqlConnection connection = new SqlConnection(connection_string);

            string query = "SELECT * FROM Account";
            //connection.Open();

            SqlCommand command = new SqlCommand(query, connection);

            connection.Open();

            SqlDataReader reader = command.ExecuteReader();
            StringBuilder sb = new StringBuilder();
            while (reader.Read())
            {
                string row = "";
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (i == reader.FieldCount - 1)
                    {
                        row += reader.GetValue(i).ToString();
                        break;
                    }
                    else
                    {
                        row += reader.GetValue(i).ToString() + ", ";
                    }
                }
                dtgvAccount.Rows.Add(row);
            }
            connection.Close();

            //DataTable data = new DataTable();

            //SqlDataAdapter adapter = new SqlDataAdapter(command);

            //adapter.Fill(data);
            //connection.Close();

            //dtgvAccount.DataSource = data;
        }

    }
}
