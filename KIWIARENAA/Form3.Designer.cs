using System;
using System.Drawing;
using System.Windows.Forms;

namespace KIWIARENAA
{
    partial class Form3
    {
        private System.ComponentModel.IContainer components = null;

        private Panel panelSidebar;
        private Panel panelSidebarBrand;
        private Label lblBrandTitle;
        private Label lblBrandTag;
        private FlowLayoutPanel flowNavButtons;
        private Panel panelSidebarFooter;
        private Label lblSidebarUser;

        private Panel panelTopBar;
        private Label lblPageTitle;
        private Label lblPageSubtitle;
        private TextBox txtQuickSearch;
        private Button btnQuickSearch;
        private Button btnBlackFridayBadge;

        private Panel panelContentHost;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelSidebar = new Panel();
            this.panelSidebarBrand = new Panel();
            this.lblBrandTitle = new Label();
            this.lblBrandTag = new Label();
            this.flowNavButtons = new FlowLayoutPanel();
            this.panelSidebarFooter = new Panel();
            this.lblSidebarUser = new Label();

            this.panelTopBar = new Panel();
            this.lblPageTitle = new Label();
            this.lblPageSubtitle = new Label();
            this.txtQuickSearch = new TextBox();
            this.btnQuickSearch = new Button();
            this.btnBlackFridayBadge = new Button();

            this.panelContentHost = new Panel();

            this.panelSidebar.SuspendLayout();
            this.panelSidebarBrand.SuspendLayout();
            this.panelSidebarFooter.SuspendLayout();
            this.panelTopBar.SuspendLayout();
            this.SuspendLayout();

            // panelSidebar
            this.panelSidebar.BackColor = Color.FromArgb(11, 19, 43);
            this.panelSidebar.Controls.Add(this.flowNavButtons);
            this.panelSidebar.Controls.Add(this.panelSidebarFooter);
            this.panelSidebar.Controls.Add(this.panelSidebarBrand);
            this.panelSidebar.Dock = DockStyle.Left;
            this.panelSidebar.Width = 245;

            // panelSidebarBrand
            this.panelSidebarBrand.BackColor = Color.FromArgb(8, 14, 32);
            this.panelSidebarBrand.Controls.Add(this.lblBrandTag);
            this.panelSidebarBrand.Controls.Add(this.lblBrandTitle);
            this.panelSidebarBrand.Dock = DockStyle.Top;
            this.panelSidebarBrand.Height = 86;

            // lblBrandTitle
            this.lblBrandTitle.AutoSize = false;
            this.lblBrandTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.lblBrandTitle.ForeColor = Color.White;
            this.lblBrandTitle.Location = new Point(18, 16);
            this.lblBrandTitle.Size = new Size(210, 32);
            this.lblBrandTitle.Text = "KIWIARENA";

            // lblBrandTag
            this.lblBrandTag.AutoSize = false;
            this.lblBrandTag.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this.lblBrandTag.ForeColor = Color.FromArgb(240, 84, 35);
            this.lblBrandTag.Location = new Point(19, 48);
            this.lblBrandTag.Size = new Size(210, 22);
            this.lblBrandTag.Text = "AOTEAROA LIVE SPORTS HUB";

            // flowNavButtons
            this.flowNavButtons.BackColor = Color.FromArgb(11, 19, 43);
            this.flowNavButtons.Dock = DockStyle.Fill;
            this.flowNavButtons.FlowDirection = FlowDirection.TopDown;
            this.flowNavButtons.Padding = new Padding(10, 14, 10, 10);
            this.flowNavButtons.WrapContents = false;

            // panelSidebarFooter
            this.panelSidebarFooter.BackColor = Color.FromArgb(16, 27, 60);
            this.panelSidebarFooter.Controls.Add(this.lblSidebarUser);
            this.panelSidebarFooter.Dock = DockStyle.Bottom;
            this.panelSidebarFooter.Height = 56;

            // lblSidebarUser
            this.lblSidebarUser.AutoSize = false;
            this.lblSidebarUser.Dock = DockStyle.Fill;
            this.lblSidebarUser.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.lblSidebarUser.ForeColor = Color.FromArgb(203, 213, 225);
            this.lblSidebarUser.Padding = new Padding(14, 0, 10, 0);
            this.lblSidebarUser.Text = "Fan Member • NZ Ticketing";
            this.lblSidebarUser.TextAlign = ContentAlignment.MiddleLeft;

            // panelTopBar
            this.panelTopBar.BackColor = Color.FromArgb(18, 30, 66);
            this.panelTopBar.Controls.Add(this.btnBlackFridayBadge);
            this.panelTopBar.Controls.Add(this.btnQuickSearch);
            this.panelTopBar.Controls.Add(this.txtQuickSearch);
            this.panelTopBar.Controls.Add(this.lblPageSubtitle);
            this.panelTopBar.Controls.Add(this.lblPageTitle);
            this.panelTopBar.Dock = DockStyle.Top;
            this.panelTopBar.Height = 76;

            // lblPageTitle
            this.lblPageTitle.AutoSize = true;
            this.lblPageTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            this.lblPageTitle.ForeColor = Color.White;
            this.lblPageTitle.Location = new Point(22, 12);
            this.lblPageTitle.Text = "Home";

            // lblPageSubtitle
            this.lblPageSubtitle.AutoSize = true;
            this.lblPageSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.lblPageSubtitle.ForeColor = Color.FromArgb(148, 163, 184);
            this.lblPageSubtitle.Location = new Point(23, 42);
            this.lblPageSubtitle.Text = "Featured matches, NZ sports highlights, news & Black Friday deals";

            // txtQuickSearch
            this.txtQuickSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.txtQuickSearch.BackColor = Color.FromArgb(28, 44, 92);
            this.txtQuickSearch.BorderStyle = BorderStyle.FixedSingle;
            this.txtQuickSearch.Font = new Font("Segoe UI", 10F);
            this.txtQuickSearch.ForeColor = Color.White;
            this.txtQuickSearch.Location = new Point(740, 22);
            this.txtQuickSearch.Size = new Size(235, 30);

            // btnQuickSearch
            this.btnQuickSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnQuickSearch.BackColor = Color.FromArgb(240, 84, 35);
            this.btnQuickSearch.FlatAppearance.BorderSize = 0;
            this.btnQuickSearch.FlatStyle = FlatStyle.Flat;
            this.btnQuickSearch.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            this.btnQuickSearch.ForeColor = Color.White;
            this.btnQuickSearch.Location = new Point(983, 22);
            this.btnQuickSearch.Size = new Size(86, 30);
            this.btnQuickSearch.Text = "Search";
            this.btnQuickSearch.UseVisualStyleBackColor = false;

            // btnBlackFridayBadge
            this.btnBlackFridayBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnBlackFridayBadge.BackColor = Color.FromArgb(245, 158, 11);
            this.btnBlackFridayBadge.FlatAppearance.BorderSize = 0;
            this.btnBlackFridayBadge.FlatStyle = FlatStyle.Flat;
            this.btnBlackFridayBadge.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnBlackFridayBadge.ForeColor = Color.FromArgb(11, 19, 43);
            this.btnBlackFridayBadge.Location = new Point(1078, 22);
            this.btnBlackFridayBadge.Size = new Size(165, 30);
            this.btnBlackFridayBadge.Text = "🔥 Black Friday 25% OFF";
            this.btnBlackFridayBadge.UseVisualStyleBackColor = false;

            // panelContentHost
            this.panelContentHost.AutoScroll = true;
            this.panelContentHost.BackColor = Color.FromArgb(15, 23, 42);
            this.panelContentHost.Dock = DockStyle.Fill;

            // Form3
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(15, 23, 42);
            this.ClientSize = new Size(1510, 860);
            this.Controls.Add(this.panelContentHost);
            this.Controls.Add(this.panelTopBar);
            this.Controls.Add(this.panelSidebar);
            this.MinimumSize = new Size(1120, 700);
            this.Name = "Form3";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "KIWIARENA — New Zealand Sports Ticketing & Hub";

            this.panelSidebar.ResumeLayout(false);
            this.panelSidebarBrand.ResumeLayout(false);
            this.panelSidebarFooter.ResumeLayout(false);
            this.panelTopBar.ResumeLayout(false);
            this.panelTopBar.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}