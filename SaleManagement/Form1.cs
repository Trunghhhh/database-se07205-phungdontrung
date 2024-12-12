using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace SaleManagement
{
    public partial class Form1 : Form
    {
        public static string connectionString = "Server=DESKTOP-GG53O03\\DONTRUNG;Database=ASM 1;Trusted_Connection=True;";
        private int roleId = -1; // Thay đổi từ userRoleId sang roleId

        public Form1()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = txb_username.Text;
            string password = txb_password.Text;

            string hashPassword = HashPassword(password); // Băm mật khẩu

            // Gọi CheckLogin và truyền roleId
            bool checkLogin = CheckLogin(username, hashPassword, out roleId);

            if (checkLogin)
            {
                // Mở form chính với roleId
                MenuForm main = new MenuForm(roleId); // Truyền roleId cho MenuForm
                main.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng!");
            }
        }

        private bool CheckLogin(string username, string hashedPassword, out int roleId)
        {
            string query = "SELECT password, roleId FROM Employee WHERE username = @username";
            roleId = -1; // Khởi tạo roleId

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@username", username));

                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedHash = reader["password"].ToString(); // Lấy mật khẩu đã băm
                                roleId = reader.GetInt32(reader.GetOrdinal("roleId")); // Lấy roleId

                                // So sánh mật khẩu
                                if (storedHash.Equals(hashedPassword, StringComparison.Ordinal))
                                {
                                    return true; // Đăng nhập thành công
                                }
                                else
                                {
                                    MessageBox.Show("Mật khẩu không khớp!"); // Thông báo mật khẩu không khớp
                                }
                            }
                            else
                            {
                                MessageBox.Show("Tên đăng nhập không tồn tại!"); // Thông báo nếu không tìm thấy tên đăng nhập
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        MessageBox.Show($"Lỗi kết nối cơ sở dữ liệu: {sqlEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}");
                    }
                }
            }

            return false; // Đăng nhập thất bại
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            txb_password.UseSystemPasswordChar = !checkBox1.Checked;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            txb_username.Clear();
            txb_password.Clear();
            txb_username.Focus();
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2")); // Chuyển đổi thành hệ thập lục phân
                }
                return builder.ToString();
            }
        }
    }
}