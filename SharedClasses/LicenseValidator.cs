using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Security.Cryptography;

namespace SharedClasses
{
    public class LicenseValidator
    {

        private String serialNumber { get; set; }

        //private String validationServer = "http://nebuchadnezzar.xn--paac-11a.net.pl/validateCopy";
        private String validationServer = "http://xn--paac-11a.net.pl/my/nebuchadnezzar/validateCopy.php";


        #region getters setters
        public String SerialNumber
        {
            get
            {
                return this.serialNumber;
            }
        }
        #endregion

        public LicenseValidator(String serial)
        {
            this.serialNumber = serial;
        }

        public bool Validate(){
            bool validationOK = false;
            System.Net.WebClient wc = new System.Net.WebClient();
            String respp = wc.DownloadString(validationServer + "?serial=" + this.serialNumber + "?hdSerial=" + GetHdId());
            Console.WriteLine("Response: " + respp);

            if (respp.Equals(getHash())) validationOK = true;

            return validationOK;
        }

        private String getHash()
        {
            StringBuilder sb = new StringBuilder();
            MD5 md5 = MD5CryptoServiceProvider.Create();
            byte[] hash = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(this.serialNumber));
            foreach (byte b in hash)
                sb.Append(b.ToString("x2"));
            Console.WriteLine("Hash: " + sb.ToString());
            return sb.ToString();
        }

        private String GetHdId(){
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");
            foreach (ManagementObject wmi_HD in searcher.Get())
            {
                String serial = wmi_HD["SerialNumber"].ToString();
                Console.WriteLine("HDD Serial: " + serial);
                return serial;
            }
            return "";
        }
    }
}
