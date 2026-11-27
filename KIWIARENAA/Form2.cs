using System;
using System.IO;
using System.Windows.Forms;

namespace KIWIARENAA
{
    public partial class Form2 : Form
    {
        private readonly string fileName = "users.csv";

        public Form2()
        {
            InitializeComponent();
            EnsureUsersFileExists();
        }

        private void EnsureUsersFileExists()
        {
            if (!File.Exists(fileName))
            {
                File.WriteAllText(fileName, string.Empty);
            }
        }

        private bool IsDuplicateEmail(string email)
        {
            if (!File.Exists(fileName))
            {
                return false;
            }

            string[] lines = File.ReadAllLines(fileName);
            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                string[] parts = lines[i].Split(',');
                if (parts.Length >= 2 &&
                    string.Equals(parts[1].Trim(), email, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string fullName = txtName.Text.Trim().Replace(",", " ");
            string email = txtMail.Text.Trim().Replace(",", "");
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPass.Text;

            if (string.IsNullOrEmpty(fullName) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Please fill all the information.", "Missing Details", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Password and Confirm Password should be the same.", "Password Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (IsDuplicateEmail(email))
            {
                MessageBox.Show("An account with this email already exists.", "Account Exists", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            EnsureUsersFileExists();
            string record = fullName + "," + email + "," + password + Environment.NewLine;
            File.AppendAllText(fileName, record);

            MessageBox.Show("Registered: " + fullName, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            txtName.Clear();
            txtMail.Clear();
            txtPassword.Clear();
            txtConfirmPass.Clear();

            Form1 loginForm = new Form1();
            loginForm.Show();
            this.Hide();
        }

        private void btnShowPassword_Click(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !txtPassword.UseSystemPasswordChar;
            btnShowPassword.Text = txtPassword.UseSystemPasswordChar ? "👁" : "🙈";
        }

        private void btnShowConfirmPassword_Click(object sender, EventArgs e)
        {
            txtConfirmPass.UseSystemPasswordChar = !txtConfirmPass.UseSystemPasswordChar;
            btnShowConfirmPassword.Text = txtConfirmPass.UseSystemPasswordChar ? "👁" : "🙈";
        }
    }
}