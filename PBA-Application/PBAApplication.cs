using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Networking.PushNotifications;
using Windows.Web.Http;

namespace PBA_Application
{
    public sealed class PBAApplication
    {
        private static PBAApplication instance;
        private DBHelper dbHelper;
        private Dictionary<string, ObservablePhoneBill> _phoneBillDictionary;
        private List<ObservablePhoneBill> _phoneBillList;
        private List<ServiceProvider> _serviceProviderList;

        private LicenseInformation _licenseInfo;

        public static PBAApplication getInstance()
        {
            if (instance == null)
            {
                instance = new PBAApplication();
            }

            return instance;
        }

        private PBAApplication()
        {
            _phoneBillList = new List<ObservablePhoneBill>();
            dbHelper = DBHelper.getInstance();
            LoadPhoneBillListFromDB();

            _serviceProviderList = new List<ServiceProvider>();
            _serviceProviderList.Add(new ServiceProvider("STPPM","SingTel Postpaid"));
            _serviceProviderList.Add(new ServiceProvider("APPM", "AirTel Postpaid"));
            _serviceProviderList.Add(new ServiceProvider("VPPM", "Vodafone Postpaid"));
            _serviceProviderList.Add(new ServiceProvider("RPPM", "Reliance Postpaid"));
            _serviceProviderList.Add(new ServiceProvider("TDPPM", "Tata Docomo Postpaid"));
            
        }

        public void InitializeApplication()
        {
            RegisterForPushNotificationsAsync();
        }

        private async void RegisterForPushNotificationsAsync()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var channel_uri = localSettings.Values["ChannelURI"];
            var reg_status = localSettings.Values["RegistrationStatus"];
            var iid = localSettings.Values["InstanceID"];
            bool save_required = true;
            string platform = "Windows";

            // Get Unique ID
            if (iid == null)
            {
                iid = Windows.System.UserProfile.AdvertisingManager.AdvertisingId;
                if (iid == null || iid.Equals(""))
                {
                    // Generate Random
                    iid = Guid.NewGuid().ToString();
                }

                localSettings.Values["InstanceID"] = iid;
            }

            // Request a Push Notification Channel
            PushNotificationChannel channel = null;

            try
            {
                // Get Channel
                channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();

                // Is it the first time or dejavu?
                if (channel_uri == null || reg_status == null)
                {
                    save_required = true;
                }
                else
                {
                    // OK, so this is Deja-Vu. Is it same as before?
                    if (channel.Uri.Equals(channel_uri) && reg_status.Equals("Success"))
                    {
                        // URI is same. and we have registered it already. so nothing to do.
                        save_required = false;
                    }
                    else
                    {
                        save_required = true;
                    }
                }

                if (!save_required)
                {
                    return;
                }

                // Save it to my server
                using (HttpClient hc = new HttpClient())
                {
                    Uri address = new Uri("http://apps.ayansh.com/HanuGCM/RegisterDevice.php");

                    TimeZoneInfo tz = TimeZoneInfo.Local;
                    Package package = Package.Current;
                    PackageVersion version = package.Id.Version;
                    string app_version = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);

                    var values = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("package", package.Id.Name),
                        new KeyValuePair<string, string>("regid", channel.Uri),
                        new KeyValuePair<string, string>("iid", iid.ToString()),
                        new KeyValuePair<string, string>("tz", tz.StandardName),
                        new KeyValuePair<string, string>("app_version", app_version),
                        new KeyValuePair<string, string>("platform", platform)
                    };

                    HttpFormUrlEncodedContent postContent = new HttpFormUrlEncodedContent(values);
                    HttpResponseMessage response = await hc.PostAsync(address, postContent).AsTask();
                    string response_text = await response.Content.ReadAsStringAsync();

                    if (response_text.Equals("Success"))
                    {
                        // Success
                        localSettings.Values["RegistrationStatus"] = "Success";
                        localSettings.Values["ChannelURI"] = channel.Uri;
                    }
                    else
                    {
                        localSettings.Values["RegistrationStatus"] = "Failed";
                    }

                }

            }

            catch (Exception ex)
            {
                // Could not create a channel. 
                localSettings.Values["ChannelURI"] = "";
                localSettings.Values["RegistrationStatus"] = "Failed";
            }
        }

        public IEnumerable<ObservablePhoneBill> PhoneBillList
        {
            get { return _phoneBillList.AsEnumerable(); }
        }

        public IEnumerable<ServiceProvider> ServiceProviderList
        {
            get { return _serviceProviderList.AsEnumerable(); }
        }

        public bool DeletePhoneBill(string billNo)
        {
            ObservablePhoneBill old_bill = GetObservablePhoneBill(billNo);
            List<DBQuery> queryList = new List<DBQuery>();
            String sql;                 // The query itself
            DBQuery query;              // Query Object

            sql = @"DELETE FROM BillMetaData WHERE BillNo = ?";
            query = new DBQuery();
            query.Query = sql;
            query.addQueryData(billNo);
            queryList.Add(query);

            sql = @"DELETE FROM BillCallDetails WHERE BillNo = ?";
            query = new DBQuery();
            query.Query = sql;
            query.addQueryData(billNo);
            queryList.Add(query);

            bool success = dbHelper.executeQueries(queryList);
            
            if (old_bill != null)
            {
                _phoneBillList.Remove(old_bill);
                _phoneBillDictionary.Remove(billNo);

            }

            return success;
        }

        public IEnumerable<ContactData> GetAppContactList()
        {
            return dbHelper.GetAppContactList();
        }

        public IAsyncOperation<bool> SyncContactsFromDevice()
        {
            return __SyncContactsFromDevice().AsAsyncOperation();
        }

        private async Task<bool> __SyncContactsFromDevice()
        {
            List<string> phone_no_list = dbHelper.GetDistinctPhoneNumbers();
            List<ContactData> contact_data = new List<ContactData>();
            ContactData cd;

            ContactStore contact_store = await ContactManager.RequestStoreAsync();

            foreach (string phone_no in phone_no_list)
            {
                IReadOnlyList<Contact> cl = await contact_store.FindContactsAsync(phone_no).AsTask();

                if (cl.Count > 0)
                {
                    Contact c = cl.FirstOrDefault();

                    cd = new ContactData();
                    cd.PhoneNo = phone_no;
                    cd.ContactName = c.FullName;

                    contact_data.Add(cd);
                }

            }

            // Now save this in DB
            return SaveContactData(contact_data);

        }

        public bool SaveContactData(IEnumerable<ContactData> contact_data)
        {
            List<DBQuery> query_list = new List<DBQuery>();
            DBQuery query;

            foreach (ContactData cd in contact_data)
            {
                string phone = cd.PhoneNo;
                string name = cd.ContactName;
                string group = cd.ContactGroup;

                // Delete Name query.
                query = new DBQuery();
                query.Query = "DELETE FROM ContactNames WHERE PhoneNo = ?";
                query.addQueryData(phone);
                query_list.Add(query);

                // Delete Group query.
                query = new DBQuery();
                query.Query = "DELETE FROM ContactGroups WHERE PhoneNo = ?";
                query.addQueryData(phone);
                query_list.Add(query);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    // Insert Query.
                    query = new DBQuery();
                    query.Query = "insert into ContactNames VALUES(?,?)";
                    query.addQueryData(phone);
                    query.addQueryData(name);
                    query_list.Add(query);
                }

                if (!string.IsNullOrWhiteSpace(group))
                {
                    // Insert Query.
                    query = new DBQuery();
                    query.Query = "insert into ContactGroups VALUES(?,?)";
                    query.addQueryData(phone);
                    query.addQueryData(group);
                    query_list.Add(query);
                }

            }

            return dbHelper.executeQueries(query_list);
        }

        private void LoadPhoneBillListFromDB()
        {
            _phoneBillList.Clear();

            _phoneBillList.AddRange(dbHelper.GetPhoneBillList());
            _phoneBillDictionary = _phoneBillList.ToDictionary(x => x.BillNo);
        }

        public ObservablePhoneBill GetObservablePhoneBill(string bill_no)
        {
            ObservablePhoneBill pb;
            _phoneBillDictionary.TryGetValue(bill_no, out pb);
            return pb;
        }

        public void AddBillToList(PhoneBill pb)
        {
            ObservablePhoneBill opb = new ObservablePhoneBill(pb);
            ObservablePhoneBill old_bill = GetObservablePhoneBill(pb._billNo);

            if (old_bill != null)
            {
                _phoneBillList.Remove(old_bill);
                _phoneBillDictionary.Remove(pb._billNo);
                
            }
            

            _phoneBillList.Add(opb);
            _phoneBillDictionary.Add(pb._billNo, opb);
        }

        public bool IsPremiumUser()
        {

            if (_licenseInfo == null)
            {
                // Get License Info
                //_licenseInfo = CurrentAppSimulator.LicenseInformation;
                _licenseInfo = CurrentApp.LicenseInformation;
            }

            try
            {
                ProductLicense pl;
                if(_licenseInfo.ProductLicenses.TryGetValue("premium_user", out pl))
                {
                    return pl.IsActive;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                return false;
            }
            
        }

    }
}
