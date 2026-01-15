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

        // BIẾN LƯU TRẠNG THÁI THÊM MÓN ĂN
        string _foodImagePath = "";
        bool _isAddingFood = false;

        // BIẾN LƯU GIÁ TRỊ BAN ĐẦU KHI CHỌN 1 FOOD
        string _originFoodName;
        float _originFoodPrice;
        int _originFoodCategory;
        string _originFoodImage;

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
            this.rpvDoanhThu.RefreshReport();
        }
        //XU LY ACCOUNT -----------------------------------------------------------------------------
        void LoadTypeAccount()
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

        void clearInformation()
        {
            txtUserName.ReadOnly = false;
            txtUserName.Text = "";
            txtDisplayName.Text = "";
            txtPassword.Text = "";
            cbTypeAccount.SelectedIndex = 0;
        }
        void AccountField_Changed(object sender, EventArgs e)
        {
            if (txtUserName.Text == "") return; // chưa chọn account

            bool isChanged =
                txtDisplayName.Text != _originDisplayName ||
                txtPassword.Text != _originPassword ||
                (bool)cbTypeAccount.SelectedValue != _originType;

            btnEditAccount.Text = isChanged ? "Cập nhật" : "Sửa tài khoản";
        }

        private void LoadGridViewAccount()
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

        private void dtgvAccount_CellClick(object sender, DataGridViewCellEventArgs e)
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
        void LoadTableStatus()
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
                                                    THEN 1
                                                    ELSE 0
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
        private void dtgvTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex < 0) return;
            DataGridViewRow row = dtgvTable.Rows[e.RowIndex];

            txtIDTable.Text = row.Cells["tableID"].Value.ToString();
            txtTableName.Text = row.Cells["tableName"].Value.ToString();
            cbTableStatus.SelectedValue = Convert.ToBoolean(row.Cells["tableStatus"].Value);

            txtIDTable.ReadOnly = true;
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

        private void dtpkIn_ValueChanged(object sender, EventArgs e)
        {
            if (dtpkOut.Value < dtpkIn.Value)
                dtpkOut.Value = dtpkIn.Value;
        }

        private void dtpkOut_ValueChanged(object sender, EventArgs e)
        {
            if (dtpkOut.Value < dtpkIn.Value)
                MessageBox.Show("Ngày ra phải >= ngày vào");
        }

        //XU LY FOOD -----------------------------------------------------------------------------
        void LoadFood()
        {
            using (SqlConnection conn = new SqlConnection(connection_string_sql))
            {
                string sql = @"
            SELECT 
                f.foodID,
                f.foodName,
                f.foodPrice,
                f.IDCategory,
                fc.foodCateName,
                f.foodImage
            FROM Food f
            JOIN FoodCategory fc ON f.IDCategory = fc.foodCateID";

                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dtgvFoodDrink.DataSource = dt;
            }

            dtgvFoodDrink.Columns["foodID"].HeaderText = "Mã món";
            dtgvFoodDrink.Columns["foodName"].HeaderText = "Tên món";
            dtgvFoodDrink.Columns["foodPrice"].HeaderText = "Giá";
            dtgvFoodDrink.Columns["foodCateName"].HeaderText = "Danh mục";
            dtgvFoodDrink.Columns["foodImage"].HeaderText = "Đường dẫn";

            // Ẩn cột IDCategory (vẫn dùng nội bộ)
            dtgvFoodDrink.Columns["IDCategory"].Visible = false;
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

        string SaveFoodImage(string sourcePath)
        {
            if (string.IsNullOrEmpty(sourcePath)) return null;

            string folder = Path.Combine(Application.StartupPath, "imageUpload", "foods");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = Path.GetFileName(sourcePath);
            string destPath = Path.Combine(folder, fileName);

            // copy đè nếu trùng tên
            File.Copy(sourcePath, destPath, true);

            // path lưu DB (tương đối)
            return @"imageUpload\foods\" + fileName;
        }

        private void btnFoodAdd_Click(object sender, EventArgs e)
        {
            // LẦN ĐẦU: chuẩn bị thêm món
            if (!_isAddingFood)
            {
                ClearFoodForm();
                _isAddingFood = true;
                btnFoodAdd.Text = "Lưu món";
                return;
            }

            // ===== TỪ ĐÂY TRỞ ĐI MỚI VALIDATE =====

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

            string query = @"INSERT INTO Food (foodName, foodPrice, IDCategory, foodImage)
                     VALUES (@name, @price, @cat, @image)";

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

        }

        private void txbFoodID_TextChanged(object sender, EventArgs e)
        {

        }

        private void txbFoodName_TextChanged(object sender, EventArgs e)
        {

        }

        private void cbFoodCategory_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txbPrice_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnChooseFoodImage_Click(object sender, EventArgs e)
        {
            ofdFoodImage.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            ofdFoodImage.Title = "Chọn ảnh món ăn";

            if (ofdFoodImage.ShowDialog() == DialogResult.OK)
            {
                _foodImagePath = ofdFoodImage.FileName;
                picFood.Image = Image.FromFile(_foodImagePath);
            }
        }

        private void dtgvFoodDrink_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dtgvFoodDrink.Rows[e.RowIndex];

            txbFoodID.Text = row.Cells["foodID"].Value.ToString();
            txbFoodName.Text = row.Cells["foodName"].Value.ToString();
            txbPrice.Text = row.Cells["foodPrice"].Value.ToString();
            cbFoodCategory.SelectedValue = Convert.ToInt32(row.Cells["IDCategory"].Value);

            string imgPath = row.Cells["foodImage"].Value?.ToString();
            _originFoodImage = imgPath;   // ⭐ lưu ảnh gốc
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

            // ⭐ LƯU GIÁ TRỊ GỐC
            _originFoodName = txbFoodName.Text;
            _originFoodPrice = float.Parse(txbPrice.Text);
            _originFoodCategory = (int)cbFoodCategory.SelectedValue;

            btnEditFoods.Text = "Sửa món";
        }
    }
}
