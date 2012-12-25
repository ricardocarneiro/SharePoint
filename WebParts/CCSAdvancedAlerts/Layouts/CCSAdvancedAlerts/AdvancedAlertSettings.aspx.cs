﻿using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Web.UI.WebControls;
using Microsoft.SharePoint.Utilities;
using System.Collections.Generic;
using System.Web.UI;
using System.Xml;

namespace CCSAdvancedAlerts.Layouts.CCSAdvancedAlerts
{


    public partial class AdvancedAlertSettings : LayoutsPageBase
    {
        private const string alertSettingsListName = "CCSAdvancedAlertsList";
        private SPList list = null;
        private bool resetControls;
        private AlertManager alertMngr;

        private AlertManager AlertMngr
        {
            get
            {
                try
                {
                    if (this.alertMngr == null)
                    {
                        this.alertMngr = new AlertManager(SPContext.Current.Site.Url);
                    }
                }
                catch 
                {
                    throw;
                }
                return alertMngr;
            }
        }

        //protected DataTable PropertyCollection = new DataTable();
        //public AdvancedAlertSettings()
        //{
        //    PropertyCollection.Columns.Add("FieldName", typeof(string));
        //    PropertyCollection.Columns.Add("ComparisionOperator", typeof(string));
        //    PropertyCollection.Columns.Add("StrValue", typeof(string));
        //}

        protected override void CreateChildControls()
        {
            if (this.List != null)
            {
                base.CreateChildControls();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                PopulateSites();
                populateStaticDropDowns();
                FillddlUserID();
            }

            //Alert based events
            this.btnAlertsave.Click += new EventHandler(btnAlertsave_Click);
            this.ddlSite.SelectedIndexChanged += new EventHandler(ddlSite_SelectedIndexChanged);
            this.ddlList.SelectedIndexChanged += new EventHandler(ddlList_SelectedIndexChanged);

            //Recipient related
            this.btnAddTO.Click += new EventHandler(btnAddTO_Click);
            this.btnAddCC.Click += new EventHandler(btnAddCC_Click);
            this.btnAddBCC.Click += new EventHandler(btnAddBCC_Click);

            //Template related
            this.btnAddToSubject.Click += new EventHandler(btnAddToSubject_Click);
            this.btnCopyToClipBoard.Click += new EventHandler(btnCopyToClipBoard_Click);

            this.btnTemplateAdd.Click += new EventHandler(btnTemplateAdd_Click);
            this.btnTemplateUpdate.Click += new EventHandler(btnTemplateUpdate_Click);
            this.btnTemplateCancel.Click += new EventHandler(btnTemplateCancel_Click);

            //AlertType
            this.rdImmediately.CheckedChanged += new EventHandler(rdImmediately_CheckedChanged);
            this.rdImmediateBusinessdays.CheckedChanged += new EventHandler(rdImmediateBusinessdays_CheckedChanged);
            this.rdDaily.CheckedChanged += new EventHandler(rdDaily_CheckedChanged);

            //Navigate Back
            this.btnOK.Click += new EventHandler(btnOK_Click);
            this.btnAlertcancel.Click += new EventHandler(btnAlertcancel_Click);
        }

        void rdDaily_CheckedChanged(object sender, EventArgs e)
        {
            pnSubDaily.Visible = rdDaily.Checked;
            pnSubImmediately.Visible = !rdDaily.Checked;
            
            //pnSubDaily
        }

        void rdImmediateBusinessdays_CheckedChanged(object sender, EventArgs e)
        {
            pnImmediateBusinessDays.Visible = rdImmediateBusinessdays.Checked;

            //pnImmediateBusinessDays
        }

        void rdImmediately_CheckedChanged(object sender, EventArgs e)
        {
             pnSubImmediately.Visible = rdImmediately.Checked;
             pnSubDaily.Visible = !rdImmediately.Checked;
            //pnSubImmediately
        }

        void btnAlertcancel_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            this.GoBack();
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            this.GoBack();
        }

        private void GoBack()
        {
            if (Context.Request["Source"] != null)
            {
                SPUtility.Redirect(Convert.ToString(Context.Request["Source"]), SPRedirectFlags.UseSource, Context);
            }
            else
            {
                string siteURL = SPContext.Current.Web.Site.Url;
                string serverRelativeURL = SPContext.Current.Web.Site.ServerRelativeUrl;
                string formURL = "";
                try
                {
                    if (Request.QueryString["Type"] == "edit")
                    {
                        formURL = SPContext.Current.List.Forms[PAGETYPE.PAGE_EDITFORM].ServerRelativeUrl;
                    }
                    else if (Request.QueryString["Type"] == "view")
                    {
                        formURL = SPContext.Current.List.Forms[PAGETYPE.PAGE_DISPLAYFORM].ServerRelativeUrl;
                    }
                    else if (Request.QueryString["Type"] == "RibbonButton" || Request.QueryString["Type"] == "EditControlBlockButton")
                    {
                        this.CloseModelDialog();
                        return;
                    }


                    if (!string.IsNullOrEmpty(serverRelativeURL) &&
                        !string.IsNullOrEmpty(siteURL) &&
                        !string.IsNullOrEmpty(formURL) &&
                        siteURL.EndsWith(serverRelativeURL) &&
                        formURL.StartsWith(serverRelativeURL))
                    {
                        siteURL = siteURL.Substring(0, siteURL.IndexOf(serverRelativeURL));
                    }

                    string url = Request.QueryString["ID"] != null ? siteURL + formURL + "?ID=" + Request.QueryString["ID"] : string.Empty;
                    if (!string.IsNullOrEmpty(url))
                    {
                        SPUtility.Redirect(url, SPRedirectFlags.Default, Context);
                    }
                    else
                    {
                        this.CloseModelDialog();
                        return;
                    }
                }
                catch { }
            }
        }

        private void CloseModelDialog()
        {
            Context.Response.Write("<script type='text/javascript'>window.frameElement.commitPopup();</script>");
            Context.Response.Flush();
            Context.Response.End();
        }

        protected void gvConditions_RowCancelEditing(object sender, GridViewCancelEditEventArgs e)
        {
            try
            {
                this.gvConditions.ShowFooter = true;
                this.gvConditions.EditIndex = -1;
                this.gvConditions.DataSource = this.Conditions;
                this.gvConditions.DataBind();
                this.EnsureConditionInsertRow();
            }
            catch (Exception exception)
            {
            }
        }

        protected void gvConditions_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                GridViewRow footerRow = null;
                string commandName = e.CommandName;
                if (commandName == null)
                {
                    return;
                }
                if (commandName != "EmptyDataTemplateInsert")
                {
                    if (commandName != "FooterInsert")
                    {
                        return;
                    }
                }
                else
                {
                    footerRow = this.gvConditions.Controls[0].Controls[0] as GridViewRow;
                }

                if (footerRow == null)
                    footerRow = this.gvConditions.FooterRow;

                if (footerRow != null)
                {
                    DropDownList ddlField = footerRow.FindControl("ddlConditionField") as DropDownList;
                    //DropDownList ddlWhen = footerRow.FindControl("ddlWhen") as DropDownList;
                    DropDownList ddlOperator = footerRow.FindControl("ddlConditionOperator") as DropDownList;
                    TextBox txtValue = footerRow.FindControl("txtConditionFieldValue") as TextBox;
                    if (((ddlField != null) && (ddlOperator != null)) && (txtValue != null))
                    {
                        this.AddUpdateCondition(ddlField, ddlOperator, txtValue, -1);
                    }
                }
            }
            catch (Exception exception)
            {
            }
        }

        protected void gvConditions_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                List<Condition> conditions = this.Conditions;
                if ((conditions != null) && (e.RowIndex < conditions.Count))
                {
                    conditions.RemoveAt(e.RowIndex);
                    this.Conditions = conditions;
                    this.gvConditions_RowCancelEditing(null, null);
                }
            }
            catch (Exception exception)
            {
            }
        }

        protected void gvConditions_RowEditing(object sender, GridViewEditEventArgs e)
        {
            try
            {
                this.gvConditions.ShowFooter = false;
                this.gvConditions.EditIndex = e.NewEditIndex;
                this.gvConditions.DataSource = this.Conditions;
                this.gvConditions.DataBind();
                GridViewRow parenControl = this.gvConditions.Rows[e.NewEditIndex];
                this.EnsureConditionInsertRow(parenControl);
                DropDownList ddlFields = parenControl.FindControl("ddlConditionField") as DropDownList;
                DropDownList ddlOps = parenControl.FindControl("ddlConditionOperator") as DropDownList;
                //DropDownList list3 = parenControl.FindControl("ddlWhen") as DropDownList;
                ddlFields.SelectedValue = this.Conditions[e.NewEditIndex].FieldName;
                ddlOps.SelectedValue = this.Conditions[e.NewEditIndex].ComparisionOperator.ToString();
                //list3.SelectedValue = this.Conditions[e.NewEditIndex].OnChange ? "AfterChange" : "Always";
            }
            catch (Exception exception)
            {
            }
        }

        protected void gvConditions_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                GridViewRow row = this.gvConditions.Rows[this.gvConditions.EditIndex];
                this.AddUpdateCondition(row.FindControl("ddlConditionField") as DropDownList, row.FindControl("ddlConditionOperator") as DropDownList, row.FindControl("txtConditionFieldValue") as TextBox, this.gvConditions.EditIndex);
                this.gvConditions_RowCancelEditing(sender, null);
            }
            catch (Exception exception)
            {
            }
        }

        private void AddUpdateCondition(DropDownList ddlField, DropDownList ddlOperator, TextBox txtValue, int editIndex)
        {
            if (this.Page.IsValid)
            {
                List<Condition> conditions = this.Conditions;
                if (conditions == null)
                {
                    conditions = new List<Condition>();
                }
                Condition condition2 = new Condition();
                condition2.FieldName = ddlField.SelectedValue;
                //condition2.OnChange = ddlWhen.SelectedValue != "Always";
                condition2.ComparisionOperator = (Operators)Enum.Parse(typeof(Operators), ddlOperator.SelectedValue);
                condition2.StrValue = txtValue.Text;
                Condition item = condition2;
                if ((editIndex == -1) || ((conditions.Count + 1) < editIndex))
                {
                    conditions.Add(item);
                }
                else
                {
                    conditions.Insert(editIndex, item);
                    conditions.RemoveAt(editIndex + 1);
                }
                this.Conditions = conditions;
                this.gvConditions.EditIndex = -1;
                //this.gvConditions.DataSource = this.Conditions;
                //this.gvConditions.DataBind();
                //this.EnsureConditionInsertRow();
            }
        }

        protected string GetValidOperatorValue(object operatorValue)
        {
            string strValue = Convert.ToString(operatorValue);
            return strValue;
        }

        #region Aletr related events

        void ddlSite_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                this.PopulateLists(this.ddlSite.SelectedValue);
            }
            catch
            {
            }
        }

        private void PopulateSites()
        {
            try
            {
                SPSite site = SPContext.Current.Site;
                if (site != null)
                {
                    SPWebCollection allWebs = site.AllWebs;
                    foreach (SPWeb web in allWebs)
                    {
                        ListItem newWebItem = new ListItem(web.Title, web.ID.ToString());
                        if (!this.ddlSite.Items.Contains(newWebItem))
                        {
                            this.ddlSite.Items.Add(newWebItem);
                        }

                    }

                    this.PopulateLists(this.ddlSite.SelectedValue);
                }

            }
            catch
            {
            }
        }

        private void PopulateLists(string webid)
        {
            try
            {
                SPListCollection allLists = SPContext.Current.Site.AllWebs[new Guid(webid)].Lists;
                if (allLists != null)
                {
                    foreach (SPList list in allLists)
                    {
                        ListItem newListItem = new ListItem(list.Title, list.ID.ToString());
                        if (!this.ddlList.Items.Contains(newListItem))
                        {
                            this.ddlList.Items.Add(newListItem);
                        }

                    }
                    ListChanged();
                }
            }
            catch
            {
            }
        }

        void ddlList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListChanged();
        }

        void ListChanged()
        {
            try
            {
                this.list = SPContext.Current.Site.AllWebs[new Guid(this.ddlSite.SelectedValue)].Lists[new Guid(ddlList.SelectedValue)];

                if (this.list != null)
                {
                    foreach (SPField field in this.list.Fields)
                    {
                        if (field.Type == SPFieldType.User)
                        {
                            ddlUsersInColumn.Items.Add(field.Title);
                        }

                        if (field.Type == SPFieldType.DateTime)
                        {
                            ddlDateColumn.Items.Add(field.Title);
                        }

                        lstPlaceHolders.Items.Add(field.Title);
                    }

                    this.Conditions = null;
                    //this.gvConditions.DataSource = this.Conditions;
                    //this.gvConditions.DataBind();
                    //this.EnsureConditionInsertRow();
                }
            }
            catch
            {
            }
        }

        void btnAddBCC_Click(object sender, EventArgs e)
        {
            AddAddress(txtBcc);
        }

        void btnAddCC_Click(object sender, EventArgs e)
        {
            AddAddress(txtCc);
        }

        void btnAddTO_Click(object sender, EventArgs e)
        {
            AddAddress(txtTo);
        }

        void AddAddress(TextBox txtAddressBox)
        {
            if (txtAddressBox != null)
            {
                string emailAddresses = string.Empty;
                if (rdCurrentUser.Checked)
                {
                    emailAddresses = SPContext.Current.Web.CurrentUser.Email;
                }
                else if (rdUsers.Checked)
                {
                    //string cC = string.Empty, ccEmail = string.Empty;
                    if (additionalUsers != null)
                    {
                        int resolvedEntitiesCount = additionalUsers.ResolvedEntities.Count;
                        if (resolvedEntitiesCount != 0)
                        {
                            for (int i = 0; i < resolvedEntitiesCount; i++)
                            {
                                try
                                {
                                    PickerEntity pEntity = (PickerEntity)additionalUsers.ResolvedEntities[i];
                                    if (pEntity != null &&
                                        !String.IsNullOrEmpty(Convert.ToString(pEntity.EntityData["Email"])))
                                    {

                                        if (!String.IsNullOrEmpty(emailAddresses))
                                        {
                                            emailAddresses = emailAddresses + ",";
                                        }
                                        emailAddresses =
                                            emailAddresses + Convert.ToString(pEntity.EntityData["Email"]);
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }


                else if (rdUsersincolumn.Checked)
                {

                }
                else if (rdEmailAddresses.Checked)
                {
                    emailAddresses = emailAddresses + "," + txtEmailAddresses;
                }

                txtAddressBox.Text += emailAddresses;
            }
        }

        void btnAlertsave_Click(object sender, EventArgs e)
        {
            try
            {
                Alert alert = new Alert();

                //Get the General Information
                alert.Title = txtTitle.Text;
                alert.WebId = ddlSite.SelectedValue;
                alert.ListId = ddlList.SelectedValue;


                //Get Recipient Section
                alert.ToAddress = txtTo.Text;
                alert.FromAdderss = txtFrom.Text;
                alert.CcAddress = txtCc.Text;
                alert.BccAddress = txtBcc.Text;


                //Event Type
                if (chkItemAdded.Checked)
                {
                    alert.AlertType.Add(AlertEventType.ItemAdded);
                }
                if (chkItemDeleted.Checked)
                {
                    alert.AlertType.Add(AlertEventType.ItemDeleted);
                }
                if (chkItemUpdated.Checked)
                {
                    alert.AlertType.Add(AlertEventType.ItemUpdated);
                }
                if (chkDateColumn.Checked)
                {
                    alert.AlertType.Add(AlertEventType.DateColumn);
                }


                //------------------------------------------------------------------
                //this.BlockedUsers = ;
                alert.DateColumnName = this.ddlDateColumn.SelectedValue;
                alert.PeriodType = (PeriodType)Enum.Parse(typeof(PeriodType), ddlPeriodType.SelectedValue);
                alert.PeriodPosition = (PeriodPosition)Enum.Parse(typeof(PeriodPosition), ddlPeriodPosition.SelectedValue); ;
                alert.Repeat = Convert.ToBoolean(chkRepeat.Checked);
                alert.RepeatType = (RepeatType)Enum.Parse(typeof(RepeatType), ddlRepeatType.SelectedValue);
                alert.ImmidiateAlways = Convert.ToBoolean(rdImmediately.Checked);
              
                //alert.BusinessStartHour = Convert.ToInt32(xmlDoc.DocumentElement.SelectSingleNode(XMLElementNames.ImmediateBusinessHoursStart).InnerText);
                alert.BusinessStartHour = 10;

                //alert.BusinessendtHour = Convert.ToInt32(xmlDoc.DocumentElement.SelectSingleNode(XMLElementNames.ImmediateBusinessHoursFinish).InnerText);
                alert.BusinessendtHour = 18;
                
                //alert.DailyBusinessDays = DesrializeDays(xmlDoc.DocumentElement.SelectSingleNode(XMLElementNames.DailyBusinessDays).InnerText);
                alert.DailyBusinessDays = new List<WeekDays>();
                if (chkDailySun.Checked)
                {
                    alert.DailyBusinessDays.Add(WeekDays.sun);
                }
                if(chkDailyMon.Checked)
                {
                    alert.DailyBusinessDays.Add(WeekDays.mon);
                }
                if (chkDailyTue.Checked)
                {
                    alert.DailyBusinessDays.Add(WeekDays.tue);
                }
                if (chkDailyWed.Checked)
                {
                    alert.DailyBusinessDays.Add(WeekDays.wed);
                }
                if (chkDailyThu.Checked)
                {
                    alert.DailyBusinessDays.Add(WeekDays.thu);
                }
                if (chkDailyFri.Checked)
                {
                    alert.DailyBusinessDays.Add(WeekDays.fri);
                }
                if (chkDailySat.Checked)
                {
                    alert.DailyBusinessDays.Add(WeekDays.sat);
                }
               
                
                //alert.ImmediateBusinessDays = DesrializeDays(xmlDoc.DocumentElement.SelectSingleNode(XMLElementNames.ImmediateBusinessDays).InnerText);
                alert.ImmediateBusinessDays = new List<WeekDays>();
                if (chkImmediateSun.Checked)
                {
                    alert.ImmediateBusinessDays.Add(WeekDays.sun);
                }
                if (chkImmediateMon.Checked)
                {
                    alert.ImmediateBusinessDays.Add(WeekDays.mon);
                }
                if (chkImmediateThu.Checked)
                {
                    alert.ImmediateBusinessDays.Add(WeekDays.tue);
                }
                if (chkImmediateWed.Checked)
                {
                    alert.ImmediateBusinessDays.Add(WeekDays.wed);
                }
                if (chkImmediateThu.Checked)
                {
                    alert.ImmediateBusinessDays.Add(WeekDays.thu);
                }
                if (chkImmediateFri.Checked)
                {
                    alert.ImmediateBusinessDays.Add(WeekDays.fri);
                }
                if (chkImmediateSat.Checked)
                {
                    alert.ImmediateBusinessDays.Add(WeekDays.sat);
                }
                
                
                //alert.CombineAlerts = Convert.ToBoolean(xmlDoc.DocumentElement.SelectSingleNode(XMLElementNames.CombineAlerts).InnerText);
                alert.CombineAlerts = true;

                //alert.SummaryMode = Convert.ToBoolean(xmlDoc.DocumentElement.SelectSingleNode(XMLElementNames.SummaryMode).InnerText);
                alert.SummaryMode = true;

                if (!string.IsNullOrEmpty(txtPeriodQty.Text))
                {
                    alert.PeriodQty = Convert.ToInt32(txtPeriodQty.Text);
                }
                else
                {
                    alert.PeriodQty = 0;
                }
                //------------------------------------------------------------------
                if (!string.IsNullOrEmpty(txtRepeatInterval.Text))
                {
                    alert.RepeatInterval = Convert.ToInt32(txtRepeatInterval.Text);
                }
                else
                {
                    alert.RepeatInterval = 0;
                }

                if (!string.IsNullOrEmpty(txtRepeatCount.Text))
                {
                    alert.RepeatCount = Convert.ToInt32(txtRepeatCount.Text);
                }
                else
                { alert.RepeatCount = 0;  }


                //when To Send
                if (rdDaily.Checked)
                { alert.SendType = SendType.Daily; }
                else if (rdImmediately.Checked)
                { alert.SendType = SendType.Immediate; }
                else if (rdWeekly.Checked)
                { alert.SendType = SendType.Weekely; }


                //Conditions
                alert.Conditions = this.Conditions;


                //Create new alert
                if (AlertManager.AddAlert(SPContext.Current.Site.RootWeb, alert))
                {
                    //Successfully added
                }

            }
            catch { }

        }

        private void EnsureConditionInsertRow()
        {
            List<Condition> dataSource = this.gvConditions.DataSource as List<Condition>;
            if (((dataSource == null) || (dataSource.Count == 0)) || (this.gvConditions.FooterRow == null))
            {
                this.EnsureConditionInsertRow(this.gvConditions.Controls[0].Controls[0]);
            }
            else
            {
                this.EnsureConditionInsertRow(this.gvConditions.FooterRow);
            }
        }

        private void EnsureConditionInsertRow(Control parenControl)
        {
            DropDownList ddlField = parenControl.FindControl("ddlConditionField") as DropDownList;
            DropDownList ddlOperator = parenControl.FindControl("ddlConditionOperator") as DropDownList;
            TextBox txtValue = parenControl.FindControl("txtConditionFieldValue") as TextBox;
            if (ddlOperator != null)
            {
                if (ddlOperator.Items.Count == 0)
                {
                    this.FillConditionField(ddlField, ddlOperator, txtValue);
                    this.FillOperatorField(ddlOperator);
                }
                else if (this.resetControls)
                {
                    this.FillConditionField(ddlField, ddlOperator, txtValue);
                }
            }
        }

        private void FillConditionField(DropDownList ddlField, DropDownList ddlOperator, TextBox txtValue)
        {
            ddlField.Items.Clear();
            if (this.list == null)
            {
                this.list = SPContext.Current.Site.AllWebs[new Guid(this.ddlSite.SelectedValue)].Lists[new Guid(ddlList.SelectedValue)];
            }

            if (this.list != null)
            {
                foreach (SPField field in this.list.Fields)
                {
                    if (field != null && !field.Hidden)
                    {
                        ListItem newFieldItem = new ListItem(field.Title, field.InternalName);
                        if (!ddlField.Items.Contains(newFieldItem) && ddlField.Items.FindByText(field.Title) == null)
                        {
                            ddlField.Items.Add(newFieldItem);
                        }
                    }
                }
            }
        }

        private void FillOperatorField(DropDownList ddlOperator)
        {
            ddlOperator.Items.Clear();
            ddlOperator.Items.Add(new ListItem("Equals", Operators.Eq.ToString()));
            ddlOperator.Items.Add(new ListItem("Not equals", Operators.Neq.ToString()));
            ddlOperator.Items.Add(new ListItem("Contains", Operators.Contains.ToString()));
            ddlOperator.Items.Add(new ListItem("Not contains", Operators.NotContains.ToString()));
            ddlOperator.Items.Add(new ListItem("Greater than", Operators.Gt.ToString()));
            ddlOperator.Items.Add(new ListItem("Greater than or equals", Operators.Geq.ToString()));
            ddlOperator.Items.Add(new ListItem("Less than", Operators.Lt.ToString()));
            ddlOperator.Items.Add(new ListItem("Less than or equals", Operators.Leq.ToString()));
            ddlOperator.Items.Add(new ListItem("Yes", Operators.Yes.ToString()));
            ddlOperator.Items.Add(new ListItem("No", Operators.No.ToString()));
        }

        protected string GetFieldName(string internalName)
        {
            if (this.List.Fields.ContainsField(internalName))
            {
                return this.List.Fields.GetFieldByInternalName(internalName).Title;
            }
            return "???";
        }

        internal List<Condition> Conditions
        {
            get
            {
                return (this.ViewState["Conditions"] as List<Condition>);
            }
            set
            {
                this.ViewState["Conditions"] = value;
                this.gvConditions.DataSource = value;
                this.gvConditions.DataBind();
                this.EnsureConditionInsertRow();
            }
        }

        public SPList List
        {
            get
            {
                if (this.list == null)
                {
                    if ((this.WebID == Guid.Empty) || (this.ListID == Guid.Empty))
                    {
                        return null;
                    }
                    if (this.WebID == SPContext.Current.Web.ID)
                    {
                        this.list = SPContext.Current.Web.Lists[this.ListID];
                    }
                    else
                    {
                        using (SPWeb web = SPContext.Current.Site.OpenWeb(this.WebID))
                        {
                            this.list = web.Lists[this.ListID];
                        }
                    }
                }
                return this.list;
            }
            set
            {
                this.list = value;
                if (this.list != null)
                {
                    this.WebID = this.list.ParentWeb.ID;
                    this.ListID = this.list.ID;
                }
                else
                {
                    this.WebID = Guid.Empty;
                    this.ListID = Guid.Empty;
                }
                this.resetControls = true;
                this.Conditions = null;
            }
        }

        private Guid ListID
        {
            get
            {
                if (this.ViewState["ListID"] == null)
                {
                    return Guid.Empty;
                }
                return (Guid)this.ViewState["ListID"];
            }
            set
            {
                this.ViewState["ListID"] = value;
            }
        }

        private Guid WebID
        {
            get
            {
                if (this.ViewState["WebID"] == null)
                {
                    return Guid.Empty;
                }
                return (Guid)this.ViewState["WebID"];
            }
            set
            {
                this.ViewState["WebID"] = value;
            }
        }

        #endregion

        #region OnStartUp
        
        void populateStaticDropDowns()
        {
            try
            {
                ddlPeriodType.Items.Clear();
                ddlPeriodType.Items.Add(new ListItem(PeriodType.Minutes.ToString(), PeriodType.Minutes.ToString()));
                ddlPeriodType.Items.Add(new ListItem(PeriodType.Hours.ToString(), PeriodType.Hours.ToString()));
                ddlPeriodType.Items.Add(new ListItem(PeriodType.Days.ToString(), PeriodType.Days.ToString()));
                ddlPeriodType.Items.Add(new ListItem(PeriodType.Weeks.ToString(), PeriodType.Weeks.ToString()));
                ddlPeriodType.Items.Add(new ListItem(PeriodType.Months.ToString(), PeriodType.Months.ToString()));
                ddlPeriodType.Items.Add(new ListItem(PeriodType.Years.ToString(), PeriodType.Years.ToString()));

                ddlRepeatType.Items.Clear();
                ddlRepeatType.Items.Add(new ListItem(PeriodType.Minutes.ToString(), PeriodType.Minutes.ToString()));
                ddlRepeatType.Items.Add(new ListItem(PeriodType.Hours.ToString(), PeriodType.Hours.ToString()));
                ddlRepeatType.Items.Add(new ListItem(PeriodType.Days.ToString(), PeriodType.Days.ToString()));
                ddlRepeatType.Items.Add(new ListItem(PeriodType.Weeks.ToString(), PeriodType.Weeks.ToString()));
                ddlRepeatType.Items.Add(new ListItem(PeriodType.Months.ToString(), PeriodType.Months.ToString()));
                ddlRepeatType.Items.Add(new ListItem(PeriodType.Years.ToString(), PeriodType.Years.ToString()));

                ddlPeriodPosition.Items.Clear();
                ddlPeriodPosition.Items.Add(new ListItem(PeriodPosition.After.ToString(), PeriodPosition.After.ToString()));
                ddlPeriodPosition.Items.Add(new ListItem(PeriodPosition.Before.ToString(), PeriodPosition.Before.ToString()));
             

                
            }
            catch { }
        }


        #endregion

        #region Grid to show All Alerts for the user
        protected void FillddlUserID()
        {
            SPUser currentUser = SPContext.Current.Web.CurrentUser;
            this.ddlUserID.Items.Add(new ListItem(currentUser.Name, currentUser.ID.ToString()));
            if (currentUser.IsSiteAdmin)
            {
                Dictionary<string, string> allAlerOwners = new Dictionary<string, string>();
                foreach (string key in allAlerOwners.Keys)
                {
                    if (key != currentUser.ID.ToString())
                    {
                        this.ddlUserID.Items.Add(new ListItem(key, allAlerOwners[key]));
                    }
                }
            }
        }

        protected void ddlUserID_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                this.gvAlerts.SelectedIndex = -1;
                this.gvAlerts.DataBind();
            }
            catch 
            {
               //Error ocurred getting elerts for the user
            }
        }

        protected void gvAlerts_PageIndexChanging(object sender, EventArgs e)
        {
            try
            {
                this.gvAlerts.SelectedIndex = -1;
                this.gvAlerts.DataBind();
            }
            catch 
            {  }
        }

        protected void gvAlerts_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                e.Cancel = true;
                int alertId = Convert.ToInt32(this.gvAlerts.DataKeys[e.RowIndex][0]);
                //this.alertMngr.DeleteAlert(alertId, this.TemplateDal);
                this.dsAlerts.DataBind();
                this.gvAlerts.DataBind();
            }
            catch 
            {
            }
        }

        protected void gvAlerts_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        #endregion



        #region Template Related events

        void btnCopyToClipBoard_Click(object sender, EventArgs e)
        {
            try
            {
                string copyText = lstPlaceHolders.SelectedItem.Text;
                System.Windows.Forms.Clipboard.SetText(copyText);
            }
            catch
            {
            }
            //lstPlaceHolders.SelectedItem.
        }
        void btnAddToSubject_Click(object sender, EventArgs e)
        {
            if (lstPlaceHolders.SelectedItem != null)
            {
                txtMailSubject.Text += " " + "[" + lstPlaceHolders.SelectedItem.Text + "]";
            }
        }
        void btnTemplateAdd_Click(object sender, EventArgs e)
        {
            try
            {
                SPList mailTemplateList = SPContext.Current.Site.RootWeb.Lists.TryGetList(ListAndFieldNames.MTListName);

                if (mailTemplateList != null)
                {
                    SPListItem listItem = mailTemplateList.AddItem();
                    listItem["Title"] = txtMailTemplateName.Text;
                    listItem[ListAndFieldNames.MTListMailSubjectFieldName] = txtMailSubject.Text;
                    listItem[ListAndFieldNames.MTListMailBodyFieldName] = txtBody.Text;
                    listItem[ListAndFieldNames.MTListInsertUpdatedFieldsFieldName] = chkIncludeUpdatedColumns.Checked;
                    listItem[ListAndFieldNames.MTListInsertAttachmentsFieldName] = chkInsertAttachments.Checked;
                    listItem[ListAndFieldNames.MTListHighLightUpdatedFieldsFieldName] = chkHighlightUpdatedColumns.Checked;
                    listItem[ListAndFieldNames.MTListOwnerFieldName] = SPContext.Current.Web.CurrentUser;


                    listItem.Update();
                }

            }
            catch { }
        }
        void btnTemplateUpdate_Click(object sender, EventArgs e)
        {


        }
        void btnTemplateCancel_Click(object sender, EventArgs e)
        {

        }

        #endregion
    }
}
