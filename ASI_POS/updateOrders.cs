using ASI_POS.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ASI_POS
{
    class updateOrders
    {
        clsSettings settings = new clsSettings();
        public bool updateorder(string ordFileName, string ordContent, string pmtFileName, string pmtContent)
        {
            settings.LoadSettings();
            XmlSerializer serializer = new XmlSerializer(typeof(VFPData));
            VFPData Ord_Data;
            VFPData Pmt_Data;
            try
            {
                using (StringReader sr = new StringReader(ordContent))
                {
                    Ord_Data = (VFPData)serializer.Deserialize(sr);
                }

                using (StringReader sr = new StringReader(pmtContent))
                {
                    Pmt_Data = (VFPData)serializer.Deserialize(sr);
                }
            }
            catch (Exception ex)
            {
                SafeShowStatus($"Deserialize Failed for {ordFileName}: {ex.Message}");
                return false;
            }
            return false;
        }
        private void SafeShowStatus(string msg)
        {
            if (Form1.Instance != null)
            {
                try { Form1.Instance.ShowStatus(msg); } catch { }
            }
            else
            {
                try { File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download.log"), DateTime.Now + " " + msg + Environment.NewLine); } catch { }
            }
        }
    }
}
