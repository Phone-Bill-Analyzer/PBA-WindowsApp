using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Web.Http;

namespace PBA_Application
{
    public sealed class PhoneBill
    {
        internal string _phoneNo, _billNo, _billType, _password;
        private DateTime _dueDate, _fromDate, _toDate, _billDate;
        private List<CallDetailItem> _callDetails;
        private StorageFile _file;

        public string BillType
        {
            get { return _billType; }
            set { _billType = value; }
        }

        public string FilePassword
        {
            get { return _password; }
            set { _password = value; }
        }

        public PhoneBill(StorageFile file)
        {
            _file = file;
            _callDetails = new List<CallDetailItem>();
        }

        internal PhoneBill()
        {
            _callDetails = new List<CallDetailItem>();
        }

        internal string BillDate
        {
            get { return _billDate.ToString(CultureInfo.CurrentCulture.DateTimeFormat); }
            set { _billDate = DateTime.Parse(value); }
        }

        internal string FromDate
        {
            get { return _fromDate.ToString(CultureInfo.CurrentCulture.DateTimeFormat); }
            set { _fromDate = DateTime.Parse(value); }
        }

        internal string ToDate
        {
            get { return _toDate.ToString(CultureInfo.CurrentCulture.DateTimeFormat); }
            set { _toDate = DateTime.Parse(value); }
        }

        internal string DueDate
        {
            get { return _dueDate.ToString(CultureInfo.CurrentCulture.DateTimeFormat); }
            set { _dueDate = DateTime.Parse(value); }
        }

        public IAsyncOperation<bool> ReadPDFFile()
        {
            return __ReadPDFFile().AsAsyncOperation();
        }

        private async Task<bool> __ReadPDFFile()
        {

            using (HttpClient hc = new HttpClient())
            {
                Uri address = new Uri("http://apps.ayansh.com/Phone-Bill-Analyzer/parse_bill.php");

                HttpMultipartFormDataContent formData = new HttpMultipartFormDataContent();
                formData.Add(new HttpStringContent(_billType), "type");
                formData.Add(new HttpStringContent(_password), "password");

                Stream file_data = await _file.OpenStreamForReadAsync();
                HttpStreamContent file_stream = new HttpStreamContent(file_data.AsInputStream());
                file_stream.Headers.Add("Content-type", "application/pdf");
                formData.Add(file_stream, "file", _file.Name);

                HttpResponseMessage response = await hc.PostAsync(address, formData).AsTask();
                string response_text = await response.Content.ReadAsStringAsync();

                try
                {
                    JObject result = JObject.Parse(response_text);

                    int status = (int)result.GetValue("ErrorCode");
                    string message = result.GetValue("Message").ToString();
                    int pages = (int)result.GetValue("PageCount");

                    JObject billDetails = (JObject)result.GetValue("BillDetails");

                    _phoneNo = billDetails.GetValue("PhoneNumber").ToString();
                    BillDate = billDetails.GetValue("BillDate").ToString();
                    _billNo = billDetails.GetValue("BillNo").ToString();

                    try { FromDate = billDetails.GetValue("FromDate").ToString(); }
                    catch (Exception ex) {}

                    try { ToDate = billDetails.GetValue("ToDate").ToString(); }
                    catch (Exception ex){}

                    try{ DueDate = billDetails.GetValue("DueDate").ToString(); }
                    catch (Exception ex){}

                    JArray call_details = (JArray)result.GetValue("CallDetails");
                    CallDetailItem cd = null;

                    foreach (JObject call_detail in call_details)
                    {
                        cd = new CallDetailItem(call_detail.ToString());
                        _callDetails.Add(cd);
                    }

                    file_data.Dispose();

                    SaveToDB();
                }
                catch (Exception ex)
                {
                    file_data.Dispose();
                    return false;
                }

            }

            return true;
        }

        internal void SaveToDB()
        {
            bool exists = DBHelper.getInstance().CheckBillNoExists(_billNo);
            bool success;

            List<DBQuery> queryList = new List<DBQuery>();
            String sql;                 // The query itself
            DBQuery query;              // Query Object

            if (exists)
            {
                success = PBAApplication.getInstance().DeletePhoneBill(_billNo);
            }

            // Insert Bill Meta Data
            sql = "INSERT INTO BillMetaData VALUES(?,?,?,?,?,?,?,?)";
            query = new DBQuery();
            query.Query = sql;
            query.addQueryData(_billNo);
            query.addQueryData(_billType);
            query.addQueryData(_phoneNo);
            query.addQueryData(BillDate);
            query.addQueryData(_billDate.Ticks / (10000*1000));   // Ticks in seconds
            query.addQueryData(FromDate);
            query.addQueryData(ToDate);
            query.addQueryData(DueDate);
            queryList.Add(query);

            // Insert Call Details
            foreach (CallDetailItem cd in _callDetails)
            {
                sql = "INSERT INTO BillCallDetails VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?)";
                query = new DBQuery();
                query.Query = sql;
                query.addQueryData(_billNo);
                query.addQueryData(cd._phoneNo);
                query.addQueryData(cd._callDate);
                query.addQueryData(cd._callTime);
                query.addQueryData(cd._callDateTime);
                query.addQueryData(cd._duration);
                query.addQueryData(cd._callCost);
                query.addQueryData(cd._callDirection);
                query.addQueryData(cd._comments);
                query.addQueryData(cd._freeCall);
                query.addQueryData(cd._roamingCall);
                query.addQueryData(cd._smsCall);
                query.addQueryData(cd._stdCall);
                query.addQueryData(cd._pulse);
                queryList.Add(query);
            }

            success = DBHelper.getInstance().executeQueries(queryList);
        }

    }

}