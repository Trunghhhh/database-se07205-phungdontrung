using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SaleManagement
{
    public partial class CustomerForm : Form
    {
        public static string connectionString = "Server=DESKTOP-GG53O03\\DONTRUNG;Database=ASM 1;Trusted_Connection=True;";
        private int roleId; // Biến để lưu trữ vai trò người dùng

        public CustomerForm(int roleId) // Nhận roleId từ constructor
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterScreen;
            dgv_customer.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Gán sự kiện cho các nút
            btnAdd.Click += btnAdd_Click;
            btnEdit.Click += btnEdit_Click;
            btnDelete.Click += btnDelete_Click;
            btnSearch.Click += btnSearch_Click;
            dgv_customer.CellClick += dgv_customer_CellContentClick;

            // Gán roleId
            SetUserRoleId(roleId);

            // Load dữ liệu
            LoadData();
        }

        public enum UserRole
        {
            admin = 1,
            sale = 2,
            warehouse = 3,
            employee = 4
        }

        // Phương thức này để thiết lập roleId từ form đăng nhập
        public void SetUserRoleId(int roleId)
        {
            this.roleId = roleId; // Gán giá trị roleId
        }

        private void dgv_customer_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow selectedRow = dgv_customer.Rows[e.RowIndex];
                txtCode.Text = selectedRow.Cells["Code"].Value.ToString();
                txtName.Text = selectedRow.Cells["Name"].Value.ToString();
                txtPhone.Text = selectedRow.Cells["phoneNumber"].Value.ToString();
                txtAddress.Text = selectedRow.Cells["Address"].Value.ToString();
            }
        }

        private void LoadData()
        {
            string query = "SELECT * FROM Customer";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dgv_customer.DataSource = dataTable;

                    // Kích hoạt chức năng cho admin
                    EnableAllButtons(roleId == (int)UserRole.admin);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        private void EnableAllButtons(bool enable)
        {
            btnAdd.Enabled = enable;
            btnEdit.Enabled = enable;
            btnDelete.Enabled = enable;
            btnSearch.Enabled = enable;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!HasPermissionToAddOrEdit())
            {
                MessageBox.Show("Bạn không có quyền thêm khách hàng.");
                return;
            }

            string name = txtName.Text;
            string phone = txtPhone.Text;
            string address = txtAddress.Text;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(address))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            string newCode = GetNextCustomerCode();
            string query = "INSERT INTO Customer (code, Name, phoneNumber, Address) VALUES (@Code, @Name, @phoneNumber, @Address)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Code", newCode);
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@phoneNumber", phone);
                    command.Parameters.AddWithValue("@Address", address);

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        LoadData();
                        MessageBox.Show("Customer added successfully!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred: " + ex.Message);
                    }
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (!HasPermissionToAddOrEdit())
            {
                MessageBox.Show("Bạn không có quyền chỉnh sửa khách hàng.");
                return;
            }

            if (dgv_customer.SelectedRows.Count > 0)
            {
                string code = dgv_customer.SelectedRows[0].Cells["Code"].Value.ToString();
                string name = txtName.Text;
                string phone = txtPhone.Text;
                string address = txtAddress.Text;

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(address))
                {
                    MessageBox.Show("Please fill in all fields.");
                    return;
                }

                string query = "UPDATE Customer SET Name = @Name, phoneNumber = @Phone, Address = @Address WHERE code = @Code";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Code", code);
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Phone", phone);
                        command.Parameters.AddWithValue("@Address", address);

                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                            LoadData();
                            MessageBox.Show("Customer updated successfully!");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("An error occurred: " + ex.Message);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a customer to edit.");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (roleId != (int)UserRole.admin) // Kiểm tra quyền
            {
                MessageBox.Show("Bạn không có quyền xóa khách hàng.");
                return;
            }

            if (dgv_customer.SelectedRows.Count > 0)
            {
                string code = dgv_customer.SelectedRows[0].Cells["Code"].Value.ToString();

                if (DeleteRelatedPurchaseHistory(code))
                {
                    string query = "DELETE FROM Customer WHERE code = @Code";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Code", code);

                            try
                            {
                                connection.Open();
                                int rowsAffected = command.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    LoadData();
                                    MessageBox.Show("Customer deleted successfully!");
                                }
                                else
                                {
                                    MessageBox.Show("Failed to delete customer.");
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("An error occurred: " + ex.Message);
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a customer to delete.");
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(searchTerm))
            {
                MessageBox.Show("Please enter a search term.");
                return;
            }

            string query = "SELECT * FROM Customer WHERE LOWER(Name) LIKE LOWER(@searchTerm) OR LOWER(phoneNumber) LIKE LOWER(@searchTerm) OR LOWER(Address) LIKE LOWER(@searchTerm)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@searchTerm", "%" + searchTerm + "%");

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();

                    try
                    {
                        connection.Open();
                        adapter.Fill(dataTable);

                        if (dataTable.Rows.Count == 0)
                        {
                            MessageBox.Show("No customers found.");
                        }

                        dgv_customer.DataSource = dataTable;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred: " + ex.Message);
                    }
                }
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();

            // Khởi tạo MenuForm với tham số cần thiết (ví dụ: roleId)
            MenuForm mainForm = new MenuForm(roleId); // Truyền roleId vào constructor
            mainForm.Show();
        }

        private string GetNextCustomerCode()
        {
            string newCode;
            int maxCodeNumber = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT MAX(CAST(SUBSTRING(code, 6, LEN(code) - 5) AS INT)) FROM Customer";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        object result = command.ExecuteScalar();
                        if (result != DBNull.Value)
                        {
                            maxCodeNumber = Convert.ToInt32(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred: " + ex.Message);
                    }
                }
            }

            newCode = "CUST" + (maxCodeNumber + 1).ToString().PadLeft(3, '0');
            return newCode;
        }

        private bool DeleteRelatedPurchaseHistory(string customerCode)
        {
            string query = "DELETE FROM PurchaseHistory WHERE CustomerCode = @CustomerCode";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerCode", customerCode);
                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred: " + ex.Message);
                        return false;
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            txtCode.Clear();
            txtName.Clear();
            txtPhone.Clear();
            txtAddress.Clear();
            txtSearch.Clear();
        }

        // Kiểm tra quyền người dùng
        private bool HasPermissionToAddOrEdit()
        {
            return roleId == (int)UserRole.admin || roleId == (int)UserRole.sale;
        }
    }
}