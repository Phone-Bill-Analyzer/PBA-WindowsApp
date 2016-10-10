using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBA_Application
{
    public sealed class ObservablePhoneBill
    {
        private PhoneBill _phoneBill;
        private string _billNo;

        public ObservablePhoneBill(string bill_no)
        {
            _billNo = bill_no;
            _phoneBill = new PhoneBill();

            _phoneBill._billNo = bill_no;
            _phoneBill._dueDate = DateTime.Now;
        }

        public string BillNo
        {
            get { return _phoneBill._billNo; }
        }

        public string DueDate
        {
            get { return _phoneBill._dueDate.ToString(CultureInfo.CurrentCulture.DateTimeFormat); }
        }

    }
}
