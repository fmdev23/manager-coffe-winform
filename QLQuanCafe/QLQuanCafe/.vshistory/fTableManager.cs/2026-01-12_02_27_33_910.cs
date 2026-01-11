using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Guna.UI2.WinForms;
using System.Windows.Forms;

namespace QLQuanCafe
{
    public partial class fTableManager : Form
    {
        string connection_string_sql = @"Data Source=THINKPADX1\SQLSEVER;Initial Catalog=QLQuanCafe;Integrated Security=True";
        string currentUserName;
        int currentTableID = -1;
        int selectedFoodID = -1;
        int originalQuantity = 0;
        bool isEditingFood = false;


        public fTableManager(string userName)
        {
            InitializeComponent();
            currentUserName = userName;
        }

        private void fTableManager_Load(object sender, EventArgs e) // load form
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
        void BtnTable_Guna_Click(object sender, EventArgs e)
        {
            var btn = sender as Guna2GradientButton;
            if (btn == null || btn.Tag == null) return;

            currentTableID = (int)btn.Tag;

            foreach (Guna2GradientButton b in flpTable.Controls.OfType<Guna2GradientButton>())
                b.BorderThickness = 0;

            btn.BorderThickness = 2;
            btn.BorderColor = Color.White;

            LoadBillByTable(currentTableID);
            LoadEmptyTableToSwitch();
        }

        void LoadBillByTable(int tableID)
        {
            lsvBill.Items.Clear();

            string query = @" SELECT f.foodName, bi.Quantity, f.foodPrice FROM Bill b JOIN BillInfo bi ON b.billID = bi.IDBill JOIN Food f ON bi.IDFood = f.foodID WHERE b.IDTable = @tableID AND b.billStatus = 1";

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
            string query = @"SELECT billID FROM Bill WHERE IDTable = @tableID AND billStatus = 1";

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
            string insert = @"INSERT INTO Bill (IDTable, dateCheckIn, billStatus) OUTPUT INSERTED.billID VALUES (@tableID, GETDATE(), 1)";

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
            string check = @"SELECT Quantity FROM BillInfo WHERE IDBill = @billID AND IDFood = @foodID";

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
                    string update = @"UPDATE BillInfo SET Quantity = Quantity + @qty WHERE IDBill = @billID AND IDFood = @foodID";

                    SqlCommand up = new SqlCommand(update, conn);
                    up.Parameters.AddWithValue("@qty", quantity);
                    up.Parameters.AddWithValue("@billID", billID);
                    up.Parameters.AddWithValue("@foodID", foodID);
                    up.ExecuteNonQuery();
                }
                else
                {
                    // chưa có → insert
                    string insert = @"INSERT INTO BillInfo (IDBill, IDFood, Quantity) VALUES (@billID, @foodID, @qty)";

                    SqlCommand ins = new SqlCommand(insert, conn);
                    ins.Parameters.AddWithValue("@billID", billID);
                    ins.Parameters.AddWithValue("@foodID", foodID);
                    ins.Parameters.AddWithValue("@qty", quantity);
                    ins.ExecuteNonQuery();
                }
            }
        }

        void LoadEmptyTableToSwitch() // load bàn trống vào combobox chuyển bàn
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
        void LoadFoodByCategory(int cateID) // load món theo danh mục
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

            string query = @"
    SELECT ft.tableID, ft.tableName,
        CASE 
            WHEN EXISTS (
                SELECT 1 FROM Bill b 
                WHERE b.IDTable = ft.tableID AND b.billStatus = 1
            ) THEN 1 ELSE 0
        END AS IsOccupied,
        ISNULL((
            SELECT SUM(bi.Quantity * f.foodPrice)
            FROM Bill b
            JOIN BillInfo bi ON b.billID = bi.IDBill
            JOIN Food f ON bi.IDFood = f.foodID
            WHERE b.IDTable = ft.tableID AND b.billStatus = 1
        ), 0) AS TotalAmount
    FROM FoodTable ft";

            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    int tableID = (int)rd["tableID"];
                    string tableName = rd["tableName"].ToString();
                    bool isOccupied = Convert.ToBoolean(rd["IsOccupied"]);
                    decimal total = Convert.ToDecimal(rd["TotalAmount"]);

                    Guna2GradientButton btn = new Guna2GradientButton();

                    btn.Parent = flpTable;
                    btn.HoverState.Parent = btn;
                    btn.DisabledState.Parent = btn;
                    btn.CheckedState.Parent = btn;

                    btn.Enabled = true;
                    btn.Visible = true;
                    btn.Cursor = Cursors.Hand;


                    btn.Width = 120;
                    btn.Height = 90;
                    btn.Margin = new Padding(10);
                    btn.Tag = tableID;

                    btn.BorderRadius = 14;
                    btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                    btn.TextAlign = HorizontalAlignment.Center;

                    //btn.ShadowDecoration.Enabled = true;
                    //btn.ShadowDecoration.Depth = 6;
                    //btn.ShadowDecoration.Color = Color.FromArgb(60, 0, 0, 0);

                    if (isOccupied)
                    {
                        btn.Text = $"{tableName}\nCó người\n{total:N0} VND";

                        btn.FillColor = Color.FromArgb(203, 213, 225); // slate
                        btn.FillColor2 = Color.FromArgb(148, 163, 184);
                        btn.ForeColor = Color.Black;
                    }
                    else
                    {
                        btn.Text = $"{tableName}\nTrống";

                        btn.FillColor = Color.FromArgb(209, 250, 229); // xanh mint
                        btn.FillColor2 = Color.FromArgb(167, 243, 208);
                        btn.ForeColor = Color.Black;
                    }

                    btn.Click += BtnTable_Guna_Click;

                    flpTable.Controls.Add(btn);
                }
            }
        }



        private void cbCategory_SelectedIndexChanged(object sender, EventArgs e) // load món khi chọn danh mục
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

        private void btnAddFood_Click(object sender, EventArgs e) // thêm món vào bill
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

        private void btnSwitchTable_Click(object sender, EventArgs e) // chuyển bàn
        {
            if (currentTableID == -1)
            {
                MessageBox.Show("Vui lòng chọn bàn cần chuyển");
                return;
            }

            if (cbSwitchTable.SelectedValue == null || !(cbSwitchTable.SelectedValue is int)) return; // không chọn bàn chuyển

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

            string query = @"UPDATE Bill SET IDTable = @newTableID WHERE IDTable = @currentTableID AND billStatus = 1";

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

        private void btnUpdateFood_Click(object sender, EventArgs e) // cập nhật số lượng món
        {
            if (currentTableID == -1 || selectedFoodID == -1) return;

            if (!isEditingFood)
            {
                MessageBox.Show("Chưa có thay đổi để cập nhật");
                return;
            }

            int newQty = (int)nmFoodCount.Value;
            if (newQty <= 0)
            {
                MessageBox.Show("Số lượng phải lớn hơn 0");
                return;
            }

            int billID = GetOrCreateBill(currentTableID);

            string update = @"UPDATE BillInfo SET Quantity = @qty WHERE IDBill = @billID AND IDFood = @foodID";

            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                SqlCommand cmd = new SqlCommand(update, conn);
                cmd.Parameters.AddWithValue("@qty", newQty);
                cmd.Parameters.AddWithValue("@billID", billID);
                cmd.Parameters.AddWithValue("@foodID", selectedFoodID);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            LoadBillByTable(currentTableID);
            LoadTable();

            btnUpdateFood.Text = "Sửa món";
            isEditingFood = false;
        }

        private void btnDelFood_Click(object sender, EventArgs e)
        {
            if (currentTableID == -1 || selectedFoodID == -1) return;

            DialogResult rs = MessageBox.Show(
                "Bạn có chắc muốn xóa món này?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (rs == DialogResult.No) return;

            int billID = GetOrCreateBill(currentTableID);

            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                conn.Open();

                //Xóa món
                string qDelFood = @"DELETE FROM BillInfo WHERE IDBill = @billID AND IDFood = @foodID";
                SqlCommand del = new SqlCommand(qDelFood, conn);

                del.Parameters.AddWithValue("@billID", billID);
                del.Parameters.AddWithValue("@foodID", selectedFoodID);
                del.ExecuteNonQuery();

                //Kiểm tra còn món không
                string qCheckFood = @"SELECT COUNT(*) FROM BillInfo WHERE IDBill = @billID";
                SqlCommand check = new SqlCommand(qCheckFood, conn);

                check.Parameters.AddWithValue("@billID", billID);
                int count = (int)check.ExecuteScalar();

                //Nếu hết món → đóng bill
                if (count == 0)
                {
                    string qCloseBill = @"UPDATE Bill SET billStatus = 0 WHERE billID = @billID";
                    SqlCommand closeBill = new SqlCommand(qCloseBill, conn);

                    closeBill.Parameters.AddWithValue("@billID", billID);
                    closeBill.ExecuteNonQuery();
                }
            }

            // reset UI
            selectedFoodID = -1;
            originalQuantity = 0;
            nmFoodCount.Value = 0;
            cbFood.DataSource = null;

            LoadBillByTable(currentTableID);
            LoadTable();
        }

        private void lsvBill_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lsvBill.SelectedItems.Count == 0) return;

            ListViewItem item = lsvBill.SelectedItems[0];

            string foodName = item.SubItems[0].Text;
            int quantity = int.Parse(item.SubItems[1].Text);

            LoadFoodInfoByName(foodName);

            nmFoodCount.Value = quantity;

            originalQuantity = quantity;
            isEditingFood = false;

            btnUpdateFood.Text = "Sửa món";
        }

        void LoadFoodInfoByName(string foodName) // load thông tin món theo tên món
        {
            string query = @"SELECT f.foodID, f.foodName, f.IDCategory FROM Food f WHERE f.foodName = @name";

            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", foodName);

                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                if (rd.Read())
                {
                    selectedFoodID = (int)rd["foodID"];
                    int cateID = (int)rd["IDCategory"];

                    cbCategory.SelectedValue = cateID;
                    LoadFoodByCategory(cateID);
                    cbFood.SelectedValue = selectedFoodID;
                }
            }
        }

        void FoodField_Changed(object sender, EventArgs e) // theo dõi thay đổi số lượng món
        {
            if (selectedFoodID == -1) return;

            bool changed =
                (int)nmFoodCount.Value != originalQuantity;

            btnUpdateFood.Text = changed ? "Cập nhật" : "Sửa món";
            isEditingFood = changed;
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

        private void btnCheckOut_Click(object sender, EventArgs e)
        {
            if (currentTableID == -1)
            {
                MessageBox.Show("Vui lòng chọn bàn cần thanh toán");
                return;
            }

            int billID = -1;
            decimal totalAmount = 0;

            string qGetBill = @"SELECT b.billID,
                                ISNULL(SUM(bi.Quantity * f.foodPrice), 0) AS Total
                                FROM Bill b
                                LEFT JOIN BillInfo bi ON b.billID = bi.IDBill
                                LEFT JOIN Food f ON bi.IDFood = f.foodID
                                WHERE b.IDTable = @tableID AND b.billStatus = 1
                                GROUP BY b.billID";

            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                SqlCommand cmd = new SqlCommand(qGetBill, conn);
                cmd.Parameters.AddWithValue("@tableID", currentTableID);
                conn.Open();

                SqlDataReader rd = cmd.ExecuteReader();
                if (!rd.Read())
                {
                    MessageBox.Show("Bàn này chưa có bill để thanh toán");
                    return;
                }

                billID = (int)rd["billID"];
                totalAmount = Convert.ToDecimal(rd["Total"]);
            }

            DialogResult rs = MessageBox.Show(
                $"Bạn có chắc muốn thanh toán cho bàn {currentTableID}?\n\n" +
                $"Tổng tiền: {totalAmount:N0} VND",
                "Xác nhận thanh toán",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (rs == DialogResult.No) return;

            // ĐÓNG BILL
            string qCheckout = @"
        UPDATE Bill
        SET billStatus = 0,
            dateCheckOut = GETDATE()
        WHERE billID = @billID";

            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                SqlCommand cmd = new SqlCommand(qCheckout, conn);
                cmd.Parameters.AddWithValue("@billID", billID);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            // === RESET UI ===
            MessageBox.Show("Thanh toán thành công!", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);

            currentTableID = -1;
            lsvBill.Items.Clear();
            cbFood.DataSource = null;
            nmFoodCount.Value = 0;

            LoadTable();              // bàn về Trống
            LoadEmptyTableToSwitch(); // refresh bàn trống
        }
    }
}
