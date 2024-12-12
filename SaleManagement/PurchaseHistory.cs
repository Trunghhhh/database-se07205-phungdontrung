using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SaleManagement
{
    public partial class PurchaseHistory : Form
    {
        public static string connectionString = "Server=DESKTOP-GG53O03\\DONTRUNG;Database=ASM 1;Trusted_Connection=True;";
        private int roleId; // Thay đổi từ userRoleId sang roleId

        public PurchaseHistory(int roleId) // Nhận roleId từ constructor
        {
            InitializeComponent();
            this.roleId = roleId; // Lưu quyền vào biến

            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterScreen;
            dgv_PurchaseHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Đăng ký sự kiện
            btnAdd.Click += new EventHandler(btnAdd_Click);
            btnEdit.Click += new EventHandler(btnEdit_Click);
            btnDelete.Click += new EventHandler(btnDelete_Click);
            btnSearch.Click += new EventHandler(btnSearch_Click);
            dgv_PurchaseHistory.CellClick += new DataGridViewCellEventHandler(dgv_PurchaseHistory_CellClick);
        }

        private void PurchaseHistory_Load(object sender, EventArgs e)
        {
            LoadPurchaseHistory(0);
            MessageBox.Show("Data loaded successfully.");
        }

        private void LoadPurchaseHistory(int statusFilter)
        {
            string query = @"
                SELECT 
                    ph.purchaseID, 
                    c.name AS customerName, 
                    p.name AS productName, 
                    ph.purchaseDate, 
                    ph.quantity, 
                    ph.status
                FROM 
                    [ASM 1].[dbo].[PurchaseHistory] ph
                INNER JOIN 
                    [ASM 1].[dbo].[Product] p ON ph.productCode = p.code
                INNER JOIN 
                    [ASM 1].[dbo].[Customer] c ON ph.customerCode = c.code";

            if (statusFilter > 0)
            {
                query += " WHERE ph.status = @status";
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (statusFilter > 0)
                    {
                        command.Parameters.AddWithValue("@status", statusFilter);
                    }

                    try
                    {
                        connection.Open();
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        dgv_PurchaseHistory.DataSource = dataTable;

                        MessageBox.Show($"Số lượng hàng: {dataTable.Rows.Count}");

                        if (dataTable.Rows.Count == 0)
                        {
                            MessageBox.Show("Không có dữ liệu để hiển thị.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}");
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
            MenuForm mainForm = new MenuForm(roleId); // Sử dụng roleId
            mainForm.Show();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!HasPermission(UserRole.Admin, UserRole.Sale)) // Check permissions
            {
                MessageBox.Show("Bạn không có quyền thêm dữ liệu.");
                return;
            }

            // Get information from the fields
            string customerCode = txtCustomerCode.Text.Trim();
            string productCode = txtProductCode.Text.Trim();
            string newPurchaseID = GetNextPurchaseID();
            DateTime purchaseDate;
            int quantity;

            // Validate date
            if (!DateTime.TryParse(txtPurchaseDate.Text, out purchaseDate))
            {
                MessageBox.Show("Please enter a valid date in the format YYYY-MM-DD.");
                return;
            }

            // Validate quantity
            if (!int.TryParse(txtQuantity.Text, out quantity))
            {
                MessageBox.Show("Please enter a valid quantity.");
                return;
            }

            string status = txtStatus.Text.Trim();

            if (string.IsNullOrWhiteSpace(customerCode) || string.IsNullOrWhiteSpace(productCode) || string.IsNullOrWhiteSpace(status))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            // Check if customerCode exists
            if (!DoesCustomerCodeExist(customerCode))
            {
                MessageBox.Show("The specified customer code does not exist.");
                return;
            }

            // Check if productCode exists
            if (!DoesProductCodeExist(productCode))
            {
                MessageBox.Show("The specified product code does not exist.");
                return;
            }

            string query = "INSERT INTO PurchaseHistory (purchaseID, customerCode, productCode, purchaseDate, quantity, status) VALUES (@PurchaseID, @CustomerCode, @ProductCode, @PurchaseDate, @Quantity, @Status)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PurchaseID", newPurchaseID);
                    command.Parameters.AddWithValue("@CustomerCode", customerCode);
                    command.Parameters.AddWithValue("@ProductCode", productCode);
                    command.Parameters.AddWithValue("@PurchaseDate", purchaseDate);
                    command.Parameters.AddWithValue("@Quantity", quantity);
                    command.Parameters.AddWithValue("@Status", status);

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        LoadPurchaseHistory(0);
                        MessageBox.Show("Purchase added successfully!");
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show($"An error occurred while adding purchase: {ex.Message}");
                    }
                }
            }
        }

        // Methods to check existence of customer and product codes
        private bool DoesCustomerCodeExist(string customerCode)
        {
            string query = "SELECT COUNT(1) FROM [ASM 1].[dbo].[Customer] WHERE code = @CustomerCode";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerCode", customerCode);
                    connection.Open();
                    return (int)command.ExecuteScalar() > 0;
                }
            }
        }

        private bool DoesProductCodeExist(string productCode)
        {
            string query = "SELECT COUNT(1) FROM [ASM 1].[dbo].[Product] WHERE code = @ProductCode";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductCode", productCode);
                    connection.Open();
                    return (int)command.ExecuteScalar() > 0;
                }
            }
        }

        private string GetNextPurchaseID()
        {
            string nextID = string.Empty;
            string query = "SELECT MAX(purchaseID) FROM PurchaseHistory"; // Adjust based on your ID generation logic

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        // Assuming purchaseID is numeric, increment it for the next ID
                        int maxId = Convert.ToInt32(result);
                        nextID = (maxId + 1).ToString(); // Adjust this logic if your IDs are not numeric
                    }
                    else
                    {
                        nextID = "1"; // Start with 1 if no records exist
                    }
                }
            }

            return nextID;
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (!HasPermission(UserRole.Admin, UserRole.Sale)) // Check permissions
            {
                MessageBox.Show("Bạn không có quyền chỉnh sửa dữ liệu.");
                return;
            }

            if (dgv_PurchaseHistory.SelectedRows.Count > 0)
            {
                int purchaseID = Convert.ToInt32(dgv_PurchaseHistory.SelectedRows[0].Cells["purchaseID"].Value);
                string customerCode = txtCustomerCode.Text.Trim();
                string productCode = txtProductCode.Text.Trim();

                // Validate date
                if (!DateTime.TryParse(txtPurchaseDate.Text, out DateTime purchaseDate))
                {
                    MessageBox.Show("Please enter a valid date.");
                    return;
                }

                // Validate quantity
                if (!int.TryParse(txtQuantity.Text, out int quantity))
                {
                    MessageBox.Show("Please enter a valid quantity.");
                    return;
                }

                string status = txtStatus.Text;

                // Check if customerCode exists
                if (!IsValidCustomerCode(customerCode))
                {
                    MessageBox.Show("The specified customer code does not exist.");
                    return;
                }

                // Check if productCode exists
                if (!IsValidProductCode(productCode))
                {
                    MessageBox.Show("The specified product code does not exist.");
                    return;
                }

                string query = "UPDATE PurchaseHistory SET customerCode = @CustomerCode, productCode = @ProductCode, purchaseDate = @PurchaseDate, quantity = @Quantity, status = @Status WHERE purchaseID = @PurchaseID";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PurchaseID", purchaseID);
                        command.Parameters.AddWithValue("@CustomerCode", customerCode);
                        command.Parameters.AddWithValue("@ProductCode", productCode);
                        command.Parameters.AddWithValue("@PurchaseDate", purchaseDate);
                        command.Parameters.AddWithValue("@Quantity", quantity);
                        command.Parameters.AddWithValue("@Status", status);

                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                            LoadPurchaseHistory(0);
                            MessageBox.Show("Purchase updated successfully!");
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show($"An error occurred while updating purchase: {ex.Message}");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a purchase to edit.");
            }
        }

        private bool IsValidCustomerCode(string customerCode)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM [ASM 1].[dbo].[Customer] WHERE code = @CustomerCode";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerCode", customerCode);
                    connection.Open();
                    int count = (int)command.ExecuteScalar();
                    return count > 0; // Returns true if customer code exists
                }
            }
        }

        private bool IsValidProductCode(string productCode)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM [ASM 1].[dbo].[Product] WHERE code = @ProductCode";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductCode", productCode);
                    connection.Open();
                    int count = (int)command.ExecuteScalar();
                    return count > 0; // Returns true if product code exists
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!HasPermission(UserRole.Admin)) // Kiểm tra quyền
            {
                MessageBox.Show("Bạn không có quyền xóa dữ liệu.");
                return;
            }

            if (dgv_PurchaseHistory.SelectedRows.Count > 0)
            {
                int purchaseID = Convert.ToInt32(dgv_PurchaseHistory.SelectedRows[0].Cells["purchaseID"].Value);
                string query = "DELETE FROM PurchaseHistory WHERE purchaseID = @PurchaseID";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PurchaseID", purchaseID);

                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                            LoadPurchaseHistory(0);
                            MessageBox.Show("Purchase deleted successfully!");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"An error occurred while deleting purchase: {ex.Message}");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a purchase to delete.");
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(searchTerm))
            {
                MessageBox.Show("Vui lòng nhập từ khóa tìm kiếm.");
                return;
            }

            string query = "SELECT * FROM PurchaseHistory WHERE LOWER(productCode) LIKE LOWER(@SearchTerm) OR LOWER(customerCode) LIKE LOWER(@SearchTerm)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();

                    try
                    {
                        connection.Open();
                        adapter.Fill(dataTable);

                        if (dataTable.Rows.Count == 0)
                        {
                            MessageBox.Show("Không tìm thấy kết quả nào.");
                        }

                        dgv_PurchaseHistory.DataSource = dataTable;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Có lỗi xảy ra trong quá trình tìm kiếm: {ex.Message}");
                    }
                }
            }
        }

        private void dgv_PurchaseHistory_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow selectedRow = dgv_PurchaseHistory.Rows[e.RowIndex];
                txtCustomerCode.Text = selectedRow.Cells["customerName"].Value?.ToString() ?? string.Empty;
                txtProductCode.Text = selectedRow.Cells["productName"].Value?.ToString() ?? string.Empty; // Updated to productName
                txtPurchaseDate.Text = selectedRow.Cells["purchaseDate"].Value?.ToString() ?? string.Empty;
                txtQuantity.Text = selectedRow.Cells["quantity"].Value?.ToString() ?? string.Empty;
                txtStatus.Text = selectedRow.Cells["status"].Value?.ToString() ?? string.Empty;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            txtCustomerCode.Clear();
            txtProductCode.Clear();
            txtPurchaseDate.Clear();
            txtQuantity.Clear();
            txtStatus.Clear();
            txtSearch.Clear();
        }

        private bool HasPermission(params UserRole[] roles)
        {
            foreach (var role in roles)
            {
                if (roleId == (int)role) // Sử dụng roleId thay vì userRoleId
                {
                    return true;
                }
            }
            return false;
        }

        public enum UserRole
        {
            Admin = 1,
            Sale = 2,
            Warehouse = 3,
            Employee = 4
        }
    }
}