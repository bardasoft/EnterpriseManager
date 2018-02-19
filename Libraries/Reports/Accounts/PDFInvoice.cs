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
 *  File: PDFInvoice.cs
 *
 *  Purpose:  
 *
 *  Date        Name                Reason
 *  
 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
using System;
using System.IO;
using System.Threading;
using System.Globalization;

using iTextSharp.text;
using iTextSharp.text.pdf;

using Languages;
using Library.BOL.Invoices;
using Library.BOL.Coupons;

#pragma warning disable IDE0005
#pragma warning disable IDE0017
#pragma warning disable IDE0028

namespace Reports.Accounts
{
    public class PDFInvoice : BaseReport
    {
        #region Private Members

        private PdfWriter _pdfWriter;
        private string _logo;
        private string _address;
        private string _paymentReceived;
        private string _orderPreparedBy;
        private int _orderPreparedAlignment;
        private bool _multipleInvoices = false;
        private string _invoicePrefix;

        #endregion Private Members

        #region Constructors / Destructors

        public PDFInvoice(Invoice invoice, string logo, string footer, 
            string address, string vatRegNumber,
            string culture, bool hideVAT, bool invoiceShowProductDiscount,
            string paymentReceived, string orderPreparedBy, int orderPreparedAlignment,
            string invoicePrefix)
            : base(UniqueFileName("Invoice", true))
        {
            _logo = logo;
            _address = address;
            _paymentReceived = paymentReceived;
            _orderPreparedBy = orderPreparedBy;
            _orderPreparedAlignment = orderPreparedAlignment;
            _multipleInvoices = false;
            _invoicePrefix = invoicePrefix;

            string currentCulture = SetCurrentCulture(culture);
            try
            {
                CreateDocument(invoice, logo, address, vatRegNumber, hideVAT, invoiceShowProductDiscount, footer);
            }
            finally
            {
                SetCurrentCulture(currentCulture);
            }
        }

        public PDFInvoice(Invoices invoices, string logo, string footer, 
            string address, string vatRegNumber,
            string culture, bool hideVAT, bool invoiceShowProductDiscount,
            string paymentReceived, string orderPreparedBy, int orderPreparedAlignment,
            string invoicePrefix)
            : base(UniqueFileName("Invoice", true))
        {
            _logo = logo;
            _address = address;
            _paymentReceived = paymentReceived;
            _orderPreparedBy = orderPreparedBy;
            _orderPreparedAlignment = orderPreparedAlignment;
            _multipleInvoices = true;
            _invoicePrefix = invoicePrefix;

            string currentCulture = SetCurrentCulture(culture);

            try
            {
                Document myDocument = new Document(PageSize.A4);

                try
                {
                    _pdfWriter = PdfWriter.GetInstance(myDocument, new FileStream(FileName, FileMode.Create));

                    _pdfWriter.PageEvent = new InvoiceHeaderFooter(this, null, vatRegNumber, true, footer);

                    myDocument.Open();
                    int InvCount = invoices.Count;
                    int CurrInv = 1;

                    foreach (Invoice inv in invoices)
                    {

                        if (inv.Version < 8)
                        {
                            PrintHeader(myDocument, logo);

                            PrintInvoiceHeader(myDocument, inv);
                            ParagraphAdd(myDocument);

                            PrintInvoiceAddress(myDocument, inv);
                            ParagraphAdd(myDocument);
                        }
                        else
                        {
                            PrintInvoiceHeaderEx(myDocument, inv, 1);
                        }

                        if (inv.Version < 8)
                        {
                            PrintInvoiceItems(myDocument, inv, hideVAT, invoiceShowProductDiscount);
                        }
                        else
                        {
                            PrintInvoiceItemsEx(myDocument, inv, hideVAT, invoiceShowProductDiscount);
                        }

                        ParagraphAdd(myDocument);

                        Coupon cpn = Coupons.ValidateCoupon(inv.CouponName);

                        if (cpn != null)
                            PrintPromotionCode(myDocument, cpn);

                        if (inv.Version >= 6 && inv.Version < 8)
                            PrintNotes(myDocument, inv);

                        if (inv.Version < 8)
                            PrintFooter(myDocument, address);

                        if (CurrInv < InvCount)
                            myDocument.NewPage();

                        CurrInv++;
                    }
                }
                catch (DocumentException de)
                {
                    Console.Error.WriteLine(de.Message);
                }
                catch (IOException ioe)
                {
                    Console.Error.WriteLine(ioe.Message);
                }
                finally
                {
                    myDocument.Close();
                    myDocument.Dispose();
                    myDocument = null;
                }
            }
            finally
            {
                SetCurrentCulture(currentCulture);
            }
        }

        #endregion Constructors / Destructors

        #region Private Methods

        /// <summary>
        /// Set's the culture for the POS
        /// </summary>
        private string SetCurrentCulture(string culture)
        {
            string Result = Thread.CurrentThread.CurrentUICulture.Name;

            //set ui and thread culture 
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

            return (Result);
        }

        private void CreateDocument(Invoice invoice, string logo, 
            string address, string vatRegNumber, bool hideVAT, bool invoiceShowProductDiscount,
            string footer)
        {
            Document myDocument = new Document(PageSize.A4);
            try
            {
                _pdfWriter = PdfWriter.GetInstance(myDocument, new FileStream(FileName, FileMode.Create));

                if (invoice.Version > 7)
                {
                    _pdfWriter.PageEvent = new InvoiceHeaderFooter(this, invoice, vatRegNumber, true, footer);
                }

                myDocument.Open();

                if (invoice.Version < 8)
                {
                    PrintHeader(myDocument, logo);

                    PrintInvoiceHeader(myDocument, invoice);
                    ParagraphAdd(myDocument);

                    PrintInvoiceAddress(myDocument, invoice);
                    ParagraphAdd(myDocument);
                }
                else
                {
                    PrintInvoiceHeaderEx(myDocument, invoice, 1);
                }

                if (invoice.Version < 8)
                {
                    PrintInvoiceItems(myDocument, invoice, hideVAT, invoiceShowProductDiscount);

                    Coupon cpn = Coupons.ValidateCoupon(invoice.CouponName);

                    if (cpn != null)
                    {
                        ParagraphAdd(myDocument);
                        PrintPromotionCode(myDocument, cpn);
                    }
                }
                else
                {
                    PrintInvoiceItemsEx(myDocument, invoice, hideVAT, invoiceShowProductDiscount);
                }

                if (invoice.Version >= 6 && invoice.Version < 8)
                    PrintNotes(myDocument, invoice);

                if (invoice.Version < 8)
                    PrintFooter(myDocument, address);
            }
            catch (DocumentException de)
            {
                Console.Error.WriteLine(de.Message);
            }
            catch (IOException ioe)
            {
                Console.Error.WriteLine(ioe.Message);
            }
            finally
            {
                myDocument.Close();
                myDocument.Dispose();
                myDocument = null;
            }
        }

        private void PrintHeader(Document document, string header)
        {
            Image jpg;

            if (header.Contains(":\\") && File.Exists(header))
            {
                jpg = Image.GetInstance(header);

                document.Add(jpg);
            }
        }

        private void PrintFooter(Document document, string footer)
        {
            string FooterText = footer;
            Paragraph p = new Paragraph(FooterText, FontText);
            p.Alignment = 1;

            document.Add(p);
        }

        private void PrintPromotionCode(Document document, Coupon coupon)
        {
            if (coupon != null && coupon.VoucherType == Library.Enums.InvoiceVoucherType.Footprint)
            {
                Paragraph p = new Paragraph(String.Format("{0}: {1}", LanguageStrings.PromotionCode, coupon.Name), FontText);
                document.Add(p);
            }
        }
        
        private void PrintNotes(Document document, Invoice invoice)
        {
            if (!String.IsNullOrEmpty(invoice.Notes))
            {
                Paragraph p = new Paragraph(String.Format("{1}\r\n{0}", invoice.Notes, LanguageStrings.Notes), FontText);
                document.Add(p);
            }
        }

        private void PrintInvoicePageFooter(Document document, int page)
        {
            PdfPTable tblInvHeader = new PdfPTable(9);
            tblInvHeader.HorizontalAlignment = 0;
            tblInvHeader.WidthPercentage = 100f;
            tblInvHeader.SpacingAfter = 5f;

            tblInvHeader.AddCell(NewCell(new Phrase(LanguageStrings.Quantity, FontTextBoldSmall), 1, false, 2));
            tblInvHeader.AddCell(NewCell(new Phrase(LanguageStrings.Item, FontTextBoldSmall), 4, false));
            tblInvHeader.AddCell(NewCell(new Phrase(LanguageStrings.Price, FontTextBoldSmall), 1, false, 2));
            tblInvHeader.AddCell(NewCell(new Phrase(LanguageStrings.InvoiceNet, FontTextBoldSmall), 1, false, 2));
            tblInvHeader.AddCell(NewCell(new Phrase(LanguageStrings.InvoiceVATPercent, FontTextBoldSmall), 1, false, 2));
            tblInvHeader.AddCell(NewCell(new Phrase(LanguageStrings.InvoiceVAT, FontTextBoldSmall), 1, false, 2));
            document.Add(tblInvHeader);
        }

        private void PrintInvoiceHeader(Document document, Invoice inv)
        {
            document.Add(new Paragraph(String.Format("{1}{0}", inv.DisplayID, LanguageStrings.InvoicePrefix), FontTextBoldLarge));

            Phrase p = new Phrase();
            p.Add(new Chunk(String.Format("\n{0}: ", LanguageStrings.Date), FontTextBoldLarge));
            p.Add(new Chunk(inv.PurchaseDateTime.ToString("d"), FontTextLarge));
            p.Add(new Chunk("\n"));
            Paragraph para = new Paragraph();
            para.Add(p);
            document.Add(para);
        }

        private void PrintInvoiceAddress(Document document, Invoice inv)
        {
            string Address = inv.User.Address(false);
            string AddressShip = inv.Address(false);
            bool Same = Address == AddressShip;

            PdfPTable tblAddr = new PdfPTable(Same ? 1 : 2);
            tblAddr.HorizontalAlignment = 0;
            tblAddr.WidthPercentage = 100f;

            PdfPCell cAddr = new PdfPCell(new Phrase(LanguageStrings.Address, FontTextBoldLarge));
            PdfPCell cAddrShip;

            BorderRemove(ref cAddr);
            tblAddr.AddCell(cAddr);

            if (!Same)
            {
                cAddrShip = new PdfPCell(new Phrase(LanguageStrings.DeliveryAddress, FontTextBoldLarge));
                BorderRemove(ref cAddrShip);
                tblAddr.AddCell(cAddrShip);
            }

            cAddr = new PdfPCell(new Phrase(inv.User.Address(false), FontText));
            BorderRemove(ref cAddr);
            tblAddr.AddCell(cAddr);

            if (!Same)
            {
                cAddrShip = new PdfPCell(new Phrase(inv.Address(false), FontText));
                BorderRemove(ref cAddrShip);
                tblAddr.AddCell(cAddrShip);
            }

            document.Add(tblAddr);

            
            Phrase Addr = new Phrase();
            Addr.Add(new Chunk(String.Format("\n{0}: ", LanguageStrings.Telephone), FontTextBold));
            Addr.Add(new Chunk(inv.User.Telephone, FontText));
            Addr.Add(new Chunk(String.Format("\n{0}: ", LanguageStrings.Email), FontTextBold));
            Addr.Add(new Chunk(inv.User.Email.ToLower(), FontText));
            Paragraph para = new Paragraph();
            para.Add(Addr);
            document.Add(para);
        }

        private void PrintInvoiceHeaderEx(Document document, Invoice inv, int pageNumber)
        {
            PdfPTable tblHeader = new PdfPTable(2);
            tblHeader.HorizontalAlignment = 0;
            tblHeader.WidthPercentage = 100f;

            PdfPTable tblAddressLines = new PdfPTable(1);
            tblAddressLines.HorizontalAlignment = 0;
            tblAddressLines.WidthPercentage = 100f;

            PdfPCell cellAddress = new PdfPCell();
            string[] lines = _address.Split('\n');

            foreach (string s in lines)
            {
                PdfPCell cellLine = new PdfPCell(new Phrase(s, FontTextSmall));
                BorderRemove(ref cellLine);
                tblAddressLines.AddCell(cellLine);
            }

            cellAddress.AddElement(tblAddressLines);

            BorderRemove(ref cellAddress);

            Image jpg;
            PdfPCell cellLogo = new PdfPCell();

            if (_logo.Contains(":\\") && File.Exists(_logo))
            {
                jpg = Image.GetInstance(_logo);

                jpg.ScaleAbsoluteHeight(jpg.Height);
                jpg.Alignment = Element.ALIGN_LEFT;
                cellLogo.AddElement(jpg);
            }

            cellLogo.HorizontalAlignment = Element.ALIGN_RIGHT;
            cellLogo.VerticalAlignment = Element.ALIGN_TOP;
            BorderRemove(ref cellLogo);
            tblHeader.AddCell(cellLogo);
            tblHeader.AddCell(cellAddress);
            document.Add(tblHeader);
            ParagraphAdd(document);

            string Address = inv.User.Address(false);
            string AddressShip = inv.Address(false);
            bool Same = Address == AddressShip;

            PdfPTable tblAddr = new PdfPTable(Same ? 1 : 2);
            tblAddr.HorizontalAlignment = 0;
            tblAddr.WidthPercentage = 100f;

            PdfPCell cAddr = new PdfPCell(new Phrase(LanguageStrings.Address, FontTextBoldSmall));
            PdfPCell cAddrShip;

            BorderRemove(ref cAddr);
            tblAddr.AddCell(cAddr);

            Phrase contactDetails = new Phrase();
            contactDetails.Add(new Chunk(String.Format("\n{0}: ", LanguageStrings.Telephone), FontTextBoldSmall));
            contactDetails.Add(new Chunk(inv.User.Telephone, FontTextSmall));
            contactDetails.Add(new Chunk(String.Format("\n{0}: ", LanguageStrings.Email), FontTextBoldSmall));
            contactDetails.Add(new Chunk(inv.User.Email.ToLower(), FontTextSmall));

            PdfPCell cContact = new PdfPCell(contactDetails);
            cContact.Colspan = 2;
            BorderRemove(ref cContact);

            if (!Same)
            {
                cAddrShip = new PdfPCell(new Phrase(LanguageStrings.DeliveryAddress, FontTextBoldSmall));
                BorderRemove(ref cAddrShip);
                tblAddr.AddCell(cAddrShip);
            }

            cAddr = new PdfPCell(new Phrase(inv.User.Address(false), FontTextSmall));
            BorderRemove(ref cAddr);
            tblAddr.AddCell(cAddr);

            if (!Same)
            {
                cAddrShip = new PdfPCell(new Phrase(inv.Address(false), FontTextSmall));
                BorderRemove(ref cAddrShip);
                tblAddr.AddCell(cAddrShip);
            }

            PdfPTable userDetails = TableAdd(null, tblAddr);

            ExtendedBorderFix borderFix = new ExtendedBorderFix(14, 6);

            PdfPTable tableInvoiceDetails = new PdfPTable(2);
            tableInvoiceDetails.WidthPercentage = 100f;
            tableInvoiceDetails.HorizontalAlignment = 2;

            tableInvoiceDetails.AddCell(NewCell(new Phrase(LanguageStrings.InvoiceNumber, FontTextBoldSmall), 1, 0,
                Borders.Bottom | Borders.Right, 3.5f, 2f, borderFix));

            tableInvoiceDetails.AddCell(NewCell(new Phrase(String.Format("{0}{1}", _invoicePrefix, inv.ID), FontTextSmall), 1, 0,
                Borders.Bottom | Borders.Left, 3.5f, 2f, borderFix));

            tableInvoiceDetails.AddCell(NewCell(new Phrase(LanguageStrings.InvoiceDate, FontTextBoldSmall), 1, 0,
                Borders.Bottom | Borders.Right, 3.5f, 2f, borderFix));

            tableInvoiceDetails.AddCell(NewCell(new Phrase(inv.PurchaseDateTime.ToShortDateString(), FontTextSmall), 1, 0,
                Borders.Bottom | Borders.Left, 3.5f, 2f, borderFix));

            tableInvoiceDetails.AddCell(NewCell(new Phrase(LanguageStrings.InvoiceOrderNumber, FontTextBoldSmall), 1, 0,
                Borders.Bottom | Borders.Right, 3.5f, 2f, borderFix));

            tableInvoiceDetails.AddCell(NewCell(new Phrase(inv.OrderID.ToString(), FontTextSmall), 1, 0,
                Borders.Bottom | Borders.Left, 3.5f, 2f, borderFix));

            tableInvoiceDetails.AddCell(NewCell(new Phrase(LanguageStrings.InvoiceAccountRef, FontTextBoldSmall), 1, 0,
                Borders.Top | Borders.Right, 3.5f, 2f, borderFix));

            tableInvoiceDetails.AddCell(NewCell(new Phrase(inv.User.ID.ToString(), FontTextSmall), 1, 0,
                Borders.Top | Borders.Left, 3.5f, 2f, borderFix));

            PdfPTable invoiceDetails = TableAdd(null, tableInvoiceDetails);
            
            PdfPTable tableHeader = new PdfPTable(5);
            tableHeader.WidthPercentage = 100f;

            PdfPCell cellHeader = new PdfPCell(userDetails);
            cellHeader.Colspan = 2;
            cellHeader.Rowspan = 2;
            cellHeader.HorizontalAlignment = 0;
            BorderRemove(ref cellHeader);
            tableHeader.AddCell(cellHeader);
            
            cellHeader = new PdfPCell(new Phrase());
            BorderRemove(ref cellHeader);
            tableHeader.AddCell(cellHeader);

            tableHeader.AddCell(NewCell(new Phrase(LanguageStrings.InvoiceInvoice), _multipleInvoices ? 1 : 2, 0, Borders.None, 3.5f, 2f));

            string page = _multipleInvoices ? String.Format(LanguageStrings.InvoicePage, pageNumber) : String.Empty;
            tableHeader.AddCell(NewCell(new Phrase(page), 1, 0, Borders.None, 3.5f, 2f));

            if (_multipleInvoices)
                tableHeader.AddCell(NewCell(new Phrase(""), 1, 0, Borders.None, 3.5f, 2f));


            cellHeader = new PdfPCell(invoiceDetails);
            cellHeader.Colspan = 2;
            cellHeader.HorizontalAlignment = 2;
            BorderRemove(ref cellHeader);
            tableHeader.AddCell(cellHeader);

            document.Add(tableHeader);
        }

        private void PrintItemHeader(Document document, int page)
        {
            PdfPTable tblInvHeader = new PdfPTable(10);
            tblInvHeader.HorizontalAlignment = 2;
            tblInvHeader.WidthPercentage = 100f;
            tblInvHeader.SpacingAfter = 2f;
            tblInvHeader.SpacingBefore = 2f;

            tblInvHeader.AddCell(NewCell(new Phrase(LanguageStrings.Quantity, FontTextVariable(9, true)), 1, false));
            tblInvHeader.AddCell(NewCell(new Phrase(LanguageStrings.Item, FontTextVariable(9, true)), 4, false));
            tblInvHeader.AddCell(NewCell(new Phrase(LanguageStrings.Price, FontTextVariable(9, true)), 1, false, 2));
            tblInvHeader.AddCell(NewCell(new Phrase(LanguageStrings.InvoiceDiscountAmount, FontTextVariable(9, true)), 1, false, 2));
            tblInvHeader.AddCell(NewCell(new Phrase(LanguageStrings.InvoiceNet, FontTextVariable(9, true)), 1, false, 2));
            tblInvHeader.AddCell(NewCell(new Phrase(LanguageStrings.InvoiceVATPercent, FontTextVariable(9, true)), 1, false, 2));
            tblInvHeader.AddCell(NewCell(new Phrase(LanguageStrings.InvoiceVAT, FontTextVariable(9, true)), 1, false, 2));
            TableAdd(document, tblInvHeader, 2.5f, 4f);
        }

        private void PrintInvoiceItemsEx(Document document, Invoice inv, bool hideVAT,
            bool invoiceShowProductDiscount)
        {
            int page = 1;
            PrintItemHeader(document, page);

            PdfPTable tblItems = new PdfPTable(10);
            tblItems.HorizontalAlignment = 0;
            tblItems.WidthPercentage = 100f;
            tblItems.SpacingAfter = 5f;
            int iItemCount = 1;
            int pageItems = 0;
            decimal userDiscount = 0;
            decimal prodDiscount = 0;

            foreach (InvoiceItem item in inv.InvoiceItems)
            {
                if (iItemCount % 19 == 0 && inv.InvoiceItems.Count > iItemCount)
                {
                    page++;
                    document.Add(tblItems);
                    pageItems = 0;
                    document.NewPage();
                    PrintInvoiceHeaderEx(document, inv, page);
                    PrintItemHeader(document, page);

                    tblItems = new PdfPTable(10);
                    tblItems.HorizontalAlignment = 0;
                    tblItems.SpacingAfter = 5f;
                    tblItems.WidthPercentage = 100f;
                }

                pageItems++;
                string description = item.Description;

                if (invoiceShowProductDiscount && item.UserDiscount > 0.00m)
                {
                    description = String.Format(Languages.LanguageStrings.AppProductDiscount, description, item.UserDiscount);
                }

                tblItems.AddCell(NewCell(new Phrase(item.Quantity.ToString(), FontTextVariable(9)), 1, false, 1));
                tblItems.AddCell(NewCell(new Phrase(description, FontTextVariable(9)), 4, false));
                tblItems.AddCell(NewCell(new Phrase(item.Cost.ToString("0.0000"), FontTextVariable(9)), 1, false, 2));
                tblItems.AddCell(NewCell(new Phrase(item.DiscountCost.ToString("0.0000"), FontTextVariable(9)), 1, false, 2));
                tblItems.AddCell(NewCell(new Phrase(item.Price.ToString("0.0000"), FontTextVariable(9)), 1, false, 2));
                tblItems.AddCell(NewCell(new Phrase(inv.VATRate.ToString("0.00"), FontTextVariable(9)), 1, false, 2));
                tblItems.AddCell(NewCell(new Phrase(item.VatStr, FontTextVariable(9)), 1, false, 2));

                userDiscount += item.UserDiscountValue;
                prodDiscount += item.ProductDiscountValue;
                iItemCount++;
            }

            if (prodDiscount > 0)
            {
                tblItems.AddCell(NewCell(new Phrase(LanguageStrings.InvoiceTotalProductDiscount, FontTextVariable(9)), 6, false, 0));
                tblItems.AddCell(NewCell(new Phrase(prodDiscount.ToString("0.0000"), FontTextVariable(9)), 1, false, 2));
                tblItems.AddCell(NewCell(new Phrase(String.Empty, FontTextSmall), 3, false, 2));
            }

            if (userDiscount > 0)
            {
                tblItems.AddCell(NewCell(new Phrase(LanguageStrings.InvoiceTotalUserDiscount, FontTextVariable(9)), 6, false, 2));
                tblItems.AddCell(NewCell(new Phrase(userDiscount.ToString("0.0000"), FontTextVariable(9)), 1, false, 2));
                tblItems.AddCell(NewCell(new Phrase(String.Empty, FontTextSmall), 3, false, 2));
            }


            document.Add(tblItems);

            if (!String.IsNullOrEmpty(_paymentReceived))
            { 
                Paragraph pPaymentReceived = new Paragraph(String.Format("{0}", _paymentReceived), FontTextBold);
                pPaymentReceived.Alignment = 1;
                document.Add(pPaymentReceived);
            }

            if (!String.IsNullOrEmpty(_orderPreparedBy))
            {
                Paragraph orderPrepared = new Paragraph(String.Format("\r\n{0}", _orderPreparedBy), FontText);
                orderPrepared.Alignment = _orderPreparedAlignment;
                document.Add(orderPrepared);
                ParagraphAdd(document);
            }

            float remainingPageSpace = _pdfWriter.GetVerticalPosition(false) - document.BottomMargin;

            while (remainingPageSpace > 150f)
            {
                ParagraphAdd(document);
                remainingPageSpace = _pdfWriter.GetVerticalPosition(false) - document.BottomMargin;
            }

            PdfPTable tblFooter = new PdfPTable(2);
            tblFooter.HorizontalAlignment = 2;
            tblFooter.WidthPercentage = 100f;
            tblFooter.SpacingAfter = 0f;
            tblFooter.SpacingBefore = 0f;

            ExtendedBorderFix borderFix = new ExtendedBorderFix(6, 6);

            tblFooter.AddCell(NewCell(new Phrase(LanguageStrings.InvoiceSubTotal, FontTextSmall), 1, 0,
                Borders.Bottom | Borders.Right, 3.5f, 2f, borderFix));
            tblFooter.AddCell(NewCell(new Phrase(inv.InvoiceItems.SubTotalStr, FontTextSmall), 1, 2,
                Borders.Bottom | Borders.Left, 3.5f, 2f, borderFix));

            if (inv.Discount > 0 || !String.IsNullOrEmpty(inv.CouponName))
            {
                if (inv.VoucherType == Library.Enums.InvoiceVoucherType.Percent)
                {
                    tblFooter.AddCell(NewCell(new Phrase(String.Format("{1} ({2}) @ {0}%", inv.Discount,
                        Languages.LanguageStrings.Discount, inv.CouponName), FontTextSmall), 1, 0,
                        Borders.Bottom | Borders.Right, 3.5f, 2f, borderFix));
                    tblFooter.AddCell(NewCell(new Phrase(inv.DiscountAmountStr, FontTextSmall), 1, 2,
                        Borders.Bottom | Borders.Left, 3.5f, 2f, borderFix));
                }
                else
                {
                    tblFooter.AddCell(NewCell(new Phrase(String.Format("{0} ({1})", Languages.LanguageStrings.Discount,
                        inv.CouponName), FontTextSmall), 1, 0,
                        Borders.Bottom | Borders.Right, 3.5f, 2f, borderFix));
                    tblFooter.AddCell(NewCell(new Phrase(inv.DiscountAmountStr, FontTextSmall), 1, 2,
                        Borders.Bottom | Borders.Left, 3.5f, 2f, borderFix));
                }
            }
            else
            {
                tblFooter.AddCell(NewCell(new Phrase(LanguageStrings.Discount, FontTextSmall), 1, 0, Borders.Bottom | Borders.Right, 3.5f, 2f, borderFix));
                tblFooter.AddCell(NewCell(new Phrase("0.00", FontTextSmall), 1, 2, Borders.Bottom | Borders.Left, 3.5f, 2f, borderFix));
            }

            tblFooter.AddCell(NewCell(new Phrase(String.Format("{1} @ {0}%", inv.VATRate,
                Languages.LanguageStrings.VAT), FontTextSmall), 1, 0,
                Borders.Bottom | Borders.Right, 3.5f, 2f, borderFix));
            tblFooter.AddCell(NewCell(new Phrase(inv.VatAmountStr, FontTextSmall), 1, 2,
                Borders.Bottom | Borders.Left, 3.5f, 2f, borderFix));

            tblFooter.AddCell(NewCell(new Phrase(Languages.LanguageStrings.PostageAndPackaging, FontTextSmall), 1, 0,
                Borders.Bottom | Borders.Right, 3.5f, 2f, borderFix));
            tblFooter.AddCell(NewCell(new Phrase(inv.ShippingCostsStr, FontTextSmall), 1, 2,
                Borders.Bottom | Borders.Left, 3.5f, 2f, borderFix));

            tblFooter.AddCell(NewCell(new Phrase(Languages.LanguageStrings.Total, FontTextBoldSmall), 1, 0,
                Borders.Top | Borders.Right, 3.5f, 2f, borderFix));
            tblFooter.AddCell(NewCell(new Phrase(inv.TotalCostStr, FontTextBoldSmall), 1, 2,
                Borders.Top | Borders.Left, 3.5f, 2f, borderFix));

            PdfPTable tblInvFooter = TableAdd(null, tblFooter, 0f, 2.5f);

            PdfPTable tblNotes = new PdfPTable(1);
            tblNotes.HorizontalAlignment = 0;
            tblNotes.WidthPercentage = 100f;

            string notes = inv.Notes.Trim();

            Coupon cpn = Coupons.ValidateCoupon(inv.CouponName);

            if (cpn != null)

            if (cpn != null && cpn.VoucherType == Library.Enums.InvoiceVoucherType.Footprint)
            {
                if (String.IsNullOrEmpty(notes))
                {
                    notes = String.Format("{0}: {1}", LanguageStrings.PromotionCode, cpn.Name);
                }
                else
                {
                    notes += String.Format("\n{0}: {1}", LanguageStrings.PromotionCode, cpn.Name);
                }
            }

            float fontSize = 12;

            if (notes.Length > 800)
                fontSize = 5.5f;
            else if (notes.Length > 700)
                fontSize = 6.0f;
            else if (notes.Length > 600)
                fontSize = 6.5f;
            else if (notes.Length > 500)
                fontSize = 7f;
            else if (notes.Length > 400)
                fontSize = 7.5f;
            else if (notes.Length > 300)
                fontSize = 8f;
            else if (notes.Length > 250)
                fontSize = 8.5f;
            else if (notes.Length > 200)
                fontSize = 9.5f;

            tblNotes.AddCell(NewCell(new Phrase(notes, FontTextVariable(fontSize)), 1, 0, Borders.None, 3.5f, 2f));
            PdfPTable tblInvNotes = TableAdd(null, tblNotes, 0f, 2.5f);


            PdfPTable tblInvFooterDetails = new PdfPTable(6);
            tblInvFooterDetails.WidthPercentage = 100f;
            tblInvFooterDetails.HorizontalAlignment = 0;

            PdfPCell cell = new PdfPCell(tblInvNotes);
            cell.Colspan = 3;
            BorderRemove(ref cell);
            tblInvFooterDetails.AddCell(cell);

            cell = new PdfPCell(tblInvFooter);
            cell.Colspan = 3;
            BorderRemove(ref cell);
            tblInvFooterDetails.AddCell(cell);

            document.Add(tblInvFooterDetails);
        }

        private void PrintInvoiceItems(Document document, Invoice inv, bool hideVAT, 
            bool invoiceShowProductDiscount)
        {
            PdfPTable tblAddr = new PdfPTable(6);
            tblAddr.HorizontalAlignment = 0;
            tblAddr.WidthPercentage = 100f;

            tblAddr.AddCell(NewCell(new Phrase(Languages.LanguageStrings.Item, FontTextBold), 3, false));
            tblAddr.AddCell(NewCell(new Phrase(Languages.LanguageStrings.Price, FontTextBold), 1, false, 2));
            tblAddr.AddCell(NewCell(new Phrase(Languages.LanguageStrings.Quantity, FontTextBold), 1, false, 2));
            tblAddr.AddCell(NewCell(new Phrase(Languages.LanguageStrings.Total, FontTextBold), 1, false, 2));

            foreach (InvoiceItem item in inv.InvoiceItems)
            {
                string description = item.Description;

                if (invoiceShowProductDiscount && item.UserDiscount > 0.00m)
                {
                    description = String.Format(Languages.LanguageStrings.AppProductDiscount, description, item.UserDiscount);
                }

                tblAddr.AddCell(NewCell(new Phrase(description, FontText), 3, false));
                tblAddr.AddCell(NewCell(new Phrase(item.CostStr, FontText), 1, false, 2));
                tblAddr.AddCell(NewCell(new Phrase(item.Quantity.ToString(), FontText), 1, false, 2));
                tblAddr.AddCell(NewCell(new Phrase(item.PriceStr, FontText), 1, false, 2));
            }

            tblAddr.AddCell(NewCell(new Phrase(LanguageStrings.InvoiceSubTotal, FontText), 5, false, 2));
            tblAddr.AddCell(NewCell(new Phrase(inv.InvoiceItems.SubTotalStr, FontText), 1, false, 2));

            if (inv.Discount > 0 || !String.IsNullOrEmpty(inv.CouponName))
            {
                if (inv.VoucherType == Library.Enums.InvoiceVoucherType.Percent)
                {
                    tblAddr.AddCell(NewCell(new Phrase(String.Format("{1} ({2}) @ {0}%", inv.Discount, 
                        Languages.LanguageStrings.Discount, inv.CouponName), FontText), 5, false, 2));
                    tblAddr.AddCell(NewCell(new Phrase(inv.DiscountAmountStr, FontText), 1, false, 2));
                }
                else
                {
                    tblAddr.AddCell(NewCell(new Phrase(String.Format("{0} ({1})", 
                        Languages.LanguageStrings.Discount, inv.CouponName), FontText), 5, false, 2));
                    tblAddr.AddCell(NewCell(new Phrase(inv.DiscountAmountStr, FontText), 1, false, 2));
                }
            }

            if (!hideVAT)
            {
                tblAddr.AddCell(NewCell(new Phrase(String.Format("{1} @ {0}%", 
                    inv.VATRate, Languages.LanguageStrings.VAT), FontText), 5, false, 2));
                tblAddr.AddCell(NewCell(new Phrase(inv.VatAmountStr, FontText), 1, false, 2));
            }

            tblAddr.AddCell(NewCell(new Phrase(Languages.LanguageStrings.PostageAndPackaging, FontText), 5, false, 2));
            tblAddr.AddCell(NewCell(new Phrase(inv.ShippingCostsStr, FontText), 1, false, 2));

            tblAddr.AddCell(NewCell(new Phrase(Languages.LanguageStrings.Total, FontTextBold), 5, false, 2));
            tblAddr.AddCell(NewCell(new Phrase(inv.TotalCostStr, FontTextBold), 1, false, 2));

            document.Add(tblAddr);
        }

        #endregion Private Methods
    }
}
