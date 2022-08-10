using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace TextManager
{
    class AES256
    {
        private string key;
        private string iv;
        private int bytelen = 32;
        private int bitlen = 256;

        public AES256()
        {
            key = "abcdefgh12345678abcdefgh12345678";
            iv = "abcdefgh12345678";
        }
        public AES256(string _key)
        {
            key = _key;
            if (key.Length > bytelen)
            {
                key = key.Substring(0, bytelen);
            }
            iv = _key;
            if (iv.Length > 16)
            {
                iv = iv.Substring(0, 16);
            }
        }
        public AES256(string _key, string _iv)
        {
            key = _key;
            if (key.Length > bytelen)
            {
                key = key.Substring(0, bytelen);
            }
            iv = _iv;
            if (iv.Length > 16)
            {
                iv = iv.Substring(0, 16);
            }
        }

        public string getKey() { return key; }
        public string getIV() { return iv; }

        public void setKey(string _key)
        { 
            key = _key;
            if (key.Length > bytelen)
            {
                key = key.Substring(0, bytelen);
            }
        }
        public void setIV(string _iv)
        {
            iv = _iv;
            if (iv.Length > 16)
            {
                iv = iv.Substring(0, 16);
            }
        }

        public string Encrypt(string textToEncrypt)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.PKCS7;

            rijndaelCipher.KeySize = bitlen;
            rijndaelCipher.BlockSize = 128;
            byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
            byte[] pwdivBytes = Encoding.UTF8.GetBytes(iv);
            byte[] keyBytes = new byte[bytelen];
            byte[] ivBytes = new byte[16];
            int keylen = pwdBytes.Length;
            int ivlen = pwdivBytes.Length;
            if (keylen > keyBytes.Length)
            {
                keylen = keyBytes.Length;
            }
            if (ivlen > ivBytes.Length)
            {
                ivlen = ivBytes.Length;
            }
            Array.Copy(pwdBytes, keyBytes, keylen);
            Array.Copy(pwdivBytes, ivBytes, ivlen);
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = ivBytes;
            ICryptoTransform transform = rijndaelCipher.CreateEncryptor();
            byte[] plainText = Encoding.UTF8.GetBytes(textToEncrypt);
            return Convert.ToBase64String(transform.TransformFinalBlock(plainText, 0, plainText.Length));
        }
        public string Decrypt(string textToDecrypt)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.PKCS7;

            rijndaelCipher.KeySize = bitlen;
            rijndaelCipher.BlockSize = 128;
            byte[] encryptedData = Convert.FromBase64String(textToDecrypt);
            byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
            byte[] pwdivBytes = Encoding.UTF8.GetBytes(iv);
            byte[] keyBytes = new byte[bytelen];
            byte[] ivBytes = new byte[16];
            int keylen = pwdBytes.Length;
            int ivlen = pwdivBytes.Length;
            if (keylen > keyBytes.Length)
            {
                keylen = keyBytes.Length;
            }
            if (ivlen > ivBytes.Length)
            {
                ivlen = ivBytes.Length;
            }
            Array.Copy(pwdBytes, keyBytes, keylen);
            Array.Copy(pwdivBytes, ivBytes, ivlen);
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = ivBytes;
            byte[] plainText = rijndaelCipher.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length);
            return Encoding.UTF8.GetString(plainText);
        }
    }
    public partial class MainWindow : Window
    {

        private bool isAble = false;
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        private AES256 aes256 = new AES256();
        private void getSettings()
        {
            StringBuilder key = new StringBuilder();
            StringBuilder iv = new StringBuilder();

            GetPrivateProfileString("KeySettings", "key", "", key, 33, "C:\\secure\\settings.ini");
            GetPrivateProfileString("KeySettings", "iv", "", iv, 17, "C:\\secure\\settings.ini");
            if (key.Length > 0) isAble = true;
            else
            {
                OriginalText.IsReadOnly = true;
                OriginalText.Text = "settings ini file not found. Check the file again.";
            }
            aes256.setKey(key.ToString());
            aes256.setIV(iv.ToString());
        }
        public MainWindow()
        {
            InitializeComponent();
            getSettings();
            OriginalText.Focus();
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            if(isAble)
            {
                try
                {
                    ChangedText.Text = aes256.Encrypt(OriginalText.Text);

                }
                catch(Exception ex)
                {
                    ChangedText.Text = ex.Message;
                }
            }  
        }

        private void Revert_Click(object sender, RoutedEventArgs e)
        {
            if (isAble)
            {
                try
                {
                    ChangedText.Text = aes256.Decrypt(OriginalText.Text);

                }
                catch (Exception ex)
                {
                    ChangedText.Text = ex.Message;
                }
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if(isAble) OriginalText.Text = "";
        }

        private void Flush_Click(object sender, RoutedEventArgs e)
        {
            if(isAble)
            {
                OriginalText.Text = "";
                ChangedText.Text = "";
            }
        }

        private void Switch_Click(object sender, RoutedEventArgs e)
        {
            if(isAble)
            {
                OriginalText.Text = ChangedText.Text;
                ChangedText.Text = "";
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if(isAble)
            {
                Clipboard.SetText(ChangedText.Text);
                ChangedText.Text += "\n!!ResultCopied!!";
            }
        }
    }
}
