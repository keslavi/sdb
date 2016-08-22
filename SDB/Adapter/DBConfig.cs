using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Reflection;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace  SDB.Adapter
{
    /// <summary>
    /// retrieves configuration information for databases
    /// </summary>
    public class DBConfig
    {
        public DBConfig()
        {
            string file = Common.Properties.Root + "DB.Config";
            if (!System.IO.File.Exists(file))
                file = Common.Properties.Root + "DB.Enc.Config";

            //if (!System.IO.File.Exists(file))
            //    throw new Exception("Missing: " + file + " and DB.Config");

            if (System.IO.File.Exists(file))
            {
                configPath = file;
                LoadConfigFile(file);
            }
        }
               
        protected DataSet ds = null;
        /// <summary>
        /// Indicates the config data file loaded successfully
        /// </summary>
        public Boolean HasData
        {
            get
            {
                Boolean b = false;
                if (ds != null)
                    b = ds.Tables.Count > 0;
                return b;
            }
        }
        private string configPath;
        /// <summary>
        /// Configuration FilePath
        /// </summary>
        public string ConfigPath
        {
            get
            {
                return configPath;
            }
            set
            {
                configPath=value;
            }
        }
        /// <summary>
        /// ConnectionSettings
        /// </summary>
        /// <returns></returns>
        public  Settings.SQL SQL()
        {
            Settings.SQL o = new Settings.SQL(ds);
            return o;
        }
        public Settings.SQL SQL(string Alias)
        {
            Settings.SQL o = new Settings.SQL(ds,Alias);
            return o;
        }
        public Settings.SQL SQL(NameValueCollection hsh)
        {
            Settings.SQL o = new Settings.SQL(ds, hsh);
            return o;
        }

        /// <summary>
        /// ConnectionSettings
        /// </summary>
        /// <returns></returns>
        public Settings.MySql MySql()
        {
            Settings.MySql o = new Settings.MySql(ds);
            return o;
        }
        public Settings.MySql MySql(string Alias)
        {
            Settings.MySql o = new Settings.MySql(ds, Alias);
            return o;
        }
        public Settings.MySql MySql(NameValueCollection hsh)
        {
            Settings.MySql o = new Settings.MySql(ds, hsh);
            return o;
        }

        /// <summary>
        /// ConnectionSettings
        /// </summary>
        /// <returns></returns>
        public Settings.ORA ORA()
        {
            Settings.ORA o = new Settings.ORA(ds);
            return o;
        }
        public Settings.ORA ORA(string Alias)
        {
            Settings.ORA o = new Settings.ORA(ds, Alias);
            return o;
        }
        public Settings.ORA ORA(NameValueCollection hshMod)
        {
            Settings.ORA o = new Settings.ORA(ds, hshMod);
            return o;
        }
        /// <summary>
        /// ConnectionSettings
        /// </summary>
        /// <returns></returns>
        public Settings.IBM IBM()
        {
            Settings.IBM o = new Settings.IBM(ds);
            return o;
        }
        public Settings.IBM IBM(string Alias)
        {
            Settings.IBM o = new Settings.IBM(ds, Alias);
            return o;
        }
        public Settings.IBM IBM(NameValueCollection hsh)
        {

            Settings.IBM o =new Settings.IBM(ds, hsh);
            return o;
        }
        public Settings.IBM IBM(string ConnectionString, string LibList)
        {
            Settings.IBM o = new Settings.IBM(ConnectionString,LibList);
            return o;
        }
        private void  LoadConfigFile(string file)
        {
            ds = new DataSet();
                ds.ReadXml(file);
        }
        /// <summary>
        /// return a datatable of the alias collection (dbType,Name)
        /// </summary>
        public DataTable GetAliasCollection
        {
            get
            {
                if (ds.Tables["AliasCollection"].Rows.Count < 2)
                {
                    foreach (DataTable tbl in ds.Tables)
                    {
                        if ("ADCIBMSQLORA".IndexOf(tbl.TableName.Substring(0, 3)) > -1)
                        {
                            foreach (DataRow srcRow in tbl.Rows)
                            {
                                DataRow trgRow = ds.Tables["AliasCollection"].Rows.Add();
                                trgRow["dbType"] = tbl.TableName;
                                trgRow["Name"] = srcRow["Alias"];
                            }
                        }
                    }
                    ds.AcceptChanges();
                }
                return ds.Tables["AliasCollection"];
            }
        }
#region Settings (consumed by methods in DBConfig)
        /// <summary>
        /// SQl Server Connection Information
        /// </summary>
        public class Settings
        {
#region MySql
            /// <summary>
            /// Connection Settings for Sql Server
            /// </summary>
            public class MySql
            {
                string alias = "";
                DataRow row = null;
                string connectionString = "";
                NameValueCollection hsh = new NameValueCollection();
                /// <summary>
                /// Initialization
                /// </summary>
                /// <param name="ds">contains the Alias and connection data</param>
                public MySql(DataSet ds)
                {
                    Initialize(ds, "");
                }
                public MySql(DataSet ds, string Alias)
                {
                    Initialize(ds, Alias);
                }
                public MySql(DataSet ds, NameValueCollection hsh)
                {
                    Initialize(ds, hsh);
                }
                private void Initialize(DataSet ds, string Alias)
                {
                    string adapterType = "MySql";
                    try
                    {
                        if (ds != null)
                        {
                            if (Alias == "")
                                alias = ds.Tables["Alias"].Select("Adapter='" + adapterType + "'")[0]["Name"].ToString();
                            else
                                alias = Alias;

                            row = ds.Tables[adapterType].Select("Alias='" + alias + "'")[0];
                            InitializeConnectionString(val("ConnectionString"));
                        }
                        else
							InitializeConnectionString(System.Configuration.ConfigurationManager.ConnectionStrings[Alias].ConnectionString);
                    }
                    catch (Exception e)
                    {
                        string msg;
                        if (alias != "")
                            msg = adapterType + "." + alias + ": Missing or Misconfigured";
                        else
                            msg = "could not find " + adapterType + ".Alias in the db.Config file or first connectionstring in the web.config file";
                        throw new Exception(msg + "\n" + e.Message, e);
                    }
                }
                /// <summary>
                /// initialize, replacing field vals with hash values
                /// </summary>
                /// <param name="ds"></param>
                /// <param name="hsh"></param>
                private void Initialize(DataSet ds, NameValueCollection hsh)
                {
                    string Alias = hsh["Alias"];
                    string adapterType = "MySql";
                    try
                    {
                        if (Alias == "")
                            alias = ds.Tables["Alias"].Select("Adapter='" + adapterType + "'")[0]["Name"].ToString();
                        else
                            alias = Alias;

                        row = ds.Tables[adapterType].Select("Alias='" + alias + "'")[0];
                        InitializeConnectionString(val("ConnectionString"), hsh);
                    }
                    catch (Exception e)
                    {
                        string msg;
                        if (alias != "")
                            msg = adapterType + "." + alias + ": Missing or Misconfigured";
                        else
                            msg = "could not find " + adapterType + ".Alias in the db.config file or first connectionstring in the web.config file";
                        throw new Exception(msg + "\n" + e.Message, e);
                    }
                }
                private string InitializeConnectionString(string sCon)
                {
                    System.Reflection.Assembly main = System.Reflection.Assembly.GetCallingAssembly();
                    string AppName = "";
                    if (main != null)
                        AppName += ";Application name=" + System.Reflection.Assembly.GetCallingAssembly().GetName().ToString().Split(",".ToCharArray())[0];
                    sCon = sCon + AppName;
                    connectionString = sCon;
                    hsh = System.Web.HttpUtility.ParseQueryString(sCon.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(";", "&").Replace(" ", ""));
                    return connectionString;
                }
                private string InitializeConnectionString(string sCon, NameValueCollection hshMod)
                {
                    System.Reflection.Assembly main = System.Reflection.Assembly.GetCallingAssembly();
                    string AppName = "";
                    if (main != null)
                        AppName += ";Application name=" + System.Reflection.Assembly.GetCallingAssembly().GetName().ToString().Split(",".ToCharArray())[0];
                    sCon = sCon + AppName;
                    hsh = CreateHash(sCon, hshMod);
                    sCon = hsh.ToString().Replace("&", ";");
                    connectionString = sCon;
                    return connectionString;
                }
                private NameValueCollection CreateHash(string sCon, NameValueCollection hshMod)
                {
                    NameValueCollection trg = System.Web.HttpUtility.ParseQueryString(sCon.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(";", "&").Replace(" ", ""));
                    foreach (string key in trg)
                    {
                        if (hshMod[key] != "")
                            trg[key] = hshMod[key];
                    }
                    return trg;
                }
                /// <summary>
                /// String to Connect to Sql With
                /// </summary>
                public string ConnectionString
                {
                    get
                    {
                        return connectionString;
                    }
                }
                /// <summary>
                /// Name to potentially alias as to database
                /// </summary>
                public string UserAlias
                {
                    get
                    {
                        return val("UserAlias");
                    }
                }
                /// <summary>
                /// is this a production database?
                /// </summary>
                public bool isProductionDB
                {
                    get
                    {
                        return val("isProductionDB") == "1";
                    }
                }
                public string Comments
                {
                    get
                    {
                        return val("Comments");
                    }
                }
                public bool PromptLogin
                {
                    get
                    {
                        bool ret = "true" == val("PromptLogin").ToLower();
                        return ret;
                    }
                }
                public bool SaveLogin
                {
                    get
                    {
                        return "true" == val("SaveLogin").ToLower();
                    }
                }
                public string DataSource
                {
                    get
                    {
                        return val("datasource");
                    }
                }
                public string UserID
                {
                    get
                    {
                        return val("userid");
                    }
                }
                public string Password
                {
                    get
                    {
                        return val("password");
                    }
                }
                public string InitialCatalog
                {
                    get
                    {
                        return val("initialcatalog");
                    }
                }
                /// <summary>
                /// retrieve the value from the row's associated column
                /// </summary>
                /// <param name="colname"></param>
                /// <returns></returns>
                public string val(string colname)
                {
                    string ret = "";
                    if (row.Table.Columns.Contains(colname) == true)
                        ret = row[colname].ToString();
                    if (ret == "")
                        ret = hsh[colname];
                    return ret.Trim();
                }
                public string Alias
                {
                    get
                    {
                        return alias;
                    }
                }
            }
#endregion
#region SAP
            /// <summary>
            /// Connection Settings for Sql Server
            /// </summary>
            public class SAP
            {
                string alias = "";
                DataRow row = null;
                string connectionString = "";
                NameValueCollection hsh = new NameValueCollection();
                /// <summary>
                /// Initialization
                /// </summary>
                /// <param name="ds">contains the Alias and connection data</param>
                public SAP(DataSet ds)
                {
                    Initialize(ds, "");
                }
                public SAP(DataSet ds, string Alias)
                {
                    Initialize(ds, Alias);
                }
                public SAP(DataSet ds, NameValueCollection hsh)
                {
                    Initialize(ds, hsh);
                }
                private void Initialize(DataSet ds, string Alias)
                {
                    string adapterType = "SAP";
                    try
                    {
                        if (ds != null)
                        {
                            if (Alias == "")
                                alias = ds.Tables["Alias"].Select("Adapter='" + adapterType + "'")[0]["Name"].ToString();
                            else
                                alias = Alias;

                            row = ds.Tables[adapterType].Select("Alias='" + alias + "'")[0];
                            InitializeConnectionString(val("ConnectionString"));
                        }
                        else
							InitializeConnectionString(System.Configuration.ConfigurationManager.ConnectionStrings[Alias].ConnectionString);
                    }
                    catch (Exception e)
                    {
                        string msg;
                        if (alias != "")
                            msg = adapterType + "." + alias + ": Missing or Misconfigured";
                        else
                            msg = "could not find " + adapterType + ".Alias in the db.config file or first connectionstring in the web.config file";
                        throw new Exception(msg + "\n" + e.Message, e);
                    }
                }
                /// <summary>
                /// initialize, replacing field vals with hash values
                /// </summary>
                /// <param name="ds"></param>
                /// <param name="hsh"></param>
                private void Initialize(DataSet ds, NameValueCollection hsh)
                {
                    string Alias = hsh["Alias"];
                    string adapterType = "SAP";
                    try
                    {
                        if (Alias == "")
                            alias = ds.Tables["Alias"].Select("Adapter='" + adapterType + "'")[0]["Name"].ToString();
                        else
                            alias = Alias;

                        row = ds.Tables[adapterType].Select("Alias='" + alias + "'")[0];
                        InitializeConnectionString(val("ConnectionString"), hsh);
                    }
                    catch (Exception e)
                    {
                        string msg;
                        if (alias != "")
                            msg = adapterType + "." + alias + ": Missing or Misconfigured";
                        else
                            msg = "could not find " + adapterType + ".Alias in the Config file";
                        throw new Exception(msg + "\n" + e.Message, e);
                    }
                }
                private string InitializeConnectionString(string sCon)
                {
                    System.Reflection.Assembly main = System.Reflection.Assembly.GetCallingAssembly();
                    string AppName = "";
                    if (main != null)
                        AppName += ";Application name=" + System.Reflection.Assembly.GetCallingAssembly().GetName().ToString().Split(",".ToCharArray())[0];
                    sCon = sCon + AppName;
                    connectionString = sCon;
                    hsh = System.Web.HttpUtility.ParseQueryString(sCon.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(";", "&").Replace(" ", ""));
                    return connectionString;
                }
                private string InitializeConnectionString(string sCon, NameValueCollection hshMod)
                {
                    System.Reflection.Assembly main = System.Reflection.Assembly.GetCallingAssembly();
                    string AppName = "";
                    if (main != null)
                        AppName += ";Application name=" + System.Reflection.Assembly.GetCallingAssembly().GetName().ToString().Split(",".ToCharArray())[0];
                    sCon = sCon + AppName;
                    hsh = CreateHash(sCon, hshMod);
                    sCon = hsh.ToString().Replace("&", ";");
                    connectionString = sCon;
                    return connectionString;
                }
                private NameValueCollection CreateHash(string sCon, NameValueCollection hshMod)
                {
                    NameValueCollection trg = System.Web.HttpUtility.ParseQueryString(sCon.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(";", "&").Replace(" ", ""));
                    foreach (string key in trg)
                    {
                        if (hshMod[key] != "")
                            trg[key] = hshMod[key];
                    }
                    return trg;
                }

                /// <summary>
                /// String to Connect to Sql With
                /// </summary>
                public string ConnectionString
                {
                    get
                    {
                        return connectionString;
                    }
                }
                /// <summary>
                /// Name to potentially alias as to database
                /// </summary>
                public string UserAlias
                {
                    get
                    {
                        return val("UserAlias");
                    }
                }
                /// <summary>
                /// is this a production database?
                /// </summary>
                public bool isProductionDB
                {
                    get
                    {
                        return val("isProductionDB") == "1";
                    }
                }
                public string Comments
                {
                    get
                    {
                        return val("Comments");
                    }
                }
                public bool PromptLogin
                {
                    get
                    {
                        bool ret = "true" == val("PromptLogin").ToLower();
                        return ret;
                    }
                }
                public bool SaveLogin
                {
                    get
                    {
                        return "true" == val("SaveLogin").ToLower();
                    }
                }
                public string DataSource
                {
                    get
                    {
                        return val("datasource");
                    }
                }
                public string UserID
                {
                    get
                    {
                        return val("userid");
                    }
                }
                public string Password
                {
                    get
                    {
                        return val("password");
                    }
                }
                public string InitialCatalog
                {
                    get
                    {
                        return val("initialcatalog");
                    }
                }
                /// <summary>
                /// retrieve the value from the row's associated column
                /// </summary>
                /// <param name="colname"></param>
                /// <returns></returns>
                public string val(string colname)
                {
                    string ret = "";
                    if (row.Table.Columns.Contains(colname) == true)
                        ret = row[colname].ToString();
                    if (ret == "")
                        ret = hsh[colname];
                    return ret.Trim();
                }

                public string Alias
                {
                    get
                    {
                        return alias;
                    }
                }
            }
#endregion
#region SQL
            /// <summary>
            /// Connection Settings for Sql Server
            /// </summary>
            public class SQL
            {
                string alias="";
                DataRow row = null;
                string connectionString = "";
                NameValueCollection hsh = new NameValueCollection();
                /// <summary>
                /// Initialization
                /// </summary>
                /// <param name="ds">contains the Alias and connection data</param>
                public SQL(DataSet ds)
                {
                    Initialize(ds,"");     
                }
                public SQL(DataSet ds, string Alias)
                {
                    Initialize(ds, Alias);
                }
                public SQL(DataSet ds,NameValueCollection hsh)
                {
                    Initialize(ds,hsh);
                }
                private void Initialize(DataSet ds,string Alias)
                {
                    string adapterType = "SQL";
                    try
                    {
                        if (ds != null)
                        {
                            if (Alias == "")
                                alias = ds.Tables["Alias"].Select("Adapter='" + adapterType + "'")[0]["Name"].ToString();
                            else
                                alias = Alias;

                            row = ds.Tables[adapterType].Select("Alias='" + alias + "'")[0];
                            InitializeConnectionString(val("ConnectionString"));
                        }
                        else
							InitializeConnectionString(System.Configuration.ConfigurationManager.ConnectionStrings[Alias].ConnectionString);
                    }
                    catch (Exception e)
                    {
                        string msg;
                        if (alias != "")
                            msg = adapterType + "." + alias +  ": Missing or Misconfigured";
                        else
                            msg = "could not find " + adapterType + ".Alias in the db.config file or first connectionstring in the web.config file";
                        throw new Exception(msg + "\n" + e.Message,e);
                    }
                }
                /// <summary>
                /// initialize, replacing field vals with hash values
                /// </summary>
                /// <param name="ds"></param>
                /// <param name="hsh"></param>
                private void Initialize(DataSet ds,NameValueCollection hsh)
                {
                    string Alias = hsh["Alias"];
                    string adapterType = "SQL";
                    try
                    {
                        if (Alias == "")
                            alias = ds.Tables["Alias"].Select("Adapter='" + adapterType + "'")[0]["Name"].ToString();
                        else
                            alias = Alias;

                        row = ds.Tables[adapterType].Select("Alias='" + alias + "'")[0];
                        InitializeConnectionString(val("ConnectionString"),hsh );
                    }
                    catch (Exception e)
                    {
                        string msg;
                        if (alias != "")
                            msg = adapterType + "." + alias + ": Missing or Misconfigured";
                        else
                            msg = "could not find " + adapterType + ".Alias in the Config file";
                        throw new Exception(msg + "\n" + e.Message, e);
                    }
                }
                private string InitializeConnectionString(string sCon)
                {
                    System.Reflection.Assembly main = System.Reflection.Assembly.GetCallingAssembly();
                    string AppName = "";
                    if (main != null)
                        AppName+=";Application name=" + System.Reflection.Assembly.GetCallingAssembly().GetName().ToString().Split(",".ToCharArray())[0];
                    sCon = sCon + AppName;
                    connectionString = sCon;
                    hsh = System.Web.HttpUtility.ParseQueryString(sCon.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(";", "&").Replace(" ",""));
                    return connectionString;
                }
                private string InitializeConnectionString(string sCon,NameValueCollection hshMod)
                {
                    System.Reflection.Assembly main = System.Reflection.Assembly.GetCallingAssembly();
                    string AppName = "";
                    if (main != null)
                        AppName += ";Application name=" + System.Reflection.Assembly.GetCallingAssembly().GetName().ToString().Split(",".ToCharArray())[0];
                    sCon = sCon + AppName;
                    hsh=CreateHash(sCon,hshMod);
                    sCon = hsh.ToString().Replace("&", ";");
                    connectionString = sCon;
                    return connectionString;
                }
                private NameValueCollection   CreateHash(string sCon,NameValueCollection hshMod)
                {
                    NameValueCollection trg = System.Web.HttpUtility.ParseQueryString(sCon.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(";", "&").Replace(" ", ""));
                    foreach (string key in trg)
                    {
                        if (hshMod[key] != "")
                            trg[key] = hshMod[key];
                    }
                    return trg;
                }
                
                /// <summary>
                /// String to Connect to Sql With
                /// </summary>
                public string ConnectionString
                {
                    get
                    {
                        return connectionString;
                    }
                }
                /// <summary>
                /// Name to potentially alias as to database
                /// </summary>
                public string UserAlias
                {
                    get
                    {
                        return val("UserAlias");
                    }
                }
                /// <summary>
                /// is this a production database?
                /// </summary>
                public bool isProductionDB
                {
                    get
                    {
                        return val("isProductionDB") =="1";
                    }
                }
                public string Comments
                {
                    get
                    {
                        return val("Comments");
                    }
                }
                public bool PromptLogin
                {
                    get
                    {
                        bool ret = "true" == val("PromptLogin").ToLower();
                        return ret;
                    }
                }
                public bool SaveLogin
                {
                    get
                    {
                        return  "true"==val("SaveLogin").ToLower() ;
                    }
                }
                public string DataSource
                {
                    get
                    {
                        return val("datasource");
                    }
                }
                public string UserID
                {
                    get
                    {
                        return val("userid");
                    }
                }
                public string Password
                {
                    get
                    {
                        return val("password");
                    }
                }
                public string InitialCatalog
                {
                    get
                    {
                        return val("initialcatalog");
                    }
                }
                /// <summary>
                /// retrieve the value from the row's associated column
                /// </summary>
                /// <param name="colname"></param>
                /// <returns></returns>
                public string val(string colname)
                {
                    string ret="";
                    if (row.Table.Columns.Contains(colname) == true)
                        ret = row[colname].ToString();
                    if (ret == "")
                        ret = hsh[colname];
                    return ret.Trim();
                }

                public string Alias
                {
                    get
                    {
                        return alias;
                    }
                }
            }
#endregion
#region ORA
            /// <summary>
            /// Connection Settings for Oracle
            /// </summary>
            public class ORA
            {
                string alias = "";
                DataRow row = null;
                string connectionString="";
                NameValueCollection hsh = new NameValueCollection();
                /// <summary>
                /// Initialization
                /// </summary>
                /// <param name="ds">contains the Alias and connection data</param>
                public ORA(DataSet ds)
                {
                    Initialize(ds,"");
                }
                public ORA(DataSet ds, string Alias)
                {
                    Initialize(ds, Alias);
                }
                public ORA(DataSet ds, NameValueCollection hshMod)
                {
                    Initialize(ds, hshMod);
                }
                private void Initialize(DataSet ds, string Alias)
                {
                    string adapterType = "ORA";
                    try
                    {
                        if (ds != null)
                        {
                            if (Alias == "")
                                alias = ds.Tables["Alias"].Select("Adapter='" + adapterType + "'")[0]["Name"].ToString();
                            else
                                alias = Alias;

                            row = ds.Tables[adapterType].Select("Alias='" + alias + "'")[0];
                            InitializeConnectionString(val("ConnectionString"));
                        }
                        else
							InitializeConnectionString(System.Configuration.ConfigurationManager.ConnectionStrings[Alias].ConnectionString);

                    }
                    catch (Exception e)
                    {
                        string msg;
                        if (alias != "")
                            msg = adapterType + "." + alias + ": Missing or Misconfigured";
                        else
                            msg = "could not find " + adapterType + ".Alias in the db.config file or first connectionstring in web.config";
                        throw new Exception(msg + "\n" + e.Message, e);
                    }
                }
                private void Initialize(DataSet ds, NameValueCollection hshMod)
                {
                    string adapterType = "ORA";
                    string Alias = hshMod["Alias"];
                    try
                    {
                        if (Alias == "")
                            alias = ds.Tables["Alias"].Select("Adapter='" + adapterType + "'")[0]["Name"].ToString();
                        else
                            alias = Alias;
                        row = ds.Tables[adapterType].Select("Alias='" + alias + "'")[0];
                        InitializeConnectionString(val("ConnectionString"),hshMod);
                    }
                    catch (Exception e)
                    {
                        string msg;
                        if (alias != "")
                            msg = adapterType + "." + alias + ": Missing or Misconfigured";
                        else
                            msg = "could not find " + adapterType + ".Alias in the Config file";
                        throw new Exception(msg + "\n" + e.Message, e);
                    }
                }
                private NameValueCollection CreateHash(string sCon, NameValueCollection hshMod)
                {
                    NameValueCollection trg = System.Web.HttpUtility.ParseQueryString(sCon.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(";", "&").Replace(" ", ""));
                    foreach (string key in trg)
                    {
                        if (hshMod[key] != "")
                            trg[key] = hshMod[key];
                    }
                    return trg;
                }


                /// <summary>
                /// is this a production database?
                /// </summary>
                public bool isProductionDB
                {
                    get
                    {
                        return val("isProductionDB") == "1";
                    }
                }
                /// <summary>
                /// no modifications made, but parses parameters
                /// </summary>
                /// <param name="sCon"></param>
                /// <returns></returns>
                private string InitializeConnectionString(string sCon)
                {
                    connectionString = sCon;
                    hsh = System.Web.HttpUtility.ParseQueryString(sCon.Replace("\r","").Replace("\n","").Replace("\t","").Replace(" ","").Replace(";","&"));
                    return connectionString;
                }
                private string InitializeConnectionString(string sCon, NameValueCollection hshMod)
                {
                    //doesn't have the equivilant of SQl Application Name in connection string
                    hsh = CreateHash(sCon, hshMod);
                    connectionString = hsh.ToString().Replace("&", ";");
                    return connectionString;
                }

                /// <summary>
                /// String to Connect to Sql With
                /// </summary>
                public string ConnectionString
                {
                    get
                    {
                        return connectionString;
                    }
                }
                /// <summary>
                /// Name to potentially alias as to database
                /// </summary>
                /// <summary>
                /// is this a production database?
                public string UserAlias
                {
                    get
                    {
                        return val("UserAlias");
                    }
                }
                public string Comments
                {
                    get
                    {
                        return val("Comments");
                    }
                }
                public bool PromptLogin
                {
                    get
                    {
                        bool ret = "true" == val("PromptLogin").ToLower();
                        return ret;
                    }
                }
                public bool SaveLogin
                {
                    get
                    {
                        return "true" == val("SaveLogin").ToLower();
                    }
                }
                public string DataSource
                {
                    get
                    {
                        return val("datasource");
                    }
                }
                public string UserID
                {
                    get
                    {
                        return val("userid");
                    }
                }
                public string Password
                {
                    get
                    {
                        return val("password");
                    }
                }
                public bool PersistSecurityInfo
                {
                    get
                    {
                        return val("persistsecurityinfo").ToLower()=="true";
                    }
                }
                /// <summary>
                /// retrieve the value from the row's associated column
                /// </summary>
                /// <param name="colname"></param>
                /// <returns></returns>
                public string val(string colname)
                {
                    string ret = "";
                    if (row.Table.Columns.Contains(colname) == true)
                        ret = row[colname].ToString();
                    if (ret == "")
                        ret = hsh[colname];
                    return ret;
                }
                string Alias
                {
                    get
                    {
                        return alias;
                    }
                }
            }
#endregion
#region IBM
            /// <summary>
            /// connection Settings for IBM DB2
            /// </summary>
            public class IBM
            {
                string alias = "";
                DataRow row = null;
                string connectionString;//the xml string is modified...
                private NameValueCollection hsh = new NameValueCollection();
                private NameValueCollection hshLibList = new  NameValueCollection();
                /// <summary>
                /// Initialization
                /// </summary>
                /// <param name="ds">contains the Alias and connection data</param>
                public IBM(DataSet ds)
                {
                    Initialize(ds,"");
                }
                public IBM(DataSet ds, string Alias)
                {
                    Initialize(ds, Alias);
                }
                public IBM(DataSet ds, NameValueCollection hshMod)
                {
                    Initialize(ds, hshMod);
                }
                public IBM(string ConnectionString,string LibList)
                {
                    InitializeConnectionStringAndLibList(ConnectionString, LibList);
                }
                private void Initialize(DataSet ds,string Alias)
                {
                    string adapterType = "IBM";
                    try
                    {
                        alias = Alias;
                        if (alias == "")
                            alias = ds.Tables["Alias"].Select("Adapter='" + adapterType + "'")[0]["Name"].ToString();
                        
                        row = ds.Tables[adapterType].Select("Alias='" + alias + "'")[0];
                        //parse the connectionstring info out

                        InitializeConnectionStringAndLibList(val("ConnectionString"), val("LibList"));
                    }
                    catch (Exception e)
                    {
                        string msg;
                        if (alias != "")
                            msg = "could not find " + alias + " in the Config file";
                        else
                            msg = "could not find Sql.Alias in the db.Config file";
                        throw new Exception(msg + "\n" + e.Message, e);
                    }
                }
                private void Initialize(DataSet ds, NameValueCollection hshMod)
                {
                    string adapterType = "IBM";
                    string Alias = hshMod["Alias"];
                    try
                    {
                        if (Alias == "")
                            alias = ds.Tables["Alias"].Select("Adapter='" + adapterType + "'")[0]["Name"].ToString();
                        else


                        row = ds.Tables[adapterType].Select("Alias='" + Alias + "'")[0];

                        InitializeConnectionStringAndLibList(val("ConnectionString"), val("LibList"));
                    }
                    catch (Exception e)
                    {
                        string msg;
                        if (alias != "")
                            msg = "could not find " + alias + " in the Config file";
                        else
                            msg = "could not find Sql.Alias in the Config file";
                        throw new Exception(msg + "\n" + e.Message, e);
                    }
                }
                private string ModifyConnectionString(string sCon, NameValueCollection hshMod)
                {
                    NameValueCollection trg = System.Web.HttpUtility.ParseQueryString(connectionString.Replace(";", "&").Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace("  ", ""));
                    foreach (string key in trg)
                    {
                        if (hshMod[key] != "")
                        {
                            trg[key] = hshMod[key];
                        }
                    }
                    return trg.ToString().Replace("&", ";");
                }
                private string InitializeConnectionStringAndLibList(string sCon,string sLibList)
                {
                    sLibList=InitializeLibList(sLibList);
                    connectionString = sCon.Replace("~InsertLibList~", LibList());
                    hsh = System.Web.HttpUtility.ParseQueryString(connectionString.Replace(";", "&").Replace("\n","").Replace("\r","").Replace("\t","").Replace("  ",""));
                    return connectionString;
                }
                /// <summary>
                /// parse the LibList into a hash and delimited string
                /// </summary>
                /// <param name="list"></param>
                /// <returns></returns>
                private string InitializeLibList(string list)
                {
                    //use the httputility to cheat on the parsing
                    list = list.Replace("\r", "").Replace("\n","").Replace("\t","").Replace(" ","");
                    list = list.Replace(",", "&");
                    hshLibList=System.Web.HttpUtility.ParseQueryString(list);

                    return LibList();
                }
                /// <summary>
                /// String to Connect to Sql With
                /// </summary>
                public string ConnectionString
                {
                    get
                    {
                        return connectionString;
                    }
                }
                public string Comments
                {
                    get
                    {
                        return val("Comments");
                    }
                }
                public bool PromptLogin
                {
                    get
                    {
                        bool ret = "true" == val("PromptLogin").ToLower();
                        return ret;
                    }
                }
                public bool SaveLogin
                {
                    get
                    {
                        return "true" == val("SaveLogin").ToLower();
                    }
                }
                public string DataSource
                {
                    get
                    {
                        return val("datasource");
                    }
                }
                public string UserID
                {
                    get
                    {
                        return val("userid");
                    }
                }
                public string Password
                {
                    get
                    {
                        return val("password");
                    }
                }
                public string Naming
                {
                    get
                    {
                        return val("naming");
                    }
                }
                /// <summary>
                /// returns the comma delimited LibList
                /// </summary>
                /// <returns></returns>
                public string LibList()
                {
                        string ret = "";
                        foreach (string key in hshLibList)//don't know why it's returning key instead of value
                        {
                            ret += "," + hshLibList[key];
                        }
                        ret = ret.Substring(1);
                        return ret;
                }
                /// <summary>
                /// replaces the aliased Libs with the actual values
                /// </summary>
                /// <param name="sql"></param>
                /// <returns></returns>
                public string SqlReplaceLibAlias(string sql)
                {
                    string ret = sql;
                        foreach (string key in hshLibList)
                        {
                            ret = Regex.Replace(ret, key, hshLibList[key], RegexOptions.IgnoreCase);
                        }

                    return ret;
                }
                public string LibList(int index)
                {
                    return hshLibList[index];
                }
                public string LibList(string key)
                {
                    return hshLibList[key];
                }

                /// <summary>
                /// retrieve the value from the row's associated column
                /// </summary>
                /// <param name="colname"></param>
                /// <returns></returns>
                public string val(string colname)
                {
                    string ret = "";
                    if (row.Table.Columns.Contains(colname) == true)
                        ret = row[colname].ToString();
                    if (ret == "")
                        ret = hsh[colname];
                    return ret.Trim();
                }

                string Alias
                {
                    get
                    {
                        return alias;
                    }
                }
            }
#endregion
        }
#endregion

    }
}
