using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SDB.Web.GridView
{
    public class GridViewControl : ITemplate
{
    #region data members

    ListItemType ItemType;
    string FieldName;
    string InfoType;

    #endregion

    #region constructor

    public GridViewControl(ListItemType item_type, string field_name, string info_type)
    {
        ItemType = item_type;
        FieldName = field_name;
        InfoType = info_type;
    }

    #endregion

    #region Methods

    public void InstantiateIn(System.Web.UI.Control Container)
    {
        switch (ItemType)
        {
            case ListItemType.Header:
                Literal header_ltrl = new Literal();
                header_ltrl.Text = "<b>" + FieldName + "</b>";
                Container.Controls.Add(header_ltrl);
                break;
            case ListItemType.Item:
                switch (InfoType)
                {
                    case "Command":
                        ImageButton edit_button = new ImageButton();
                        edit_button.ID = "edit_button";
                        edit_button.ImageUrl = "~/script/template/DynamicGrid/edit.gif";
                        edit_button.CommandName = "Edit";
                        edit_button.Click += new ImageClickEventHandler(edit_button_Click);
                        edit_button.ToolTip = "Edit";
                        Container.Controls.Add(edit_button);

                        ImageButton delete_button = new ImageButton();
                        delete_button.ID = "delete_button";
                        delete_button.ImageUrl = "~/script/template/DynamicGrid/delete.gif";
                        delete_button.CommandName = "Delete";
                        delete_button.ToolTip = "Delete";
                        delete_button.OnClientClick = "return confirm('Are you sure to delete the record?')";
                        Container.Controls.Add(delete_button);

                        /* Similarly add button for insert.
                         * It is important to know when 'insert' button is added 
                         * its CommandName is set to "Edit"  like that of 'edit' button 
                         * only because we want the GridView enter into Edit mode, 
                         * and this time we also want the text boxes for corresponding fields empty*/
                        ImageButton insert_button = new ImageButton();
                        insert_button.ID = "insert_button";
                        insert_button.ImageUrl = "~/script/template/DynamicGrid/insert.gif";
                        insert_button.CommandName = "Edit";
                        insert_button.ToolTip = "Insert";
                        insert_button.Click += new ImageClickEventHandler(insert_button_Click);
                        Container.Controls.Add(insert_button);

                        break;

                    default:
                        Label field_lbl = new Label();
                        field_lbl.ID = FieldName;
                        field_lbl.Text = string.Empty; //we will bind it later through 'OnDataBinding' event
                        field_lbl.DataBinding += new EventHandler(OnDataBinding);
                        Container.Controls.Add(field_lbl);
                        break;

                }
                break;
            case ListItemType.EditItem:
                if (InfoType == "Command")
                {
                    ImageButton update_button = new ImageButton();
                    update_button.ID = "update_button";
                    update_button.CommandName = "Update";
                    update_button.ImageUrl = "~/script/template/DynamicGrid/update.gif";
                    if ((int)new Page().Session["InsertFlag"] == 1)
                        update_button.ToolTip = "Add";
                    else
                        update_button.ToolTip = "Update";
                    update_button.OnClientClick = "return confirm('Are you sure to update the record?')";
                    Container.Controls.Add(update_button);

                    ImageButton cancel_button = new ImageButton();
                    cancel_button.ImageUrl = "~/script/template/DynamicGrid/cancel.gif";
                    cancel_button.ID = "cancel_button";
                    cancel_button.CommandName = "Cancel";
                    cancel_button.ToolTip = "Cancel";
                    Container.Controls.Add(cancel_button);

                }
                else// for other 'non-command' i.e. the key and non key fields, bind textboxes with corresponding field values
                {
                    TextBox field_txtbox = new TextBox();
                    field_txtbox.ID = FieldName;
                    field_txtbox.Text = string.Empty;
                    // if Inert is intended no need to bind it with text..keep them empty
                    if ((int)new Page().Session["InsertFlag"] == 0)
                        field_txtbox.DataBinding += new EventHandler(OnDataBinding);
                    Container.Controls.Add(field_txtbox);

                }
                break;

        }

    }

    #endregion

    #region Event Handlers

    //just sets the insert flag ON so that we ll be able to decide in OnRowUpdating event whether to insert or update
    protected void insert_button_Click(Object sender, EventArgs e)
    {
        new Page().Session["InsertFlag"] = 1;
    }
    //just sets the insert flag OFF so that we ll be able to decide in OnRowUpdating event whether to insert or update 
    protected void edit_button_Click(Object sender, EventArgs e)
    {
        new Page().Session["InsertFlag"] = 0;
    }

    private void OnDataBinding(object sender, EventArgs e)
    {

        object bound_value_obj = null;
        Control ctrl = (Control)sender;
        IDataItemContainer data_item_container = (IDataItemContainer)ctrl.NamingContainer;
        bound_value_obj = DataBinder.Eval(data_item_container.DataItem, FieldName);

        switch (ItemType)
        {
            case ListItemType.Item:
                Label field_ltrl = (Label)sender;
                field_ltrl.Text = bound_value_obj.ToString();

                break;
            case ListItemType.EditItem:
                TextBox field_txtbox = (TextBox)sender;
                field_txtbox.Text = bound_value_obj.ToString();

                break;
        }


    }
    #endregion
}

}
