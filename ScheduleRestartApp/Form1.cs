using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
        string RegistryLocation = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon";
        private void Form1_Load(object sender, EventArgs e)
        {
            //            Application will check for autologon enabled if it is enabled then application exists. 
            //Otherwise it runs AutoLogon -sysinternal app(both apps need to be placed in the same directory with files for task scheduler) which asks user for password.
            //And it recursively waits for 60 seconds and then rechecks if the autologon is enabled.
            //If autologon is enabled then it adds a task to task scheduler to restart the system at 2 AM with autologon account and adds a shortcut to run this app on windows startup and rechecks all the steps.


            //            Kamaraj...This application is not doing what I asked for.  A couple of things that are important.I can't distribute the Sysinternals application so that is not going to work.  I need an application that opens a form that I can design and edit.  The form needs to ask for the password and then handles the tasks.  The form would look something along the line of the attached graphic.  The User Name could be autopopulated with the current user or it could be left off entirely.  I need it to be a self-contained application and I can't have it use external applications that I can't distribute.
            //The image is now attached.  Also, I noticed that the application kept running every minute or so.I do not what it to do that.I can set it to run when the computer starts.I don't need this to be done by the application.

            //----------------check auto  logon

            //var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            //            key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", RegistryKeyPermissionCheck.ReadWriteSubTree);
            //            key.SetValue("AutoAdminLogon", "1", RegistryValueKind.String);
            //            key.SetValue("DefaultUserName", "guest", RegistryValueKind.String);
            //            key.SetValue("DefaultPassword", "password", RegistryValueKind.String);

            try
            { 
              
                var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                txtDomain.Text = (string)(key.OpenSubKey(RegistryLocation, RegistryKeyPermissionCheck.ReadWriteSubTree)).GetValue("DefaultDomainName");
                txtUser.Text = (string)(key.OpenSubKey(RegistryLocation, RegistryKeyPermissionCheck.ReadWriteSubTree)).GetValue("DefaultUserName");
                Application.DoEvents();
                string IsEnabled = (string)(key.OpenSubKey(RegistryLocation, RegistryKeyPermissionCheck.ReadWriteSubTree)).GetValue("AutoAdminLogon");
                key.Close();
                if (IsEnabled=="1")
                {
                    //Do nothing and exit
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Application.Exit();
                }
                else
                {
                    // do nothing wait for button click
                }
            }
            catch (Exception ex)
            {
                LogExceptions(ex);
                MessageBox.Show("Application exited with error message: " + Environment.NewLine +Environment.NewLine + ex.Message, "Ooops! something went wrong", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Application.Exit();
            }
            finally
            {
              // do not exit here
             
            }

        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUser.Text)|| string.IsNullOrEmpty(txtPass.Text)||string.IsNullOrEmpty(txtDomain.Text))
            {
                MessageBox.Show("All fields are required!", "Ooops! something went wrong",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
            else
            {
                var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
               // key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", RegistryKeyPermissionCheck.ReadWriteSubTree);
                key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("AutoAdminLogon", "1", RegistryValueKind.String);
                key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("DefaultUserName",txtUser.Text, RegistryValueKind.String);
                key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("DefaultPassword", txtPass.Text , RegistryValueKind.String);
                key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("DefaultDomainName", txtDomain.Text, RegistryValueKind.String);
                key.Close();
                MessageBox.Show("Auto-Logon Enabled Successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Application.Exit();
            }

        }


        private static void LogExceptions(Exception e) {
            try
            {
                if(!Directory.Exists(@"C:\ScheduleRestartAppLogs"))
                    {
                    Directory.CreateDirectory(@"C:\ScheduleRestartAppLogs");
                }
                using (StreamWriter sw = new StreamWriter(@"C:\ScheduleRestartAppLogs\ErrorLog_" + DateTime.Now.ToString("yyyymmddhhMMss") + ".txt"))
                {
                    sw.WriteLine(e.Message);
                    sw.WriteLine(Environment.NewLine);
                    sw.WriteLine(e.ToString());
                    sw.Flush();
                }
            }
            catch(Exception)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Application.Exit();
            }

        }
    }
}
