using System;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using System.Web.UI;

/*
 Sub LoadElementValues(ByVal dsDet As DataSet)
        Dim col As DataColumn
        Dim row As DataRow = dsDet.Tables(0).Rows(0)

        Dim prefix As String
        Dim fldName As String
        Dim fldVal As String

        For Each col In dsDet.Tables(0).Columns
            fldName = col.ColumnName
            If Not row.IsNull(fldName) Then
                fldVal = row(fldName)
                If IsDate(fldVal) Then
                    fldVal = FormatDate(fldVal)
                End If
            Else
                fldVal = ""
            End If

            prefix = GetPrefix(fldName)
            Select Case prefix
                Case "txt"
                    Dim ctl As System.Web.Ui.WebControls.TextBox = FindControl(prefix & fldName)
                    ctl.Text = fldVal
                Case "cmb", "lst"
                    Dim ctl As System.Web.UI.WebControls.ListControl = FindControl(prefix & fldName)
                    Dim arVal() = fldVal.Split(",")
                    For Each fldVal In arVal
                        Dim itm As System.Web.UI.WebControls.ListItem
                        For Each itm In ctl.Items
                            If itm.Value = fldVal Then itm.Selected = True
                        Next
                    Next
                Case "chk"
                    Throw New Exception("not implemented")
            End Select
        Next

    End Sub
 */
namespace SDB.Web
{
    /// <summary>
    /// populates information dynamically from codebehind
    /// (web content pages have their javascript IDs jiggered with by Master Page)
    /// </summary>
    public class WebContentPage : System.Web.UI.Page
    {
        private SDB.Adapter.SQL.DB data;
        private DataSet dsLook;
        /// <summary>
        ///  Initialize object, redirect on session timeout
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
        /// 
        /// </summary>
        public WebContentPage()
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
        protected DataSet dsLookup
        {
            get
            {
                return dsLook;
            }
        }
        #region Submit
        #region SubmitStoredProcedure

        ///// <summary>
        ///// Sets the focus of the initial control on startup
        ///// </summary>
        ///// <param name="ctlName"></param>
        //protected void SetFocus(string ctlName)
        //{
        //    System.Text.StringBuilder sb = new System.Text.StringBuilder();
        //    sb.Append("<script language='javascript' type='text/javascript'>");
        //    sb.Append("document.getElementById('" + ctlName + "').focus();");
        //    sb.Append("</script>");
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
            //deprecated, it's writing before the doctype and causing errors
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

        #endregion
        #region SubmitScript

        ///// <summary>
        ///// Sets the focus of the initial control on startup
        ///// </summary>
        ///// <param name="ctlName"></param>
        //protected void SetFocus(string ctlName)
        //{
        //    System.Text.StringBuilder sb = new System.Text.StringBuilder();
        //    sb.Append("<script language='javascript' type='text/javascript'>");
        //    sb.Append("document.getElementById('" + ctlName + "').focus();");
        //    sb.Append("</script>");
        //    RegisterStartupScript("Focus", sb.ToString);
        //}
        /// <summary>
        /// Executes a procedure, returns a Dataset and creates dynamic javascript to bind the elements and dropdowns of a page.  Determines if Form or Querystring values are being used to bind parameters
        ///
        /// </summary>
        /// <param name="spName"></param>
        protected DataSet SubmitScript(string ScriptName)
        {
            NameValueCollection hsh = new NameValueCollection();
            if (Request.Form.Count > 0)
               hsh= Common.Functions.hshSimplifyKey(Request.Form);
            else
                hsh=Common.Functions.hshSimplifyKey(Request.QueryString);

            return SubmitScript(ScriptName, hsh);
        }
        /// <summary>
        /// Executes a procedure, returns a Dataset and creates dynamic javascript to bind the elements and dropdowns of a page.  Determines if Form or Querystring values are being used to bind parameters
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="hsh">a custom NameValue collection</param>
        /// <returns></returns>
        protected DataSet SubmitScript(string ScriptName, NameValueCollection hsh)
        {
            hsh.Add("IDUserAudit", Common.User.ID);
            DataSet ds = data.FillScript(ScriptName, hsh);

            LoadComboAndElements(ScriptName, ds);

            //if (Common.Flags.isTestMode == true)
            //    System.Web.HttpContext.Current.Response.Write("<!--sql*******\n" + Data.GetSql  + "\n**********-->");

            return ds;
        }
        /// <summary>
        /// Executes a procedure, returns a Dataset and creates dynamic javascript to bind the elements and dropdowns of a page.  Determines if Form or Querystring values are being used to bind parameters
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="querystring">a string formatted like a querystring (currently not removing 'real' querystring formatting</param>
        /// <returns></returns>
        protected DataSet SubmitScript(string ScriptName, string querystring)
        {
            NameValueCollection hsh = new NameValueCollection();
            string[] s = querystring.Split("&".ToCharArray());
            foreach (string x in s)
            {
                hsh.Add(x.Split("=".ToCharArray())[0], x.Split("=".ToCharArray())[1]);
            }
            return SubmitScript(ScriptName, hsh);
        }

         #endregion
        #endregion

        /// <summary>
        /// dynamically load the combo boxes
        /// </summary>
        /// <param name="dt">a data table that must have 'NameField','Text', and 'NoBlank'</param>
        public void LoadCombo(DataTable dt)
        {
            ListControl ctl = null;
            string el = "";
            string elPrev = "";
            string key = "";
            string txt = "";
            //ContentPlaceHolder mc = (ContentPlaceHolder)Master.FindControl("ContentPlaceHolder1");

            foreach (DataRow row in dt.Rows)
            {
                el = row["NameField"].ToString();
                //if (el=="LKStatus")//for a previous debugging
                //    el=el;
                if (el != elPrev)
                {
                    ctl = null;
                    elPrev = el;
                    Control c = FindControlRecursive(el);
                    if (c != null)
                    {
                        if (c is DropDownList)
                            ctl = (DropDownList)c;
                        else if (c is ListBox)
                            ctl = (ListBox)c;

                        ctl.Items.Clear();
                        if (ctl!=null)
                            if (Common.Functions.GetRowVal(row, "NoBlank", "0") == "0")
                                ctl.Items.Add("");
                    }
                }
                if (ctl != null)
                {
                    txt = "";
                    key = "";
                    if (!row.IsNull("Text"))
                        txt = row["Text"].ToString();
                    key = row["ID"].ToString();
                    ListItem itm = new ListItem(txt, key);
                    ctl.Items.Add(itm);
                }
            }
        }
        /// <summary>
        /// Populate the combo boxes (uses LookupClass)
        /// </summary>
        /// <param name="spName"></param>
        protected void LoadCombo(string spName)
        {
            spName = System.IO.Path.GetFileNameWithoutExtension(spName);
            dsLook = data.GetLookup(spName);
            LoadCombo(dsLook.Tables[0]);
        }
        public void LoadElements(DataSet ds)
        {
            if (ds.Tables.Count == 0)
                return;//might just be loading combos
            DataRow row = null;
            if (ds.Tables[0].Rows.Count > 0)
            {
                row = ds.Tables[0].Rows[0];
            }

            string key = "";
            string txt = "";
            System.Web.UI.Control ctl = null;

            foreach (DataColumn col in ds.Tables[0].Columns)
            {
                key = col.ColumnName;
                txt = "";
                if (row != null)
                    txt = SDB.Common.Functions.GetRowVal(row, key);//"";

                //if (!row.IsNull(col))
                //       txt=row[col].ToString();

                ctl = FindControlRecursive(key);
                if (ctl != null)
                {
                    try
                    {
                        if (ctl is DropDownList)
                            SetDropDownList(ctl, txt);
                        else if (ctl is TextBox)
                            SetTextBox(ctl, txt);
                        else if (ctl is ListBox)
                            SetListBox(ctl, txt);
                        else
                            SetCtl(ctl, txt);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(key + ": " + ctl.ID + " type: " + ctl.GetType().ToString() + "\n\n" + e.Message);
                    }
                }
            }
        }
       public void LoadElements(DataSet ds,int TableIndex)
        {
            if (ds.Tables.Count < TableIndex)
                return;//might just be loading combos
            DataRow row = null;
            if (ds.Tables[TableIndex].Rows.Count > 0)
            {
                row = ds.Tables[TableIndex].Rows[0];
            }

            string key = "";
            string txt = "";
            System.Web.UI.Control ctl = null;

            foreach (DataColumn col in ds.Tables[TableIndex].Columns)
            {
                key = col.ColumnName;
                txt = "";
                if (row != null)
                    txt = SDB.Common.Functions.GetRowVal(row, key);//"";

                //if (!row.IsNull(col))
                //       txt=row[col].ToString();

                ctl = FindControlRecursive(key);
                if (ctl != null)
                {
                    try
                    {
                        if (ctl is DropDownList)
                            SetDropDownList(ctl, txt);
                        else if (ctl is TextBox)
                            SetTextBox(ctl, txt);
                        else
                            SetCtl(ctl, txt);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(key + ": " + ctl.ID + " type: " + ctl.GetType().ToString() + "\n\n" + e.Message);
                    }
                }
            }
        }

        private void SetCtl(Control ctl, string text)
        {
            System.Web.UI.WebControls.WebControl c = (System.Web.UI.WebControls.WebControl)ctl;
            c.Attributes["Text"] = text;

            //Infragistics.WebUI.WebDataInput.WebTextEdit c = (Infragistics.WebUI.WebDataInput.WebTextEdit)ctl;
            ///c.Text = text;
        }
        private void SetTextBox(Control ctl, string text)
        {
            TextBox c = (TextBox)ctl;
            c.Text = text;
        }
        private void SetDropDownList(Control ctl, string text)
        {
            DropDownList c = (DropDownList)ctl;
            c.SelectedValue = text;
        }
        private void SetListBox(Control ctl, string text)
        {
            ListBox c = (ListBox)ctl;
            c.SelectedValue = text;
        }
        /// <summary>
        /// retrive a Control based on it's name regardless of case or hungarian notation
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public Control FindControlRecursive(string ID)
        {
            return FindControlRecursive(this, ID);
        }
        /// <summary>
        /// retrieve a Control based on it's name regardless of case or hungarian notation
        /// </summary>
        /// <param name="Root"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static Control FindControlRecursive(Control Root, string ID)
        {
            if (VerifyControlName(Root, ID) == true)
                return Root;
            foreach (Control Ctl in Root.Controls)
            {
                Control FoundCtl = FindControlRecursive(Ctl, ID);
                if (FoundCtl != null)
                    if (VerifyControlName(FoundCtl, ID) == true)
                        return FoundCtl;
            }
            return null;
        }
        private static bool VerifyControlName(Control ctl, string ID)
        {
            bool ret = false;

            if (ctl.ID != null)
            {
                string k = ctl.ID;
                int pos = k.Length - ID.Length;
                if (k.Length >= ID.Length)
                    if (k.Substring(pos).ToUpper() == ID.ToUpper())
                    {
                        while (k.Substring(0, 1) != k.Substring(0, 1).ToUpper())
                        {
                            k = k.Substring(1);
                        }
                        if (k.ToUpper() == ID.ToUpper())
                            ret = true;
                    }
            }
            return ret;
        }

        /// <summary>
        /// creates dynamic javascript to bind elements named like the fields in the dataset.  elements can have hungarian notation, but the first letter of the fieldname must be capitalized
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="ds"></param>
        protected void LoadComboAndElements(string spName, DataSet ds)
        {
            if (!IsPostBack)
                LoadCombo(spName);

            LoadElements(ds);
            /*
            string Elements = "";
            Elements = LoadComboAndElements_A_Elements(ds) + LoadComboAndElements_B_Combo(spName);
            RegisterJavascript(Elements, "sdbElements");
            LoadJavascript("_sdbLoadData", "sdbLoadElements");
            */
        }
        /// <summary>
        /// Load the combo boxes without calling a load elements procedure
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="OnlyLoadCombo"></param>
        protected void LoadComboAndElements(string spName, bool OnlyLoadCombo)
        {
            if (!IsPostBack)
                LoadCombo(spName);
        }
        /// <summary>
        /// creates a dynamic redirect script to run client side
        /// </summary>
        /// <param name="url"></param>
        /// <param name="target"></param>
        protected void Redirect(string url, string target)
        {
            string script = LoadJavascript("Redirect.js").Replace("</script>", "openTarget ('" + url + "','" + target + "');\n</script>");
            this.RegisterJavascript(script, "redirect");
        }
        /// <summary>
        /// creates a dynamic redirect script to run client side
        /// </summary>
        /// <param name="url"></param>
        protected void Redirect(string url)
        {
            string script = LoadJavascript("Redirect").Replace("</script>", "\nopenTarget ('" + url + "',null);\n</script>");
            this.RegisterJavascript(script, "redirect");
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
            {
                Random r = new Random();
                int i = r.Next();

                RegisterClientScriptBlock(RegisterName + i.ToString(), "\n" + script + "\n");
            }
            else
                throw new Exception("Duplicate Script Registration: " + RegisterName);
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
            string ret="";

            FileName = GetFilePath(FileName, "script/javascript/", "js");//Common.Properties.RootPath + @"Script\Javascript\" + FileName + ".js";
            System.IO.StreamReader sr = new System.IO.StreamReader(FileName);
            ret = "<script language=javascript>\n\t" + sr.ReadToEnd() + "\n</script>";
            sr.Close();

            return ret;
        }
        private string GetFilePath(string FileName, string DefaultPath, string Extension)
        {
            FileName = FileName.Replace("/", @"\");
            //try to find the path if it isn't specified.
            if (FileName.IndexOf("." + Extension) == -1)
                FileName = FileName + "." + Extension;
            
            //if this is a full path don't add the root info
            if (FileName.IndexOf(@"\\") == -1 & FileName.IndexOf(":") == -1)
            {
                //if there is no partial path info then add the default
                if (FileName.Substring(0, 1) != @"\" & FileName.IndexOf("/") == -1)
                    FileName = DefaultPath + @"\" + FileName;
                else //take the root indicator off.
                    FileName = FileName.Substring(1);

                if (System.IO.File.Exists(SDB.Common.Properties.Root + FileName))
                    FileName = SDB.Common.Properties.Root + FileName;
                else
                {
                    if (System.IO.File.Exists(SDB.Common.Properties.RootPath + FileName))
                        FileName = SDB.Common.Properties.RootPath + FileName;
                }
            }
            return FileName;            
        }
        private void ShowDialog(string Text)
        {
            ShowDialog(Text, "", "");
        }
        private void ShowDialog(string Text, string YesButtonText, string NoButtonText)
        {

            Text = Text.Replace(@"\", @"\\");
            Text = Text.Replace("\t", "\\t");
            Text = Text.Replace("\r\n", "\\n");
            Text = Text.Replace("\n\r", "\\n");
            Text = Text.Replace("\r", "\\r");
            Text = Text.Replace("<br>", "\\n");
            Text = Text.Replace("<BR>", "\\n");
            Text = Text.Replace("&nbsp;", " ");
            Text = Text.Replace("\n", "\\n");
            Text = Text.Replace("\"", "\\\"");

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
