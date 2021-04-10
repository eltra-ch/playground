using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using System.ServiceProcess;
using Microsoft.Win32;

namespace TrayIconSample
{
    partial class MainForm
    {
        #region Private fields 

        private IContainer _container = null;

        private NotifyIcon _notifyIcon;
        private ContextMenuStrip _contextMenu;
        private ToolStripMenuItem _exitMenuItem;
        private ToolStripMenuItem _settingsMenuItem;
        private ToolStripMenuItem _startMenuItem;
        private ToolStripMenuItem _stopMenuItem;

        private Point _initialLocation;
        private Size _initialSize;
        private bool _isVisible;

        string _serviceName = "Snoop";
        private ServiceController _serviceController;

        #endregion

        #region Methods

        private void AddMenuItems()
        {
            _settingsMenuItem = new ToolStripMenuItem() { Text = "Settin&gs" };
            _settingsMenuItem.Click += new System.EventHandler(OnSettingsClicked);

            _exitMenuItem = new ToolStripMenuItem() { Text = "E&xit" };
            _exitMenuItem.Click += new System.EventHandler(OnExitClicked);

            _startMenuItem = new ToolStripMenuItem() { Text = "&Start" };
            _startMenuItem.Click += new System.EventHandler(OnStartClicked);

            _stopMenuItem = new ToolStripMenuItem() { Text = "S&top" };
            _stopMenuItem.Click += new System.EventHandler(OnStopClicked);
        }

        private void CreateContextMenu()
        {
            _contextMenu = new ContextMenuStrip();

            AddMenuItems();

            _contextMenu.Items.AddRange(
                new ToolStripItem[] { _settingsMenuItem, 
                                      new ToolStripSeparator(), 
                                      _startMenuItem, 
                                      _stopMenuItem, 
                                      new ToolStripSeparator (),  
                                      _exitMenuItem });
        }

        private void HideWindow()
        {
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;

            _initialLocation = Location;
            _initialSize = Size;

            Location = new Point(-2000, -2000);
            Size = new Size(1, 1);

            _isVisible = false;

            if (_exitMenuItem != null)
            {
                _exitMenuItem.Enabled = true;
            }

            if (_settingsMenuItem != null)
            {
                _settingsMenuItem.Enabled = true;
            }
        }

        private void ShowWindow()
        {
            Location = _initialLocation;
            Size = _initialSize;

            _isVisible = true;

            if (_exitMenuItem != null)
            {
                _exitMenuItem.Enabled = false;
            }

            if(_settingsMenuItem != null)
            {
                _settingsMenuItem.Enabled = false;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            HideWindow();

            CreateContainer();
            
            CreateContextMenu();

            CreateNotifyicon();

            CreateServiceController();

            OpenRegistry();

            base.OnLoad(e);
        }

        private void OpenRegistry()
        {
            try
            {
                using (var snoop = Registry.LocalMachine.OpenSubKey("Software\\WOW6432Node\\Eltra\\Snoop\\"))
                {
                    if (snoop != null)
                    {
                        textBoxUserName.Text = snoop.GetValue("Login") as string;
                        textBoxPassword.Text = snoop.GetValue("LoginPasswd") as string;

                        textBoxAliasUserName.Text = snoop.GetValue("Alias") as string;
                        textBoxAliasPassword.Text = snoop.GetValue("AliasPasswd") as string;

                        snoop.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SaveRegistry()
        {
            try
            {
                using (var snoop = Registry.LocalMachine.OpenSubKey("Software\\WOW6432Node\\Eltra\\Snoop\\", true))
                {
                    if (snoop != null)
                    {
                        snoop.SetValue("Login", textBoxUserName.Text);
                        snoop.SetValue("LoginPasswd", textBoxPassword.Text);

                        snoop.SetValue("Alias", textBoxAliasUserName.Text);
                        snoop.SetValue("AliasPasswd", textBoxAliasPassword.Text);

                        snoop.Close();
                    }
                }

                MessageBox.Show("Changes applied");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CreateServiceController()
        {
            _serviceController = new ServiceController(_serviceName);

            UpdateMenuItems();
        }

        private void CreateContainer()
        {
            _container = new Container();
        }

        private void CreateNotifyicon()
        {
            _notifyIcon = new NotifyIcon(this._container) { Icon = SnoopNotify.AppResource.App };
            
            _notifyIcon.ContextMenuStrip = this._contextMenu;

            _notifyIcon.Text = Text;
            _notifyIcon.Visible = true;

            _notifyIcon.DoubleClick += (sender, args) =>
            {
                if (!_isVisible)
                {
                    ShowWindow();
                }
                else
                {
                    HideWindow();
                }
            };
        }

        private void UpdateMenuItems()
        {
            _serviceController.Refresh();

            try
            {
                if (_serviceController.Status == ServiceControllerStatus.Stopped)
                {
                    _startMenuItem.Enabled = true;
                    _stopMenuItem.Enabled = false;
                }
                else if (_serviceController.Status == ServiceControllerStatus.Running)
                {
                    _startMenuItem.Enabled = false;
                    _stopMenuItem.Enabled = true;
                }
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Menu commands handling

        private void OnStartClicked(object sender, EventArgs e)
        {
            StartService();
        }

        private void StartService()
        {
            try
            {
                _serviceController.Refresh();

                if (_serviceController.Status == ServiceControllerStatus.Stopped)
                {
                    _serviceController.Start();
                    _serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(5));
                }

                UpdateMenuItems();
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnStopClicked(object sender, EventArgs e)
        {
            StopService();
        }

        private void StopService()
        {
            try
            {
                _serviceController.Refresh();

                if (_serviceController.Status == ServiceControllerStatus.Running)
                {
                    _serviceController.Stop();
                    _serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(5));
                }

                UpdateMenuItems();
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnExitClicked(object Sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void buttonApply_Click(object sender, System.EventArgs e)
        {
            StopService();
            
            SaveRegistry();

            StartService();
        }
        
        private void buttonClose_Click(object sender, System.EventArgs e)
        {
            HideWindow();
        }

        private void OnSettingsClicked(object sender, System.EventArgs e)
        {
            ShowWindow();
        }

        #endregion

        #region Dispose

        protected override void Dispose(bool disposing)
        {
            if (disposing && (_container != null))
            {
                _container.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonApply = new System.Windows.Forms.Button();
            this.textBoxUserName = new System.Windows.Forms.TextBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.textBoxAliasPassword = new System.Windows.Forms.TextBox();
            this.textBoxAliasUserName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonClose = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonApply
            // 
            this.buttonApply.Location = new System.Drawing.Point(390, 31);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(154, 33);
            this.buttonApply.TabIndex = 0;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // textBoxUserName
            // 
            this.textBoxUserName.Location = new System.Drawing.Point(110, 29);
            this.textBoxUserName.Name = "textBoxUserName";
            this.textBoxUserName.Size = new System.Drawing.Size(178, 23);
            this.textBoxUserName.TabIndex = 1;
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(110, 71);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(178, 23);
            this.textBoxPassword.TabIndex = 2;
            // 
            // textBoxAliasPassword
            // 
            this.textBoxAliasPassword.Location = new System.Drawing.Point(110, 67);
            this.textBoxAliasPassword.Name = "textBoxAliasPassword";
            this.textBoxAliasPassword.PasswordChar = '*';
            this.textBoxAliasPassword.Size = new System.Drawing.Size(178, 23);
            this.textBoxAliasPassword.TabIndex = 4;
            // 
            // textBoxAliasUserName
            // 
            this.textBoxAliasUserName.Location = new System.Drawing.Point(110, 30);
            this.textBoxAliasUserName.Name = "textBoxAliasUserName";
            this.textBoxAliasUserName.Size = new System.Drawing.Size(178, 23);
            this.textBoxAliasUserName.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(25, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "User name:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(25, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "Password:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBoxUserName);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBoxPassword);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(24, 22);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(329, 116);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Device";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.textBoxAliasUserName);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.textBoxAliasPassword);
            this.groupBox2.Location = new System.Drawing.Point(24, 155);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(329, 113);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Alias";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(25, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 15);
            this.label3.TabIndex = 8;
            this.label3.Text = "Password:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(25, 33);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(66, 15);
            this.label4.TabIndex = 7;
            this.label4.Text = "User name:";
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(390, 85);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(154, 31);
            this.buttonClose.TabIndex = 9;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(563, 282);
            this.ControlBox = false;
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonApply);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Snoop";
            this.TopMost = true;
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Button buttonApply;
        private TextBox textBoxUserName;
        private TextBox textBoxPassword;
        private TextBox textBoxAliasPassword;
        private TextBox textBoxAliasUserName;
        private Label label1;
        private Label label2;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private Label label3;
        private Label label4;
        private Button buttonClose;
    }
}

