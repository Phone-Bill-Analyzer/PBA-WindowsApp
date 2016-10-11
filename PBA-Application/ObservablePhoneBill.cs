using Newtonsoft.Json.Linq;
using Windows.Foundation.Metadata;

namespace PBA_Application
{
    [AllowForWeb]
    public sealed class ObservablePhoneBill
    {
        private PhoneBill _phoneBill;

        public ObservablePhoneBill(PhoneBill phone_bill)
        {
            _phoneBill = phone_bill;
        }

        public string BillNo
        {
            get { return _phoneBill._billNo; }
        }

        public string DueDate
        {
            get { return _phoneBill.DueDate; }
        }

        public string BillDate
        {
            get { return _phoneBill.BillDate; }
        }

        public string summarizeByContactNames()
        {
            JArray result = DBHelper.getInstance().GetSummaryByContactNames(_phoneBill._billNo);
            return result.ToString();
        }

        public string getAllBillDetails()
        {
            JArray result = DBHelper.getInstance().GetAllBillDetails(_phoneBill._billNo);
            return result.ToString();
        }

        public string getTop5ContactsByAmount()
        {
            JArray result = DBHelper.getInstance().GetTop5ContactsByAmount(_phoneBill._billNo);
            return result.ToString();
        }

        public string getSummaryByContactGroups()
        {
            JArray result = DBHelper.getInstance().GetSummaryByContactGroups(_phoneBill._billNo);
            return result.ToString();
        }
    }
}
