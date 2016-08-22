using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Data;

namespace SDB.Web.GridView
{
    public class GridVals
    {
        NameValueCollection idx = new NameValueCollection();
        //Sub SetIdx(ByVal grd As Infragistics.WebUI.UltraWebGrid.UltraWebGrid)
        //    idx.Clear()
        //    For i As Integer = 0 To uwg.Columns.Count - 1
        //        idx.Add(uwg.Columns(i).BaseColumnName, i)
        //    Next
        //End Sub
        public void SetIndex(System.Web.UI.WebControls.GridView grd)
        {
            for (int i = 0; i == grd.Columns.Count; i++)
            {
                string fld = grd.Columns[i].SortExpression;
                idx.Add(fld, i.ToString());
            }
        }
        public void SetIndex(DataSet ds, int TableIndex)
        {
            idx.Clear();
            foreach (DataColumn col in ds.Tables[TableIndex].Columns)
            {
                idx.Add(col.ColumnName, col.Ordinal.ToString());
            }
        }
        public int GetIndex(string key)
        {
            return int.Parse(idx[key]);
        }
        public NameValueCollection GetIndex()
        {
            NameValueCollection hsh = new NameValueCollection(idx);
            foreach (string key in hsh.AllKeys)
            {
                hsh[key] = "";
            }
            return hsh;
        }
    }
}
