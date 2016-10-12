using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBA_Application
{
    public sealed class ContactData
    {
        public ContactData()
        {
            PhoneNo = ContactName = ContactGroup = "";
        }

        public string PhoneNo { get; set; }
        public string ContactName { get; set; }
        public string ContactGroup { get; set; }
    }
}
