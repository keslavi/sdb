using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Xsl;

namespace SDB.Common
{
	/// <summary>
	/// Various important settings such as the root path of the web application
	/// </summary>
	public class Properties
	{
        /// <summary>
        /// The Root of the executing dll or exe
        /// </summary>
        public static string Root
        {
            get
            {
                string ret = "";

                if (System.Web.HttpContext.Current != null)
                    ret = System.Web.HttpContext.Current.Server.MapPath("/");
                else
                {
                    ret = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    ret = ret.Replace(@"file:\", "") + @"\";
                }
                return ret;
            }
        }

		/// <summary>
		/// The Root physical directory of the web application
		/// </summary>
		public static string RootPath
		{ 
			get
			{
				string ret = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
				ret=ret.Replace(@"file:\", "").Replace(@"\bin", "") + @"\";
				return ret;
			}
		}
		/// <summary>
		/// The Root Virtual path of the web application
		/// </summary>
		public static string RootWeb
		{
			get
			{
                string s = System.Web.HttpContext.Current.Request.ApplicationPath + "/";
                s = s.Replace("//", "/");
                return s;
			}
		}
        public static string RootWebAbsolute
        {
            get
            {
                string AbsoluteRoot = "http://" + System.Web.HttpContext.Current.Request.Url.Authority;
                string s = System.Web.HttpContext.Current.Request.ApplicationPath + "/";
                s = s.Replace("//", "/");
                s = AbsoluteRoot + s;
                return s;
            }
        }
		/// <summary>
		/// Indicates if the system is in test mode for optional debug code
		/// </summary>
		public bool  TestMode
		{
			get
			{
				string s = Common.Functions.Coalesce(ConfigurationManager.AppSettings["TestMode"],"").ToUpper();
				bool b =s=="ON";
				return b;
			}
		}
		/// <summary>
		/// Indicates if the system is automatically storing data about procedure calls
		/// </summary>
		public bool  AuditMode
		{
			get
			{
				string s = Common.Functions.Coalesce(ConfigurationManager.AppSettings["AuditMode"],"").ToUpper();
				bool b =s=="ON";
				return b;
			}
		}
	}
	/// <summary>
	/// Flags (Usually from web.config)
	/// </summary>
	public class Flags
	{
        /// <summary>
        /// returns 'hidden' if not in test mode
        /// </summary>
        public static string cssClassVisTest
        {
            get
            {
                string ret = "";
                if (!isTestMode)
                {
                    ret = "hidden";
                }
                return ret;
            }
        }
        /// <summary>
        /// returns 'hidden' if not an MIS user or aliased
        /// </summary>
        public static string cssClassVisMIS
        {
            get
            {
                string ret = "";
                if (Common.User.SecurityLevel<9)
                {
                    ret = "hidden";
                }
                return ret;
            }
        }

		/// <summary>
		/// use this flag to set conditional code for testing
		/// </summary>
		public static bool isTestMode
		{
			get
			{
				return Common.Functions.Coalesce(ConfigurationManager.AppSettings["TestMode"],"").ToLower()=="on";
			}
		}
		/// <summary>
		/// If audit mode is set, stored procedure and parameters are logged into audit tables.
		/// </summary>
		public static bool isAuditMode
		{
			get
			{
				return Common.Functions.Coalesce(ConfigurationManager.AppSettings["AuditMode"],"").ToLower()=="on";
			}
		}
	}
	/// <summary>
	/// Commonly used 'Global' functions
	/// </summary>
	public class Functions
	{
        public static string ToProperCase(string value)
        {
            CultureInfo ci = new CultureInfo("en"); //Create a new CultureInfo class for the english language
            TextInfo cc = ci.TextInfo; //Get the textinfo class from the CultureInfo object
            return cc.ToTitleCase(value);
        }
        /// <summary>
        /// convert a dataset into a string
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static string DataSetToString(System.Data.DataSet ds)
        {
            string ret;
            System.IO.StringWriter sw = new System.IO.StringWriter();
            ds.WriteXml(sw);
            ret = sw.ToString();
            sw.Close();
            return ret;
        }
        /// <summary>
        /// Convert the dataset to another xml form using an xsl sheet
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="xsl"></param>
        /// <returns></returns>
        public static string TransformXML(System.Data.DataSet ds,string xsl)
        {
            StringWriter  sw =  new StringWriter();
            XmlTextWriter xWrite =new XmlTextWriter(sw);
            System.Xml.Xsl.XslCompiledTransform xslt = new XslCompiledTransform();

            xslt.Load(XmlReader.Create(new StringReader(xsl)));
            xslt.Transform(new System.Xml.XmlTextReader(new StringReader(ds.GetXml())), xWrite);
            string xml = sw.ToString();

            return  xml;
        }
        /// <summary>
        /// Convert the xml string to another xml form using an xsl sheet
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="xsl"></param>
        /// <returns></returns>
        public static string TransformXML(string xml, string xsl)
        {
            StringWriter sw = new StringWriter();
            XmlTextWriter xWrite = new XmlTextWriter(sw);
            System.Xml.Xsl.XslCompiledTransform xslt = new XslCompiledTransform();

            xslt.Load(XmlReader.Create(new StringReader(xsl)));
            xslt.Transform(new System.Xml.XmlTextReader(new StringReader(xml)), xWrite);
            xml = sw.ToString();
            return xml;
        }
		/// <summary>
		/// a stronger Split method that allows a string delimiter instead of a char
		/// </summary>
		/// <param name="StringToSplit"></param>
		/// <param name="Delimiter"></param>
		/// <returns></returns>
		public static string[] Split(string StringToSplit, string Delimiter) 
		{ 
			StringToSplit=StringToSplit.Replace("\ngo\n","\nGO\n");//this has to do with potential database scripting usage
			char c=(char)1;
			StringToSplit=StringToSplit.Replace( Delimiter ,c.ToString());
			string[] ret=StringToSplit.Split(c);
			return ret;
		}
        /// <summary>
        /// coerce nulls into empty strings
        /// </summary>
        /// <param name="row"></param>
        /// <param name="ColumnName"></param>
        /// <returns></returns>
        public static string GetRowVal(System.Data.DataRow row, string ColumnName)
        {
            string ret="";
            if (!row.IsNull(ColumnName))
                ret=row[ColumnName].ToString();
            return ret;
        }
        public static string GetRowVal(System.Data.DataRow row, string ColumnName, string DefaultVal)
        {
            string ret = DefaultVal;
            if (!row.IsNull(ColumnName))
                ret = row[ColumnName].ToString();
            return ret;
        }
        public static string GetRowVal(System.Data.DataRow row, string ColumnName, bool TransformToJavaScript)
        {
            string ElVal = GetRowVal(row, ColumnName);
            if (!TransformToJavaScript)
                return ElVal;

            ElVal = ElVal.Replace("\"", "\"\"");//replace quotes with double quotes for javascript
            ElVal = ElVal.Replace("\r\n", @"\r\n");
            ElVal = ElVal.Replace("\n", @"\n");

            return ElVal;
        }
        /// <summary>
        /// take a master page's controls and get the root element name
        /// </summary>
        /// <param name="hshSrc"></param>
        /// <returns></returns>
        public static NameValueCollection hshSimplifyKey(NameValueCollection hshSrc)
        {
            NameValueCollection hsh = new NameValueCollection();
            foreach (string key in hshSrc.AllKeys)
            {
                string keyName = key;
                string keyVal = hshSrc[key].Trim();

                if (keyName != null)
                {
                    keyName = key.Split("$".ToCharArray())[key.Split("$".ToCharArray()).Length - 1];
                    if (keyName.ToLower() != keyName && keyName.Substring(0, 2) != "__")
                    {
                        keyName = keyName.Replace("00_ContentPlaceHolder1_", "");
                        keyName = keyName.Replace("_input", "");
                        keyName = Regex.Replace(keyName, "^[abcdefghijklmnopqrstuvwxyz]+", "");
                        if (keyName.Substring(0, 2) != "__")
                        {
                            //TODO: build a regex, don't add anything with the following:
                            Match m = Regex.Match(" " + keyName, ".+(ScriptManager|ContentPlaceHolder|_hidden|_Calend)");
                            if (!m.Success)
                                hsh.Add(keyName, keyVal);
                        }
                    }
                }
            }
            return hsh;
        }
        /// <summar>convert
        /// y a DataRow to a Hash Collection
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <returns>NameValueCollection</returns>
        public static NameValueCollection RowToHash(DataRow row)
        {
            NameValueCollection hsh=new NameValueCollection();
            foreach (DataColumn col in row.Table.Columns)
                if (!row.IsNull(col.ColumnName))
                    hsh.Add ( col.ColumnName.ToUpper(),row[col.ColumnName].ToString().Trim()  );
            return hsh;
        }

        /// <summary>
        /// returns the first non null/blank string or a blank if none found
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Coalesce (params object[] args)
        {
            string ret = string.Empty;
            string arg="";
            try
            {
                foreach (object o in args)
                {
                    if (o != null)
                    {
                        arg = o.ToString();
                        if (!string.IsNullOrEmpty(arg))
                        {
                            ret = arg;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return ret;
        }
    }
	/// <summary>
	/// frequently used User session information
	/// </summary>
	public class User
	{
		/// <summary>
		/// sets a session value (usually for use in Namesspace Common)
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		private static void SesVal(string key,string value)
		{
			HttpContext context=HttpContext.Current;
			System.Web.SessionState.HttpSessionState ses= context.Session;
			if (ses!=null)
				ses[key]=value;
		}
		/// <summary>
		/// Returns the specified session value
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		private static string SesVal(string key)
		{
			string ret="";
			HttpContext context=HttpContext.Current;
			System.Web.SessionState.HttpSessionState ses= context.Session;
			if (ses[key]!=null)
				ret=ses[key].ToString();
			return ret;
		}
        /// <summary>
        /// return an html formatted display of the username, email, phone, ad status, and notification to state contact info
        /// </summary>
        public static string ToHtml
        {
            get
            {
                System.Text.StringBuilder s=new System.Text.StringBuilder();
                s.Append("<hr/><table><tr><td>");
                s.Append("<b>Current User: </b>" + SDB.Common.User.Name);
                if (Email != "")
                    s.Append(" <b>Email: </b>" + Email ) ;
                if (Phone != "")
                    s.Append(" <b>Phone: </b>" + Phone);
                s.Append("</td></tr></table>\r");
                if (isNonADUser)
                    s.Append("\r<br/><font color=yellow><b> NON AD USER! </b></font>");
                if (Email == "" && Phone == "")
                {
                    if (s.ToString().IndexOf("<br") == -1)
                        s.Append("\r<br/>");
                    s.Append("<font color=yellow><b> Please state contact info in comments. </b></font>");
                }
                s.Append("<hr/>\r");
                return s.ToString();
            }
        }

        /// <summary>
		/// Set or return the ID of the User. Although this is string for simpler coercion, it is usually an int value
		/// </summary>
		public static string ID
		{
			get
			{
				string s=SesVal("sesIDUser");
				if (s=="")
					s="0";
				return s;
			}
			set
			{
				SesVal("sesIDUser",value);
			}
		}
        /// <summary>
        /// checks to see if user is aliased or removes the alias if it exists
        /// </summary>
        public static bool Aliased
        {
            get
            {
                return SesVal("sesAliased")=="True";
            }
            set
            {
                string s = "False";
                if (value)
                {
                    s = "True";
                    AliasID = ID;
                    AliasUserName = SDB.Common.User.Name;
                    AliasOU = SDB.Common.User.OU;
                    AliasSecurityLevel = SecurityLevel;
                    AliasGroup = Group;
                    AliasEmail = Email;
                    AliasPhone = Phone;
                }
                else
                {
                    if (Aliased == true)
                    {
                        Name = AliasUserName;
                        ID = AliasID;
                        OU = AliasOU;
                        SecurityLevel = AliasSecurityLevel;
                        Group = AliasGroup;
                        Email = AliasEmail;
                        Phone = AliasPhone;
                        isNonADUser = false;

                        AliasID = "";
                        AliasUserName = "";
                        AliasOU = "";
                        AliasSecurityLevel = 0;
                        AliasGroup = "";
                        AliasEmail = "";
                        AliasPhone = "";
                    }
                }
                SesVal("sesAliased", s );
            }
        }
        /// <summary>
        /// mark the user as an ad or non ad user
        /// </summary>
        public static bool isNonADUser
        {
            get
            {
                return SesVal("sesisNonADUser")=="True";
            }
            set 
            {
                if (value)
                   SesVal("sesisNonADUser","True"); 
                else
                    SesVal("sesisNonADUser","False"); 

            }
        }
        /// the AliasPhone of the logged in user
        /// </summary>
        public static string AliasPhone
        {
            get
            {
                return SesVal("sesAliasPhoneUser");
            }
            set
            {
                SesVal("sesAliasPhoneUser", value);
            }
        }
        /// <summary>
        /// the user id of the original user
        /// </summary>
        public static string AliasID
        {
            get
            {
                string s = SesVal("sesAliasIDUser");
                if (s == "")
                    s = "0";
                return s;
            }
            set
            {
                SesVal("sesAliasIDUser", value);
            }
        }
        public static string AliasUserName
        {
            get
            {
                return SesVal("sesAliasUserName");
            }
            set
            {
                SesVal("sesAliasUserName", value);
            }
        }
        public static string AliasOU
        {
            get
            {
                return SesVal("sesAliasOU");
            }
            set
            {
                SesVal("sesAliasOU", value);
            }
        }

        public static string AliasEmail
        {
            get
            {
                return SesVal("sesAliasEmail");
            }
            set
            {
                SesVal("sesAliasEmail", value);
            }
        }
        public static string AliasGroup
        {
            get
            {
                return SesVal("sesAliasGroupUser");
            }
            set
            {
                SesVal("sesAliasGroupUser", value);
            }
        }

        public static string OU
        {
            get
            {
                return SesVal("sesOUUser");
            }
            set
            {
                SesVal("sesOUUser", value);
            }
        }

		/// <summary>
		/// Sets or returns the User's Group
		/// </summary>
		public static string Group
		{
			get
			{
				return SesVal("sesGroupUser");
			}
			set
			{
				SesVal("sesGroupUser",value);
			}
		}
        /// <summary>
        /// verify a user belongs to a specific group by Group ID
        /// </summary>
        /// <param name="IDGroup"></param>
        /// <returns></returns>
        public static bool isInGroup(int IDGroup)
        {
            //System.Collections.Specialized.NameValueCollection hsh = GroupToHash();
            //return hsh[IDGroup.ToString()] != null;
            string g = "\r" + Group;
            return g.IndexOf("\r" + IDGroup.ToString() + "\t") > -1;
        }

        /// <summary>
        /// verify a user belongs to a specific group by group text
        /// </summary>
        /// <param name="GroupText"></param>
        /// <returns></returns>
        public static bool isInGroup(string GroupText)
        {
            return Group.IndexOf(GroupText) > -1;
        }
        public static System.Collections.Specialized.NameValueCollection GroupToHash()
        {
            System.Collections.Specialized.NameValueCollection hsh=new System.Collections.Specialized.NameValueCollection();
            string[] s =Group.Split("\r".ToCharArray());

            string key = "";
            string val = "";
            string x2 = "";
            foreach (string x in s)
            {
                try
                {
                    if (x != "")
                    {
                        x2 = x;
                        key = x.Split("\t".ToCharArray())[0];
                        val = x.Split("\t".ToCharArray())[1];
                        hsh.Add(key, val);
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }

            }
            return hsh;
        }	/// <summary>
	/// the email of the logged in user
	/// </summary>
		public static string Email
		{
			get
			{
				return SesVal("sesEmailUser");
			}
			set
			{
				SesVal("sesEmailUser",value);
			}
		}
        /// the Phone of the logged in user
        /// </summary>
        public static string Phone
        {
            get
            {
                return SesVal("sesPhoneUser");
            }
            set
            {
                SesVal("sesPhoneUser", value);
            }
        }

        /// <summary>
		/// Name of the user
		/// </summary>
		public static string Name
		{
			get
			{
				return SesVal("sesNameUser");
			}
			set
			{
				SesVal("sesNameUser",value);
			}
		}
	/// <summary>
	/// how many times the login has been attempted (for stopping the login process)
	/// </summary>
		public static int SecurityLevel
		{
			get
			{
                string s = SesVal("sesSecurityLevel");
                if (Aliased == true)
                {
                    s = AliasSecurityLevel.ToString();
                }

                if (s == "")
                    s = "0";
				return int.Parse(s);
			}
			set
			{
                SesVal("sesSecurityLevel", value.ToString());
			}
		}
        /// <summary>
        /// the security level of the aliased user
        /// </summary>
        public static int AliasSecurityLevel
        {
            get
            {
                string s = SesVal("sesAliasSecurityLevel");
                if (s == "")
                    s = "0";
                return int.Parse(s);
            }
            set
            {
                SesVal("sesAliasSecurityLevel", value.ToString());
            }
        }
        /// <summary>
        /// how many times the login has been attempted (for stopping the login process)
        /// </summary>
        public static int LoginAttempts
        {
            get
            {
                string s = SesVal("sesGroupUser");
                if (s == "")
                    s = "1";
                return int.Parse(s);
            }
            set
            {
                SesVal("sesGroupUser", value.ToString());
            }
        }
	
	}
}

