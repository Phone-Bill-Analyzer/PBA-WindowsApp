using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBA_Application
{
    public sealed class PBAApplication
    {
        private static PBAApplication instance;
        private DBHelper dbHelper;
        private Dictionary<string, ObservablePhoneBill> _phoneBillDictionary;
        private List<ObservablePhoneBill> _phoneBillList;

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
        }

        public IEnumerable<ObservablePhoneBill> PhoneBillList
        {
            get { return _phoneBillList.AsEnumerable(); }
        }

        internal bool DeletePhoneBill(string billNo)
        {
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

            bool success = DBHelper.getInstance().executeQueries(queryList);
            return success;
        }

        private void LoadPhoneBillListFromDB()
        {
            _phoneBillList.Clear();

            _phoneBillList.AddRange(DBHelper.getInstance().GetPhoneBillList());
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

            if (GetObservablePhoneBill(pb._billNo) != null)
            {
                _phoneBillList.Remove(opb);
                
            }
            else
            {
                _phoneBillDictionary.Add(pb._billNo, opb);
            }

            _phoneBillList.Add(opb);
        }

    }
}
