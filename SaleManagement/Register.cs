using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaleManagement
{
    public partial class Register : Form
    {
        public Register()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string phone = txtPhone.Text;
            string email = txtEmail.Text;
            string password = txtPassword.Text;
            string enterthepassword = txtEnterThePassword.Text;
            if (string.IsNullOrEmpty(username)
                || string.IsNullOrEmpty(phone)
                || string.IsNullOrEmpty(email)
                || string.IsNullOrEmpty(enterthepassword)
                || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Cannot register", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("Register success", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            Form1 login = new Form1();
            this.Hide();
            login.Show();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            txtUsername.Text = "";
            txtPhone.Text = "";
            txtEmail.Text = "";
            txtEnterThePassword.Text = "";
            txtPassword.Text = "";
        }
    }
}
