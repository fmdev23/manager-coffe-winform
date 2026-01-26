using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLQuanCafe
{
    public partial class Form1 : Form
    {
        string connectionString = "Data Source=DESKTOP-4H1FJ6S\\SQLEXPRESS;Initial Catalog=QLQuanCafe;Integrated Security=True";
        public Form1()
        {
            InitializeComponent();
        }

        private void dtgvmoi_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            Sqlconnection con = new SqlConnection(connectionString);
            string a = dtgvmoi.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
        }
    }
}
