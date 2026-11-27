using System;
using System.IO;
using System.Windows.Forms;

namespace KIWIARENAA
{
    public partial class Form1 : Form
    {
        private readonly string fileName = "users.csv";

        public Form1()
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

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string userInput = txtUserName.Text.Trim();
            string passInput = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(userInput) || string.IsNullOrEmpty(passInput))
            {
                MessageBox.Show("Please enter username and password.", "KIWIARENA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            EnsureUsersFileExists();
            string[] users = File.ReadAllLines(fileName);
            bool found = false;
            string matchedName = userInput;

            for (int i = 0; i < users.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(users[i]))
                {
                    continue;
                }

                string[] details = users[i].Split(',');
                if (details.Length < 3)
                {
                    continue;
                }

                string storedName = details[0].Trim();
                string storedEmail = details[1].Trim();
                string storedPass = details[2].Trim();

                bool userMatches =
                    string.Equals(storedName, userInput, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(storedEmail, userInput, StringComparison.OrdinalIgnoreCase);

                if (userMatches && storedPass == passInput)
                {
                    found = true;
                    matchedName = string.IsNullOrEmpty(storedName) ? userInput : storedName;
                    break;
                }
            }

            if (found)
            {
                MessageBox.Show("Welcome " + matchedName, "KIWIARENA", MessageBoxButtons.OK, MessageBoxIcon.Information);

                Form3 nextForm = new Form3();
                nextForm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Wrong username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            Form2 registerForm = new Form2();
            registerForm.Show();
            this.Hide();
        }

        private void btnShowPassword_Click(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !txtPassword.UseSystemPasswordChar;
            btnShowPassword.Text = txtPassword.UseSystemPasswordChar ? "👁" : "🙈";
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }
    }
}