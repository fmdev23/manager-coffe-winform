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
            string connection_string =
                @"Data Source=THINKPADX1\SQLSEVER;
          Initial Catalog=QLQuanCafe;
          Integrated Security=True";

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
                    txtUserName.Text = reader["userName"].ToString();
                    txtDisplayName.Text = reader["displayName"].ToString();
                }
            }

            txtUserName.ReadOnly = true;

            // clear password fields
            txtPassword.Clear();
            txtNewPass.Clear();
            txtReEnterPass.Clear();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {

        }
    }
}
