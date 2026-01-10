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
            LoadCategory();

            lsvBill.View = View.Details;
            lsvBill.FullRowSelect = true;
            lsvBill.GridLines = true;

            lsvBill.Columns.Clear();
            lsvBill.Columns.Add("Tên món", 150);
            lsvBill.Columns.Add("Số lượng", 80);
            lsvBill.Columns.Add("Đơn giá", 100);
            lsvBill.Columns.Add("Thành tiền", 120);

        }
        void BtnTable_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int tableID = (int)btn.Tag;
            LoadBillByTable(tableID);
        }

        void LoadBillByTable(int tableID)
        {
            lsvBill.Items.Clear();

            string query = @"
                        SELECT f.foodName, bi.Quantity, f.foodPrice
                        FROM Bill b
                        JOIN BillInfo bi ON b.billID = bi.IDBill
                        JOIN Food f ON bi.IDFood = f.foodID
                        WHERE b.IDTable = @tableID AND b.billStatus = 1";

            using (SqlConnection connection = new SqlConnection(connection_string_sql))
            {
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@tableID", tableID);

                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string foodName = reader["foodName"].ToString();
                    int quantity = (int)reader["Quantity"];
                    float price = Convert.ToSingle(reader["foodPrice"]);
                    float total = quantity * price;

                    ListViewItem item = new ListViewItem(foodName);
                    item.SubItems.Add(quantity.ToString());
                    item.SubItems.Add(price.ToString("N0"));
                    item.SubItems.Add(total.ToString("N0"));

                    lsvBill.Items.Add(item);
                }
                lsvBill.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
        }

        void LoadCategory()
        {
            string query = "SELECT foodCateID, foodCateName FROM FoodCategory";

            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cbCategory.DataSource = dt;
                cbCategory.DisplayMember = "foodCateName";
                cbCategory.ValueMember = "foodCateID";
            }
        }

        void LoadFoodByCategory(int cateID)
        {
            string query = "SELECT foodID, foodName FROM Food WHERE IDCategory = @id";

            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", cateID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cbFood.DataSource = dt;
                cbFood.DisplayMember = "foodName";
                cbFood.ValueMember = "foodID";
            }
        }

        void LoadTable()
        {
            flpTable.Controls.Clear();

            string query = @"SELECT ft.tableID, ft.tableName,
                                CASE 
                                    WHEN EXISTS (
                                        SELECT 1 
                                        FROM Bill b 
                                        WHERE b.IDTable = ft.tableID AND b.billStatus = 1
                                    )
                                    THEN 1
                                    ELSE 0
                                END AS IsOccupied
                            FROM FoodTable ft";

            using (SqlConnection connection = new SqlConnection(connection_string_sql))
            {
                SqlCommand cmd = new SqlCommand(query, connection);
                connection.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int tableID = (int)reader["tableID"];
                    string tableName = reader["tableName"].ToString();
                    bool isOccupied = Convert.ToBoolean(reader["IsOccupied"]);

                    Button btn = new Button();
                    btn.Width = 80;
                    btn.Height = 80;
                    btn.Margin = new Padding(10);

                    // GẮN tableID
                    btn.Tag = tableID;

                    btn.Click += BtnTable_Click;

                    // Text hiển thị
                    if (isOccupied)
                    {
                        btn.Text = tableName + Environment.NewLine + "(Có người)";
                        btn.BackColor = Color.LightGray;
                    }
                    else
                    {
                        btn.Text = tableName + Environment.NewLine + "(Trống)";
                        btn.BackColor = Color.LightGreen;
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

        private void cbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbCategory.SelectedItem == null) return;
            
            // CHỐNG DataRowView
            if (!(cbCategory.SelectedValue is int)) return;

            int cateID = (int)cbCategory.SelectedValue;
            LoadFoodByCategory(cateID);
        }

        private void cbFood_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void nmFoodCount_ValueChanged(object sender, EventArgs e)
        {

        }

        private void btnAddFood_Click(object sender, EventArgs e)
        {

        }

        private void cbSwitchTable_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnSwitchTable_Click(object sender, EventArgs e)
        {

        }
    }
}
