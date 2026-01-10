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
        int currentTableID = -1;
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
            if (btn == null || btn.Tag == null) return;

            currentTableID = (int)btn.Tag;

            foreach (Button b in flpTable.Controls.OfType<Button>())
                b.FlatAppearance.BorderSize = 0;

            btn.FlatAppearance.BorderSize = 2;
            btn.FlatAppearance.BorderColor = Color.Red;

            LoadBillByTable(currentTableID);
            LoadEmptyTableToSwitch();
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

                DataRow row = dt.NewRow();
                row["foodCateID"] = -1;
                row["foodCateName"] = "-- Chọn danh mục --";
                dt.Rows.InsertAt(row, 0);

                cbCategory.DataSource = dt;
                cbCategory.DisplayMember = "foodCateName";
                cbCategory.ValueMember = "foodCateID";
            }
            cbCategory.SelectedIndex = 0;
            cbFood.DataSource = null; // reset food
        }

        int GetOrCreateBill(int tableID)
        {
            string query = @"SELECT billID FROM Bill 
                     WHERE IDTable = @tableID AND billStatus = 1";

            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@tableID", tableID);
                conn.Open();

                object result = cmd.ExecuteScalar();

                if (result != null)
                    return (int)result;
            }

            // chưa có bill → tạo mới
            string insert = @"INSERT INTO Bill (IDTable, dateCheckIn, billStatus)
                      OUTPUT INSERTED.billID
                      VALUES (@tableID, GETDATE(), 1)";

            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                SqlCommand cmd = new SqlCommand(insert, conn);
                cmd.Parameters.AddWithValue("@tableID", tableID);
                conn.Open();

                return (int)cmd.ExecuteScalar();
            }
        }

        void AddOrUpdateBillInfo(int billID, int foodID, int quantity)
        {
            string check = @"SELECT Quantity FROM BillInfo 
                     WHERE IDBill = @billID AND IDFood = @foodID";

            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                SqlCommand cmd = new SqlCommand(check, conn);
                cmd.Parameters.AddWithValue("@billID", billID);
                cmd.Parameters.AddWithValue("@foodID", foodID);

                conn.Open();
                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    // đã có → cộng thêm
                    string update = @"UPDATE BillInfo 
                              SET Quantity = Quantity + @qty
                              WHERE IDBill = @billID AND IDFood = @foodID";

                    SqlCommand up = new SqlCommand(update, conn);
                    up.Parameters.AddWithValue("@qty", quantity);
                    up.Parameters.AddWithValue("@billID", billID);
                    up.Parameters.AddWithValue("@foodID", foodID);
                    up.ExecuteNonQuery();
                }
                else
                {
                    // chưa có → insert
                    string insert = @"INSERT INTO BillInfo (IDBill, IDFood, Quantity)
                              VALUES (@billID, @foodID, @qty)";

                    SqlCommand ins = new SqlCommand(insert, conn);
                    ins.Parameters.AddWithValue("@billID", billID);
                    ins.Parameters.AddWithValue("@foodID", foodID);
                    ins.Parameters.AddWithValue("@qty", quantity);
                    ins.ExecuteNonQuery();
                }
            }
        }

        void LoadEmptyTableToSwitch()
        {
            string query = @" SELECT ft.tableID, ft.tableName FROM FoodTable ft WHERE NOT EXISTS (SELECT 1 FROM Bill b WHERE b.IDTable = ft.tableID AND b.billStatus = 1)";

            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // thêm dòng giả
                DataRow row = dt.NewRow();
                row["tableID"] = -1;
                row["tableName"] = "-- Chọn bàn trống --";
                dt.Rows.InsertAt(row, 0);

                cbSwitchTable.DataSource = dt;
                cbSwitchTable.DisplayMember = "tableName";
                cbSwitchTable.ValueMember = "tableID";
            }

            cbSwitchTable.SelectedIndex = 0;
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

        private void cbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbCategory.SelectedValue == null) return;
            if (!(cbCategory.SelectedValue is int)) return;

            int cateID = (int)cbCategory.SelectedValue;

            if (cateID == -1)
            {
                cbFood.DataSource = null; // chưa chọn danh mục
                return;
            }

            LoadFoodByCategory(cateID);
        }

        private void btnAddFood_Click(object sender, EventArgs e)
        {
            if (currentTableID == -1)
            {
                MessageBox.Show("Vui lòng chọn bàn trước khi thêm món");
                return;
            }

            if (cbFood.SelectedValue == null) return;

            int foodID = (int)cbFood.SelectedValue;
            int quantity = (int)nmFoodCount.Value;

            if (quantity <= 0)
            {
                MessageBox.Show("Số lượng phải lớn hơn 0");
                return;
            }

            int billID = GetOrCreateBill(currentTableID);

            AddOrUpdateBillInfo(billID, foodID, quantity);

            LoadBillByTable(currentTableID);
            LoadTable(); // cập nhật trạng thái bàn
        }

        private void btnSwitchTable_Click(object sender, EventArgs e)
        {
            if (currentTableID == -1)
            {
                MessageBox.Show("Vui lòng chọn bàn cần chuyển");
                return;
            }

            if (cbSwitchTable.SelectedValue == null ||
                !(cbSwitchTable.SelectedValue is int))
                return;

            int newTableID = (int)cbSwitchTable.SelectedValue;

            if (newTableID == -1)
            {
                MessageBox.Show("Vui lòng chọn bàn trống để chuyển");
                return;
            }

            if (newTableID == currentTableID)
            {
                MessageBox.Show("Bàn chuyển phải khác bàn hiện tại");
                return;
            }

            DialogResult rs = MessageBox.Show(
                "Bạn có chắc muốn chuyển bàn không?",
                "Xác nhận chuyển bàn",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (rs == DialogResult.No) return;

            string query = @"
        UPDATE Bill 
        SET IDTable = @newTableID
        WHERE IDTable = @currentTableID AND billStatus = 1
    ";

            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@newTableID", newTableID);
                cmd.Parameters.AddWithValue("@currentTableID", currentTableID);

                conn.Open();
                int affected = cmd.ExecuteNonQuery();

                if (affected > 0)
                {
                    MessageBox.Show("Chuyển bàn thành công");

                    currentTableID = newTableID;

                    LoadTable();
                    LoadBillByTable(currentTableID);
                    LoadEmptyTableToSwitch();
                }
                else
                {
                    MessageBox.Show("Bàn đang trống, bạn có thể order trực tiếp");
                }
            }
        }
    }
}
