﻿using Newtonsoft.Json.Linq;
using System.Globalization;
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

        public string BillNoText
        {
            get { return BillType + " No: " + _phoneBill._billNo; }
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
            get { return "Bill Date: " + _phoneBill._billDate.ToString("d"); }
        }

        public string BillType
        {
            get
            {
                if (_phoneBill.BillType.Equals("STPPM")) { return "SingTel Bill"; }
                if (_phoneBill.BillType.Equals("APPM")) { return "AirTel Bill"; }
                if (_phoneBill.BillType.Equals("VPPM")) { return "Vodafone Bill"; }
                if (_phoneBill.BillType.Equals("RPPM")) { return "Reliance Bill"; }
                if (_phoneBill.BillType.Equals("TDPPM")) { return "Tata Docomo Bill"; }
                return "Bill";
            }
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
