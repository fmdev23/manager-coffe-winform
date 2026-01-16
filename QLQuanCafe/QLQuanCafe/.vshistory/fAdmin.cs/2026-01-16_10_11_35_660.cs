using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLQuanCafe
{
    public partial class fAdmin : Form
    {
        // KẾT NỐI SQL SERVER
        string connection_string_sql = @"Data Source=THINKPADX1\SQLSEVER;Initial Catalog=QLQuanCafe;Integrated Security=True";

        // BIẾN LƯU GIÁ TRỊ BAN ĐẦU KHI CHỌN 1 ACCOUNT
        string _originDisplayName;
        string _originPassword;
        bool _originType;

        // BIẾN LƯU GIÁ TRỊ BAN ĐẦU KHI CHỌN 1 TABLE
        string _originTableName;
        bool _originTableStatus;
        bool _isAddingTable = false;

        // BIẾN LƯU TRẠNG THÁI THÊM MÓN ĂN
        string _foodImagePath = "";
        bool _isAddingFood = false;

        // BIẾN LƯU GIÁ TRỊ BAN ĐẦU KHI CHỌN 1 FOOD
        string _originFoodName;
        float _originFoodPrice;
        int _originFoodCategory;
        string _originFoodImage;

        // BIẾN LƯU GIÁ TRỊ BAN ĐẦU KHI CHỌN 1 CATEGORY
        string _originCategoryName;
        bool _isAddingCategory = false;


        // KẾT NỐI SQL SERVER
        SqlConnection ketNoi;
        SqlDataAdapter boDocGhi;
        DataSet dsAccount;
        public fAdmin()
        {
            InitializeComponent();
        }        
        private void fAdmin_Load(object sender, EventArgs e)
        {
            LoadGridViewAccount();
            LoadGridViewFoodTable();
            LoadTypeAccount();
            LoadTableStatus();
            LoadFoodCategory();
            LoadFood();
            LoadGridViewCategory();
            this.rpvDoanhThu.RefreshReport();
        }
        //XU LY ACCOUNT -----------------------------------------------------------------------------
        void LoadTypeAccount() // load loại tài khoản vào combobox
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Value", typeof(bool));
            dt.Columns.Add("Text", typeof(string));

            dt.Rows.Add(false, "Nhân viên");
            dt.Rows.Add(true, "Admin");

            cbTypeAccount.DataSource = dt;
            cbTypeAccount.DisplayMember = "Text";
            cbTypeAccount.ValueMember = "Value";
        }    
        
        void clearInformation() // xóa trắng thông tin trên account
        {
            txtUserName.ReadOnly = false;
            txtUserName.Text = "";
            txtDisplayName.Text = "";
            txtPassword.Text = "";
            cbTypeAccount.SelectedIndex = 0;
        }
        
        void AccountField_Changed(object sender, EventArgs e) // theo dõi thay đổi trên form account
        {
            if (txtUserName.Text == "") return; // chưa chọn account

            bool isChanged =
                txtDisplayName.Text != _originDisplayName ||
                txtPassword.Text != _originPassword ||
                (bool)cbTypeAccount.SelectedValue != _originType;

            btnEditAccount.Text = isChanged ? "Cập nhật" : "Sửa tài khoản";
        }

        private void LoadGridViewAccount() // load dữ liệu account từ DB lên DataGridView
        {
            string connection_string = connection_string_sql;
            ketNoi = new SqlConnection(connection_string);
            string sql = "SELECT * FROM Account";
            boDocGhi = new SqlDataAdapter(sql, ketNoi);
            dsAccount = new DataSet("DSAccount");
            boDocGhi.Fill(dsAccount, "Account");
            dtgvAccount.DataSource = dsAccount.Tables["Account"];

            //doi ten cot
            dtgvAccount.Columns["userName"].HeaderText = "Tên đăng nhập";
            dtgvAccount.Columns["displayName"].HeaderText = "Tên hiển thị";
            dtgvAccount.Columns["PassWord"].HeaderText = "Mật khẩu";
            dtgvAccount.Columns["Type"].HeaderText = "Loại tài khoản";
        }
        private void txbFindAccountName_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnFindAccount_Click(object sender, EventArgs e)
        {

        }

        private void dtgvAccount_CellClick(object sender, DataGridViewCellEventArgs e) // khi click vào 1 dòng trong datagridview account
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dtgvAccount.Rows[e.RowIndex];

            txtUserName.Text = row.Cells["userName"].Value.ToString();
            txtDisplayName.Text = row.Cells["displayName"].Value.ToString();
            txtPassword.Text = row.Cells["PassWord"].Value.ToString();
            cbTypeAccount.SelectedValue = Convert.ToBoolean(row.Cells["Type"].Value);

            txtUserName.ReadOnly = true;

            // LƯU GIÁ TRỊ BAN ĐẦU
            _originDisplayName = txtDisplayName.Text;
            _originPassword = txtPassword.Text;
            _originType = (bool)cbTypeAccount.SelectedValue;

            // reset nút
            btnEditAccount.Text = "Sửa tài khoản";
        }
        
        private void btnAddAccount_Click(object sender, EventArgs e)
        {

            if (txtUserName.Text.Length <= 1)
            {
                MessageBox.Show("Username phải nhiều hơn 1 ký tự");
                return;
            }
            else
            {
                string qSelect = "SELECT COUNT(*) FROM Account WHERE userName = @userName";
                SqlCommand boLenh = new SqlCommand(qSelect, ketNoi);
                boLenh.Parameters.AddWithValue("@userName", txtUserName.Text);

                try
                {
                    ketNoi.Open();
                    int checkUserName = (int)boLenh.ExecuteScalar();

                    if (checkUserName != 0)
                    {
                        MessageBox.Show("Username đã tồn tại, vui lòng chọn username khác");
                        clearInformation();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi kiểm tra username: " + ex.Message);
                    return;
                }
                finally
                {
                    if (ketNoi.State == ConnectionState.Open)
                        ketNoi.Close();
                }
            }

            if (txtUserName.Text == "")
            {
                MessageBox.Show("Vui lòng nhập Username");
                return;
            }

            if (txtDisplayName.Text == "")
            {
                MessageBox.Show("Vui lòng nhập tên hiển thị");
                return;
            }

            string qAdd = "INSERT INTO Account (userName, PassWord, displayName, type) " +
                          "VALUES (@userName, @PassWord, @displayName, @Type)";
            boDocGhi.InsertCommand = new SqlCommand(qAdd, ketNoi);
            boDocGhi.InsertCommand.Parameters.AddWithValue("@userName", txtUserName.Text);
            boDocGhi.InsertCommand.Parameters.AddWithValue("@PassWord", txtPassword.Text);
            boDocGhi.InsertCommand.Parameters.AddWithValue("@displayName", txtDisplayName.Text);
            boDocGhi.InsertCommand.Parameters.AddWithValue("@Type", cbTypeAccount.SelectedValue);
            try
            {
                ketNoi.Open();
                boDocGhi.InsertCommand.ExecuteNonQuery();
                MessageBox.Show("Thêm tài khoản thành công");
                LoadGridViewAccount();
                clearInformation();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm tài khoản: " + ex.Message);
            }
            finally
            {
                if (ketNoi.State == ConnectionState.Open)
                    ketNoi.Close();
            }
        }

        private void btnDelAccount_Click(object sender, EventArgs e)
        {
            if (txtUserName.Text == "")
            {
                MessageBox.Show("Vui lòng chọn tài khoản cần xóa");
                return;
            }

            DialogResult rs = MessageBox.Show(
                $"Bạn có chắc muốn xóa tài khoản [{txtUserName.Text}]?",
                "Xác nhận",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

            if (rs == DialogResult.No) return;

            string qDel = "DELETE FROM Account WHERE userName = @userName";
            SqlCommand cmdDel = new SqlCommand(qDel, ketNoi);
            cmdDel.Parameters.AddWithValue("@userName", txtUserName.Text);

            try
            {
                ketNoi.Open();
                int kq = cmdDel.ExecuteNonQuery();

                if (kq > 0)
                {
                    MessageBox.Show("Xóa tài khoản thành công");
                    LoadGridViewAccount();
                    clearInformation();
                    txtUserName.ReadOnly = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kiểm tra tài khoản: " + ex.Message);
                return;
            }
            finally
            {
                if (ketNoi.State == ConnectionState.Open)
                    ketNoi.Close();
            }
        }

        private void btnEditAccount_Click(object sender, EventArgs e)
        {
            if(txtUserName.Text == "")
            {
                MessageBox.Show("Vui lòng chọn tài khoản cần sửa");
                return;
            }

            if(txtDisplayName.Text == "")
            {
                MessageBox.Show("Vui lòng nhập tên hiển thị");
                return;
            }

            DialogResult rs = MessageBox.Show(
                $"Bạn có chắc muốn sửa thông tin tài khoản [{txtUserName.Text}]?",
                "Xác nhận",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

            if (rs == DialogResult.No) return;

            string qUpdate = @"UPDATE Account SET 
                                displayName = @displayName,
                                PassWord = @PassWord,
                                Type = @Type WHERE userName = @userName";
            
            SqlCommand cmdUpdate = new SqlCommand(qUpdate, ketNoi);
            cmdUpdate.Parameters.AddWithValue("@displayName", txtDisplayName.Text);
            cmdUpdate.Parameters.AddWithValue("@PassWord", txtPassword.Text);
            cmdUpdate.Parameters.AddWithValue("@Type", cbTypeAccount.SelectedValue);
            cmdUpdate.Parameters.AddWithValue("@userName", txtUserName.Text);

            try
            {
                ketNoi.Open();
                int kq = cmdUpdate.ExecuteNonQuery();

                if (kq > 0)
                {
                    MessageBox.Show("Cập nhật tài khoản thành công");
                    LoadGridViewAccount();

                    // reset lại trạng thái
                    _originDisplayName = txtDisplayName.Text;
                    _originPassword = txtPassword.Text;
                    _originType = (bool)cbTypeAccount.SelectedValue;

                    btnEditAccount.Text = "Sửa tài khoản";
                    txtUserName.ReadOnly = false;
                    clearInformation();
                }
                else
                {
                    MessageBox.Show("Không tìm thấy tài khoản để cập nhật");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật tài khoản: " + ex.Message);
                return;
            }
            finally
            {
                if (ketNoi.State == ConnectionState.Open)
                    ketNoi.Close();
            }
        }

        //XU LY TABLE -----------------------------------------------------------------------------
        void LoadTableStatus() // load trạng thái bàn vào combobox
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Value", typeof(bool));
            dt.Columns.Add("Text", typeof(string));

            dt.Rows.Add(false, "Trống");
            dt.Rows.Add(true, "Có người");

            cbTableStatus.DataSource = dt;
            cbTableStatus.DisplayMember = "Text";
            cbTableStatus.ValueMember = "Value";
        }

        void ClearTableInformation()
        {
            txtIDTable.Text = "";
            txtTableName.Text = "";
            cbTableStatus.SelectedIndex = 0;
            txtIDTable.ReadOnly = false;

            _isAddingTable = false;
            btnEditTable.Text = "Sửa bàn";
        }

        void TableField_Changed(object sender, EventArgs e)
        {
            if (txtIDTable.Text == "") return;

            bool currentStatus = Convert.ToBoolean(cbTableStatus.SelectedValue);

            bool isChanged =
                txtTableName.Text != _originTableName ||
                currentStatus != _originTableStatus;

            btnEditTable.Text = isChanged ? "Cập nhật" : "Sửa bàn";

        }

        private void LoadGridViewFoodTable() // load dữ liệu bàn từ DB lên DataGridView
        {
            using (SqlConnection connection = new SqlConnection(connection_string_sql))
            {
                string queryCheckStatusTable = @"SELECT ft.tableID, ft.tableName,
                                                CASE 
                                                    WHEN EXISTS (
                                                        SELECT 1 
                                                        FROM Bill b 
                                                        WHERE b.IDTable = ft.tableID AND b.billStatus = 1
                                                    )
                                                    THEN N'Có người'
                                                    ELSE N'Trống'
                                                END AS tableStatus
                                                FROM FoodTable ft";

                SqlDataAdapter da = new SqlDataAdapter(queryCheckStatusTable, connection);

                DataTable dtTable = new DataTable();
                da.Fill(dtTable);

                dtgvTable.DataSource = dtTable;
            }

            dtgvTable.Columns["tableID"].HeaderText = "Mã bàn";
            dtgvTable.Columns["tableName"].HeaderText = "Tên bàn";
            dtgvTable.Columns["tableStatus"].HeaderText = "Trạng thái";
        }
        
        private void dtgvTable_CellClick(object sender, DataGridViewCellEventArgs e) // khi click vào 1 dòng trong datagridview table
        {
            if (e.RowIndex < 0) return;

            if (_isAddingTable)
            {
                _isAddingTable = false;
                btnEditTable.Text = "Sửa bàn";
            }

            DataGridViewRow row = dtgvTable.Rows[e.RowIndex];

            txtIDTable.Text = row.Cells["tableID"].Value.ToString();
            txtTableName.Text = row.Cells["tableName"].Value.ToString();

            bool status = row.Cells["tableStatus"].Value.ToString() == "Có người";
            cbTableStatus.SelectedValue = status;

            txtIDTable.ReadOnly = true;

            //LƯU GIÁ TRỊ BAN ĐẦU
            _originTableName = txtTableName.Text;
            _originTableStatus = status;

            // reset nút
            btnEditTable.Text = "Sửa bàn";
        }

        private void btnAddTable_Click(object sender, EventArgs e)
        {
            if (!_isAddingTable)
            {
                ClearTableInformation();
                _isAddingTable = true;
                txtTableName.Focus();
                return;
            }

            if (txtTableName.Text.Trim().Length <= 1)
            {
                MessageBox.Show("Tên bàn phải nhiều hơn 1 ký tự");
                return;
            }

            // 2. Kiểm tra trùng tên bàn
            string qCheck = "SELECT COUNT(*) FROM FoodTable WHERE tableName = @tableName";
            SqlCommand cmdCheck = new SqlCommand(qCheck, ketNoi);
            cmdCheck.Parameters.AddWithValue("@tableName", txtTableName.Text.Trim());

            try
            {
                ketNoi.Open();
                int check = (int)cmdCheck.ExecuteScalar();

                if (check != 0)
                {
                    MessageBox.Show("Tên bàn đã tồn tại, vui lòng nhập tên khác");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kiểm tra tên bàn: " + ex.Message);
                return;
            }
            finally
            {
                if (ketNoi.State == ConnectionState.Open)
                    ketNoi.Close();
            }

            // 3. Thêm bàn (giống Add Account)
            string qAdd = "INSERT INTO FoodTable (tableName, tableStatus) VALUES (@tableName, @tableStatus)";
            boDocGhi = new SqlDataAdapter();
            boDocGhi.InsertCommand = new SqlCommand(qAdd, ketNoi);
            boDocGhi.InsertCommand.Parameters.AddWithValue("@tableName", txtTableName.Text.Trim());
            boDocGhi.InsertCommand.Parameters.AddWithValue("@tableStatus", cbTableStatus.SelectedValue);

            try
            {
                ketNoi.Open();
                boDocGhi.InsertCommand.ExecuteNonQuery();

                MessageBox.Show("Thêm bàn thành công");
                LoadGridViewFoodTable();

                ClearTableInformation();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm bàn: " + ex.Message);
            }
            finally
            {
                if (ketNoi.State == ConnectionState.Open)
                    ketNoi.Close();
            }
        }

        private void btnDelTable_Click(object sender, EventArgs e)
        {
            if (txtIDTable.Text == "")
            {
                MessageBox.Show("Vui lòng chọn bàn cần xóa");
                return;
            }

            DialogResult rs = MessageBox.Show(
                $"Bạn có chắc muốn xóa bàn [{txtTableName.Text}]?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (rs == DialogResult.No) return;

            string qDel = "DELETE FROM FoodTable WHERE tableID = @tableID";
            SqlCommand cmdDel = new SqlCommand(qDel, ketNoi);
            cmdDel.Parameters.AddWithValue("@tableID", txtIDTable.Text);

            try
            {
                ketNoi.Open();
                int kq = cmdDel.ExecuteNonQuery();

                if (kq > 0)
                {
                    MessageBox.Show("Xóa bàn thành công");
                    LoadGridViewFoodTable();

                    // reset dữ liệu
                    txtIDTable.Text = "";
                    txtTableName.Text = "";
                    cbTableStatus.SelectedIndex = 0;
                    txtIDTable.ReadOnly = false;
                }
                else
                {
                    MessageBox.Show("Không tìm thấy bàn để xóa");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa bàn: " + ex.Message);
            }
            finally
            {
                if (ketNoi.State == ConnectionState.Open)
                    ketNoi.Close();
            }
        }

        private void btnEditTable_Click(object sender, EventArgs e)
        {
            if (txtIDTable.Text == "")
            {
                MessageBox.Show("Vui lòng chọn bàn cần sửa");
                return;
            }

            if (txtTableName.Text == "")
            {
                MessageBox.Show("Vui lòng nhập tên bàn");
                return;
            }

            DialogResult rs = MessageBox.Show(
                $"Bạn có chắc muốn sửa thông tin bàn [{txtTableName.Text}]?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (rs == DialogResult.No) return;

            string qUpdate = @"UPDATE FoodTable SET
                tableName = @tableName,
                tableStatus = @tableStatus
                WHERE tableID = @tableID";

            SqlCommand cmdUpdate = new SqlCommand(qUpdate, ketNoi);
            cmdUpdate.Parameters.AddWithValue("@tableName", txtTableName.Text);
            cmdUpdate.Parameters.AddWithValue("@tableStatus", cbTableStatus.SelectedValue);
            cmdUpdate.Parameters.AddWithValue("@tableID", txtIDTable.Text);

            try
            {
                ketNoi.Open();
                int kq = cmdUpdate.ExecuteNonQuery();

                if (kq > 0)
                {
                    MessageBox.Show("Cập nhật bàn thành công");
                    LoadGridViewFoodTable();

                    // reset dữ liệu
                    txtIDTable.Text = "";
                    txtTableName.Text = "";
                    cbTableStatus.SelectedIndex = 0;
                    txtIDTable.ReadOnly = false;
                }
                else
                {
                    MessageBox.Show("Không tìm thấy bàn để cập nhật");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi sửa bàn: " + ex.Message);
            }
            finally
            {
                if (ketNoi.State == ConnectionState.Open)
                    ketNoi.Close();
            }
        }
        
        
        //XU LY DOANH THU -----------------------------------------------------------------------------
        private void btnShowTKDoanhThu_Click(object sender, EventArgs e) // XU LY THONG KE DOANH THU
        {
            DateTime dateFrom = dtpkIn.Value.Date;
            DateTime dateTo = dtpkOut.Value.Date.AddDays(1).AddSeconds(-1);

            if (dateFrom > dateTo)
            {
                MessageBox.Show("Ngày bắt đầu không được lớn hơn ngày kết thúc");
                return;
            }

            DataTable dt = new DataTable();

            string query = @"
        SELECT  
            b.billID        AS BillID,
            ft.tableName    AS TableName,
            b.dateCheckIn   AS DateCheckIn,
            b.dateCheckOut  AS DateCheckOut,
            SUM(bi.Quantity) AS TotalQuantity,
            SUM(bi.Quantity * f.foodPrice) AS TotalAmount
        FROM Bill b
        JOIN FoodTable ft ON b.IDTable = ft.tableID
        JOIN BillInfo bi ON b.billID = bi.IDBill
        JOIN Food f ON bi.IDFood = f.foodID
        WHERE 
            b.billStatus = 0
            AND b.dateCheckOut BETWEEN @dateFrom AND @dateTo
        GROUP BY 
            b.billID, ft.tableName, b.dateCheckIn, b.dateCheckOut
        ORDER BY b.dateCheckOut";

            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@dateFrom", dateFrom);
                cmd.Parameters.AddWithValue("@dateTo", dateTo);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            rpvDoanhThu.Reset();
            rpvDoanhThu.LocalReport.DataSources.Clear();
            rpvDoanhThu.LocalReport.ReportEmbeddedResource = "QLQuanCafe.rpDoanhThu.rdlc";

            //rpvDoanhThu.LocalReport.ReportPath = "rpDoanhThu.rdlc";

            ReportDataSource rds = new ReportDataSource("DoanhThu", dt);
            rpvDoanhThu.LocalReport.DataSources.Add(rds);

            rpvDoanhThu.RefreshReport();
        }

        private void dtpkIn_ValueChanged(object sender, EventArgs e) // XU LY NGAY BAT DAU
        {
            if (dtpkOut.Value < dtpkIn.Value)
                dtpkOut.Value = dtpkIn.Value;
        }

        private void dtpkOut_ValueChanged(object sender, EventArgs e) // XU LY NGAY KET THUC
        {
            if (dtpkOut.Value < dtpkIn.Value)
                MessageBox.Show("Ngày ra phải >= ngày vào");
        }

        
        //XU LY FOOD -----------------------------------------------------------------------------
        void LoadFood(string keyword = "") // load dữ liệu food từ DB lên DataGridView
        {
            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                string sql = @"SELECT 
                                f.foodID,
                                f.foodName,
                                f.foodPrice,
                                f.IDCategory,
                                fc.foodCateName,
                                f.foodImage
                            FROM Food f
                            JOIN FoodCategory fc ON f.IDCategory = fc.foodCateID
                            WHERE (@key = '' OR f.foodName LIKE '%' + @key + '%')";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@key", keyword);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dtgvFoodDrink.DataSource = dt;
            }

            dtgvFoodDrink.Columns["foodID"].HeaderText = "Mã món";
            dtgvFoodDrink.Columns["foodName"].HeaderText = "Tên món";
            dtgvFoodDrink.Columns["foodPrice"].HeaderText = "Giá";
            dtgvFoodDrink.Columns["foodCateName"].HeaderText = "Danh mục";
            dtgvFoodDrink.Columns["foodImage"].HeaderText = "Đường dẫn";

            dtgvFoodDrink.Columns["IDCategory"].Visible = false;
        }
        private void txbFindFoodName_TextChanged(object sender, EventArgs e)
        {
            LoadFood(txbFindFoodName.Text.Trim());
        }

        private void btnFindFood_Click(object sender, EventArgs e)
        {
            string keyword = txbFindFoodName.Text.Trim();
            LoadFood(keyword);
        }

        void FoodField_Changed() // theo dõi thay đổi trên form food
        {
            if (string.IsNullOrEmpty(txbFoodID.Text)) return;

            bool isChanged =
                txbFoodName.Text != _originFoodName ||
                float.TryParse(txbPrice.Text, out float p) && p != _originFoodPrice ||
                (int)cbFoodCategory.SelectedValue != _originFoodCategory ||
                !string.IsNullOrEmpty(_foodImagePath); // chọn ảnh mới

            btnEditFoods.Text = isChanged ? "Cập nhật" : "Sửa món";
        }

        void LoadFoodCategory()
        {
            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                string sql = "SELECT foodCateID, foodCateName FROM FoodCategory";
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // thêm dòng trống ở đầu
                DataRow row = dt.NewRow();
                row["foodCateID"] = DBNull.Value;
                row["foodCateName"] = "-- Chọn danh mục --";
                dt.Rows.InsertAt(row, 0);

                cbFoodCategory.DataSource = dt;
                cbFoodCategory.DisplayMember = "foodCateName";
                cbFoodCategory.ValueMember = "foodCateID";
                cbFoodCategory.SelectedIndex = 0;
            }
        }

        void ClearFoodForm()
        {
            txbFoodID.Text = "";
            txbFoodName.Text = "";
            txbPrice.Text = "";
            cbFoodCategory.SelectedIndex = 0;

            picFood.Image?.Dispose();
            picFood.Image = null;

            _foodImagePath = "";

            txbFoodName.Focus();
        }

        private void txbFoodName_TextChanged(object sender, EventArgs e)
        {
            FoodField_Changed();
        }

        private void txbPrice_TextChanged(object sender, EventArgs e)
        {
            FoodField_Changed();
        }

        private void cbFoodCategory_SelectedValueChanged(object sender, EventArgs e)
        {
            FoodField_Changed();
        }

        string SaveFoodImage(string sourcePath) // lưu ảnh món ăn vào thư mục và trả về path tương đối để lưu DB
        {
            if (string.IsNullOrEmpty(sourcePath)) return null;

            string folder = Path.Combine(Application.StartupPath, "imageUpload", "foods");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = Path.GetFileName(sourcePath);
            string destPath = Path.Combine(folder, fileName);

            // copy đè nếu trùng tên
            File.Copy(sourcePath, destPath, true);

            // path lưu DB
            return @"imageUpload\foods\" + fileName;
        }

        private void btnFoodAdd_Click(object sender, EventArgs e)
        {
            // Nếu chưa ở trạng thái thêm món
            if (!_isAddingFood)
            {
                ClearFoodForm();
                _isAddingFood = true;
                btnFoodAdd.Text = "Lưu món";
                return;
            }

            if (txbFoodName.Text.Trim() == "")
            {
                MessageBox.Show("Vui lòng nhập tên món");
                txbFoodName.Focus();
                return;
            }

            if (!float.TryParse(txbPrice.Text, out float price) || price <= 0)
            {
                MessageBox.Show("Giá không hợp lệ");
                txbPrice.Focus();
                return;
            }

            if (cbFoodCategory.SelectedIndex == 0)
            {
                MessageBox.Show("Vui lòng chọn danh mục");
                return;
            }

            if (string.IsNullOrEmpty(_foodImagePath))
            {
                MessageBox.Show("Vui lòng chọn ảnh món ăn");
                return;
            }

            // kiểm tra trùng tên
            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                string checkQuery = "SELECT COUNT(*) FROM Food WHERE foodName = @name";
                SqlCommand cmdCheck = new SqlCommand(checkQuery, conn);
                cmdCheck.Parameters.AddWithValue("@name", txbFoodName.Text.Trim());

                conn.Open();
                if ((int)cmdCheck.ExecuteScalar() > 0)
                {
                    MessageBox.Show("Tên món đã tồn tại");
                    txbFoodName.Focus();
                    return;
                }
            }

            string imageDbPath = SaveFoodImage(_foodImagePath);

            string query = @"INSERT INTO Food (foodName, foodPrice, IDCategory, foodImage) VALUES (@name, @price, @cat, @image)";

            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", txbFoodName.Text);
                cmd.Parameters.AddWithValue("@price", price);
                cmd.Parameters.AddWithValue("@cat", cbFoodCategory.SelectedValue);
                cmd.Parameters.AddWithValue("@image", imageDbPath);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Thêm món thành công");
            LoadFood();

            ClearFoodForm();
            _isAddingFood = false;
            btnFoodAdd.Text = "Thêm món";

        }
        
        private void btnDelFood_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txbFoodID.Text))
            {
                MessageBox.Show("Vui lòng chọn món cần xóa");
                return;
            }

            DialogResult rs = MessageBox.Show(
                $"Bạn có chắc muốn xóa món [{txbFoodName.Text}]?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (rs == DialogResult.No) return;

            string query = "DELETE FROM Food WHERE foodID = @id";

            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", txbFoodID.Text);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Xóa món thành công");
                    LoadFood();
                    ClearFoodForm();

                    _isAddingFood = false;
                    btnFoodAdd.Text = "Thêm món";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xóa món: " + ex.Message);
                }
            }
        }

        private void btnEditFoods_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txbFoodID.Text))
            {
                MessageBox.Show("Vui lòng chọn món cần sửa");
                return;
            }

            // Nếu chưa có thay đổi
            if (btnEditFoods.Text == "Sửa món")
            {
                MessageBox.Show("Chưa có thay đổi");
                return;
            }


            if (txbFoodName.Text.Trim() == "")
            {
                MessageBox.Show("Tên món không được để trống");
                return;
            }

            if (!float.TryParse(txbPrice.Text, out float price) || price <= 0)
            {
                MessageBox.Show("Giá không hợp lệ");
                return;
            }

            if (cbFoodCategory.SelectedIndex == 0)
            {
                MessageBox.Show("Vui lòng chọn danh mục");
                return;
            }

            // KIỂM TRA TRÙNG TÊN (TRỪ CHÍNH NÓ) 
            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                string checkQuery = @"SELECT COUNT(*) FROM Food WHERE foodName = @name AND foodID <> @id";

                SqlCommand cmd = new SqlCommand(checkQuery, conn);
                cmd.Parameters.AddWithValue("@name", txbFoodName.Text.Trim());
                cmd.Parameters.AddWithValue("@id", txbFoodID.Text);

                conn.Open();
                if ((int)cmd.ExecuteScalar() > 0)
                {
                    MessageBox.Show("Tên món đã tồn tại");
                    return;
                }
            }

            // XỬ LÝ ẢNH 
            string imageDbPath = _originFoodImage;
            if (!string.IsNullOrEmpty(_foodImagePath))
            {
                imageDbPath = SaveFoodImage(_foodImagePath);
            }

            // UPDATE 
            string query = @"UPDATE Food SET foodName = @name, foodPrice = @price, IDCategory = @cat, foodImage = @image WHERE foodID = @id";

            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", txbFoodName.Text.Trim());
                cmd.Parameters.AddWithValue("@price", price);
                cmd.Parameters.AddWithValue("@cat", cbFoodCategory.SelectedValue);
                cmd.Parameters.AddWithValue("@image", imageDbPath);
                cmd.Parameters.AddWithValue("@id", txbFoodID.Text);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Cập nhật món thành công");
            LoadFood();
            ClearFoodForm();
            btnEditFoods.Text = "Sửa món";
        }       

        private void btnChooseFoodImage_Click(object sender, EventArgs e)
        {
            ofdFoodImage.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            ofdFoodImage.Title = "Chọn ảnh món ăn";

            if (ofdFoodImage.ShowDialog() == DialogResult.OK)
            {
                _foodImagePath = ofdFoodImage.FileName;
                picFood.Image = Image.FromFile(_foodImagePath);
                FoodField_Changed();
            }
        }

        private void dtgvFoodDrink_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (_isAddingFood)
            {
                _isAddingFood = false;
                btnFoodAdd.Text = "Thêm món";
            }

            DataGridViewRow row = dtgvFoodDrink.Rows[e.RowIndex];

            txbFoodID.Text = row.Cells["foodID"].Value.ToString();
            txbFoodName.Text = row.Cells["foodName"].Value.ToString();
            txbPrice.Text = row.Cells["foodPrice"].Value.ToString();
            cbFoodCategory.SelectedValue = Convert.ToInt32(row.Cells["IDCategory"].Value);

            string imgPath = row.Cells["foodImage"].Value?.ToString();
            _originFoodImage = imgPath;   // lưu ảnh gốc
            _foodImagePath = "";          // reset ảnh mới

            if (!string.IsNullOrEmpty(imgPath))
            {
                string fullPath = Path.Combine(Application.StartupPath, imgPath);
                if (File.Exists(fullPath))
                    picFood.Image = Image.FromFile(fullPath);
                else
                    picFood.Image = null;
            }
            else
            {
                picFood.Image = null;
            }

            // LƯU GIÁ TRỊ GỐC
            _originFoodName = txbFoodName.Text;
            _originFoodPrice = float.Parse(txbPrice.Text);
            _originFoodCategory = (int)cbFoodCategory.SelectedValue;

            btnEditFoods.Text = "Sửa món";
        }
        
        
        //XU LY CATEGORY -----------------------------------------------------------------------------
        void LoadGridViewCategory()
        {
            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                string query = "SELECT foodCateID, foodCateName FROM FoodCategory";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dtgvCategory.DataSource = dt;
            }
            dtgvCategory.Columns["foodCateID"].HeaderText = "Mã danh mục";
            dtgvCategory.Columns["foodCateName"].HeaderText = "Tên danh mục";
        }
        
        void CategoryField_Changed()
        {
            if (string.IsNullOrEmpty(txbCategoryID.Text)) return;

            bool isChanged = txtNameCate.Text.Trim() != _originCategoryName;

            btnEditCategory.Text = isChanged ? "Cập nhật" : "Sửa danh mục";
        }

        void ClearFoodCatogoryInput()
        {
            txtNameCate.Text = "";
            txbCategoryID.Text = "";

            _originCategoryName = "";
            _isAddingCategory = false;

            btnEditCategory.Text = "Sửa danh mục";
        }
        
        private void dtgvCategory_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (_isAddingCategory)
            {
                _isAddingCategory = false;
                btnEditCategory.Text = "Sửa danh mục";
            }

            DataGridViewRow row = dtgvCategory.Rows[e.RowIndex];

            txtNameCate.Text = row.Cells["foodCateName"].Value.ToString();
            txbCategoryID.Text = row.Cells["foodCateID"].Value.ToString();

            // LƯU GIÁ TRỊ GỐC
            _originCategoryName = txtNameCate.Text;

            btnEditCategory.Text = "Sửa danh mục";
        }

        private void btnAddCategory_Click(object sender, EventArgs e)
        {
            if (!_isAddingCategory)
            {
                ClearFoodCatogoryInput();
                _isAddingCategory = true;
                txtNameCate.Focus();
                return;
            }
            if (txtNameCate.Text.Trim().Length <= 1)
            {
                MessageBox.Show("Tên danh mục phải nhiều hơn 1 ký tự");
                return;
            }

            string qCheck = "SELECT COUNT(*) FROM FoodCategory WHERE foodCateName = @name";
            SqlCommand cmdCheck = new SqlCommand(qCheck, ketNoi);
            cmdCheck.Parameters.AddWithValue("@name", txtNameCate.Text.Trim());

            try
            {
                ketNoi.Open();
                int check = (int)cmdCheck.ExecuteScalar();
                if (check > 0)
                {
                    MessageBox.Show("Tên danh mục đã tồn tại");
                    ClearFoodCatogoryInput();
                    return;

                }
            }
            finally
            {
                if (ketNoi.State == ConnectionState.Open)
                    ketNoi.Close();
            }

            string qAdd = "INSERT INTO FoodCategory(foodCateName) VALUES(@name)";
            SqlCommand cmdAdd = new SqlCommand(qAdd, ketNoi);
            cmdAdd.Parameters.AddWithValue("@name", txtNameCate.Text.Trim());

            try
            {
                ketNoi.Open();
                cmdAdd.ExecuteNonQuery();
                MessageBox.Show("Thêm danh mục thành công");
                LoadGridViewCategory();
                ClearFoodCatogoryInput();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm danh mục: " + ex.Message);
                return;
            }
            finally
            {
                if (ketNoi.State == ConnectionState.Open)
                    ketNoi.Close();
            }
        }

        private void txtNameCate_TextChanged(object sender, EventArgs e)
        {
            CategoryField_Changed();
        }

        private void btnDelCategory_Click(object sender, EventArgs e)
        {
            if (dtgvCategory.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn danh mục cần xóa");
                return;
            }
            DialogResult rs = MessageBox.Show(
                $"Bạn có chắc muốn xóa danh mục [{dtgvCategory.CurrentRow.Cells["foodCateName"].Value}]?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (rs == DialogResult.No) return;

            string qDel = "DELETE FROM FoodCategory WHERE foodCateID = @foodCateID";
            SqlCommand cmdDel = new SqlCommand(qDel, ketNoi);
            cmdDel.Parameters.AddWithValue("@foodCateID", dtgvCategory.CurrentRow.Cells["foodCateID"].Value);

            try
            {
                ketNoi.Open();
                int kq = cmdDel.ExecuteNonQuery();
                if (kq > 0)
                {
                    MessageBox.Show("Xóa danh mục thành công");
                    LoadGridViewCategory();

                    txtNameCate.Text = "";
                }
                else
                {
                    MessageBox.Show("Không tìm thấy danh mục để xóa");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa danh mục: " + ex.Message);
                return;
            }
            finally
            {
                if (ketNoi.State == ConnectionState.Open)
                    ketNoi.Close();
            }
        }

        private void btnEditCategory_Click(object sender, EventArgs e)
        {
            if (btnEditCategory.Text == "Sửa danh mục")
            {
                MessageBox.Show("Chưa có thay đổi");
                return;
            }

            if (dtgvCategory.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn danh mục cần sửa");
                return;
            }

            if (txtNameCate.Text.Trim() == "")
            {
                MessageBox.Show("Tên danh mục không được để trống");
                return;
            }

            DialogResult rs = MessageBox.Show(
                $"Bạn có chắc muốn sửa danh mục?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (rs == DialogResult.No) return;

            string qUpdate = @"UPDATE FoodCategory 
               SET foodCateName = @name 
               WHERE foodCateID = @id";

            SqlCommand cmd = new SqlCommand(qUpdate, ketNoi);
            cmd.Parameters.AddWithValue("@name", txtNameCate.Text.Trim());
            cmd.Parameters.AddWithValue("@id", dtgvCategory.CurrentRow.Cells["foodCateID"].Value);

            try
            {
                ketNoi.Open();
                cmd.ExecuteNonQuery();
                MessageBox.Show("Cập nhật danh mục thành công");

                LoadGridViewCategory();

                // reset trạng thái
                _originCategoryName = txtNameCate.Text;
                btnEditCategory.Text = "Sửa danh mục";
                txtNameCate.Text = "";
                txbCategoryID.Text = "";
            }
            finally
            {
                ketNoi.Close();
            }
        }

        
    }
}
