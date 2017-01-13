using System;
using System.Collections.Generic;
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
        string password = "ghfdhgfdhgfdhgfd";
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
        }
        void OnStateChanged(object sender, EventArgs args)
        {
            if (WindowState == WindowState.Minimized)
                Hide();
            else
                m_storedWindowState = WindowState;
        }
        void m_notifyIcon_Click(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                Show();
                this.Top = Top;
                this.Left = Left;
                WindowState = m_storedWindowState;
            }
            else
                Hide();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            m_notifyIcon.Dispose();
            m_notifyIcon = null;
            Application.Current.Shutdown();
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            List<PAStruct> db = new List<PAStruct>();
            var value = new PAStruct();
            value.program = EncryptString("steam");
            value.login = EncryptString("haskil37");
            value.password = EncryptString("Пароль");

            db.Add(value);

            string path = @"C:\data.pass";

            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate)))
                {
                    foreach (PAStruct s in db)
                    {
                        writer.Write(s.program);
                        writer.Write(s.login);
                        writer.Write(s.password);
                    }
                }
            }
            catch 
            {
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = @"C:\data.pass";

                // создаем объект BinaryWriter
                // создаем объект BinaryReader
                using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
                {
                    // пока не достигнут конец файла
                    // считываем каждое значение из файла
                    while (reader.PeekChar() > -1)
                    {
                        string program = reader.ReadString();
                        string login = reader.ReadString();
                        string password = reader.ReadString();
                        program = DecryptString(program);
                        login = DecryptString(login);
                        password = DecryptString(password);

                        string t = String.Format("Страна: {0}  столица: {1}  площадь {2}",
                            program, login, password);
                        MessageBox.Show(t);
                    }
                }
            }
            catch 
            {
            }
        }
    }
}
