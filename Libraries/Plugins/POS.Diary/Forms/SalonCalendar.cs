﻿using System;
using System.Windows.Forms;

using Languages;

using Library;
using Library.BOL.Users;
using Library.BOL.Appointments;

using POS.Base.Classes;
using POS.Base.Forms;

namespace POS.Diary.Forms
{
    public partial class SalonCalendar : BaseForm
    {
        #region Private / Protected Members

        #region Global

        #endregion Global

        #endregion Private / Protected Members

        #region Constructors

        public SalonCalendar()
        {
            InitializeComponent();
        }

        public SalonCalendar(User user)
            : this()
        {
            AppController.ApplicationController.OnUserChanged += User_OnUserChanged;

            SetCurrentUser(AppController.ActiveUser);
        }

        #endregion Constructors

        #region Overridden Methods

        protected override void LanguageChanged(System.Globalization.CultureInfo culture)
        {
 	        this.Text = String.Format(StringConstants.PREFIX_AND_SUFFIX_HYPHEN, 
                LanguageStrings.AppDiarySalon, AppController.ActiveUser.UserName);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
            }
            finally
            {
                base.OnClosing(e);
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            HelpTopic = POS.Base.Classes.HelpTopics.Diary;
        }

        #endregion Overridden Methods

        #region Form

        private void SalonCalendar_Activated(object sender, EventArgs e)
        {
            //salonDiary1.ForceRefresh();
        }

        #endregion Form

        #region Events

        internal void RaisePayNow(Library.BOL.Appointments.Appointment appointment)
        {
            if (appointment.MasterAppointment > -1)
                appointment = Appointments.Get(appointment.MasterAppointment);

            if (PayNow != null)
                PayNow(this, new SalonDiary.Controls.AppointmentPayNowEventArgs(appointment));
        }

        public event SalonDiary.Controls.PayNowEventHandler PayNow;


        internal void RaiseStatusChanged()
        {
            if (AppointmentStatusChanged != null)
                AppointmentStatusChanged(this, EventArgs.Empty);
        }

        public event EventHandler AppointmentStatusChanged;

        #endregion Events

        #region Private Methods

        private void User_OnUserChanged(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                EventHandler eh = new EventHandler(User_OnUserChanged);
                this.Invoke(eh, new object[] { sender, e });
            }
            else
            {
                this.Text = String.Format(StringConstants.PREFIX_AND_SUFFIX_HYPHEN,
                    LanguageStrings.AppDiarySalon, AppController.ActiveUser.UserName);
                SetCurrentUser(AppController.ActiveUser);
            }
        }

        private void SetCurrentUser(User user)
        {
            Text = String.Format(StringConstants.PREFIX_AND_SUFFIX_HYPHEN, 
                LanguageStrings.AppDiarySalon, AppController.ActiveUser.UserName);
        }

        private void salonDiary1_PayNow(object sender, SalonDiary.Controls.AppointmentPayNowEventArgs e)
        {
            if (AppController.ActiveUser.HasPermissionCalendar(SecurityEnums.SecurityPermissionsCalendar.TakePayment))
                RaisePayNow(e.Appointment);
            else
                ShowError(LanguageStrings.AppTillTakePayments, LanguageStrings.AppPermissionTakePayment);
        }

        private void SalonCalendar_FormClosed(object sender, FormClosedEventArgs e)
        {
            AppController.ApplicationController.OnUserChanged -= User_OnUserChanged;
        }

        #endregion Private Methods
    }

}