﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Heavenskincare.WebsiteTemplate.Controls
{
    public partial class EditSettingsText : System.Web.UI.UserControl
    {
        private string _settingCode;
        private bool _isCulture;
        private bool _isNumber;
        private bool _isDateTime;
        private int _minValue;
        private int _maxValue;
        private int _defaultValue;
        private DateTime _defaultDateValue;
        private bool _isGlobal;

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public void UpdateSetting(string name, DateTime value, string description, string settingCode, bool isGlobal)
        {
            _isGlobal = isGlobal;
            _isNumber = false;
            _isCulture = false;
            _isDateTime = true;
            _defaultDateValue = value;
            string dateFormatted = Shared.Utilities.FormatDate(value, "en-GB");
            UpdateSetting(name, dateFormatted, description, settingCode, 200, false, false, 1, isGlobal);
        }

        public void UpdateSetting(string name, int value, string description, string settingCode, 
            int defaultValue, int minValue, int maxValue, bool isGlobal)
        {
            _isGlobal = isGlobal;
            _isNumber = true;
            _minValue = minValue;
            _maxValue = maxValue;
            _defaultValue = defaultValue;

            UpdateSetting(name, value.ToString(), description, settingCode, 100, false, false, 1, isGlobal);
        }

        public void UpdateSetting(string name, string value, string description, string settingCode,
            int width = 250, bool isPassword = false, bool isCulture = false, int numberOfLines = 1, bool isGlobal = false)
        {
            _isGlobal = isGlobal;
            _isCulture = isCulture;
            _settingCode = settingCode;
            settingDescription.InnerHtml = description;
            settingName.InnerText = name;

            string savedValue = Global.ConfigSettingGet(settingCode, value, isGlobal);

            settingText.Text = value;

            if (value != savedValue)
            {
                settingText.Text = savedValue;
            }

            settingText.Width = width;
            cbSetting.Visible = false;
            settingText.Visible = true;

            if (isPassword)
            {
                settingText.TextMode = TextBoxMode.Password;
            }
            else if (numberOfLines == 1)
            {
                settingText.TextMode = TextBoxMode.SingleLine;
            }
            else
            {
                settingText.TextMode = TextBoxMode.MultiLine;
                settingText.Rows = numberOfLines;
            }
        }

        public void UpdateSetting(string name, bool value, string description, string settingCode, int width = 250, bool isGlobal = false)
        {
            _isGlobal = isGlobal;
            _settingCode = settingCode;
            settingDescription.InnerHtml = description;
            settingName.InnerText = name;
            cbSetting.Checked = value;
            settingText.Visible = false;

            bool savedValue = Global.ConfigSettingGet(settingCode, value, isGlobal);

            if (value != savedValue)
            {
                cbSetting.Checked = savedValue;
            }

            cbSetting.Visible = true;
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (cbSetting.Visible)
            {
                Global.ConfigSettingSet(_settingCode, cbSetting.Checked.ToString(), _isGlobal);
            }
            else
            {
                if (_isCulture)
                {
                    // try and create a test culture
                    try
                    {
                        CultureInfo test = new CultureInfo(settingText.Text);
                    }
                    catch
                    {
                        settingText.Text = Global.ConfigSettingGet(_settingCode, Global.WebsiteCulture.Name, _isGlobal);
                    }
                }
                else if (_isNumber)
                {
                    if (_minValue != 0 && _maxValue != 0)
                    {
                        int value = Shared.Utilities.StrToInt(settingText.Text, _defaultValue);
                        settingText.Text = Shared.Utilities.CheckMinMax(value, _minValue, _maxValue).ToString();
                    }
                }
                else if (_isDateTime)
                {

                }

                Global.ConfigSettingSet(_settingCode, settingText.Text, _isGlobal);
            }
        }
    }
}