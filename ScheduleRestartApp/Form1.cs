using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScheduleRestartApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void chkAutoLogOn_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoLogOn.Checked)
            {
                DialogResult result = MessageBox.Show("Are you sure to use current Logon?", "Confirm!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                Application.DoEvents();
                if (result == DialogResult.Yes)
                {
                    //yes...
                    WindowsIdentity currUser = WindowsIdentity.GetCurrent();
                    if (currUser != null)
                    {
                        string userName = currUser.Name;
                        MessageBox.Show(userName);
                        Application.DoEvents();
                    }
                    else
                    {
                        //ask password and loginto windows
                    }
                }
                else if (result == DialogResult.No)
                {
                    //no...
                    MessageBox.Show("NO");
                    Application.DoEvents();
                }
            }
            
           
            
           
        }


    }
}
