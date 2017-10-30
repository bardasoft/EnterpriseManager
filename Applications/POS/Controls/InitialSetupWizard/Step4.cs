﻿using System;

using Languages;
using PointOfSale.Classes;
using POS.Base.Classes;

using System.Globalization;

#pragma warning disable IDE1005 // Delegate invocation can be simplified
#pragma warning disable IDE0017 // object initialization can be simplified
#pragma warning disable IDE0029 // Null checks can be simplified
#pragma warning disable IDE1006 // naming rule violation

namespace PointOfSale.Controls.InitialSetupWizard
{
    public partial class Step4 : SharedControls.WizardBase.BaseWizardPage
    {
        #region Private Members

        private InitialSetupSettings _settings;

        #endregion Private Members

        #region Constructors

        public Step4()
        {
            InitializeComponent();
        }

        internal Step4(InitialSetupSettings settings)
            : this()
        {
            _settings = settings;
        }

        #endregion Constructors

        #region Overridden Methods

        public override void LanguageChanged(CultureInfo culture)
        {
            base.LanguageChanged(culture);

            lblHeader.Text = LanguageStrings.Website;

            lblFTPHost.Text = LanguageStrings.AppFTPHost;
            lblFTPPassword.Text = LanguageStrings.AppFTPPassword;
            lblFTPPort.Text = LanguageStrings.AppFTPPort;
            lblFTPUsername.Text = LanguageStrings.AppFTPUsername;
            lblWebsite.Text = LanguageStrings.Website;
            lblRootPath.Text = LanguageStrings.AppFTPRootPath;

            btnTestFTP.Text = LanguageStrings.AppMenuButtonTest;

        }

        public override void PageShown()
        {
            AppController.ActiveHelpTopic = HelpTopics.InitialStep4;
        }

        public override bool NextClicked()
        {
            try
            {
                if (String.IsNullOrEmpty(txtWebsite.Text))
                    return (true);

                if (_settings.Website == null)
                {
                    _settings.Website = Library.BOL.Websites.Websites.Create(txtWebsite.Text, txtFTPHost.Text,
                        (int)udFTPPort.Value, txtFTPUsername.Text, txtFTPPassword.Text, txtRootPath.Text);
                }
                else
                {
                    _settings.Website.URL = txtWebsite.Text;
                    _settings.Website.FtpHost = txtFTPHost.Text;
                    _settings.Website.FtpPassword = txtFTPPassword.Text;
                    _settings.Website.FtpPort = (int)udFTPPort.Value;
                    _settings.Website.FtpUserName = txtFTPUsername.Text;
                    _settings.Website.RootPath = txtRootPath.Text;
                }

                return (true);
            }
            catch (Exception err)
            {
                ShowError(LanguageStrings.AppError, err.Message);
                return (false);
            }
        }

        #endregion Overridden Methods

        #region Private Methods

        private void btnTestFTP_Click(object sender, EventArgs e)
        {
            Library.BOL.Websites.Website website = new Library.BOL.Websites.Website(-1,
                txtWebsite.Text,
                txtFTPHost.Text,
                (int)udFTPPort.Value,
                txtFTPUsername.Text,
                Library.BOL.Websites.Website.EncryptPassword(txtFTPPassword.Text),
                POS.Base.Classes.StringConstants.SYMBOL_FORWARD_SLASH);

            try
            {
                if (website.TestFTPConnection())
                {
                    _settings.FTPTested = true;
                    ShowInformation(LanguageStrings.AppFTPConnection, LanguageStrings.AppFTPConnectSuccess);
                }
            }
            catch (Exception err)
            {
                ShowError(LanguageStrings.AppError,
                    String.Format(LanguageStrings.AppFTPConnectionError, err.Message));
            }
        }

        #endregion Private Methods
    }
}