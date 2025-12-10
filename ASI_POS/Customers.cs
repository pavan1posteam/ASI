using ASI_POS.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Data.OleDb;

namespace ASI_POS
{
    class Customers
    {
        clsSettings settings = new clsSettings();
        private object ToDbValue(string s) => string.IsNullOrWhiteSpace(s) ? "" : s.Trim();
        public bool customerupdate(string fileName, string content)
        {
            settings.LoadSettings();
            XmlSerializer serializer = new XmlSerializer(typeof(VFPData));
            VFPData data;
            try
            {
                using (StringReader sr = new StringReader(content))
                {
                    data = (VFPData)serializer.Deserialize(sr);
                }
            }
            catch (Exception ex)
            {
                SafeShowStatus($"Deserialize Failed for {fileName}: {ex.Message}");
                return false;
            }
            if (data?.CusTables == null || data.CusTables.Count < 0)
                return false;
            if (!settings.updateCustomerFiles)
            {
                SafeShowStatus("Update Customer Files flag is OFF in Settings!!!");
                return false;
            }
            foreach (custable CusUpd in data.CusTables)
            {
                string cusphone = (CusUpd.home ?? string.Empty).Trim();
                string cphone = NormalizePhone(cusphone);
                int Lnwebcustid = 0;
                if (!string.IsNullOrWhiteSpace(CusUpd.custid))
                {
                    int.TryParse(CusUpd.custid, out Lnwebcustid);
                }

                bool LInsertCus = false;
                if (Lnwebcustid != 0 && !LInsertCus)
                {
                    var existing = getCustomersInfo().FirstOrDefault(c => c.Webcustid == Lnwebcustid);
                    if (existing != null)
                    {
                        UpsertCustomerFields(existing, CusUpd, cphone, Lnwebcustid);
                        LInsertCus = true;
                        SafeShowStatus($"Processed Customer Id: {CusUpd.custid}");
                        bool ok = updateCustomer(existing);
                        if (!ok)
                            SafeShowStatus($"Failed to update DBF for customer {Lnwebcustid}");
                    }
                }
                if (!LInsertCus && !string.IsNullOrWhiteSpace(CusUpd.email))
                {
                    string email = CusUpd.email.Trim();
                    var existing = getCustomersInfo().FirstOrDefault(c => !string.IsNullOrEmpty(c.Email) && c.Email.Trim().Equals(email, StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        UpsertCustomerFields(existing, CusUpd, cphone, Lnwebcustid);
                        LInsertCus = true;
                        SafeShowStatus($"Processed Customer Id: {CusUpd.custid}");
                        bool ok = updateCustomer(existing);
                        if (!ok)
                            SafeShowStatus($"Failed to update DBF for customer {Lnwebcustid}");
                    }
                }
                if (!LInsertCus && !string.IsNullOrWhiteSpace(cphone))
                {
                    var existing = getCustomersInfo().FirstOrDefault(c =>
                        (!string.IsNullOrEmpty(c.Phone) && c.Phone.Trim() == cphone));
                    if (existing != null)
                    {
                        UpsertCustomerFields(existing, CusUpd, cphone, Lnwebcustid);
                        LInsertCus = true;
                        SafeShowStatus($"Processed Customer Id: {CusUpd.custid}");
                        bool ok = updateCustomer(existing);
                        if (!ok)
                            SafeShowStatus($"Failed to update DBF for customer {Lnwebcustid}");
                    }
                }
                if (!LInsertCus && !string.IsNullOrWhiteSpace(CusUpd.loyaltyno))
                {
                    string loyalty = CusUpd.loyaltyno.Trim();
                    var existing = getCustomersInfo().FirstOrDefault(c => !string.IsNullOrEmpty(c.Altid) && c.Altid.Trim() == loyalty);
                    if (existing == null)
                    {
                        existing = getCustomersInfo().FirstOrDefault(c => !string.IsNullOrEmpty(c.Clubcard) && c.Clubcard.Trim() == loyalty);
                    }
                    if (existing != null)
                    {
                        UpsertCustomerFields(existing, CusUpd, cphone, Lnwebcustid);
                        LInsertCus = true;
                        SafeShowStatus($"Processed Customer Id: {CusUpd.custid}");
                        bool ok = updateCustomer(existing);
                        if (!ok)
                            SafeShowStatus($"Failed to update DBF for customer {Lnwebcustid}");
                    }
                }
                if (!LInsertCus)
                {
                    var newCustomer = new cus
                    {
                        Firstname = string.IsNullOrWhiteSpace(CusUpd.firstname) ? null : CusUpd.firstname.Trim(),
                        Lastname = string.IsNullOrWhiteSpace(CusUpd.lastname) ? null : CusUpd.lastname.Trim(),
                        Store = settings.webstore,
                        Status = "WEB",
                        Street1 = CusUpd.street,
                        City = CusUpd.city,
                        State = CusUpd.state,
                        Zip = CusUpd.zip,
                        Email = string.IsNullOrWhiteSpace(CusUpd.email) ? null : CusUpd.email.Trim(),
                        Phone = string.IsNullOrWhiteSpace(cphone) ? null : cphone,
                        Taxcode = string.IsNullOrWhiteSpace(CusUpd.taxcode) ? settings.TaxCode : CusUpd.taxcode,
                        Fson = true,
                        Webcustid = Lnwebcustid == 0 ? (int?)null : Lnwebcustid,
                        Startdate = DateTime.Now,
                        Lstore = settings.webstore
                    };

                    if (settings.updateClubcardNo)
                    {
                        if (!string.IsNullOrWhiteSpace(CusUpd.loyaltyno))
                            newCustomer.Clubcard = CusUpd.loyaltyno.Trim();
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(CusUpd.loyaltyno))
                            newCustomer.Clubcard = CusUpd.loyaltyno.Trim();
                    }
                    getCustomersInfo().Add(newCustomer);
                    SafeShowStatus($"Processed New Customer Id: {CusUpd.custid}");
                }
            }
            return true;
        }
        public List<cus> getCustomersInfo()
        {
            settings.LoadSettings();
            if (string.IsNullOrWhiteSpace(settings.serverpath))
                throw new ArgumentException("dbfFolder required", nameof(settings.serverpath));

            if (!System.IO.Directory.Exists(settings.serverpath))
                throw new System.IO.DirectoryNotFoundException($"DBF folder not found: {settings.serverpath}");

            var customers = new List<cus>();

            using (var conn = new OleDbConnection(settings.ConnectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM cus";

                using (var cmd = new OleDbCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var c = new cus();

                        string GetString(int idx1)
                        {
                            if (idx1 < 0) return null;
                            if (reader.IsDBNull(idx1)) return null;
                            return reader.GetValue(idx1)?.ToString().Trim();
                        }

                        int GetOrdinalOrMinus(string name)
                        {
                            try { return reader.GetOrdinal(name); }
                            catch (IndexOutOfRangeException) { return -1; }
                        }
                        int idx;

                        idx = GetOrdinalOrMinus("customer"); if (idx >= 0) { var v = GetString(idx); if (int.TryParse(v, out int ci)) c.Customer = ci; }
                        idx = GetOrdinalOrMinus("firstname"); if (idx >= 0) c.Firstname = GetString(idx);
                        idx = GetOrdinalOrMinus("lastname"); if (idx >= 0) c.Lastname = GetString(idx);
                        idx = GetOrdinalOrMinus("phone"); if (idx >= 0) c.Phone = GetString(idx);
                        idx = GetOrdinalOrMinus("email"); if (idx >= 0) c.Email = GetString(idx);
                        idx = GetOrdinalOrMinus("webcustid"); if (idx >= 0) { var v = GetString(idx); if (int.TryParse(v, out int wcid)) c.Webcustid = wcid; }
                        idx = GetOrdinalOrMinus("clubcard"); if (idx >= 0) c.Clubcard = GetString(idx);
                        idx = GetOrdinalOrMinus("altid"); if (idx >= 0) c.Altid = GetString(idx);
                        idx = GetOrdinalOrMinus("street1"); if (idx >= 0) c.Street1 = GetString(idx);
                        idx = GetOrdinalOrMinus("city"); if (idx >= 0) c.City = GetString(idx);
                        idx = GetOrdinalOrMinus("state"); if (idx >= 0) c.State = GetString(idx);
                        idx = GetOrdinalOrMinus("zip"); if (idx >= 0) c.Zip = GetString(idx);
                        idx = GetOrdinalOrMinus("taxcode"); if (idx >= 0) c.Taxcode = GetString(idx);
                        idx = GetOrdinalOrMinus("store"); if (idx >= 0) c.Store = GetString(idx);
                        idx = GetOrdinalOrMinus("status"); if (idx >= 0) c.Status = GetString(idx);
                        idx = GetOrdinalOrMinus("fson");
                        if (idx >= 0)
                        {
                            var fv = GetString(idx);
                            if (!string.IsNullOrEmpty(fv))
                            {
                                if (fv.Equals("T", StringComparison.OrdinalIgnoreCase) || fv.Equals("Y", StringComparison.OrdinalIgnoreCase) || fv == "1")
                                    c.Fson = true;
                                else if (fv.Equals("F", StringComparison.OrdinalIgnoreCase) || fv == "N" || fv == "0")
                                    c.Fson = false;
                            }
                        }
                        idx = GetOrdinalOrMinus("startdate");
                        if (idx >= 0 && !reader.IsDBNull(idx))
                        {
                            var val = reader.GetValue(idx);
                            if (val is DateTime dt) c.Startdate = dt;
                            else
                            {
                                var str = val?.ToString();
                                if (DateTime.TryParse(str, out DateTime parsed)) c.Startdate = parsed;
                            }
                        }

                        customers.Add(c);
                    }
                }
                conn.Close();
            }

            return customers;
        }
        private void UpsertCustomerFields(cus existing, custable CusUpd, string cphone, int Lnwebcustid)
        {
            if (!string.IsNullOrWhiteSpace(CusUpd.firstname))
                existing.Firstname = CusUpd.firstname.Trim();
            if (!string.IsNullOrWhiteSpace(CusUpd.lastname))
                existing.Lastname = CusUpd.lastname.Trim();
            if (!string.IsNullOrWhiteSpace(cphone))
                existing.Phone = cphone.Trim();
            if (!string.IsNullOrWhiteSpace(CusUpd.email))
                existing.Email = CusUpd.email.Trim();
            if (!string.IsNullOrWhiteSpace(CusUpd.street))
                existing.Street1 = CusUpd.street.Trim();
            if (!string.IsNullOrWhiteSpace(CusUpd.state))
                existing.State = CusUpd.state.Trim();
            if (!string.IsNullOrWhiteSpace(CusUpd.city))
                existing.City = CusUpd.city.Trim();
            if (!string.IsNullOrWhiteSpace(CusUpd.zip))
                existing.Zip = CusUpd.zip.Trim();
            if (Lnwebcustid != 0)
                existing.Webcustid = Lnwebcustid;
            if (settings.updateClubcardNo)
            {
                if (!string.IsNullOrWhiteSpace(CusUpd.loyaltyno))
                    existing.Clubcard = CusUpd.loyaltyno.Trim();
            }
        }
        private bool updateCustomer(cus existing)
        {
            if (existing == null) return false;
            if (string.IsNullOrWhiteSpace(settings.serverpath))
                throw new ArgumentException("dbfFolder required", nameof(settings.serverpath));
            string sql = @"UPDATE cus SET 
                        firstname = ?, 
                        lastname  = ?, 
                        phone     = ?, 
                        email     = ?, 
                        street1     = ?, 
                        state     = ?, 
                        city     = ?, 
                        zip     = ?, 
                        clubcard  = ?, 
                        webcustid = ?
                   WHERE webcustid = ?";
            try
            {
                using (var conn = new OleDbConnection(settings.ConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = sql;
                    cmd.Parameters.Add(new OleDbParameter("p1", ToDbValue(existing.Firstname)));
                    cmd.Parameters.Add(new OleDbParameter("p2", ToDbValue(existing.Lastname)));
                    cmd.Parameters.Add(new OleDbParameter("p3", ToDbValue(existing.Phone)));
                    cmd.Parameters.Add(new OleDbParameter("p4", ToDbValue(existing.Email))); 
                    cmd.Parameters.Add(new OleDbParameter("p5", ToDbValue(existing.Street1)));
                    cmd.Parameters.Add(new OleDbParameter("p6", ToDbValue(existing.State)));
                    cmd.Parameters.Add(new OleDbParameter("p7", ToDbValue(existing.City)));
                    cmd.Parameters.Add(new OleDbParameter("p8", ToDbValue(existing.Zip)));
                    cmd.Parameters.Add(new OleDbParameter("p9", ToDbValue(existing.Clubcard)));
                    cmd.Parameters.Add(new OleDbParameter("p10", existing.Webcustid.HasValue ? (object)existing.Webcustid.Value : DBNull.Value));
                    cmd.Parameters.Add(new OleDbParameter("p11", existing.Webcustid.HasValue ? (object)existing.Webcustid.Value : DBNull.Value));
                    int rows = cmd.ExecuteNonQuery();
                    conn.Close();
                    return rows > 0; 
                }
            }
            catch (OleDbException ex)
            {
                SafeShowStatus($"DBF update failed for WebCustId {existing.Webcustid}: {ex.Message}");
                return false;
            }
        }
        private string NormalizePhone(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            var digits = new string(raw.Where(char.IsDigit).ToArray());
            if (digits.Length >= 10)
            {
                var part1 = digits.Substring(0, 3);
                var part2 = digits.Substring(3, 3);
                var part3 = digits.Substring(6);
                return $"{part1}-{part2}-{part3}";
            }
            return raw.Trim();
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
