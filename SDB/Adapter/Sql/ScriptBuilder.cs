using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Data;
using System.Globalization;

namespace SDB.Adapter.SQL
{
    public class ScriptBuilder
    {
        private string objectName;
        public DataSet ds;
        public ScriptBuilder(string ObjectName)
        {
            objectName = ObjectName;
            SDB.Adapter.SQL.DB db = new SDB.Adapter.SQL.DB();
            ds = db.Fill("_spFieldDef","Procname=" + objectName);
        }
        private string str(DataRow row, string key)
        {
            return SDB.Common.Functions.GetRowVal(row, key);
        }
        public string getStoredProc()
        {
            string ret = getStoredProcTemplate();

            //assumes 1st field is PK
            string primaryKey = Common.Functions.GetRowVal(ds.Tables[0].Rows[0], "name");
            StringBuilder declareStatement = new StringBuilder();
            StringBuilder initializeStatement = new StringBuilder();
            StringBuilder insertStatement = new StringBuilder();
            StringBuilder insertStatement2 = new StringBuilder();
            StringBuilder updateStatement = new StringBuilder();
            //no delete stringbuilder needed.
            StringBuilder selectDetStatement = new StringBuilder();
            StringBuilder selectSumStatement = new StringBuilder();



            foreach (DataRow row in ds.Tables[0].Rows)
            {
                string key = str(row, "name");
                string type = str(row, "type");

                //use quotes and lengths where appropriate for varchars and datetimes
                string len = "(" + str(row, "length") + ")";
                string quote = "'";

                if (type.IndexOf("varchar") == -1 && type.IndexOf("date") == -1)
                {
                    len = "";
                    quote = "";
                }

                declareStatement.Append(",@" + key + "\t\t" + type + len + "\t\t=null\r\t");
                initializeStatement.Append(",@" + key + "\t\t=null\t\t--" + quote + "<@" + key + ">" + quote + "--\r\t");
                insertStatement.Append("," + key + "\r\t\t\t");
                insertStatement2.Append(",@" + key + "\r\t\t\t");
                updateStatement.Append("," + key + "\t\t=@" + key + "\r\t\t\t");
                selectDetStatement.Append("," + key + "\r\t\t");
                selectSumStatement.Append("," + key + "\r\t\t");


            }
            ret = ret.Replace("~PrimaryKey~", primaryKey);
            ret = ret.Replace("~ObjectName~", objectName);
            ret = ret.Replace("~DeclareStatment~", declareStatement.ToString().Substring(1));
            ret = ret.Replace("~InitializeStatement~", initializeStatement.ToString().Substring(1));
            ret = ret.Replace("~InsertStatement~", insertStatement.ToString().Substring(1));
            ret = ret.Replace("~InsertStatement2~", insertStatement2.ToString().Substring(1));
            //remove the PK from the update statement
            ret = ret.Replace("~UpdateStatement~", updateStatement.ToString().Substring(updateStatement.ToString().IndexOf("\r\t\t\t,") + 5));
            ret = ret.Replace("~SelectDetStatement~", selectDetStatement.ToString().Substring(1));
            ret = ret.Replace("~SelectSumStatement~", selectSumStatement.ToString().Substring(1));
            return ret;
        }
        public string getCodeBehind()
        {
            return
 @"    Inherits SDB.Web.WebContentPage

Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If Not IsPostBack Then LoadData()
        Catch ex As Exception
            Message(ex)
        End Try
    End Sub

    Protected Sub btnSubmit_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSubmit.Click
        Try
            LoadData()
        Catch ex As Exception
            Message(ex)
        End Try
    End Sub
    Sub LoadData()
" + "SubmitScript(\"" + objectName + "CRUD\")" + @"'(Note: without path info /script/sql/ is assumed)
    End Sub";
        }
        public string getAspx()
        {
            StringBuilder sb = new StringBuilder();

            string html = "\r</td></tr><tr><th>\t~key~\r</th><td>\t\t\t<asp:TextBox ID='txt~key~' runat='server'></asp:TextBox>";

             foreach (DataRow row in ds.Tables[0].Rows)
            {
                string key = SDB.Common.Functions.ToProperCase (str(row, "Name")).Replace(" ", "");
                //string key = StrConv(str(row, "name"), VbStrConv.ProperCase).Replace(" ", "");//just in case

                sb.Append(html.Replace("~key~",key));
                //s.Append("\r\t<tr>\r\t\t<th>\r\t\t\t\t" + key + "\r\t\t</th>\r\t\t<td>\r\t\t\t\t<asp:TextBox ID='" + key + "' runat='server'></asp:TextBox>\r\t\t</td>\r");
                //s.Append("\r\t<tr><th>\t" + key + "\r\t</th><td>\t<asp:TextBox ID='" + key + "' runat='server'></asp:TextBox>\r\t</td></tr>\r");
            }
            sb=new StringBuilder("\r<tr><th>\t\t" + sb.ToString().Substring(19) + "\t\t\t\t\t\t\t\t\t\t\r</td></tr>\r</table>");
 //           sb.Append("\r<%--/alt drag\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\r\t(tabs added to ensure a clean copy when alt dragging)--%>\r");
            return
                "<%--\rtemplate (tip: hold the alt key & drag over fieldname/text boxes to remove from template table" 
                +"\r--%>"
                +"\r<asp:LinkButton ID='cmdSubmit' runat='server'>Save</asp:LinkButton><br/>\r<table>"
                + sb.ToString() + "\r"
                + "<%--\r/template\r--%>\r";
        }
        public string getSqlScript()
        {
            string ret =getSqlScriptTemplate();
            
            //assumes 1st field is PK
            string primaryKey=Common.Functions.GetRowVal(ds.Tables[0].Rows[0],"name");
            StringBuilder declareStatement=new StringBuilder();
            StringBuilder initializeStatement = new StringBuilder();
            StringBuilder insertStatement = new StringBuilder();
            StringBuilder insertStatement2 = new StringBuilder();
            StringBuilder updateStatement = new StringBuilder();
            //no delete stringbuilder needed.
            StringBuilder selectDetStatement = new StringBuilder();
            StringBuilder selectSumStatement = new StringBuilder();



            foreach (DataRow row in ds.Tables[0].Rows)
            {
                string key=str(row,"name");
                string type=str(row,"type");
                
                //use quotes and lengths where appropriate for varchars and datetimes
                string len="(" + str(row,"length") + ")";
                string quote="'";

                if (type.IndexOf("varchar")==-1 && type.IndexOf("date")==-1)
                {
                    len="";
                    quote="";
                }
                
                declareStatement.Append(",@" + key + "\t\t" + type + len + "\r\t");
                initializeStatement.Append(",@" + key + "\t\t=" + quote + "<@" + key + ">" + quote + "\t\t\t--null--\r\t");
                insertStatement.Append("," + key + "\r\t\t\t");
                insertStatement2.Append(",@" + key + "\r\t\t\t");
                updateStatement.Append("," + key + "\t\t=@" + key + "\r\t\t\t");
                selectDetStatement.Append("," + key + "\r\t\t");
                selectSumStatement.Append("," + key + "\r\t\t");


            }
            ret = ret.Replace("~PrimaryKey~", primaryKey);
            ret = ret.Replace("~ObjectName~", objectName);
            ret = ret.Replace("~DeclareStatment~", declareStatement.ToString().Substring(1));
            ret = ret.Replace("~InitializeStatement~", initializeStatement.ToString().Substring(1));
            ret = ret.Replace("~InsertStatement~", insertStatement.ToString().Substring(1));
            ret = ret.Replace("~InsertStatement2~", insertStatement2.ToString().Substring(1));
            //remove the PK from the update statement
            ret = ret.Replace("~UpdateStatement~", updateStatement.ToString().Substring(updateStatement.ToString().IndexOf("\r\t\t\t,") + 5));
            ret = ret.Replace("~SelectDetStatement~", selectDetStatement.ToString().Substring(1));
            ret = ret.Replace("~SelectSumStatement~", selectSumStatement.ToString().Substring(1));
            return ret;
        }

        /// <summary>
        /// returns the sql script template
        /// </summary>
        /// <returns>went back and forth about putting it in an external file, decided to do it this way</returns>
        private string getSqlScriptTemplate()
        {
            return
@"--~ObjectName~CRUD.sql
--Edit History:
--Date  Initials Change Description
--yymmdd  ------- --------------------------------------------
--null   xxx     Initial Creation
--
--Description: 
--

--Changelog at bottom

declare
	 ~DeclareStatment~
	,@Debug					tinyint
	,@Submit				varchar(10)

--This Procedure returns the summary/detail record view as well as performing all CRUD operations for the table
begin
select 
	 ~InitializeStatement~
	,@Submit					='<@Submit>'
	,@Debug						=0
	
if @Submit is not null
begin
if @Debug=1 print 'Submit is not null'
set nocount on
	--insert SQL goes here
	if @~PrimaryKey~ is null  --Insert Data Here
  	begin
        if @Debug=1 print 'Insert'
		--exec @~PrimaryKey~=_NewID '~ObjectName~' --note this is for sdb controlled sequence
		Insert into ~ObjectName~ (
			 ~InsertStatement~
		)	    
		Select 
			 ~InsertStatement2~
		
		select @~PrimaryKey~=@@Identity --not needed if using sdb sequence     
end
else
begin
	if @Submit='Delete'  --Delete Data Here
	begin
        if @Debug=1 print 'Delete'
		delete from ~ObjectName~ where ~PrimaryKey~=@~PrimaryKey~
   	end
   	else
	begin --Save Data Here
        if @Debug=1 print 'Update'
        Update  ~ObjectName~ set 
		     ~UpdateStatement~
        Where ~PrimaryKey~=@~PrimaryKey~
   	end 
end
set nocount off
end
 
---now attempt to pull the data, determine if this is a detail or summary view.
if @~PrimaryKey~ is not null --detail query
begin
    if @Debug=1 print 'Detail Cursor'
    Select
         ~SelectDetStatement~
    From ~ObjectName~ 
    where ~PrimaryKey~=@~PrimaryKey~
end
else
begin
    if @Debug=1 print 'Summary Cursor'
    Select top 500 --NOTE: REMOVE THE TOP STATEMENT for developer safety net only.
	     ~SelectSumStatement~
    from ~ObjectName~
    --where --NOTE: insert limiting criteria such as a @Criteria parameter of some sort
end
end
";
        }
        private string getStoredProcTemplate()
        {
            return
@"Print '   sp~ObjectName~'
_spDropObject 'sp~ObjectName','Stored Procedure'
GO
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO
--sp~ObjectName~
--Edit History:
--Date  Initials Change Description
--yymmdd  ------- --------------------------------------------
--null   xxx     Initial Creation
--
--Description: 
--

--Changelog at bottom
CREATE PROCEDURE [dbo].[sp~ObjectName~] (
--declare
	 ~DeclareStatment~
	,@Debug					tinyint		=null
	,@Submit				varchar(10))=null
AS
begin
/* for testing
select 
	 ~InitializeStatement~
	,@Submit					='<@Submit>'
	,@Debug						=0
*/	
if @Submit is not null
begin
if @Debug=1 print 'Submit is not null'
set nocount on
	--insert SQL goes here
	if @~PrimaryKey~ is null  --Insert Data Here
  	begin
        if @Debug=1 print 'Insert'
		--exec @~PrimaryKey~=_NewID '~ObjectName~' --note this is for sdb controlled sequence
		Insert into ~ObjectName~ (
			 ~InsertStatement~
		)	    
		Select 
			 ~InsertStatement2~
		
		select @~PrimaryKey~=@@Identity --not needed if using sdb sequence     
end
else
begin
	if @Submit='Delete'  --Delete Data Here
	begin
        if @Debug=1 print 'Delete'
		delete from ~ObjectName~ where ~PrimaryKey~=@~PrimaryKey~
   	end
   	else
	begin --Save Data Here
        if @Debug=1 print 'Update'
        Update  ~ObjectName~ set 
		     ~UpdateStatement~
        Where ~PrimaryKey~=@~PrimaryKey~
   	end 
end
set nocount off
end
 
---now attempt to pull the data, determine if this is a detail or summary view.
if @~PrimaryKey~ is not null --detail query
begin
    if @Debug=1 print 'Detail Cursor'
    Select
         ~SelectDetStatement~
    From ~ObjectName~ 
    where ~PrimaryKey~=@~PrimaryKey~
end
else
begin
    if @Debug=1 print 'Summary Cursor'
    Select top 500 --NOTE: REMOVE THE TOP STATEMENT for developer safety net only.
	     ~SelectSumStatement~
    from ~ObjectName~
    --where --NOTE: insert limiting criteria such as a @Criteria parameter of some sort
end
end

GO



";
        }
    }
}
