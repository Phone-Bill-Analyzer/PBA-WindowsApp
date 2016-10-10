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
            dbHelper = DBHelper.getInstance();
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

    }
}
