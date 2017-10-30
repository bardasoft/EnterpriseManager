﻿using System.Windows.Forms;

using Languages;
using Library.BOL.Users;
using POS.Base.Classes;

using POS.Staff.Classes;

#pragma warning disable IDE1005 // Delegate invocation can be simplified
#pragma warning disable IDE0017 // object initialization can be simplified
#pragma warning disable IDE0029 // Null checks can be simplified
#pragma warning disable IDE1006 // naming rule violation

namespace POS.Staff.Controls.Wizards.Expenses
{
    public partial class Step1 : SharedControls.WizardBase.BaseWizardPage
    {
        #region Private Members

        private ExpensesSettings _settings;

        #endregion Private Members

        #region Constructors

        public Step1()
        {
            InitializeComponent();
        }

        public Step1(ExpensesSettings settings)
            : this ()
        {
            _settings = settings;

            bool allStaff = AppController.ActiveUser.HasPermissionStaff(
                Library.SecurityEnums.SecurityPermissionsStaff.ApproveExpenses);
                
            foreach (User user in User.StaffMembers(false))
            {
                int idx = 0;

                if (allStaff)
                {
                    idx = cmbStaff.Items.Add(user);
                }
                else if (_settings.Edit)
                {
                    if (user.ID ==_settings.Expense.StaffId)
                    {
                        idx = cmbStaff.Items.Add(user);
                        cmbStaff.Enabled = false;
                        break;
                    }
                }
                else
                {
                    if (user.ID == AppController.ActiveUser.ID)
                    {
                        idx = cmbStaff.Items.Add(user);
                    }
                }

                if (user.ID == AppController.ActiveUser.ID)
                    cmbStaff.SelectedIndex = idx;
            }
        }

        #endregion Constructors

        #region Overridden Methods

        public override void LanguageChanged(System.Globalization.CultureInfo culture)
        {
            lblSelectNewStaffMember.Text = LanguageStrings.AppStaffSelect;
            POS.Base.Classes.AppController.ActiveHelpTopic = POS.Base.Classes.HelpTopics.StaffCreateExpensesStep1;
        }

        public override void PageShown()
        {
            AppController.ActiveHelpTopic = HelpTopics.StaffCreateExpensesStep1;

            if (!AppController.ActiveUser.HasPermissionStaff(
                Library.SecurityEnums.SecurityPermissionsStaff.ApproveExpenses) ||
                _settings.Edit)
            {
                MainWizardForm.SelectNextPage();
            }
        }

        public override bool NextClicked()
        {
            _settings.Employee = (User)cmbStaff.Items[cmbStaff.SelectedIndex];

            return (true);
        }

        #endregion Overridden Methods

        #region Private Methods

        private void cmbStaff_Format(object sender, ListControlConvertEventArgs e)
        {
            User user = (User)e.ListItem;
            e.Value = user.UserName;
        }

        #endregion Private Methods
    }
}