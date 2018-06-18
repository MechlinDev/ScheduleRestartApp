using Microsoft.Win32;
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
using System.Security.Cryptography;

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
                if (txtUser.Text == string.Empty)
                {
                        txtUser.Text = (string)(key.OpenSubKey(RegistryLocation, RegistryKeyPermissionCheck.ReadWriteSubTree)).GetValue("LastUsedUsername");
                }
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
                MessageBox.Show("Application exited with error message: " + Environment.NewLine + Environment.NewLine + ex.Message, "Ooops! something went wrong", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        //key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("DefaultPassword", txtPass.Text, RegistryValueKind.String);
                        LSAutil obj = new LSAutil("DefaultPassword");
                        obj.SetSecret(txtPass.Text);
                        // key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("DefaultPassword", Encrypt(txtPass.Text), RegistryValueKind.String);
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
                    string Author = Application.ExecutablePath.Replace(Application.StartupPath, "").Replace(".exe", "").Trim('\\') + "-RestartTask";
                    // Create a new task definition and assign properties
                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Description = "Restart at 2 AM using Autologon credentials";
                    td.Principal.LogonType = TaskLogonType.InteractiveToken;
                    td.RegistrationInfo.Author = Author;
                    // Add a trigger that will fire the task at this time every other day
                    DailyTrigger dt = (DailyTrigger)td.Triggers.Add(new DailyTrigger { DaysInterval = 1, StartBoundary = DateTime.Today + TimeSpan.FromHours(2) });
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
                    //MessageBox.Show("Task created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                //task to auto run exe on window start
                using (TaskService ts = new TaskService())
                {
                    string Author = Application.ExecutablePath.Replace(Application.StartupPath, "").Replace(".exe", "").Trim('\\') + "AppRunTask";
                    // Create a new task definition and assign properties
                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Description = "Auto-run on window start";
                    td.Principal.LogonType = TaskLogonType.InteractiveToken;
                    td.RegistrationInfo.Author = Author;
                    // Add a trigger that will fire the task at this time every other day

                    //DailyTrigger dt = (DailyTrigger)td.Triggers.Add(new DailyTrigger { DaysInterval = 1, StartBoundary = DateTime.Today + TimeSpan.FromHours(2) });
                    LogonTrigger lt = (LogonTrigger)td.Triggers.Add(new LogonTrigger());
                    //dt.Repetition.Duration = TimeSpan.Zero;
                    //dt.Repetition.Interval = TimeSpan.FromDays(1);
                    lt.Repetition.Duration = TimeSpan.Zero;
                    td.Principal.RunLevel = TaskRunLevel.Highest;

                    td.Actions.Add(new ExecAction(Application.ExecutablePath));

                    // Register the task in the root folder
                    ts.RootFolder.RegisterTaskDefinition(Author, td);

                }




            }
            catch (Exception ex) { LogExceptions(ex); }
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
                        if (File.Exists(StartupFolder + "\\" + ShortcutName))
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
            catch (Exception ex) { LogExceptions(ex); }
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
            catch (Exception ex) { LogExceptions(ex); return false; }
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

        private static void LogExceptions(Exception e)
        {
            try
            {
                if (!Directory.Exists(@"C:\ScheduleRestartAppLogs"))
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
            catch (Exception)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Application.Exit();
            }

        }

        /*****
        // encoding
        public static string Encrypt(string strData)
        {
            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(strData)));
            // reference https://msdn.microsoft.com/en-us/library/ds4kkd55(v=vs.110).aspx

        }
        // decoding
        public static string Decrypt(string strData)
        {
            return Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(strData)));
            // reference https://msdn.microsoft.com/en-us/library/system.convert.frombase64string(v=vs.110).aspx

        }

        // encrypt
        public static byte[] Encrypt(byte[] strData)
        {
            PasswordDeriveBytes passbytes =
            new PasswordDeriveBytes(Global.strPermutation,
            new byte[] { Global.bytePermutation1,
                         Global.bytePermutation2,
                         Global.bytePermutation3,
                         Global.bytePermutation4
            });

            MemoryStream memstream = new MemoryStream();
            Aes aes = new AesManaged();
            aes.Key = passbytes.GetBytes(aes.KeySize / 8);
            aes.IV = passbytes.GetBytes(aes.BlockSize / 8);

            CryptoStream cryptostream = new CryptoStream(memstream,
            aes.CreateEncryptor(), CryptoStreamMode.Write);
            cryptostream.Write(strData, 0, strData.Length);
            cryptostream.Close();
            return memstream.ToArray();
        }

        // decrypt
        public static byte[] Decrypt(byte[] strData)
        {
            PasswordDeriveBytes passbytes =
            new PasswordDeriveBytes(Global.strPermutation,
            new byte[] { Global.bytePermutation1,
                         Global.bytePermutation2,
                         Global.bytePermutation3,
                         Global.bytePermutation4
            });

            MemoryStream memstream = new MemoryStream();
            Aes aes = new AesManaged();
            aes.Key = passbytes.GetBytes(aes.KeySize / 8);
            aes.IV = passbytes.GetBytes(aes.BlockSize / 8);

            CryptoStream cryptostream = new CryptoStream(memstream,
            aes.CreateDecryptor(), CryptoStreamMode.Write);
            cryptostream.Write(strData, 0, strData.Length);
            cryptostream.Close();
            return memstream.ToArray();
        }
        // reference 
        // https://msdn.microsoft.com/en-us/library/system.security.cryptography.cryptostream%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396
        // https://msdn.microsoft.com/en-us/library/system.security.cryptography.rfc2898derivebytes(v=vs.110).aspx


    ******/




    }
    /*******
    public static class Global
    {
   

        // Testing
        //public const String strPassword = "ABCZYZabczyx123890!@\"\\#/:;<>?$%^&*()-_+={}[]";

        // set permutations
        public const string strPermutation = "ouiveyxaqtd";
        public const int bytePermutation1 = 0x19;
        public const int bytePermutation2 = 0x59;
        public const int bytePermutation3 = 0x17;
        public const int bytePermutation4 = 0x41;

 

    }
    *******/


    /// <summary>
    /// /https://gist.github.com/RezaAmbler/bc91bfeb57458bb9a9bc
    /// </summary>
    public class LSAutil
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct LSA_UNICODE_STRING
        {
            public UInt16 Length;
            public UInt16 MaximumLength;
            public IntPtr Buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LSA_OBJECT_ATTRIBUTES
        {
            public int Length;
            public IntPtr RootDirectory;
            public LSA_UNICODE_STRING ObjectName;
            public uint Attributes;
            public IntPtr SecurityDescriptor;
            public IntPtr SecurityQualityOfService;
        }

        private enum LSA_AccessPolicy : long
        {
            POLICY_VIEW_LOCAL_INFORMATION = 0x00000001L,
            POLICY_VIEW_AUDIT_INFORMATION = 0x00000002L,
            POLICY_GET_PRIVATE_INFORMATION = 0x00000004L,
            POLICY_TRUST_ADMIN = 0x00000008L,
            POLICY_CREATE_ACCOUNT = 0x00000010L,
            POLICY_CREATE_SECRET = 0x00000020L,
            POLICY_CREATE_PRIVILEGE = 0x00000040L,
            POLICY_SET_DEFAULT_QUOTA_LIMITS = 0x00000080L,
            POLICY_SET_AUDIT_REQUIREMENTS = 0x00000100L,
            POLICY_AUDIT_LOG_ADMIN = 0x00000200L,
            POLICY_SERVER_ADMIN = 0x00000400L,
            POLICY_LOOKUP_NAMES = 0x00000800L,
            POLICY_NOTIFICATION = 0x00001000L
        }

        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        private static extern uint LsaRetrievePrivateData(
                    IntPtr PolicyHandle,
                    ref LSA_UNICODE_STRING KeyName,
                    out IntPtr PrivateData
        );

        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        private static extern uint LsaStorePrivateData(
                IntPtr policyHandle,
                ref LSA_UNICODE_STRING KeyName,
                ref LSA_UNICODE_STRING PrivateData
        );

        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        private static extern uint LsaOpenPolicy(
            ref LSA_UNICODE_STRING SystemName,
            ref LSA_OBJECT_ATTRIBUTES ObjectAttributes,
            uint DesiredAccess,
            out IntPtr PolicyHandle
        );

        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        private static extern uint LsaNtStatusToWinError(
            uint status
        );

        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        private static extern uint LsaClose(
            IntPtr policyHandle
        );

        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        private static extern uint LsaFreeMemory(
            IntPtr buffer
        );

        private LSA_OBJECT_ATTRIBUTES objectAttributes;
        private LSA_UNICODE_STRING localsystem;
        private LSA_UNICODE_STRING secretName;

        public LSAutil(string key)
        {
            if (key.Length == 0)
            {
                throw new Exception("Key lenght zero");
            }

            objectAttributes = new LSA_OBJECT_ATTRIBUTES();
            objectAttributes.Length = 0;
            objectAttributes.RootDirectory = IntPtr.Zero;
            objectAttributes.Attributes = 0;
            objectAttributes.SecurityDescriptor = IntPtr.Zero;
            objectAttributes.SecurityQualityOfService = IntPtr.Zero;

            localsystem = new LSA_UNICODE_STRING();
            localsystem.Buffer = IntPtr.Zero;
            localsystem.Length = 0;
            localsystem.MaximumLength = 0;

            secretName = new LSA_UNICODE_STRING();
            secretName.Buffer = Marshal.StringToHGlobalUni(key);
            secretName.Length = (UInt16)(key.Length * UnicodeEncoding.CharSize);
            secretName.MaximumLength = (UInt16)((key.Length + 1) * UnicodeEncoding.CharSize);
        }

        private IntPtr GetLsaPolicy(LSA_AccessPolicy access)
        {
            IntPtr LsaPolicyHandle;

            uint ntsResult = LsaOpenPolicy(ref this.localsystem, ref this.objectAttributes, (uint)access, out LsaPolicyHandle);

            uint winErrorCode = LsaNtStatusToWinError(ntsResult);
            if (winErrorCode != 0)
            {
                throw new Exception("LsaOpenPolicy failed: " + winErrorCode);
            }

            return LsaPolicyHandle;
        }

        private static void ReleaseLsaPolicy(IntPtr LsaPolicyHandle)
        {
            uint ntsResult = LsaClose(LsaPolicyHandle);
            uint winErrorCode = LsaNtStatusToWinError(ntsResult);
            if (winErrorCode != 0)
            {
                throw new Exception("LsaClose failed: " + winErrorCode);
            }
        }

        public void SetSecret(string value)
        {
            LSA_UNICODE_STRING lusSecretData = new LSA_UNICODE_STRING();

            if (value.Length > 0)
            {
                //Create data and key
                lusSecretData.Buffer = Marshal.StringToHGlobalUni(value);
                lusSecretData.Length = (UInt16)(value.Length * UnicodeEncoding.CharSize);
                lusSecretData.MaximumLength = (UInt16)((value.Length + 1) * UnicodeEncoding.CharSize);
            }
            else
            {
                //Delete data and key
                lusSecretData.Buffer = IntPtr.Zero;
                lusSecretData.Length = 0;
                lusSecretData.MaximumLength = 0;
            }

            IntPtr LsaPolicyHandle = GetLsaPolicy(LSA_AccessPolicy.POLICY_CREATE_SECRET);
            uint result = LsaStorePrivateData(LsaPolicyHandle, ref secretName, ref lusSecretData);
            ReleaseLsaPolicy(LsaPolicyHandle);

            uint winErrorCode = LsaNtStatusToWinError(result);
            if (winErrorCode != 0)
            {
                throw new Exception("StorePrivateData failed: " + winErrorCode);
            }
        }
    }
}
