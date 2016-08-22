using System;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using System.Diagnostics;

namespace SDB.Web
{
    /// <summary>
    /// Summary description for WebPage.
    /// </summary>
    public class WebPage : System.Web.UI.Page
    {
        private DataSet dsLook;
        private SDB.Adapter.SQL.DB data;
        /// <summary>
        /// Initialize object, redirect on session timeout
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (Context.Session != null)
            {
                if (Session.IsNewSession)
                {
                    string sCookieHeader = Request.Headers["Cookie"];
                    if ((null != sCookieHeader) && (sCookieHeader.IndexOf("ASP.Net_SessionId") >= 0))
                    {
                        Response.Redirect("~Login.aspx?msg=Session Timeout. Please Log In");
                    }
                }
            }
        }
        /// <summary>
        /// return the lookup dataset for additional use
        /// </summary>
        protected DataSet dsLookup
        {
            get
            {
                return dsLook;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public WebPage()
        {
            data = new SDB.Adapter.SQL.DB();
        }
        /// <summary>
        /// The underlying data wrapper object for use when 'Submit' is not sufficient
        /// </summary>
        protected SDB.Adapter.SQL.DB Data
        {
            get
            {
                return data;
            }
        }

        protected void Redirect(string url, string target)
        {
            string script = LoadJavascript("Redirect.js").Replace("</script>", "openTarget ('" + url + "','" + target + "');\n</script>");
            this.RegisterJavascript(script, "redirect");
        }
        protected void Redirect(string url)
        {
            string script = LoadJavascript("Redirect").Replace("</script>", "\nopenTarget ('" + url + "',null);\n</script>");
            this.RegisterJavascript(script, "redirect");
        }
        ///// <summary>
        ///// Sets the focus of the initial control on startup
        ///// </summary>
        ///// <param name="ctlName"></param>
        //protected void SetFocus(string ctlName)
        //{
        //    System.Text.StringBuilder sb = new System.Text.StringBuilder();
        //    sb.Append ("<script language='javascript' type='text/javascript'>");
        //    sb.Append ("document.getElementById('" + ctlName + "').focus();");
        //    sb.Append ("</script>");
        //    RegisterStartupScript("Focus", sb.ToString); 
        //}
        /// <summary>
        /// Executes a procedure, returns a Dataset and creates dynamic javascript to bind the elements and dropdowns of a page.  Determines if Form or Querystring values are being used to bind parameters
        ///
        /// </summary>
        /// <param name="spName"></param>
        protected DataSet Submit(string spName)
        {
            NameValueCollection hsh = new NameValueCollection();
            if (Request.Form.Count > 0)
                hsh.Add(Request.Form);
            else
                hsh.Add(Request.QueryString);

            return Submit(spName, hsh);
        }
        /// <summary>
        /// Executes a procedure, returns a Dataset and creates dynamic javascript to bind the elements and dropdowns of a page.  Determines if Form or Querystring values are being used to bind parameters
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="hsh">a custom NameValue collection</param>
        /// <returns></returns>
        protected DataSet Submit(string spName, NameValueCollection hsh)
        {
            hsh.Add("IDUserAudit", Common.User.ID);
            DataSet ds = data.Fill(spName, hsh);
            LoadComboAndElements(spName, ds);

            //if (Common.Flags.isTestMode == true)
            //    System.Web.HttpContext.Current.Response.Write("<!--sql*******\n" + Data.TestScript + "\n**********-->");

            return ds;
        }
        /// <summary>
        /// Executes a procedure, returns a Dataset and creates dynamic javascript to bind the elements and dropdowns of a page.  Determines if Form or Querystring values are being used to bind parameters
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="querystring">a string formatted like a querystring (currently not removing 'real' querystring formatting</param>
        /// <returns></returns>
        protected DataSet Submit(string spName, string querystring)
        {
            NameValueCollection hsh = new NameValueCollection();
            string[] s = querystring.Split("&".ToCharArray());
            foreach (string x in s)
            {
                hsh.Add(x.Split("=".ToCharArray())[0], x.Split("=".ToCharArray())[1]);
            }
            DataSet ds = data.Fill(spName, hsh);
            LoadComboAndElements(spName, ds);
            return ds;
        }
        private void LoadCombo(string spName)
        {
            dsLook = data.GetLookup(spName);
            ListControl ctl = null;
            string el = "";
            string elPrev = "";
            string key = "";
            string txt = "";

            foreach (DataRow row in dsLook.Tables[0].Rows)
            {
                el = row["NameField"].ToString();
                if (el != elPrev)
                {

                    foreach (WebControl c in Controls)
                    {
                        string s = c.ID;
                    }
                    elPrev = el;
                    ctl = (ListControl)FindControl("cmb" + el);
                    if (ctl == null)
                        ctl = (ListControl)FindControl("lst" + el);
                    if (ctl != null)
                        if (Common.Functions.GetRowVal(row, "NoBlank", "0") == "0")
                            ctl.Items.Add("");
                }
                if (ctl != null)
                {
                    ListItem itm = new ListItem(txt, key);
                    ctl.Items.Add(itm);
                }
            }
        }
        private void LoadElements(DataSet ds)
        {
            if (ds.Tables.Count == 0)
                return;
            if (ds.Tables[0].Rows.Count == 0)
                return;

            DataRow row = ds.Tables[0].Rows[0];
            string key = "";
            string txt = "";
            System.Web.UI.Control ctl = null;

            foreach (DataColumn col in ds.Tables[0].Columns)
            {
                key = col.ColumnName;
                txt = row[col].ToString();
                foreach (System.Web.UI.Control c in Controls[1].Controls)
                {
                    string ctlName = c.ID;
                    if (ctlName.Length >= key.Length)
                    {/*
                        if (ctlName.Substring(ctlName.Length-key.Length).ToUpper=key.ToUpper())
                        {
                            ctl = c;
                            Type t = typeof(char);

                            break;
                        }
                      */
                    }
                }


            }







        }
        /// <summary>
        /// creates dynamic javascript to bind elements named like the fields in the dataset.  elements can have hungarian notation, but the first letter of the fieldname must be capitalized
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="ds"></param>
        protected void LoadComboAndElements(string spName, DataSet ds)
        {
            string Elements = "";
            Elements = LoadComboAndElements_A_Elements(ds) + LoadComboAndElements_B_Combo(spName);
            RegisterJavascript(Elements, "sdbElements");
            LoadJavascript("_sdbLoadData", "sdbLoadElements");
        }
        /// <summary>
        /// load combo boxes without running anything to retrieve element values
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="OnlyLoadCombo"></param>
        protected void LoadComboAndElements(string spName, bool OnlyLoadCombo)
        {
            string Elements = "";
            Elements = LoadComboAndElements_B_Combo(spName);
            RegisterJavascript(Elements, "sdbElements");
            LoadJavascript("_sdbLoadData", "sdbLoadElements");
        }
        /// <summary>
        /// read through the dataset and build a javascript array for loading at the client
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        private string LoadComboAndElements_A_Elements(DataSet ds)
        {
            //stop if no record exists
            if (ds.Tables.Count < 1)
                return "";
            if (ds.Tables[0].Rows.Count < 1)
                return "";

            System.Text.StringBuilder s = new System.Text.StringBuilder();
            s.Append("//Elements Created Dynamically by SDB 2.0");
            DataRow row = ds.Tables[0].Rows[0];
            string ElName;//Element name
            string ElVal;//Element value

            s.Append("\nhshEl=new Array(); //creating a name-value pair\n");

            DataTable tbl = ds.Tables[0];
            foreach (DataColumn col in ds.Tables[0].Columns)
            {
                ElName = col.ColumnName;
                ElVal = row[ElName].ToString();
                ElVal = ElVal.Replace("\\", @"\\");
                //check for ", verify preprocessing hasn't already added \"
                //                if (ElVal.IndexOf(@"""") > -1)
                ElVal = ElVal.Replace(@"""", @"\""");//replace quotes with double quotes for javascript
                ElVal = ElVal.Replace("\r\n", @"\r\n");
                ElVal = ElVal.Replace("\n", @"\n");

                s.Append("hshEl[\"" + ElName.ToUpper() + "\"]=\"" + ElVal + "\";\n");

            }

            return s.ToString();
        }
        /// <summary>
        /// creates the dynamic javascript for populating combo boxes
        /// </summary>
        /// <param name="spName"></param>
        /// <returns></returns>
        public string LoadComboAndElements_B_Combo(string spName)
        {
            if (spName.IndexOf(@"\")>0)
                spName=spName.Substring(spName.LastIndexOf(@"\")+1);
            if (spName.IndexOf(@"/") > 0)
                spName = spName.Substring(spName.LastIndexOf(@"/") + 1);

            dsLook = data.GetLookup(spName);
            return LoadComboAndElements_B_Combo(dsLook.Tables[0]);
        }
        private string LoadComboAndElements_B_Combo(DataTable Table)
        {
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            s.Append("//Combo Elements Dynamically created in SDB2.0");
            s.Append("\n\tvar hshCmb=new Array();\n\t\tvar hshOpt = new Array();\n");
            if (Table.Rows.Count > 0)
            {
                string NameField = Table.Rows[0]["NameField"].ToString();
                s.Append("\t\thshOpt[\"\"]='';\n");
                foreach (DataRow row in Table.Rows)
                {
                    if (row["Namefield"].ToString() == NameField)
                    {
                        s.Append("\t\thshOpt[\"" + row["Text"].ToString() + "\"]='" + row["ID"].ToString() + "';\n");
                    }
                    else
                    {
                        s.Append("\thshCmb[\"" + NameField.ToUpper() + "\"]=hshOpt;\n");
                        s.Append("\t\thshOpt=new Array();\n");

                        NameField = row["NameField"].ToString();
                        //if (NameField == "LKRequestMode")
                        //    NameField = NameField;
                        if (Common.Functions.GetRowVal(row, "NoBlank", "0") == "0")
                            s.Append("\t\thshOpt[\"\"]='';\n");
                        s.Append("\t\thshOpt[\"" + row["Text"].ToString() + "\"]='" + row["ID"].ToString() + "';\n");

                    }
                }
                s.Append("\thshCmb[\"" + NameField.ToUpper() + "\"]=hshOpt;\n");
            }
            return s.ToString();
        }
        /// <summary>
        /// wraps the RegisterClientScriptBlock with a test to check if it already exists
        /// </summary>
        /// <param name="script"></param>
        /// <param name="RegisterName"></param>
        public void RegisterJavascript(string script, string RegisterName)
        {
            if (script.Substring(0, 1) != "<")
                script = "<script language=javascript>\n" + script + "\n</script>";

            if (!IsClientScriptBlockRegistered(RegisterName))
                RegisterClientScriptBlock(RegisterName, "\n" + script + "\n");
            //??? checking on consequence of not catching a client script block registration
            //else
            //	throw new Exception("Duplicate Script Registration: " + RegisterName);
        }
        /// <summary>
        /// Load and Register Javascript File using the specified Register block name
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="RegisterName"></param>
        /// <returns></returns>
        public string LoadJavascript(string FileName, string RegisterName)
        {
            string ret = LoadJavascript(FileName);
            if (!IsClientScriptBlockRegistered(RegisterName))
                RegisterClientScriptBlock(RegisterName, ret);
            else
                throw new Exception(FileName + " has already been registered under " + RegisterName);
            return ret;
        }
        /// <summary>
        /// Load and register a javascript file
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public string LoadJavascript(string FileName)
        {
            string ret;
            FileName = Common.Properties.RootPath + @"Script\Javascript\" + FileName + ".js";
            System.IO.StreamReader sr = new System.IO.StreamReader(FileName);
            ret = "<script language=javascript>\n\t" + sr.ReadToEnd() + "\n</script>";
            sr.Close();

            return ret;
        }

        public void ShowDialog(string Text)
        {
            ShowDialog(Text, "", "");
        }
        public void ShowDialog(string Text, string YesButtonText, string NoButtonText)
        {

            Text = Text.Replace(@"\", @"\\");
            Text = Text.Replace("\t", "\\t");
            Text = Text.Replace("\r\n", "\\n");
            Text = Text.Replace("\r", "");
            Text = Text.Replace("<br>", "\\n");
            Text = Text.Replace("<BR>", "\\n");
            Text = Text.Replace("&nbsp;", " ");
            Text = Text.Replace("\n", "\\n");

            string Script = "ShowDialog(url,msg,btnYes,btnNo);";
            string url = "\"" + Common.Properties.RootWeb + "script/template/dlgMsg.htm" + "\"";
            url = url.Replace("//", "/");
            Script = Script.Replace("url", url);
            //Script = Script.Replace("url", "\"" + System.Web.HttpContext.Current.Request.ApplicationPath + "/script/template/dlgMsg.htm" + "\"");
            Script = Script.Replace("msg", "\"" + Text + "\"");
            Script = Script.Replace("btnYes", "\"" + YesButtonText + "\"");
            Script = Script.Replace("btnNo", "\"" + NoButtonText + "\"");

            Script = LoadJavascript("_SDBMessage") + "\n<script language=javascript>\n" + Script + "\n</script>\n";
            RegisterJavascript(Script, "sdbMessage");
        }
        /// <summary>
        /// emulates a MessageBox ability on the client for server side code
        /// </summary>
        /// <param name="Text"></param>
        public void Message(string Text)
        {
            ShowDialog(Text);
        }
        /// <summary>
        /// emulate a message box for server side; reports the Exception information
        /// </summary>
        /// <param name="ex"></param>
        public void Message(System.Exception ex)
        {
            string stack = ex.StackTrace;//.Replace("at ","* ").Replace(" in ","\n\t");
            stack = Regex.Replace(stack, @" in.+:line", @":");
            stack = stack.Replace("at ", "* ");
            string msg = "<b><i>" + ex.Source + "</i></b>\n\n<font color=red>" + ex.Message + "</font>\n\n" + stack;
            ShowDialog(msg);
        }
        /// <summary>
        /// Emulates a messagebox, allows the addition of yes/no buttons.  note that additional client script must be written on aspx form to process the return values.
        /// view source of running app and look at script/template/dlgmsg.htm for details
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="YesButtonText"></param>
        /// <param name="NoButtonText"></param>
        public void Message(string Text, string YesButtonText, string NoButtonText)
        {
            ShowDialog(Text, YesButtonText, NoButtonText);
        }
    }
}
