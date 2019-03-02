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
 *  Copyright (c) 2010 - 2018 Simon Carter.  All Rights Reserved.
 *
 *  Product:  Enterprise Manager
 *  
 *  File: InvoicePartiallyDispatchedTrayStatus.cs
 *
 *  Purpose:  
 *
 *  Date        Name                Reason
 *  
 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
using System;
using Languages;

using SharedBase;

using POS.Base.Classes;
using POS.Base.Plugins;

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE1017 // Object Initialization can be simplified

namespace POS.Invoices
{
    public class InvoicePartiallyDispatchedTrayStatus : TrayNotification
    {
        #region Constructors

        public InvoicePartiallyDispatchedTrayStatus(BasePlugin parent)
            : base(parent)
        {
            this.UpdateFrequency = new TimeSpan(0, 0, 30);
            this.CanBlink = false;
            this.Position = 0;
        }

        #endregion Constructors

        #region Overridden Methods

        public override string HintText()
        {
            return (String.Empty);
        }

        public override string StatusText()
        {
            string Result = String.Empty;
            try
            {
                int _countInvoices = AppController.Administration.StatsInvoices(InvoiceStatistics.PartiallyDispatched);
                Result = StringConstants.PREFIX_AND_SPACE + LanguageStrings.AppInvoicesToProcessPartiallyDispatched1;

                if (_countInvoices == 1)
                    Result = StringConstants.PREFIX_AND_SPACE + LanguageStrings.AppInvoiceToProcessPartiallyDispatched;

                Result = String.Format(Result, _countInvoices);
            }
            catch (Exception err)
            {
                Result = LanguageStrings.AppErrorShowingStats +
                    StringConstants.SYMBOL_HYPHON_SPACES + err.Message;
            }

            return (Result);
        }

        public override string Name()
        {
            return (LanguageStrings.AppPluginTrayInvoicesPartiallyDispatched);
        }

        public override void DoubleClick()
        {
            ((InvoicesPluginModule)Parent).ShowInvoiceManager();
        }

        public override bool CanLoad()
        {
            return (true);
        }

        public override bool CanUnload()
        {
            return (true);
        }

        public override void Unload()
        {

        }

        public override void Load()
        {

        }

        #endregion Overridden Methods
    }
}
