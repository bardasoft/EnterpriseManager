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
 *  File: AuthoriseLeave.cs
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
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Languages;

namespace POS.Staff.Forms
{
    public partial class AuthoriseLeave : POS.Base.Forms.BaseForm
    {
        public AuthoriseLeave()
        {
            InitializeComponent();
            requestsAuthorise.AllStaff = true;
        }
        
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            HelpTopic = POS.Base.Classes.HelpTopics.StaffAuthoriseLeave;
        }

        protected override void LanguageChanged(System.Globalization.CultureInfo culture)
        {
            this.Text = LanguageStrings.AppStaffAuthoriseLeave;

            btnAuthorise.Text = LanguageStrings.AppMenuButtonAuthorise;
            btnClose.Text = LanguageStrings.AppMenuButtonClose;

            lblApprovePrompt.Text = LanguageStrings.AppStaffAuthoriseLeavePrompt;
        }

        private void btnApprove_Click(object sender, EventArgs e)
        {
            requestsAuthorise.AuthoriseAllChecked();
        }
    }
}
