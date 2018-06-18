﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.DirectoryServices.AccountManagement;
using Microsoft.Win32.TaskScheduler;
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

            //recode according to
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
                    //txtDomain.Text = (string)(key.OpenSubKey(RegistryLocation, RegistryKeyPermissionCheck.ReadWriteSubTree)).GetValue("DefaultDomainName");
                    txtUser.Text = (string)(key.OpenSubKey(RegistryLocation, RegistryKeyPermissionCheck.ReadWriteSubTree)).GetValue("DefaultUserName");
                    Application.DoEvents();
                    string IsEnabled = (string)(key.OpenSubKey(RegistryLocation, RegistryKeyPermissionCheck.ReadWriteSubTree)).GetValue("AutoAdminLogon");
                    key.Close();
                    if (IsEnabled == "1")
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

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                //if (string.IsNullOrEmpty(txtUser.Text) || string.IsNullOrEmpty(txtPass.Text) || string.IsNullOrEmpty(txtDomain.Text))
                if (string.IsNullOrEmpty(txtUser.Text) || string.IsNullOrEmpty(txtPass.Text))
                {
                    MessageBox.Show("All fields are required!", "Ooops! something went wrong", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {

                    if (!ValidateLogin())
                    {
                        MessageBox.Show("Incorrect Password!", "Ooops! something went wrong", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtPass.Text = string.Empty;
                    }
                    else
                    {
                        var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                        // key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", RegistryKeyPermissionCheck.ReadWriteSubTree);
                        key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("AutoAdminLogon", "1", RegistryValueKind.String);
                        key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("DefaultUserName", txtUser.Text, RegistryValueKind.String);
                        key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("DefaultPassword", txtPass.Text, RegistryValueKind.String);
                        //key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("DefaultDomainName", txtDomain.Text, RegistryValueKind.String);
                        key.Close();
                        MessageBox.Show("Auto-Logon Enabled Successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CreateTask();
                       // CreateShortcut();
                       //as window startup does not run administrator priviledge app we create task shedulaer to auto run this app on window start for indefinite period
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        Application.Exit();
                    }
                }
            }
            catch (Exception ex)
            {
                LogExceptions(ex);
            }
        }

        //private void BtnDisable_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        //        // key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", RegistryKeyPermissionCheck.ReadWriteSubTree);
        //        key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("AutoAdminLogon", "0", RegistryValueKind.String);
        //        key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", RegistryKeyPermissionCheck.ReadWriteSubTree).DeleteValue("DefaultUserName", false);
        //        key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", RegistryKeyPermissionCheck.ReadWriteSubTree).DeleteValue("DefaultPassword", false);
        //        key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", RegistryKeyPermissionCheck.ReadWriteSubTree).DeleteValue("DefaultDomainName",false);
        //        key.Close();
        //        MessageBox.Show("Auto-Logon Disabled Successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        DeleteTask();
        //        GC.Collect();
        //        GC.WaitForPendingFinalizers();
        //        Application.Exit();
        //    }
        //    catch(Exception ex)
        //    {
        //        LogExceptions(ex);
        //    }
        //}

        private void CreateTask()
        {
            try
            {
                
                //task to shut down
                // Get the service on the local machine
                using (TaskService ts = new TaskService())
                {
                    string Author = Application.ExecutablePath.Replace(Application.StartupPath, "").Replace(".exe", "").Trim('\\')+"-RestartTask";
                    // Create a new task definition and assign properties
                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Description = "Restart at 2 AM using Autologon credentials";
                    td.Principal.LogonType = TaskLogonType.InteractiveToken;
                    td.RegistrationInfo.Author = Author;
                    // Add a trigger that will fire the task at this time every other day
                    DailyTrigger dt = (DailyTrigger)td.Triggers.Add(new DailyTrigger { DaysInterval=1,StartBoundary=DateTime.Today +TimeSpan.FromHours(2)});
                    //dt.Repetition.Duration = TimeSpan.FromHours(1);
                    //dt.Repetition.Interval = TimeSpan.FromHours(1);
                    //dt.Repetition.Duration = TimeSpan.FromDays(365);
                    dt.Repetition.Duration = TimeSpan.Zero;
                    dt.Repetition.Interval = TimeSpan.FromDays(1);
                    // Add a trigger that will fire every week on Friday
                    //td.Triggers.Add(new WeeklyTrigger
                    //{
                    //    StartBoundary = DateTime.Today
                    //   + TimeSpan.FromHours(2),
                    //    DaysOfWeek = DaysOfTheWeek.Friday
                    //});

                    // Add an action that will launch Notepad whenever the trigger fires
                    //td.Actions.Add(new ExecAction("notepad.exe", "c:\\test.log", null));
                    td.Actions.Add(new ExecAction("shutdown.exe", arguments: "-r -t 10"));

                    // Register the task in the root folder

                    ts.RootFolder.RegisterTaskDefinition(Author, td);
                    MessageBox.Show("Task created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                //task to auto run exe on window start
                using (TaskService ts = new TaskService())
                {
                    string Author = Application.ExecutablePath.Replace(Application.StartupPath, "").Replace(".exe", "").Trim('\\')+"AppRunTask";
                    // Create a new task definition and assign properties
                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Description = "Auto-run on window start";
                    td.Principal.LogonType = TaskLogonType.InteractiveToken;
                    td.RegistrationInfo.Author = Author;
                  // Add a trigger that will fire the task at this time every other day
                    
                    //DailyTrigger dt = (DailyTrigger)td.Triggers.Add(new DailyTrigger { DaysInterval = 1, StartBoundary = DateTime.Today + TimeSpan.FromHours(2) });
                    LogonTrigger lt= (LogonTrigger)td.Triggers.Add(new LogonTrigger());
                    //dt.Repetition.Duration = TimeSpan.Zero;
                    //dt.Repetition.Interval = TimeSpan.FromDays(1);
                    lt.Repetition.Duration = TimeSpan.Zero;
                    td.Principal.RunLevel = TaskRunLevel.Highest;
                     
                    td.Actions.Add(new ExecAction(Application.ExecutablePath ));

                    // Register the task in the root folder
                    ts.RootFolder.RegisterTaskDefinition(Author, td);
                    
                }




            }
            catch(Exception ex) { LogExceptions(ex); }
        }

        private void DeleteTask()
        {
            throw new NotImplementedException();
        }

        private void CreateShortcut()
        {
            try
            {
                Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); //Windows Script Host Shell Object
                dynamic shell = Activator.CreateInstance(t);
                try
                {
                    string ShortcutName = "SamrtLogin.lnk";
                    var lnk = shell.CreateShortcut(ShortcutName);
                    try
                    {
                        
                        lnk.TargetPath = Application.ExecutablePath;
                        lnk.IconLocation = "shell32.dll, 1";
                        
                        lnk.Save();

                        string StartupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                        if (File.Exists(StartupFolder + "\\"+ ShortcutName))
                        {
                            File.Delete(StartupFolder + "\\" + ShortcutName);
                        }
                        File.Move(Application.StartupPath + "\\" + ShortcutName, StartupFolder + "\\" + ShortcutName);
                    }
                    finally
                    {
                        Marshal.FinalReleaseComObject(lnk);
                    }
                }
                finally
                {
                    Marshal.FinalReleaseComObject(shell);
                }
            }
            catch(Exception ex) { LogExceptions(ex); }
        }

        private bool ValidateLogin()
        {
            try
            {
                bool valid = false;

                using (PrincipalContext context = new PrincipalContext(ContextType.Machine))
                {
                    valid = context.ValidateCredentials(txtUser.Text, txtPass.Text);
                }
                return valid;
            }  
            catch(Exception ex) { LogExceptions(ex); return false; }
        }

        public bool IsUserAdministrator()
        {
            bool isAdmin;
            try
            {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException)
            {
                isAdmin = false;
            }
            catch (Exception)
            {
                isAdmin = false;
            }
            return isAdmin;
        }

        private static void LogExceptions(Exception e) {
            try
            {
                if(!Directory.Exists(@"C:\ScheduleRestartAppLogs"))
                    {
                    Directory.CreateDirectory(@"C:\ScheduleRestartAppLogs");
                }
                using (StreamWriter sw = new StreamWriter(@"C:\ScheduleRestartAppLogs\ErrorLog_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".txt"))
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
