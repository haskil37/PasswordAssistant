using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            FileStream stream = new FileStream("C:\\mytext.txt", FileMode.OpenOrCreate, FileAccess.Write);

            DESCryptoServiceProvider cryptic = new DESCryptoServiceProvider();

            cryptic.Key = ASCIIEncoding.ASCII.GetBytes("ABCDEFGH");
            cryptic.IV = ASCIIEncoding.ASCII.GetBytes("ABCDEFGH");

            CryptoStream crStream = new CryptoStream(stream,
               cryptic.CreateEncryptor(), CryptoStreamMode.Write);


            byte[] data = ASCIIEncoding.ASCII.GetBytes("Hello World!");

            crStream.Write(data, 0, data.Length);

            crStream.Close();
            stream.Close();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            FileStream stream = new FileStream("C:\\mytext.txt",
                              FileMode.Open, FileAccess.Read);

            DESCryptoServiceProvider cryptic = new DESCryptoServiceProvider();

            cryptic.Key = ASCIIEncoding.ASCII.GetBytes("ABCDEFGH");
            cryptic.IV = ASCIIEncoding.ASCII.GetBytes("ABCDEFGH");

            CryptoStream crStream = new CryptoStream(stream,
                cryptic.CreateDecryptor(), CryptoStreamMode.Read);

            StreamReader reader = new StreamReader(crStream);

            string data = reader.ReadToEnd();

            reader.Close();
            stream.Close();
            MessageBox.Show(data);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            List<PAStruct> db = new List<PAStruct>();
            var value = new PAStruct();
            value.program = "steam";
            value.login = "haskil37";
            value.password = "";
            db.Add(value);

            string path = @"C:\states.dat";

            try
            {
                // создаем объект BinaryWriter
                using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate)))
                {
                    // записываем в файл значение каждого поля структуры
                    foreach (PAStruct s in db)
                    {
                        writer.Write(s.program);
                        writer.Write(s.login);
                        writer.Write(s.password);
                    }
                }
                // создаем объект BinaryReader
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = @"C:\states.dat";

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

                        string t = String.Format("Страна: {0}  столица: {1}  площадь {2}",
                            program, login, password);
                        MessageBox.Show(t);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}
