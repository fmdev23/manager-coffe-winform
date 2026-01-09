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
            string connection_string = @"Data Source=THINKPADX1\SQLSEVER;Initial Catalog=QLQuanCafe;Integrated Security=True";
            string query = "SELECT userName, displayName, PassWord, Type FROM Account WHERE userName = @userName";

            using(SqlConnection connection = )
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {

        }
    }
}
