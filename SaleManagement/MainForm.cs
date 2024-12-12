using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SaleManagement
{
    public partial class MainForm : Form
    {
        public static string connectionString = "Server=DESKTOP-GG53O03\\DONTRUNG;Database=ASM 1;Trusted_Connection=True;";
        private int roleId; // Thay đổi từ userRoleId sang roleId

        public MainForm(int roleId)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterScreen;
            dgv_product.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Gán sự kiện cho các button
            btnAdd.Click += new EventHandler(btnAdd_Click);
            btnEdit.Click += new EventHandler(btnEdit_Click);
            btnDelete.Click += new EventHandler(btnDelete_Click);
            btnSearch.Click += new EventHandler(btnSearch_Click);
            button2.Click += new EventHandler(ClearFields); // Clear fields button

            dgv_product.CellClick += new DataGridViewCellEventHandler(dgv_product_CellContentClick);

            // Gán roleId
            SetUserRoleId(roleId);

            LoadData();
        }

        public enum UserRole
        {
            Admin = 1,
            Sale = 2,
            Warehouse = 3,
            Employee = 4
        }

        public void SetUserRoleId(int roleId) // Nhận roleId từ constructor
        {
            this.roleId = roleId; // Lưu quyền vào biến
        }

        private void dgv_product_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow selectedRow = dgv_product.Rows[e.RowIndex];
                txtCode.Text = selectedRow.Cells["code"].Value.ToString();
                txtName.Text = selectedRow.Cells["name"].Value.ToString();
                txtQuantity.Text = selectedRow.Cells["quantity"].Value.ToString();
                txtPrice.Text = selectedRow.Cells["price"].Value.ToString();
            }
        }

        private void LoadData()
        {
            string query = "SELECT * FROM Product";
            ExecuteQuery(query, null);
        }

        private void ExecuteQuery(string query, SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    if (parameters != null)
                    {
                        adapter.SelectCommand.Parameters.AddRange(parameters);
                    }
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dgv_product.DataSource = dataTable;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                    LogError(ex.Message);
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!HasPermission(UserRole.Admin, UserRole.Sale))
            {
                MessageBox.Show("Bạn không có quyền thêm sản phẩm.");
                return;
            }

            if (!ValidateProductInputs(out string code, out string name, out int quantity, out decimal price))
                return;

            if (!IsProductCodeUnique(code))
            {
                MessageBox.Show("Product code must be unique.");
                return;
            }

            string query = "INSERT INTO Product (code, name, quantity, price) VALUES (@Code, @Name, @Quantity, @Price)";
            SqlParameter[] parameters = {
                new SqlParameter("@Code", code),
                new SqlParameter("@Name", name),
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@Price", price)
            };

            ExecuteNonQuery(query, parameters);
            LoadData();
            MessageBox.Show("Product added successfully!");
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (!HasPermission(UserRole.Admin, UserRole.Sale))
            {
                MessageBox.Show("Bạn không có quyền chỉnh sửa sản phẩm.");
                return;
            }

            if (dgv_product.SelectedRows.Count > 0)
            {
                if (!ValidateProductInputs(out string code, out string name, out int quantity, out decimal price))
                    return;

                string query = "UPDATE Product SET name = @Name, quantity = @Quantity, price = @Price WHERE code = @Code";
                SqlParameter[] parameters = {
                    new SqlParameter("@Code", code),
                    new SqlParameter("@Name", name),
                    new SqlParameter("@Quantity", quantity),
                    new SqlParameter("@Price", price)
                };

                ExecuteNonQuery(query, parameters);
                LoadData();
                MessageBox.Show("Product updated successfully!");
            }
            else
            {
                MessageBox.Show("Please select a product to edit.");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!HasPermission(UserRole.Admin))
            {
                MessageBox.Show("Bạn không có quyền xóa sản phẩm.");
                return;
            }

            if (dgv_product.SelectedRows.Count > 0)
            {
                string code = dgv_product.SelectedRows[0].Cells["code"].Value.ToString();
                string query = "DELETE FROM Product WHERE code = @Code";
                SqlParameter[] parameters = {
                    new SqlParameter("@Code", code)
                };

                ExecuteNonQuery(query, parameters);
                LoadData();
                MessageBox.Show("Product deleted successfully!");
            }
            else
            {
                MessageBox.Show("Please select a product to delete.");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();

            // Khởi tạo MenuForm với tham số cần thiết (ví dụ: roleId)
            MenuForm mainForm = new MenuForm(roleId); // Truyền roleId vào constructor
            mainForm.Show();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim(); // Trim whitespace
            string query = "SELECT * FROM Product WHERE LOWER(name) LIKE LOWER(@SearchTerm) OR LOWER(code) LIKE LOWER(@SearchTerm)";
            SqlParameter[] parameters = {
                new SqlParameter("@SearchTerm", "%" + searchTerm + "%")
            };
            ExecuteQuery(query, parameters);
        }

        private bool ValidateProductInputs(out string code, out string name, out int quantity, out decimal price)
        {
            code = txtCode.Text.Trim();
            name = txtName.Text.Trim();
            price = 0;

            if (!int.TryParse(txtQuantity.Text.Trim(), out quantity))
            {
                MessageBox.Show("Please enter a valid quantity.");
                return false;
            }

            if (!decimal.TryParse(txtPrice.Text.Trim(), out price))
            {
                MessageBox.Show("Please enter a valid price.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("Please enter a valid product code.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter a valid product name.");
                return false;
            }

            return true;
        }

        private void LogError(string message)
        {
            using (StreamWriter writer = new StreamWriter("error.log", true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportToCSV("products.csv");
            MessageBox.Show("Data exported successfully!");
        }

        private void ClearFields(object sender, EventArgs e)
        {
            txtCode.Clear();
            txtName.Clear();
            txtQuantity.Clear();
            txtPrice.Clear();
            txtSearch.Clear();
        }

        private void ExecuteNonQuery(string query, SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        private bool IsProductCodeUnique(string code)
        {
            string query = "SELECT COUNT(1) FROM Product WHERE code = @Code";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Code", code);
                    connection.Open();
                    return (int)command.ExecuteScalar() == 0;
                }
            }
        }

        private void ExportToCSV(string filePath)
        {
            StringBuilder csvContent = new StringBuilder();
            foreach (DataGridViewColumn column in dgv_product.Columns)
            {
                csvContent.Append(column.HeaderText + ",");
            }
            csvContent.AppendLine();

            foreach (DataGridViewRow row in dgv_product.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    csvContent.Append(cell.Value?.ToString() + ",");
                }
                csvContent.AppendLine();
            }

            File.WriteAllText(filePath, csvContent.ToString());
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
    }
}