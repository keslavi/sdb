using System;
using System.Data;
using System.Data.OracleClient;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Text.RegularExpressions;
namespace SDB.Adapter.ORA
{
    /// <summary>
    /// Summary description for DataOracle.
    /// </summary>
    public class DB
    {
        #region Private variables and Properties
        private OracleConnection  connection;
        private string connectionString = "";
        private SDB.Adapter.DBConfig.Settings.ORA config;
        public SDB.Adapter.DBConfig.Settings.ORA Config
        {
            get
            {
                return config;
            }
        }
        /// <summary>
        /// inner variable for returned procedure statement
        /// </summary>
        private string sql = ""; //used to store the last sql statement or procedure call run

        #endregion
        #region new
        /// <summary>
        /// 
        /// </summary>
        public DB()
        {
            Initialize();
        }
        /// <summary>
        /// alternate method of instantiating specifying a different connection string
        /// </summary>
        /// <param name="ConnectionName"></param>
        public DB(string Alias)
        {
            Initialize(Alias);
        }
        /// <summary>
        /// alternate method of instatiating object
        /// </summary>
        /// <param name="hsh">optional 'Alias', other keys should match *.config connection string names</param>
        public DB(NameValueCollection hsh)
        {
            Initialize(hsh);
        }
        private void Initialize()
        {
            config = new SDB.Adapter.DBConfig().ORA();
            connectionString = config.ConnectionString;
            OracleConnection o = cn;
            o = null;
        }
        private void Initialize(string Alias)
        {
            config = new SDB.Adapter.DBConfig().ORA(Alias);
            connectionString = config.ConnectionString;
        }
        private void Initialize(NameValueCollection hsh)
        {
            config = new SDB.Adapter.DBConfig().ORA(hsh);
            connectionString = config.ConnectionString;
        }
        #endregion
        #region Connection and Connection String
        /// <summary>
        /// open and close a connection to test connectivity
        /// normally you don't test, this is for unit testing
        /// </summary>
        public bool isAbleToConnect
        {
            get
            {
                bool ret = false;
                try
                {
                    cn.Close();
                    ret = true;
                }
                catch
                {
                    ret = false;
                }
                return ret;
            }
        }
        /// <summary>
        /// retrieve message if connectivity failed
        /// </summary>
        public string ConnectionErrMessage
        {
            get
            {
                string ret = "Connection Established, no error!";
                try
                {
                    cn.Close();
                }
                catch (Exception e)
                {
                    ret = e.Message;
                }
                return ret;
            }
        }

        /// <summary>
        /// sets or retrives a connection string Alias or actual connection string
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return connectionString;
            }
        }


        /// <summary>
        /// Returns the last Sql statement or procedure performed.
        /// </summary>
        public string GetSql
        {
            get
            {
                return sql;
            }
        }

        /// <summary>
        /// returns an open Sql Connection
        /// </summary>
        public OracleConnection cn
        {
            get
            {
                if (connection == null)
                {
                    connection = new OracleConnection(ConnectionString);
                    connection.Open();
                }
                else
                {

                    if (connection.State == ConnectionState.Closed)
                        connection.Open();
                }
                return connection;
            }
        }
        /// <summary>
        /// return an open Oracle Connection (
        /// </summary>
        public OracleConnection Connection
        {
            get
            {
                return cn;
            }
        }
#endregion
        #region ExecuteScript
        /// <summary>
        /// Runs a script with or without transactions.  (I'm usually using this to update the database objects)
        /// </summary>
        /// <param name="Script"></param>
        /// <param name="UseTransactions"></param>
        /// <returns></returns>
        public string ExecuteScript(string Script, bool UseTransactions)
        {
            string DatabaseOwner = "dbo";
            string ObjectQualifier = "";
            string Exceptions = "";
            string Delimiter = "\nGO"; //formerly \nGO\n

            Script = Script.Replace("{databaseOwner}", DatabaseOwner);
            Script = Script.Replace("{objectQualifier}", ObjectQualifier);
            Script = Script.Replace("go", "GO");
            Script = Script.Replace("\r\n", "\r");
            Script = Script.Replace("\r", "\r\n");

            string[] arSql = SDB.Common.Functions.Split(Script, Delimiter);
            if (UseTransactions)
            {
                string SqlRun = "";
                try
                {
                   OracleTransaction Trans = cn.BeginTransaction();
                    bool IgnoreErrors;
                    foreach (string SQL in arSql)
                    {
                        SqlRun = SQL;
                        if (SQL.Trim() != "")
                        {
                            int pos = SQL.ToLower().IndexOf("print");
                            int Length = 0;
                            //try 
                            //{
                            //    while (!(pos < 1)) 
                            //    {
                            //        pos = SQL.ToLower().IndexOf("'",pos);
                            //        Length = SQL.ToLower().IndexOf("\n",pos);
                            //        if (Length < 1) 
                            //        {
                            //            Length = SQL.Length;
                            //        }
                            //        Length = Length - pos;
                            //        Console.WriteLine(SQL.Substring(pos, Length - 2));
                            //        pos = SQL.ToLower().IndexOf("print",pos+1);
                            //    }
                            //} 
                            //catch (Exception ex) 
                            //{
                            //    throw ex;
                            //}

                            IgnoreErrors = false;
                            if (SQL.Trim().StartsWith("{IgnoreError}"))
                            {
                                IgnoreErrors = true;
                                SqlRun = SQL.Replace("{IgnoreError}", "");
                            }
                            try
                            {
                                ExecuteNonQuery(ref Trans, SqlRun);
                            }
                            catch (OracleException objException)
                            {
                                if (!(IgnoreErrors))
                                {
                                    Exceptions += objException.Message + "\n\n" + SqlRun + "\n\n";
                                }
                            }
                        }
                    }
                    if (Exceptions.Length == 0)
                    {
                        Trans.Commit();
                    }
                    else
                    {
                        Trans.Rollback();
                        Exceptions += "SQL Execution failed. Database was rolled back\n\n" + SqlRun + "\n\n";
                    }
                }
                catch (Exception e)
                {
                    //throw new Exception(Exceptions + "\r\n" + e.Message, e);
                    Exceptions += "\r\n" + e.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                string SqlRun = "";
                foreach (string SQL in arSql)
                {
                    SqlRun = SQL;
                    if (SqlRun.Trim() != "")
                    {
                        SqlRun = SqlRun.Replace("{databaseOwner}", DatabaseOwner);
                        SqlRun = SqlRun.Replace("{objectQualifier}", ObjectQualifier);
                        try
                        {
                            ExecuteNonQuery(SqlRun);
                        }
                        catch (Exception e)
                        {
                            Exceptions += e.Message + "\n\n" + SqlRun + "\n\n";
                        }
                    }
                }
            }
            return Exceptions;

        }
        /// <summary>
        /// run the sql
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="Sql"></param>
        /// <returns>number of records affected</returns>
        public int ExecuteNonQuery(ref OracleTransaction transaction, string Sql)
        {
            int ret = 0;
            if ((transaction == null))
            {
                throw new ArgumentNullException("transaction");
            }

            if (!((transaction == null)) && (transaction.Connection == null))
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            OracleCommand cmd = new OracleCommand(Sql, cn, transaction);
            cmd.CommandType = CommandType.Text;

            try
            {
                ret = cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch (Exception e)
            {
                throw e;
            }
            return ret;
        }
        /// <summary>
        /// Run the Sql
        /// </summary>
        /// <param name="Sql"></param>
        /// <returns>number of records affected</returns>
        public int ExecuteNonQuery(string Sql)
        {
            int ret = 0;
            OracleCommand cmd = new OracleCommand(Sql, cn);
            cmd.CommandType = CommandType.Text;

            ret = cmd.ExecuteNonQuery();
            cmd.Connection.Close();
            cmd.Dispose();
            return ret;
        }

        #endregion
        #region Batch Update
        /// <summary>
        /// perform a batch update specifying a single procedure for all crud
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="ds"></param>
        /// <param name="TableIndex"></param>
        public void Update(string spName, DataSet ds, int TableIndex)
        {
            //*******************
            OracleCommand cmd = BuildCommand(spName, DataRowToHash(ds.Tables[TableIndex].Rows[0]));
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach (DataRow row in ds.Tables[TableIndex].Rows)
            {
                sb.Append("\r" + spName);
                foreach (OracleParameter prm in cmd.Parameters)
                {
                    prm.Value = row[prm.ParameterName.Substring(1)];
                    sb.Append("\r\t" + prm.ParameterName + "=" + prm.Value);
                }
                try
                {
                    Fill(cmd);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            sql = sb.ToString();
            //optional methods of doing this in case of performance issues.
            //********************
            //SqlCommand cmd = BuildCommand(spName, dsToHash(ds, TableIndex));
            //cmd.Connection = Connection;
            //foreach (SqlParameter prm in cmd.Parameters)
            //{
            //    prm.SourceColumn = prm.ParameterName.Substring(1);
            //}
            //System.Data.Common.DataTableMapping map = new System.Data.Common.DataTableMapping(ds.Tables[0].TableName, ds.Tables[0].TableName);
            //foreach (SqlParameter prm in cmd.Parameters)
            //{
            //    map.ColumnMappings.Add(prm.ParameterName.Substring(1), prm.ParameterName.Substring(1));
            //}


            //SqlDataAdapter da = new SqlDataAdapter(cmd);
            //da.TableMappings.Add(map);
            //da.InsertCommand = cmd;
            //da.UpdateCommand = cmd;
            //da.DeleteCommand = cmd;

            //try
            //{

            //    da.Fill(ds);
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}
            //*******************
            //NameValueCollection hsh = new NameValueCollection();
            //string FieldName = "";
            //string FieldVal = "";
            //foreach (DataRow row in ds.Tables[TableIndex].Rows)
            //{
            //    hsh.Clear();
            //    foreach (DataColumn col in ds.Tables[TableIndex].Columns)
            //    {
            //        if (!row.IsNull(col))
            //        {
            //            FieldName = col.ColumnName;
            //            FieldVal = row[col].ToString();
            //            hsh.Add(FieldName, FieldVal);
            //        }
            //    }
            //        Fill(spName, hsh);
            //}
        }
        NameValueCollection DataRowToHash(DataRow row)
        {
            NameValueCollection hsh = new NameValueCollection();
            foreach (DataColumn col in row.Table.Columns)
            {
                hsh.Add(col.ColumnName, row[col.ColumnName].ToString());
            }
            return hsh;
        }

        /// <summary>
        /// perform a batch update specifying a single procedure for all crud
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="ds"></param>
        public void Update(string spName, DataSet ds)
        {
            Update(spName, ds, 0);
        }

#endregion
        #region Fill
        /// <summary>
        /// fills a strongly typed dataset using a fully mapped data adapter
        /// </summary>
        /// <param name="da">Connection autofilled if not present</param>
        /// <param name="ds">Dataset or strongly typed dataset</param>
        /// <returns></returns>
        public object FillTypedDS(OracleDataAdapter da, object ds)
        {
            //			if (da.SelectCommand==null)
            //				da.SelectCommand.Connection=cn;
            //
            //			try
            //			{
            //				da.Fill(ds);
            //			}
            //			catch (Exception e)
            //			{
            //				throw new Exception ("Data.Fill (da,ds)/n" + e.Message);
            //			}
            //			finally 
            //			{
            //				da.SelectCommand.Connection.Close();
            //				da.Dispose();
            //			}
            //			return ds;
            throw new Exception("not implemented yet");
        }
        /// <summary>
        /// Fill a strongly typed dataset with the results of the Command
        /// </summary>
        /// <param name="cmd">Connection Autofilled if not present</param>
        /// <param name="ds">Typed or untyped dataset</param>
        /// <returns></returns>
        public object FillTypedDS(OracleCommand cmd, object ds)
        {
            throw new Exception("Not implemented");
        }
        /// <summary>
        /// Fill a strongly typed dataset with the results of the stored procedure
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="hsh"></param>
        /// <param name="dsTyped"></param>
        /// <returns></returns>
        public object FillTypedDS(string spName, NameValueCollection hsh, object dsTyped)
        {
            throw new Exception("Not implemented");
            //DataSet ds = Fill(spName, hsh);
            //dsTyped.Merge(ds);
            //return dsTyped;
        }
        /// <summary>
        /// Fill an untyped dataset.
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public DataSet Fill(OracleCommand cmd)
        {
            OracleDataAdapter da = new OracleDataAdapter(cmd);
            DataSet ds = new DataSet();
            OracleTransaction trans = cmd.Transaction;

            if (da.SelectCommand.Connection == null)
                da.SelectCommand.Connection = cn;

            try
            {
                da.Fill(ds);
                if (trans != null)
                    trans.Commit();
            }
            catch (Exception e)
            {
                if (trans != null)
                    trans.Rollback();
                throw new Exception("Data.Fill (cmd)\n" + e.Message + "\n" + sql);
            }
            finally
            {
                if (trans == null)
                    da.SelectCommand.Connection.Close();
                da.Dispose();
            }
            return ds;
        }
        /// <summary>
        /// returns a dataset from a sql statement. use Execute or ExecuteScalar for scripts that don't return values
        /// </summary>
        /// <param name="sql">straight sql</param>
        /// <returns></returns>
        public DataSet Fill(string sql)
        {
            OracleCommand cmd = new OracleCommand(sql);
            cmd.CommandType = CommandType.Text;
            return Fill(cmd);
        }
        /// <summary>
        /// retrieves a sql script file from (bin) ../scripts/sql, fills the parameters, and returns a dataset of any results
        /// </summary>
        /// <param name="sqlFilename">The name of the file with no extensions</param>
        /// <param name="hsh">collection of values, with or without hungarian notation the hungarian notation is stripped by looking for the first uppercase character.</param>
        /// <returns></returns>
        public DataSet FillScript(string sqlFilename, NameValueCollection hsh)
        {
            sql = GetSqlScript(sqlFilename);

            foreach (string key in hsh.Keys)
            {
                string fldName = key;
                string fldVal = hsh[key];
                if (!(fldVal == null))
                {
                    if (fldVal.IndexOf("'") > -1)
                    {
                        fldVal = fldVal.Replace("'", "''");
                    }
                    if (fldVal == "")
                    {
                        fldVal = "null";
                    }
                }
                else
                {
                    fldVal = "null";
                }
                try
                {
                    while (!(fldName.Substring(0, 1) == fldName.Substring(0, 1).ToUpper()))
                    {
                        fldName = fldName.Substring(1);
                    }
                }
                catch (Exception ex)
                {
                    //throw new Exception("the html element " + key + " is not formatted correctly, the first letter of the sql data field must be capitalized/n" + ex.Message);
                }
                //sql = sql.Replace("<@" + fldName.ToUpper() + ">", fldVal);
                sql = Regex.Replace(sql, "<@" + fldName.Replace(".", @"\.")+ ">", fldVal, RegexOptions.IgnoreCase);
            }
            sql = Regex.Replace(sql, "'?<@.*>'?", "null", RegexOptions.IgnoreCase);
            //while (sql.IndexOf("<@") > -1)
            //{
            //    int pos = sql.IndexOf("<@");
            //    int posTo = sql.IndexOf(">", pos);
            //    sql = sql.Replace(sql.Substring(pos, posTo - pos + 1), "null");
            //}
            //sql = sql.Replace("'null'", "null");
            DataSet ds;
            try
            {
                ds = Fill(sql);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "/n/n" + sql);
            }
            //ds.WriteXml(@"c:\data\Nettemp\data.xml");
            return ds;
        }
        /// <summary>
        /// Fill a Dataset from the sql statement
        /// </summary>
        /// <param name="SqlFilename">the name of a script (assumed to be at /scripts/sql)</param>
        /// <param name="Querystring">old asp style querystring, delimited by ampersand and =.  Note that hungarian notation is stripped by looking for the first capitalized letter.</param>
        /// <returns>untyped Dataset</returns>
        public DataSet FillScript(string SqlFilename, string Querystring)
        {
            System.Collections.Specialized.NameValueCollection hsh = new NameValueCollection();
            string[] s = Querystring.Split("&".ToCharArray());
            foreach (string val in s)
            {
                string fldName = val.Split("=".ToCharArray())[0];
                string fldVal = val.Split("=".ToCharArray())[1];
                while (fldName.Substring(0, 1) != fldName.Substring(0, 1).ToUpper())
                {
                    fldName = fldName.Substring(1);
                    if (fldName == "")
                        fldName = " ";
                }
                if (fldName != " ")
                    hsh.Add(fldName, fldVal);
            }
            return FillScript(SqlFilename, hsh);
        }

        /// <summary>
        /// retrieve a dataset from a stored procedure
        /// </summary>
        /// <param name="spName">Stored Procedure Name</param>
        /// <param name="hsh">collection of values (such as Request.Form), will strip hungarian notation</param>
        /// <returns>Dataset of results</returns>
        public DataSet Fill(string spName, NameValueCollection hsh)
        {
            DataSet ds = new DataSet();
            OracleCommand cmd = null;

            try
            {
                cmd = BuildCommand(spName, hsh);
                ds = Fill(cmd);
            }
            catch (Exception e)
            {
                throw new Exception("Data.Fill(spName,hsh): " + spName + "\n" + e.Message, e);
            }

            return ds;
        }
        /// <summary>
        /// return a dataset using a stored procedure name and an old asp style querystring
        /// </summary>
        /// <param name="spName">stored procedure name</param>
        /// <param name="Querystring">old asp style querystring</param>
        /// <returns></returns>
        public DataSet Fill(string spName, string Querystring)
        {
            NameValueCollection hsh = new NameValueCollection();
            if (Querystring != "")
            {
                string[] s = Querystring.Split("&".ToCharArray());
                foreach (string val in s)
                {
                    string fldName = val.Split("=".ToCharArray())[0];
                    string fldVal = val.Split("=".ToCharArray())[1];
                    while (fldName.Substring(0, 1) != fldName.Substring(0, 1).ToUpper())
                    {
                        fldName = fldName.Substring(1);
                        if (fldName == "")
                            fldName = " ";
                    }
                    if (fldName != " ")
                        hsh.Add(fldName, fldVal);
                }
            }
            return Fill(spName, hsh);
        }
        #endregion
        #region Parameter Building Functions
        /// <summary>
        /// create procedures for the sp dynamically
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="hsh"></param>
        /// <returns></returns>
        public OracleCommand BuildCommand(string spName, NameValueCollection hsh)
        {
            OracleCommand cmd = new OracleCommand(spName);
            cmd.CommandType = CommandType.StoredProcedure;

            string FieldName;
            string ElementFieldName;
            string FieldVal;
            string Prms = "";

            foreach (string key in hsh.AllKeys)
            {
                FieldName = key;
                FieldVal = hsh[key];
                cmd.Parameters.AddWithValue(FieldName, FieldVal);
                if (FieldName.Substring(0, 1).ToLower() == "o")
                    cmd.Parameters[FieldName].Direction = ParameterDirection.Output;
                
            }
            //OracleCommand spFieldDef = new OracleCommand("_spFieldDef", Connection);
            //spFieldDef.CommandType = CommandType.StoredProcedure;
            //spFieldDef.Parameters.AddWithValue("@ProcName", cmd.CommandText);
            //OracleDataReader drProperties = null;
            //try
            //{
            //    drProperties = spFieldDef.ExecuteReader();
            //    //System.DBNull null;
            //    if (hsh.Count > 0)
            //    {
            //        while (drProperties.Read())
            //        {
            //            FieldName = drProperties.GetString(0);
            //            ElementFieldName = FieldName.Remove(0, 1);
            //            FieldVal = FindKeyVal(ElementFieldName, hsh);
            //            OracleParameter  prm = cmd.Parameters.AddWithValue(FieldName, FieldVal);
            //            prm.SourceColumn = FieldName;
            //            if (drProperties.GetInt32(4) == 1)
            //            {
            //                prm.Direction = ParameterDirection.InputOutput;
            //                switch (drProperties.GetString(1))
            //                {
            //                    case "int":
            //                        prm.DbType = DbType.Int32;
            //                        break;
            //                    case "bigint":
            //                        prm.DbType = DbType.Int64;
            //                        break;
            //                    case "varchar":
            //                        prm.DbType = DbType.String;
            //                        break;
            //                    default:
            //                        break;
            //                }
            //            }
            //            if (FieldVal == null)
            //            {
            //                FieldVal = "null";
            //            }
            //            else
            //                //now put quotes around the FieldVal
            //                switch (drProperties.GetString(1))
            //                {
            //                    case "int":
            //                        break;
            //                    case "bigint":
            //                        break;
            //                    case "bit":
            //                        break;
            //                    default:
            //                        FieldVal = "'" + FieldVal + "'";
            //                        break;
            //                }
            //            Prms = Prms + "\n\t" + "," + FieldName + "=" + FieldVal;
            //        }
            //        Prms = Prms.Substring(3);
            //    }
            //    Prms = "\nexecute " + cmd.CommandText + "\n\t" + Prms;
            //    sql = Prms;
            //}
            //catch (Exception ex)
            //{
            //    Exception e = new Exception("Couldn't Fill parameters. the most likely reason is an incorrect procedure name: " + cmd.CommandText + "/n" + ex.Message);
            //    throw e;
            //}
            //finally
            //{
            //    if (drProperties != null)
            //        drProperties.Close();
            //    spFieldDef.Dispose();
            //}
            return cmd;
        }
        string FindKeyVal(string elementFieldName, NameValueCollection hsh)
        {
            string KeyFound = "";
            string ret = "";
            string K;
            elementFieldName = elementFieldName.ToUpper();

            int elL = elementFieldName.Length;
            foreach (string Key in hsh.Keys)
            {
                K = Key;
                if (Key != null)
                {
                    try
                    {
                        string Key2 = Key;
                        try
                        {
                            if (Key2.LastIndexOf("$") > 0)
                            {
                                Key2 = Key2.Substring(Key2.LastIndexOf("$") + 1);
                            }
                            while (Key2.Substring(0, 1) != Key2.Substring(0, 1).ToUpper())
                            {
                                Key2 = Key2.Substring(1);
                                if (Key2.Length == 1)
                                    break;
                            }
                        }
                        catch (Exception e2)
                        {
                            throw e2;
                        }
                        int KeyL = Key2.Length;
                        if (KeyL >= elL)
                        {

                            //if (Key2.Substring(Key.Length - elementFieldName.Length).ToUpper() == elementFieldName)
                            if (Key2.ToUpper() == elementFieldName)
                            {
                                KeyFound = Key;
                                break;
                            }
                        }
                        /*
                                            if (K.ToLower() != K) 
                                            {
                                                while (K.Substring(0, 1) == K.Substring(0, 1).ToLower()) 
                                                {
                                                    K = K.Substring(1);
                                                }
                                            }
                                            if (K.ToUpper() == elementFieldName) 
                                            {
                                                KeyFound = Key;
                                            }
                        */
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            if (KeyFound != "")
            {
                ret = hsh[KeyFound];
            }
            if (ret == "")
            {
                ret = null;
            }
            return ret;
        }

        #endregion
        #region Lookup value retrieval
        /// <summary>
        /// Retrieve the values from the Lookup table for population by drop boxes
        /// </summary>
        /// <param name="spName"></param>
        /// <returns></returns>
        public DataSet GetLookup(string spName)
        {
            OracleCommand cmd = new OracleCommand("_spLookupClass");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@Class", spName);
            return Fill(cmd);
        }
        #endregion
        #region Sql Script retrieval
        /// <summary>
        /// read a script file from scripts\sql
        /// </summary>
        /// <param name="sqlFileName"></param>
        /// <returns></returns>
        public string GetSqlScript(string sqlFileName)
        {
            //try to find the path if it isn't specified.
            if (sqlFileName.IndexOf(".sql") == -1)
                sqlFileName = sqlFileName + ".sql";

            //if this is a full path don't add the root info
            if (sqlFileName.IndexOf(@"\\") == -1 & sqlFileName.IndexOf(":") == -1)
            {
                //if there is no partial path info then add the default
                if (sqlFileName.IndexOf(@"\") == -1 & sqlFileName.IndexOf("/") == -1)
                    sqlFileName = @"scripts\sql\ORA\" + sqlFileName;

                if (System.IO.File.Exists(SDB.Common.Properties.Root + sqlFileName))
                    sqlFileName = SDB.Common.Properties.Root + sqlFileName;
                else
                {
                    if (System.IO.File.Exists(SDB.Common.Properties.RootPath + sqlFileName))
                        sqlFileName = SDB.Common.Properties.RootPath + sqlFileName;
                }
            }

            string ret = "";
            try
            {
                System.IO.StreamReader sr = new System.IO.StreamReader(sqlFileName);
                ret = sr.ReadToEnd();
                sr.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
            return ret;
        }
#endregion
    }
}

