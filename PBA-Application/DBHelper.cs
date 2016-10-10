using Newtonsoft.Json.Linq;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.ApplicationModel.Resources;

namespace PBA_Application
{
    public class DBHelper
    {
        private static DBHelper instance;
        private string dbPath;

        public static DBHelper getInstance()
        {
            if (instance == null)
            {
                instance = new DBHelper();
            }

            return instance;
        }

        private DBHelper()
        {
            // Get Application ID
            ResourceLoader rl = new ResourceLoader();
            string app_id = rl.GetString("ApplicationID");

            // Made DB Path
            dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, app_id);

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (localSettings.Values["DBExists"] == null)
            {
                // DB Does not exist. So create
                createDB();
                localSettings.Values["DBExists"] = "X";
            }
            else
            {
                // Nothing to do.
            }
        }

        private void createDB()
        {
            using (SQLiteConnection dbConn = new SQLiteConnection(dbPath))
            {

                string createOptionsTable = "CREATE TABLE Options ("
                + "ParamName VARCHAR(20), " + // Parameter Name
                "ParamValue VARCHAR(20)" + // Parameter Value
                ")";

                string createContactNamesTable = "CREATE TABLE ContactNames ("
                        + "PhoneNo VARCHAR, " + // Phone No
                        "Name VARCHAR" + // Name
                        ")";

                string createContactGroupsTable = "CREATE TABLE ContactGroups ("
                        + "PhoneNo VARCHAR, " + // Phone No
                        "GroupName VARCHAR" + // GroupName
                        ")";

                string createBillMetaDataTable = "CREATE TABLE BillMetaData (" +
                        "BillNo VARCHAR , " + // Bill No
                        "BillType VARCHAR, " +
                        "PhoneNo VARCHAR, " +
                        "BillDate VARCHAR, " +
                        "FromDate VARCHAR, " +
                        "ToDate VARCHAR, " +
                        "DueDate VARCHAR" +
                        ")";

                string createBillCallDetailsTable = "CREATE TABLE BillCallDetails (" +
                        "BillNo VARCHAR , " + // Bill No
                        "PhoneNo VARCHAR, " +
                        "CallDate VARCHAR, " +
                        "CallTime VARCHAR, " +
                        "CallDuration VARCHAR, " +
                        "Amount VARCHAR, " +
                        "Comments VARCHAR, " +
                        "IsFreeCall VARCHAR, " +
                        "IsRoaming VARCHAR, " +
                        "IsSMS VARCHAR, " +
                        "IsSTD VARCHAR, " +
                        "Pulse INTEGER" +
                        ")";

                ISQLiteStatement statement;
                statement = dbConn.Prepare(createOptionsTable);
                statement.Step();

                statement = dbConn.Prepare(createContactNamesTable);
                statement.Step();

                statement = dbConn.Prepare(createContactGroupsTable);
                statement.Step();

                statement = dbConn.Prepare(createBillMetaDataTable);
                statement.Step();

                statement = dbConn.Prepare(createBillCallDetailsTable);
                statement.Step();

            }
        }

        internal bool executeQueries(List<DBQuery> queryList)
        {
            using (SQLiteConnection dbConn = new SQLiteConnection(dbPath))
            {
                var statement = dbConn.Prepare("BEGIN TRANSACTION");
                statement.Step();

                foreach (DBQuery query in queryList)
                {
                    statement = dbConn.Prepare(query.Query);
                    int i = 1;

                    foreach (Object data in query.QueryData)
                    {
                        statement.Bind(i, data);
                        i++;
                    }

                    statement.Step();
                }

                statement = dbConn.Prepare("COMMIT TRANSACTION");
                statement.Step();
            }

            return true;
        }

        internal List<PhoneBill> GetPhoneBillList()
        {
            List<PhoneBill> billList = new List<PhoneBill>();

            return billList;
        }

        internal bool CheckBillNoExists(string billNo)
        {
            var exists = false;

            using (SQLiteConnection dbConn = new SQLiteConnection(dbPath))
            {
                string sql = @"select * from BillMetaData where BillNo = ?;";

                using (var statement = dbConn.Prepare(sql))
                {
                    statement.Bind(1, billNo);
                    statement.Step();

                    if (statement.DataCount != 0)
                    {
                        exists = true;
                    }
                    else
                    {
                        exists = false;
                    }
                }
            }

            return exists;
        }

        internal JArray GetSummaryByContactNames(string billNo)
        {
            JArray resultData = new JArray();

            using (SQLiteConnection dbConn = new SQLiteConnection(dbPath))
            {
                string sql = "select case when cn.Name is null then cd.PhoneNo else cn.Name end as n, "
                    + "sum(cd.Amount) as Amount from BillCallDetails as cd "
                    + "left outer join ContactNames as cn on cd.PhoneNo = cn.PhoneNo "
                    + "where cd.BillNo = ? group by n order by Amount desc";

                using (var statement = dbConn.Prepare(sql))
                {
                    statement.Bind(1, billNo);

                    while (statement.Step() == SQLiteResult.ROW)
                    {
                        JObject row = new JObject();
                        row.Add("name", statement[0].ToString());
                        row.Add("amount", (double)statement[1]);

                        resultData.Add(row);
                    }
                }
            }

            return resultData;
        }
        
    }
}