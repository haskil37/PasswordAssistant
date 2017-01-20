using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PasswordAssistant
{
    struct PAStruct
    {
        public string program;
        public string login;
        public string password;

        public PAStruct(string program, string login, string password)
        {
            this.program = program;
            this.login = login;
            this.password = password;
        }
    }
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon m_notifyIcon;
        private WindowState m_storedWindowState = WindowState.Normal;
        string password = "AnyPa$$w0rd";
        string path = System.Windows.Forms.Application.StartupPath + "\\data.pass";
        List<PAStruct> data;
        private static readonly byte[] SALT = new byte[] { 0x26, 0xdc, 0xff, 0x00, 0xad, 0xed, 0x7a, 0xee, 0xc5, 0xfe, 0x07, 0xaf, 0x4d, 0x08, 0x22, 0x3c };
        public string EncryptString(string plainText)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, SALT);
            rijndaelCipher.Key = pdb.GetBytes(32);
            rijndaelCipher.IV = pdb.GetBytes(16);
            MemoryStream memoryStream = new MemoryStream();
            ICryptoTransform rijndaelEncryptor = rijndaelCipher.CreateEncryptor();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndaelEncryptor, CryptoStreamMode.Write);
            byte[] plainBytes = UTF8Encoding.UTF8.GetBytes(plainText);
            cryptoStream.Write(plainBytes, 0, plainBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            string cipherText = Convert.ToBase64String(cipherBytes, 0, cipherBytes.Length);
            return cipherText;
        }
        public string DecryptString(string cipherText)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, SALT);
            rijndaelCipher.Key = pdb.GetBytes(32);
            rijndaelCipher.IV = pdb.GetBytes(16);
            MemoryStream memoryStream = new MemoryStream();
            ICryptoTransform rijndaelDecryptor = rijndaelCipher.CreateDecryptor();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndaelDecryptor, CryptoStreamMode.Write);
            string plainText = String.Empty;
            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);
                cryptoStream.FlushFinalBlock();
                byte[] plainBytes = memoryStream.ToArray();
                plainText = UTF8Encoding.UTF8.GetString(plainBytes, 0, plainBytes.Length);
            }
            finally
            {
                memoryStream.Close();
                cryptoStream.Close();
            }
            return plainText;
        }
        public MainWindow()
        {
            InitializeComponent();

            Microsoft.Win32.RegistryKey reg = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            if (reg.GetValue("PasswordAssistant") == null)
                checkBox.IsChecked = false;
            else
                checkBox.IsChecked = true;
            reg.Close();

            var primaryMonitorArea = SystemParameters.WorkArea;
            Left = primaryMonitorArea.Right - Width;
            Top = primaryMonitorArea.Bottom - Height;
            this.Top = Top;
            this.Left = Left;

            var bitmap = new Bitmap(Properties.Resources._lock);
            var handle = bitmap.GetHicon();
            m_notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = System.Drawing.Icon.FromHandle(handle),
                Visible = true,
                Text = "Парольный помощник",
            };
            m_notifyIcon.Click += new EventHandler(m_notifyIcon_Click);
            Hide();
            WindowState = WindowState.Minimized;
            Read();
        }
        private void Window_Deactivated(object sender, EventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            const string name = "PasswordAssistant";
            string ExePath = System.Windows.Forms.Application.ExecutablePath;
            Microsoft.Win32.RegistryKey reg = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            try
            {
                reg.SetValue(name, ExePath);
                reg.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.RegistryKey reg = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            try
            {
                reg.DeleteValue("PasswordAssistant");
                reg.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        void OnStateChanged(object sender, EventArgs args)
        {
            if (WindowState == WindowState.Minimized)
                Hide();
            else
                m_storedWindowState = WindowState;
            AddValues.IsOpen = false;
            ShowValues.IsOpen = false;
            listBox.SelectedIndex = -1;
        }
        void m_notifyIcon_Click(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                Show();
                WindowState = m_storedWindowState;
            }
            else
                Hide();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_notifyIcon.Dispose();
            m_notifyIcon = null;
        }
        private void Read()
        {
            data = new List<PAStruct>();
            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.OpenOrCreate)))
                {
                    while (reader.PeekChar() > -1)
                    {
                        var entry = new PAStruct
                        {
                            program = DecryptString(reader.ReadString()),
                            login = DecryptString(reader.ReadString()),
                            password = DecryptString(reader.ReadString()),
                        };
                        data.Add(entry);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            foreach (var item in data)
            {
                var textblock = new TextBlock();
                var run = new Run();
                run.Text = item.program;
                run.FontSize = 16;
                textblock.Inlines.Add(run);
                textblock.Cursor = Cursors.Hand;
                textblock.Padding = new Thickness(10, 10, 10, 10);
                textblock.MouseLeftButtonDown += listBox_MouseLeftButtonDown;
                listBox.Items.Add(textblock);
            }
        }
        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            passVisible.Visibility = Visibility.Hidden;
            pass.Visibility = Visibility.Visible;
            var index = ((ListBox)sender).SelectedIndex;
            if (index != -1)
            {
                var value = data[index];
                programm.Content = value.program;
                login.Text = value.login;
                pass.Password = value.password;
                ShowValues.IsOpen = true;
            }
        }
        private void listBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowValues.IsOpen = false;
            if (sender is ListBox)
            {
                listBox.SelectionChanged -= listBox_SelectionChanged;
                listBox.UnselectAll();
                listBox.SelectionChanged += listBox_SelectionChanged;
            }
        }
        private void add_Click(object sender, RoutedEventArgs e)
        {
            AddValues.IsOpen = true;
            ShowValues.IsOpen = false;
            programmAdd.Focus();
        }
        private void save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(programmAdd.Text))
                return;
            if (string.IsNullOrEmpty(loginAdd.Text))
                return;
            if (string.IsNullOrEmpty(passAdd.Text))
                return;

            var value = new PAStruct();
            value.program = EncryptString(programmAdd.Text);
            value.login = EncryptString(loginAdd.Text);
            value.password = EncryptString(passAdd.Text);
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Append)))
                {
                    writer.Write(value.program);
                    writer.Write(value.login);
                    writer.Write(value.password);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            AddValues.IsOpen = false;
            listBox.Items.Clear();
            Read();
        }
        private void edit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(login.Text))
                return;
            if (string.IsNullOrEmpty(pass.Password))
                return;

            if (!string.IsNullOrEmpty(passVisible.Text))
                if (passVisible.Text != pass.Password)
                    pass.Password = passVisible.Text;

            var del = data.Where(u => u.program == programm.Content.ToString()).SingleOrDefault();
            data.Remove(del);
            var newProgramm = new PAStruct();
            newProgramm.program = programm.Content.ToString();
            newProgramm.login = login.Text;
            newProgramm.password = pass.Password;            
            data.Add(newProgramm);
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Truncate)))
                {
                    foreach (var item in data)
                    {
                        writer.Write(EncryptString(item.program));
                        writer.Write(EncryptString(item.login));
                        writer.Write(EncryptString(item.password));
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            add.Focus();
            ShowValues.IsOpen = false;
            listBox.Items.Clear();
            Read();
        }
        private void del_Click(object sender, RoutedEventArgs e)
        {
            var del = data.Where(u => u.program == programm.Content.ToString()).SingleOrDefault();
            data.Remove(del);
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Truncate)))
                {
                    foreach (var item in data)
                    {
                        writer.Write(EncryptString(item.program));
                        writer.Write(EncryptString(item.login));
                        writer.Write(EncryptString(item.password));
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            add.Focus();
            ShowValues.IsOpen = false;
            listBox.Items.Clear();
            Read();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (pass.Focusable)
                if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
                    Clipboard.SetText(pass.Password);
            if (login.Focusable)
                if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
                    Clipboard.SetText(login.Text);
        }
        new private void GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox)
            {
                login.SelectAll();
                Clipboard.SetText(login.Text);
            }
            if (sender is PasswordBox)
            {
                pass.SelectAll();
                Clipboard.SetText(pass.Password);
            }
        }
        new private void GotMouseCapture(object sender, MouseEventArgs e)
        {
            if (sender is TextBox)
            {
                login.SelectAll();
                Clipboard.SetText(login.Text);
            }
            if (sender is PasswordBox)
            {
                pass.SelectAll();
                Clipboard.SetText(pass.Password);
            }
        }
        private void eye_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (passVisible.Visibility != Visibility.Hidden)
            {
                pass.Password = passVisible.Text;
                passVisible.Visibility = Visibility.Hidden;
                pass.Visibility = Visibility.Visible;
            }
            else
            {
                passVisible.Text = pass.Password;
                passVisible.Visibility = Visibility.Visible;
                pass.Visibility = Visibility.Hidden;
            }
        }
    }
}