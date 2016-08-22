//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Data;
//using IBM.Data.DB2.iSeries;
//namespace SDB.Adapter.IBM
//{
//	public class DB
//	{
//		iDB2Connection connection = null;
       
//		SDB.Adapter.DBConfig.Settings.IBM config = null;
//		private string connectionString = "";
//		public SDB.Adapter.DBConfig.Settings.IBM Config
//		{
//			get {return config;}
//		}
//		string _sql = "";
//#region Initialization
//		public DB()
//		{
//			Initialize();
//		}
//		public DB(string alias)
//		{
//			Initialize(alias);
//		}
//		public DB(NameValueCollection hsh)
//		{
//			Initialize(hsh);
//		}
//		public DB(string ConnectionString, string LibList)
//		{
//			Initialize(ConnectionString, LibList);
//		}
//		private void Initialize()
//		{
//			config = new DBConfig().IBM();
//			connectionString = config.ConnectionString.Replace("\r","").Replace("\n","").Replace("\t","").Replace(" ","");
//		}
//		private void Initialize(string Alias)
//		{
//			if (Alias.IndexOf(";") == -1)
//			{
//				config = new SDB.Adapter.DBConfig().IBM(Alias);
//				connectionString = config.ConnectionString;
//			}
//			else
//				connectionString = Alias;
//		}
//		private void Initialize(NameValueCollection hsh)
//		{
//			config = new SDB.Adapter.DBConfig().IBM(hsh);
//			connectionString = config.ConnectionString;
//			//test the connection,throws error on failure
//			iDB2Connection o = cn;
//			o = null;
//		}
//		private void Initialize(string ConnectionString, string LibList)
//		{
//			config = new SDB.Adapter.DBConfig().IBM(ConnectionString, LibList);
//			connectionString = config.ConnectionString;
//			iDB2Connection o = cn;
//			o = null;
//		}
//#endregion
//#region Connection
//		public string ConnectionString
//		{
//			get { return ConnectionString;}
//		}
//		protected iDB2Connection cn
//		{
//			get
//			{
//				try
//				{
//					if (connection == null)
//					{
//						connection = new iDB2Connection(connectionString);
//						connection.Open();
//					}
//					else
//					{
//						if (connection.State == System.Data.ConnectionState.Closed)
//						{
//							connection.Open();
//						}
//					}
//				}
//				catch (Exception e)
//				{
//					throw new Exception(e.Message + "\r" + connectionString, e);
//				}
//				return connection;
//			}
//		}
//		public iDB2Connection Connection
//		{
//			get
//			{
//				return cn;
//			}
//		}
//		/// <summary>
//		/// open and close a connection to test connectivity
//		/// normally you don't test, this is for unit testing
//		/// </summary>
//		public bool isAbleToConnect
//		{
//			get
//			{
//				bool ret = false;
//				try
//				{
//					cn.Close();
//					ret = true;
//				}
//				catch
//				{
//					ret = false;
//				}
//				return ret;
//			}
//		}
//		/// <summary>
//		/// retrieve message if connectivity failed
//		/// </summary>
//		public string ConnectionErrMessage
//		{
//			get
//			{
//				string ret = "Connection Established, no error!";
//				try
//				{
//					cn.Close();
//				}
//				catch (Exception e)
//				{
//					ret = e.Message;
//				}
//				return ret;
//			}
//		}
//		private void Close()
//		{
//			if (connection != null)
//				if (connection.State != ConnectionState.Closed)
//					connection.Close();
//		}
//#endregion

//#region Execute
//		/// <summary>
//		/// run a sql statement, returns nothing
//		/// </summary>
//		/// <param name="sql"></param>
//		public int ExecuteNonQuery(string sql) 
//		{
//			sql = SqlReplaceLibAlias(sql);
//			iDB2Command cmd = new iDB2Command(sql);
//			cmd.CommandType = CommandType.Text;
//			cmd.Connection = cn;
//			iDB2Transaction trans = null;// cn.BeginTransaction();
//			cmd.Transaction=trans;
//			int ret=0;
//			try
//			{
//				ret = cmd.ExecuteNonQuery();
//				if (trans!=null)
//					trans.Commit();
//			}
//			catch (Exception e)
//			{
//				if (trans!=null)
//					trans.Rollback();
//				throw e;
//			}
//			finally
//			{
//				trans = null;
//				Close();
//			}
//			return ret;
//		}
//		/// <summary>
//		/// convert the lib aliases into the correct names.
//		/// since adding the direct connection string, there may not be a config file
//		/// </summary>
//		/// <param name="sql"></param>
//		/// <returns></returns>
//		private string SqlReplaceLibAlias(string sql)
//		{
//			if (config != null)
//				sql = config.SqlReplaceLibAlias(sql);
//			return sql;
//		}
//		/// <summary>
//		/// run a sql statment, returns nothing
//		/// </summary>
//		/// <param name="sql"></param>
//		/// <param name="hsh"></param>
//		public int ExecuteNonQuery(string sql, NameValueCollection hsh)
//		{
//			throw new Exception("Not Implemented");
//			Close();
//			return 0;
//		}
//		public int ExecuteNonQuery(string sql, string queryString)
//		{
//			throw new Exception("Not Implemented");
//			Close();
//			return 0;
//		}
//		/// <summary>
//		/// run a sql statement, returning 1st value or records affected
//		/// </summary>
//		/// <param name="sql"></param>
//		public string ExecuteScalar(string sql) 
//		{
//			throw new Exception("Not Implemented");
//			Close();
//			return "";
//		}
//		public string ExecuteScalar(string sql, NameValueCollection hsh)
//		{
//			throw new Exception("Not Implemented");
//			return "";
//		}
//#endregion

//#region Fill
//		/// <summary>
//		/// retrieve the last executed sql statement
//		/// </summary>
//		public string GetSql
//		{
//			get
//			{
//				return _sql;
//			}
//		}

//		/// <summary>
//		/// retrieves a sql script file from (bin) ../scripts/sql, fills the parameters, and returns a dataset of any results
//		/// </summary>
//		/// <param name="sqlFilename">The name of the file with no extensions</param>
//		/// <param name="hsh">collection of values, with or without hungarian notation the hungarian notation is stripped by looking for the first uppercase character.</param>
//		/// <returns></returns>
//		public DataSet FillScript(string sqlFilename, NameValueCollection hsh)
//		{
//			string sql = GetSqlScript(sqlFilename);

//			foreach (string key in hsh.Keys)
//			{
//				string fldName = key;
//				string fldVal = hsh[key];
//				if (fldVal.Trim() != "")
//					fldVal = fldVal.Trim();

//				if (!(fldVal == null))
//				{
//					if (fldVal.IndexOf("'") > -1)
//					{
//						fldVal = fldVal.Replace("'", "''");
//					}
//					if (fldVal == "")
//					{
//						fldVal = "null";
//					}
//				}
//				else
//				{
//					fldVal = "null";
//				}
//				try
//				{
//					while (!(fldName.Substring(0, 1) == fldName.Substring(0, 1).ToUpper()))
//					{
//						fldName = fldName.Substring(1);
//					}
//				}
//				catch (Exception ex)
//				{
//					throw new Exception("the element " + key + " is not formatted correctly, the first letter of the sql data field must be capitalized/n" + ex.Message);
//				}
//				//sql = sql.Replace("<@" + fldName.ToUpper() + ">", fldVal);
//				sql = Regex.Replace(sql, "<@" + fldName.Replace(".", @"\.")+ ">", fldVal, RegexOptions.IgnoreCase);
//			}
//			//sql = Regex.Replace(sql, "'?<@.*>'?", "null", RegexOptions.IgnoreCase);
//			sql = Regex.Replace(sql, "<@.*>", "null", RegexOptions.IgnoreCase);//replace nulls in quotes with blank space
//			sql = Regex.Replace(sql, "'null'", "' '", RegexOptions.IgnoreCase);//replace any null numeric with zero
//			sql = Regex.Replace(sql, @"=\s?null", "=0", RegexOptions.IgnoreCase);//replace any null numeric with zero


//			//sql = sql.Replace("'null'", "null");
//			//while (sql.IndexOf("<@") > -1)
//			//{
//			//    int pos = sql.IndexOf("<@");
//			//    int posTo = sql.IndexOf(">", pos);
//			//    sql = sql.Replace(sql.Substring(pos, posTo - pos + 1), "null");
//			//}
//			//sql = sql.Replace("'null'", "null");
//			DataSet ds;
//			try
//			{
//				ds = Fill(sql);
//			}
//			catch (Exception ex)
//			{
//				throw new Exception("From file: " + sqlFilename + "\r\r" + ex.Message);
//			}
//			//ds.WriteXml(@"c:\data\Nettemp\data.xml");
//			return ds;
//		}
//		/// <summary>
//		/// Fill a Dataset from the sql statement
//		/// </summary>
//		/// <param name="SqlFilename">the name of a script (assumed to be at /scripts/sql)</param>
//		/// <param name="Querystring">old asp style querystring, delimited by ampersand and =.  Note that hungarian notation is stripped by looking for the first capitalized letter.</param>
//		/// <returns>untyped Dataset</returns>
//		public DataSet FillScript(string SqlFilename, string Querystring)
//		{
//			System.Collections.Specialized.NameValueCollection hsh = new NameValueCollection();
//			if (Querystring!="")
//			{
//				string[] s = Querystring.Split("&".ToCharArray());
//				foreach (string val in s)
//				{
//					string fldName = val.Split("=".ToCharArray())[0];
//					string fldVal = val.Split("=".ToCharArray())[1];
//					while (fldName.Substring(0, 1) != fldName.Substring(0, 1).ToUpper())
//					{
//						fldName = fldName.Substring(1);
//						if (fldName == "")
//							fldName = " ";
//					}
//					//if (fldName != " ")
//						hsh.Add(fldName, fldVal);
//				}
//			}
//			return FillScript(SqlFilename, hsh);
//		}
//		/// <summary>
//		/// returns a dataset from a sql statement. use Execute or ExecuteScalar for scripts that don't return values
//		/// </summary>
//		/// <param name="sql">straight sql</param>
//		/// <returns></returns>
//		public DataSet Fill(string sql)
//		{
//				sql = SqlReplaceLibAlias(sql);

//			iDB2Command cmd = new iDB2Command("");
//			cmd.CommandTimeout = 0;
//			cmd.CommandType = CommandType.Text;
//			//return Fill(cmd);
//			DataSet ds= new DataSet();
//			int cnt = 0;
//			string[] sqls = sql.Split(";".ToCharArray());
//			foreach (string s in sqls)
//			{
//				if (isSqlStatement(s))
//				{
//					//???replace the lib statements here
//					string s2=s;
//					if (isSelectStatement(s2))
//					{
//						try
//						{
//							cmd.CommandText = s2;
//							DataSet d = Fill(cmd);
//							if (cnt == 0)
//							{
//								ds.Merge(d);
//								cnt++;
//							}
//							else
//							{
//								d.Tables[0].TableName = "Table" + cnt.ToString();
//								d.AcceptChanges();
//								ds.Merge(d);
//							}
//						}
//						catch (Exception e)
//						{
//							throw new Exception(e.Message + "\r" + s2);
//						}
//					}
//					else
//					{
//						try
//						{
//							ExecuteNonQuery(s2);
//						}
//						catch (Exception e)
//						{
//							throw new Exception(e.Message + "\r" + s2);
//						}
//					}
//				}
//			}

//			return ds;

//			//SqlCommand cmd = new SqlCommand(sql);
//			//cmd.CommandType = CommandType.Text;
//			//return Fill(cmd);
//		}
//		private bool isSelectStatement(string sql)
//		{
//			//"^(insert|update|delete|drop|alter|create|replace)"
//			//Regex objAlphaPattern = new Regex("insert|update|delete|create|drop|alter|replace");
//			Regex objAlphaPattern = new Regex(@"^\s*(insert|update|delete|drop|alter|create|replace)",RegexOptions.Multiline | RegexOptions.IgnoreCase);

//			return !objAlphaPattern.IsMatch(sql);
//		}
//		private bool isSqlStatement(string sql)
//		{
//			Regex objAlphaPattern = new Regex(@"^\s*(insert|update|delete|drop|alter|create|replace|select)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
//			return objAlphaPattern.IsMatch(sql);
//		}
//		/// <summary>
//		/// retrieve a dataset from a stored procedure
//		/// </summary>
//		/// <param name="spName">Stored Procedure Name</param>
//		/// <param name="hsh">collection of values (such as Request.Form), will strip hungarian notation</param>
//		/// <returns>Dataset of results</returns>
//		public DataSet Fill(string spName, NameValueCollection hsh)
//		{
//			//throw new Exception("Not Implemeneted");
//			DataSet ds = new DataSet();
//			iDB2Command cmd = null;

//			try
//			{
//				//cmd = BuildCommand(spName, hsh);
//				//add each parameter to the command
//				cmd = new iDB2Command(spName);
//				cmd.CommandType = CommandType.StoredProcedure;

//				foreach (string key in hsh.Keys)
//				{
//					cmd.Parameters.Add( key, hsh[key]);
//				}
//				ds = Fill(cmd);
//			}
//			catch (Exception e)
//			{
//				throw new Exception("Data.Fill(spName,hsh): " + spName + "\n" + e.Message, e);
//			}

//			return ds;
//		}
//		/// <summary>
//		/// return a dataset using a stored procedure name and an old asp style querystring
//		/// </summary>
//		/// <param name="spName">stored procedure name</param>
//		/// <param name="Querystring">old asp style querystring</param>
//		/// <returns></returns>
//		public DataSet Fill(string spName, string Querystring)
//		{            
//			//throw new Exception("Not Implemeneted");

//			NameValueCollection hsh = System.Web.HttpUtility.ParseQueryString(Querystring);
//			//get rid of things like __event and __viewstate
//			foreach (string key in hsh.AllKeys)
//			{
//				if (key.Substring(0, 2) == "__")
//					hsh.Remove(key);
//			}
//			return Fill(spName, hsh);
//		}
//		/// <summary>
//		/// Fill an untyped dataset.
//		/// </summary>
//		/// <param name="cmd"></param>
//		/// <returns></returns>
//		public DataSet Fill(iDB2Command  cmd)
//		{
//			iDB2DataAdapter da = new iDB2DataAdapter(cmd);
//			DataSet ds = new DataSet();

            
//			if (da.SelectCommand.Connection == null)
//				da.SelectCommand.Connection = cn;
//			iDB2Transaction trans = null;//cn.BeginTransaction();
//			//cmd.Transaction=trans;
//			try
//			{
//				da.Fill(ds);
//				if (trans != null)
//					trans.Commit();
//			}
//			catch (Exception e)
//			{
//				if (trans != null)
//					trans.Rollback();
//				throw new Exception("Data.Fill (cmd)\n" + e.Message + "\n" + cmd.ToString());
//			}
//			finally
//			{
//				Close();
//			}
//			return ds;
//		}
//#endregion
//#region Sql Script retrieval
//		/// <summary>
//		/// read a script file from scripts\sql
//		/// </summary>
//		/// <param name="sqlFileName"></param>
//		/// <returns></returns>
//		public string GetSqlScript(string sqlFileName)
//		{
//			//try to find the path if it isn't specified.
//			if (sqlFileName.IndexOf(".sql")==-1)
//				sqlFileName=sqlFileName + ".sql";

//			//if this is a full path don't add the root info
//			if (sqlFileName.IndexOf(@"\\") == -1 & sqlFileName.IndexOf(":") == -1)
//			{
//				//if there is no partial path info then add the default
//				if (sqlFileName.IndexOf(@"\") == -1 & sqlFileName.IndexOf("/") == -1)
//					sqlFileName=@"scripts\sql\IBM\" + sqlFileName;

//				if (System.IO.File.Exists(SDB.Common.Properties.Root + sqlFileName))
//					sqlFileName = SDB.Common.Properties.Root + sqlFileName;
//				else
//				{
//					if (System.IO.File.Exists(SDB.Common.Properties.RootPath + sqlFileName))
//						sqlFileName = SDB.Common.Properties.RootPath + sqlFileName;
//				}
//			}
         
//			string ret="";
//			try
//			{
//				System.IO.StreamReader sr=new System.IO.StreamReader(sqlFileName);
//				ret=sr.ReadToEnd();
//				sr.Close();
//			}
//			catch(Exception e)
//			{
//				throw e;
//			}
//			return ret;
//		}
//#endregion

//	}
//}
