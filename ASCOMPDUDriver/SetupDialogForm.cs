using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ASCOM.Utilities;
using ASCOM.APCPDU;

namespace ASCOM.APCPDU
{
    [ComVisible(false)]					// Form not registered for COM!
    public partial class SetupDialogForm : Form
    {
        public SetupDialogForm()
        {
            InitializeComponent();
            // Initialise current values of user settings from the ASCOM Profile
            InitUI();
        }

        private void cmdOK_Click(object sender, EventArgs e) // OK button event handler
        {
            // Place any validation constraint checks here
            // Update the state variables with results from the dialogue
            Switch.pdu_host = txtHost.Text;
            Switch.pdu_username = txtUsername.Text;
            Switch.pdu_password = txtPassword.Text;

            int port;
            if (int.TryParse(txtPort.Text, out port))
                Switch.pdu_port = port;
            else
                Switch.pdu_port = 22;

            int keepAliveInterval;
            if (int.TryParse(txtKeepAliveInterval.Text, out keepAliveInterval))
                Switch.pdu_keepAliveInterval = port;
            else
                Switch.pdu_keepAliveInterval = 22;

            Switch.tl.Enabled = chkTrace.Checked;
        }

        private void cmdCancel_Click(object sender, EventArgs e) // Cancel button event handler
        {
            Close();
        }

        private void BrowseToAscom(object sender, EventArgs e) // Click on ASCOM logo event handler
        {
            try
            {
                System.Diagnostics.Process.Start("http://ascom-standards.org/");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        private void InitUI()
        {
            txtHost.Text = Switch.pdu_host;
            txtPort.Text = Switch.pdu_port.ToString();
            txtUsername.Text = Switch.pdu_username;
            txtPassword.Text = Switch.pdu_password;
            txtKeepAliveInterval.Text = Switch.pdu_keepAliveInterval.ToString();
        }
    }
}