﻿using Newtonsoft.Json.Linq;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
            string app_id = "PBA";

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
                        "EpochBillDate VARCHAR, " +
                        "FromDate VARCHAR, " +
                        "ToDate VARCHAR, " +
                        "DueDate VARCHAR" +
                        ")";

                string createBillCallDetailsTable = "CREATE TABLE BillCallDetails (" +
                        "BillNo VARCHAR , " + // Bill No
                        "PhoneNo VARCHAR, " +
                        "CallDate VARCHAR, " +
                        "CallTime VARCHAR, " +
                        "CallTimeStamp VARCHAR, " +
                        "CallDuration VARCHAR, " +
                        "Amount VARCHAR, " +
                        "CallDirection VARCHAR, " +
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

        internal List<ObservablePhoneBill> GetPhoneBillList()
        {
            List<ObservablePhoneBill> billList = new List<ObservablePhoneBill>();

            using (SQLiteConnection dbConn = new SQLiteConnection(dbPath))
            {
                string sql = @"select * from BillMetaData order by EpochBillDate desc;";
                using (var statement = dbConn.Prepare(sql))
                {
                    while (statement.Step() == SQLiteResult.ROW)
                    {
                        PhoneBill phone_bill = new PhoneBill();

                        phone_bill._billNo = statement[0].ToString();
                        phone_bill.BillType = statement[1].ToString();
                        phone_bill._phoneNo = statement[2].ToString();
                        phone_bill.BillDate = statement[3].ToString();

                        try { phone_bill.FromDate = statement[4].ToString(); }
                        catch (Exception ex) { }

                        try { phone_bill.ToDate = statement[5].ToString(); }
                        catch (Exception ex) { }

                        try { phone_bill.DueDate = statement[6].ToString(); }
                        catch (Exception ex) { }

                        ObservablePhoneBill bill = new ObservablePhoneBill(phone_bill);
                        billList.Add(bill);
                    }
                    
                }
            }

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

        internal JArray GetTop5ContactsByAmount(string billNo)
        {
            JArray resultData = new JArray();

            using (SQLiteConnection dbConn = new SQLiteConnection(dbPath))
            {
                string sql = "select case when cn.Name is null then cd.PhoneNo else cn.Name end as n, "
                    + "sum(cd.Amount) as Amount from BillCallDetails as cd "
                    + "left outer join ContactNames as cn on cd.PhoneNo = cn.PhoneNo "
                    + "where cd.BillNo = ? group by n order by Amount desc limit 5";

                using (var statement = dbConn.Prepare(sql))
                {
                    statement.Bind(1, billNo);

                    while (statement.Step() == SQLiteResult.ROW)
                    {
                        JObject row = new JObject();
                        row.Add("contact", statement[0].ToString());
                        row.Add("amount", (double)statement[1]);

                        resultData.Add(row);
                    }
                }
            }

            return resultData;
        }

        internal JArray GetAllBillDetails(string billNo)
        {
            JArray resultData = new JArray();

            using (SQLiteConnection dbConn = new SQLiteConnection(dbPath))
            {
                string sql = "select case when cn.Name is null then cd.PhoneNo else cn.Name end as n, "
                    + "cd.CallDate, cd.CallTime, cd.Amount as Amount, cd.CallDirection from BillCallDetails as cd "
                    + "left outer join ContactNames as cn on cd.PhoneNo = cn.PhoneNo "
                    + "where cd.BillNo = ? order by cd.CallTimeStamp";

                try
                {
                    using (var statement = dbConn.Prepare(sql))
                    {
                        statement.Bind(1, billNo);

                        while (statement.Step() == SQLiteResult.ROW)
                        {
                            JObject row = new JObject();
                            row.Add("name", statement[0].ToString());

                            DateTime dt = DateTime.Parse(statement[1].ToString() + " " + statement[2].ToString());
                            row.Add("date_time", dt.ToString(CultureInfo.CurrentCulture.DateTimeFormat));

                            try
                            {
                                row.Add("amount", (double)statement[3]);
                            }
                            catch(Exception e)
                            {
                                row.Add("amount", statement[3].ToString());
                            }

                            row.Add("direction", statement[4].ToString());

                            resultData.Add(row);
                        }
                    }
                }
                catch (Exception e)
                {
                    return new JArray();
                }
            }

            return resultData;
        }

        internal JArray GetSummaryByContactGroups(string billNo)
        {
            JArray resultData = new JArray();

            using (SQLiteConnection dbConn = new SQLiteConnection(dbPath))
            {
                string sql = "select case when cg.GroupName is null then 'Others' else cg.GroupName end as GroupN, "
                    + "sum(cd.Amount) as Amount from BillCallDetails as cd "
                    + "left outer join (select distinct PhoneNo, GroupName from ContactGroups) as cg "
                    + "on cd.PhoneNo = cg.PhoneNo where cd.BillNo = ? group by GroupN order by Amount desc";

                using (var statement = dbConn.Prepare(sql))
                {
                    statement.Bind(1, billNo);

                    while (statement.Step() == SQLiteResult.ROW)
                    {
                        JObject row = new JObject();
                        row.Add("group", statement[0].ToString());
                        row.Add("amount", (double)statement[1]);

                        resultData.Add(row);
                    }
                }
            }

            return resultData;
        }

        private int CompareByDateTime(JObject l, JObject r)
        {
            DateTime lhs = DateTime.Parse(l.GetValue("date_time").ToString());
            DateTime rhs = DateTime.Parse(r.GetValue("date_time").ToString());
            return lhs.CompareTo(rhs);
        }

    }
}