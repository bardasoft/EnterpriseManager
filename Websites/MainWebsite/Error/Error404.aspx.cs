﻿using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;

using Library.BOL.MissingLinks;
using Website.Library.Classes;

namespace Heavenskincare.WebsiteTemplate.Error
{
    public partial class Error404 : BaseWebForm
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            string page = GetFormValue("aspxerrorpath", Request.Path);

            btnSearch.ServerClick += btnSearch_ServerClick;

            if (!IsPostBack)
            {
                txtSearchTerms.Text = GetSearchString();
            }

            if (page == "/error/error404.aspx" | page.Contains("ScriptResource.axd"))
                page = String.Empty;

            
            if (!String.IsNullOrEmpty(page))
            {
                string redirectPage = CanRedirect(page);

                if (String.IsNullOrEmpty(redirectPage))
                {
                    Shared.Classes.UserSession session = GetUserSession();

                    if (!session.IsBot && !BaseWebApplication.IgnoreMissingPage(page))
                    {
                        Global.SendEMail(Global.SupportName, Global.SupportEMail,
                            String.Format("Missing Link - Heaven WebsiteTemplate - {0}", Global.DistributorWebsite),
                            String.Format("<p>The following page could not be found<p>{0}<P>Referrer: {1}<P>Query String: {3}<P>Session: {2}", 
                            page, session.InitialReferrer, Session.SessionID, Request.QueryString.ToString()));
                    }
                }
                else
                {
                    DoRedirect(redirectPage, true);
                }
            }
        }

        private string CanRedirect(string File)
        {
            string Result = "";

            string s = File;

            //get the missing page
            int pos = s.IndexOf("'") + 1;

            s = s.Substring(pos);
            int i = 0;

            while (s.Contains("'"))
            {
                s = s.Substring(0, s.Length - 1);

                //as a safety precaution break out the loop after thirty iterations
                i++;

                if (i > 30)
                    break;
            }

            MissingLink link = MissingLinks.MissingLinkGet(s);

            if (link != null)
                Result = link.RedirectLink;

            return (Result);
        }

        protected string GetSearchString()
        {
            if (Thread.CurrentThread.CurrentCulture.TextInfo.IsRightToLeft)
                return ("..." + Languages.LanguageStrings.Search);
            else
                return (Languages.LanguageStrings.Search + "...");
        }

        private void btnSearch_ServerClick(object sender, ImageClickEventArgs e)
        {
            if (!String.IsNullOrEmpty(txtSearchTerms.Text) & txtSearchTerms.Text != "Search...")
            {
                DoRedirect(String.Format("/Search/SearchResults.aspx?search={0}", txtSearchTerms.Text), true);
            }
        }
    }
}