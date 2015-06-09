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

        private String validationServer = "http://nebuchadnezzar.xn--paac-11a.net.pl/validateCopy";


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
            System.Net.HttpWebRequest req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(validationServer + "/?serial=" + this.serialNumber + "?hdSerial=" + GetHdId());
            System.Net.HttpWebResponse res = (System.Net.HttpWebResponse)req.GetResponse();
            System.IO.Stream respStream = res.GetResponseStream();
            byte[] response = new byte[32];
            respStream.Read(response, 0, 32);
            if (response.Equals(getHash())) validationOK = true;
            return validationOK;
        }

        private byte[] getHash()
        {
            StringBuilder sb = new StringBuilder();
            MD5 md5 = MD5CryptoServiceProvider.Create();
            byte[] bytes = new byte[this.serialNumber.Length * sizeof(char)];
            System.Buffer.BlockCopy(this.serialNumber.ToCharArray(), 0, bytes, 0, bytes.Length);
            byte[] hash = md5.ComputeHash(bytes);
            return hash;
            //foreach (byte b in hash)
            //    sb.Append(b.ToString("x2"));

            //return sb.ToString();
        }

        private String GetHdId(){
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");
            foreach (ManagementObject wmi_HD in searcher.Get())
            {
                String serial = wmi_HD["SerialNumber"].ToString();
                return serial;
            }
            return "";
        }
    }
}
