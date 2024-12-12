using System;
using System.Windows.Forms;

namespace SaleManagement
{
    public partial class MenuForm : Form
    {
        private int _roleId; // Đổi tên biến thành _roleId

        public MenuForm(int roleId) // Nhận roleId từ constructor
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            _roleId = roleId; // Gán roleId
            SetupPermissions();
        }

        private void SetupPermissions()
        {
            // Kiểm tra vai trò và ẩn/show các nút tương ứng
            switch (_roleId) // Sử dụng _roleId ở đây
            {
                case 1: // Admin
                    EnableAllButtons(true);
                    break;

                case 2: // Sale
                    SetButtonState(true, true, true, false, true, true);
                    break;

                case 3: // Warehouse
                    SetButtonState(true, false, false, false, false, true);
                    break;

                case 4: // Employee
                    SetButtonState(true, false, false, false, false, true);
                    break;

                default:
                    MessageBox.Show("Vai trò không hợp lệ.");
                    break;
            }
        }

        private void SetButtonState(bool button1State, bool button2State, bool button3State, bool button4State, bool button5State, bool button6State)
        {
            button1.Enabled = button1State;
            button2.Enabled = button2State;
            button3.Enabled = button3State;
            button4.Enabled = button4State;
            button5.Enabled = button5State;
            button6.Enabled = button6State;
        }

        private void EnableAllButtons(bool enable)
        {
            foreach (Control control in this.Controls)
            {
                if (control is Button button)
                {
                    button.Enabled = enable;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MainForm main = new MainForm(_roleId);
            main.SetUserRoleId(_roleId); // Truyền _roleId vào MainForm
            main.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CustomerForm customerForm = new CustomerForm(_roleId); // Truyền _roleId vào constructor
            customerForm.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PurchaseHistory purchaseHistory = new PurchaseHistory(_roleId);
            purchaseHistory.Show();
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            EmployeeManagement employeeManagement = new EmployeeManagement(_roleId); // Truyền _roleId vào EmployeeManagement
            employeeManagement.Show();
            this.Hide();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Setting settings = new Setting();
            settings.Show();
            this.Hide();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Close();
            Form1 mainForm = new Form1();
            mainForm.Show();
        }

        private void MenuForm_Load(object sender, EventArgs e)
        {
            // Có thể thêm logic nào đó nếu cần khi form được tải
        }
    }
}