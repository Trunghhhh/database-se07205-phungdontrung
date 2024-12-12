using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaleManagement
{
    public partial class AddProductForm : Form
    {
        public static string connectionString
          = "Server=DESKTOP-GG53O03\\DONTRUNG;Database=ASM 1;Trusted_Connection=True;";
        public AddProductForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string MaSp = txb_code.Text.ToString();
            string TenSp = txb_name.Text.ToString();
            int SoLuong = int.Parse(txb_amount.Text.ToString());
            int Gia = int.Parse(txb_price.Text.ToString());

            InsertData(MaSp, TenSp, SoLuong, Gia);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            txb_amount.Text = "";
            txb_code.Text = "";
            txb_name.Text = "";
            txb_price.Text = "";
        }
        private void InsertData(string code, string name, int ammount, int price)
        {
            // Connection string to your database

            // SQL query to insert data
            string query = "INSERT INTO Product (Code, Name, Quantity, Price) VALUES (@Code, @Name, @Quantity, @Price)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // Open the connection
                    connection.Open();

                    // Create the SQL command
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters to prevent SQL injection
                        command.Parameters.AddWithValue("@Name", name);

                        command.Parameters.AddWithValue("@Code", code);
                        command.Parameters.AddWithValue("@Quantity", ammount);
                        command.Parameters.AddWithValue("@Price", price);


                        // Execute the command
                        int rowsAffected = command.ExecuteNonQuery();
                        MessageBox.Show($"{rowsAffected} row(s) inserted successfully.");
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
