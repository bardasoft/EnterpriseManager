/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 *  Enterprise Manager is distributed under the GNU General Public License version 3 and  
 *  is also available under alternative licenses negotiated directly with Simon Carter.  
 *  If you obtained Enterprise Manager under the GPL, then the GPL applies to all loadable 
 *  Enterprise Manager modules used on your system as well. The GPL (version 3) is 
 *  available at https://opensource.org/licenses/GPL-3.0
 *
 *  This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 *  without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 *  See the GNU General Public License for more details.
 *
 *  The Original Code was created by Simon Carter (s1cart3r@gmail.com)
 *
 *  Copyright (c) 2010 - 2019 Simon Carter.  All Rights Reserved.
 *
 *  Product:  Enterprise Manager
 *  
 *  File: Step3.cs
 *
 *  Purpose:  
 *
 *  Date        Name                Reason
 *  
 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using SalonDiary.Classes;
using Languages;
using SharedBase.BOL.Appointments;
using SharedBase.BOL.Therapists;
using SharedControls.WizardBase;

namespace SalonDiary.Controls.Wizards.WaitingListWizard
{
    public partial class Step3 : BaseWizardPage
    {
        #region Private Members

        private WaitingListWizardOptions _options;

        #endregion Private Members

        #region Constructors

        public Step3()
        {
            InitializeComponent();
        }
        
        public Step3(WaitingListWizardOptions options)
            : this()
        {
            _options = options;
            calPreferredDate.MinDate = DateTime.Now.Date;
        }

        #endregion Constructors

        #region Overridden Methods

        public override void LanguageChanged(System.Globalization.CultureInfo culture)
        {
            lblPreferredDate.Text = LanguageStrings.PreferredStartDate;
            lblPreferredStaff.Text = LanguageStrings.AppPreferredStaff;
            lblPreferredTime.Text = LanguageStrings.PreferredStartTime;
            lblNotes.Text = LanguageStrings.Notes;
        }

        public override void PageShown()
        {
            calPreferredDate.SetDate(_options.WaitingListItem.PreferredDate < DateTime.Now.Date ? DateTime.Now.Date : _options.WaitingListItem.PreferredDate);

            LoadTimes();
            LoadTherapists();

        }

        public override bool NextClicked()
        {
            _options.WaitingListItem.PreferredTime = (double)cmbPreferredTime.Items[cmbPreferredTime.SelectedIndex];
            Therapist ther = (Therapist)cmbStaffMembers.Items[cmbStaffMembers.SelectedIndex];
            _options.WaitingListItem.StaffID = ther.EmployeeID;
            _options.WaitingListItem.Notes = txtNotes.Text;
            _options.WaitingListItem.PreferredDate = calPreferredDate.SelectionStart;
            _options.WaitingListItem.Expires = DateTime.Now.AddDays(35);
            _options.WaitingListItem.LastReviewed = DateTime.Now;
            _options.WaitingListItem.ReviewedBy = _options.Diary.User.ID;
            
            return (true);
        }

        #endregion Overridden Methods

        #region Private Methods

        private void LoadTimes()
        {
            cmbPreferredTime.Items.Clear();
            cmbPreferredTime.Items.Add(0.0);

            for (Double t = 8.0; t <= 21.75; t = t + 0.25)
            {
                int idx = cmbPreferredTime.Items.Add(t);

                if (t == _options.WaitingListItem.PreferredTime)
                    cmbPreferredTime.SelectedIndex = idx;
            }

            if (cmbPreferredTime.SelectedIndex == -1)
                cmbPreferredTime.SelectedIndex = 0;
        }

        private void LoadTherapists()
        {
            cmbStaffMembers.Items.Clear();
            cmbStaffMembers.Items.Add(new Therapist(-1, LanguageStrings.AppAny));

            Therapists staffMembers = Therapists.Get();

            foreach (Therapist therapist in staffMembers)
            {
                // can the therapist do the treatments
                if (!therapist.CompareTreatments(_options.WaitingListItem.Treatments))
                    continue;

                int idx = cmbStaffMembers.Items.Add(therapist);

                if (therapist.EmployeeID == _options.WaitingListItem.StaffID)
                    cmbStaffMembers.SelectedIndex = idx;
            }

            if (cmbStaffMembers.SelectedIndex == -1)
                cmbStaffMembers.SelectedIndex = 0;
        }

        private void cmbPreferredTime_Format(object sender, ListControlConvertEventArgs e)
        {
            if ((double)e.ListItem == 0.0)
                e.Value = LanguageStrings.AppAny;
            else
                e.Value = Shared.Utilities.DoubleToTime((double)e.ListItem);
        }

        private void cmbStaffMembers_Format(object sender, ListControlConvertEventArgs e)
        {
            Therapist ther = (Therapist)e.ListItem;
            e.Value = ther.EmployeeName;
        }

        #endregion Private Methods
    }
}
