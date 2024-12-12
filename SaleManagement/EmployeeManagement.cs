using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace SaleManagement
{
    public partial class EmployeeManagement : Form
    {
        private int roleId; // Thay đổi từ userRoleId sang roleId
        public static string connectionString = "Server=DESKTOP-GG53O03\\DONTRUNG;Database=ASM 1;Trusted_Connection=True;";

        public EmployeeManagement(int roleId)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterScreen;
            dgv_employeeManagement.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            btnAdd.Click += new EventHandler(btnAdd_Click);
            btnEdit.Click += new EventHandler(btnEdit_Click);
            btnDelete.Click += new EventHandler(btnDelete_Click);
            btnSearch.Click += new EventHandler(btnSearch_Click);
            dgv_employeeManagement.CellClick += new DataGridViewCellEventHandler(dgv_employeeManagement_CellContentClick);

            this.LoadData();
            // Gán roleId
            SetUserRoleId(roleId);
            LoadRoles();
        }

        public enum UserRole
        {
            Admin = 1,
            Sale = 2,
            Warehouse = 3,
            Employee = 4
        }

        private void LoadData()
        {
            string query = "SELECT * FROM Employee";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dgv_employeeManagement.DataSource = dataTable;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        private void LoadRoles()
        {
            string query = "SELECT * FROM Role";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable roleTable = new DataTable();
                    adapter.Fill(roleTable);

                    cmbRole.DataSource = roleTable;
                    cmbRole.DisplayMember = "roleName";
                    cmbRole.ValueMember = "id";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        private void dgv_employeeManagement_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow selectedRow = dgv_employeeManagement.Rows[e.RowIndex];
                txtCode.Text = selectedRow.Cells["code"].Value.ToString();
                txtName.Text = selectedRow.Cells["name"].Value.ToString();
                txtPosition.Text = selectedRow.Cells["position"].Value.ToString();
                txtUsername.Text = selectedRow.Cells["username"].Value.ToString();
                txtPassword.Text = ""; // Không hiển thị mật khẩu
                cmbRole.SelectedValue = selectedRow.Cells["roleId"].Value;
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
            if (roleId != (int)UserRole.Admin)
            {
                MessageBox.Show("Bạn không có quyền thêm nhân viên.");
                return;
            }

            if (!ValidateInput()) return;

            string code = txtCode.Text;
            string name = txtName.Text;
            string position = txtPosition.Text;
            string username = txtUsername.Text;
            string password = HashPassword(txtPassword.Text);
            int selectedRoleId = (int)cmbRole.SelectedValue;

            string query = "INSERT INTO Employee (code, name, position, username, password, roleId) VALUES (@Code, @Name, @Position, @Username, @Password, @RoleId)";
            ExecuteNonQuery(query, new SqlParameter[]
            {
                new SqlParameter("@Code", code),
                new SqlParameter("@Name", name),
                new SqlParameter("@Position", position),
                new SqlParameter("@Username", username),
                new SqlParameter("@Password", password),
                new SqlParameter("@RoleId", selectedRoleId)
            });
            LoadData();
            MessageBox.Show("Employee added successfully!");
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {

            if (roleId != (int)UserRole.Admin)
            {
                MessageBox.Show("Bạn không có quyền thêm nhân viên.");
                return;
            }

            if (dgv_employeeManagement.SelectedRows.Count > 0)
            {
                var selectedValue = dgv_employeeManagement.SelectedRows[0].Cells["code"].Value;
                if (selectedValue != null)
                {
                    string code = selectedValue.ToString();
                    string name = txtName.Text;
                    string position = txtPosition.Text;
                    string username = txtUsername.Text;
                    string password = HashPassword(txtPassword.Text);
                    int selectedRoleId = (int)cmbRole.SelectedValue;

                    string query = "UPDATE Employee SET name = @Name, position = @Position, username = @Username, password = @Password, roleId = @RoleId WHERE code = @Code";
                    ExecuteNonQuery(query, new SqlParameter[]
                    {
                        new SqlParameter("@Code", code),
                        new SqlParameter("@Name", name),
                        new SqlParameter("@Position", position),
                        new SqlParameter("@Username", username),
                        new SqlParameter("@Password", password),
                        new SqlParameter("@RoleId", selectedRoleId)
                    });
                    LoadData();
                    MessageBox.Show("Employee updated successfully!");
                }
            }
            else
            {
                MessageBox.Show("Please select an employee to edit.");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (roleId != (int)UserRole.Admin)
            {
                MessageBox.Show("Bạn không có quyền xóa nhân viên.");
                return;
            }

            if (dgv_employeeManagement.SelectedRows.Count > 0)
            {
                var selectedValue = dgv_employeeManagement.SelectedRows[0].Cells["code"].Value;
                if (selectedValue != null)
                {
                    string code = selectedValue.ToString();
                    string query = "DELETE FROM Employee WHERE code = @Code";
                    ExecuteNonQuery(query, new SqlParameter[]
                    {
                        new SqlParameter("@Code", code)
                    });
                    LoadData();
                    MessageBox.Show("Employee deleted successfully!");
                }
                else
                {
                    MessageBox.Show("Selected employee ID is not valid.");
                }
            }
            else
            {
                MessageBox.Show("Please select an employee to delete.");
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text;
            string query = "SELECT * FROM Employee WHERE name LIKE @SearchTerm OR code LIKE @SearchTerm";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    adapter.SelectCommand.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dgv_employeeManagement.DataSource = dataTable;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            txtCode.Clear();
            txtName.Clear();
            txtPosition.Clear();
            txtUsername.Clear();
            txtPassword.Clear();
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

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred: " + ex.Message);
                    }
                }
            }
        }

        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private bool AuthenticateUser(string username, string password)
        {
            string hashedPassword = HashPassword(password);
            string query = "SELECT RoleId FROM Employee WHERE username = @Username AND password = @Password";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", hashedPassword);

                    try
                    {
                        connection.Open();
                        var roleIdObj = command.ExecuteScalar();
                        if (roleIdObj != null)
                        {
                            roleId = (int)roleIdObj; // Lưu vai trò người dùng
                            return true;
                        }
                        return false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred: " + ex.Message);
                        return false;
                    }
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin.");
                return false;
            }
            return true;
        }

        public void SetUserRoleId(int roleId)
        {
            this.roleId = roleId; // Gán giá trị roleId
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Logic cho button nếu cần
        }
    }
}
