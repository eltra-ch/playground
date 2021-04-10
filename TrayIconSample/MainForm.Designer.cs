using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using System.ServiceProcess;

namespace TrayIconSample
{
    partial class MainForm
    {
        #region Private fields 

        private IContainer _container = null;

        private NotifyIcon _notifyIcon;
        private ContextMenuStrip _contextMenu;
        private ToolStripMenuItem _exitMenuItem;
        private ToolStripMenuItem _startMenuItem;
        private ToolStripMenuItem _stopMenuItem;

        private Point _initialLocation;
        private Size _initialSize;
        private bool _isVisible;

        string _serviceName = "ConsentUxUserSvc_c9c04";
        private ServiceController _serviceController;

        #endregion

        #region Methods

        private void AddMenuItems()
        {
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
                new ToolStripItem[] { _exitMenuItem, _startMenuItem, _stopMenuItem });
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
        }

        private void ShowWindow()
        {
            Location = _initialLocation;
            Size = _initialSize;

            _isVisible = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            HideWindow();

            CreateContainer();
            
            CreateContextMenu();

            CreateNotifyicon();

            CreateServiceController();

            base.OnLoad(e);
        }

        private void CreateServiceController()
        {
            _serviceController = new ServiceController(_serviceName, ".");

            UpdateMenuItems();
        }

        private void CreateContainer()
        {
            _container = new Container();
        }

        private void CreateNotifyicon()
        {
            _notifyIcon = new NotifyIcon(this._container) { Icon = TrayIconSample.AppResource.App };
            
            _notifyIcon.ContextMenuStrip = this._contextMenu;

            _notifyIcon.Text = "Console App (Console example)";
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

        #endregion

        #region Menu commands handling

        private void OnStartClicked(object sender, EventArgs e)
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
                Console.WriteLine(ex);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void OnStopClicked(object sender, EventArgs e)
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
                Console.WriteLine(ex);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void OnExitClicked(object Sender, EventArgs e)
        {
            Application.Exit();
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
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(543, 250);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion
    }
}

