using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Exchange.WebServices.Data;

namespace EmailReceive.Aruna
{
    public partial class Form1 : Form
    {
        ExchangeService exchange = null;

        public Form1()
        {
            InitializeComponent();
            lstMsg.Clear();
            lstMsg.View = View.Details;
            lstMsg.Columns.Add("Tarih", 150);
            lstMsg.Columns.Add("Gönderen", 250);
            lstMsg.Columns.Add("Konu", 300);
            lstMsg.Columns.Add("Ekli Dosya", 50);
            lstMsg.Columns.Add("İçerik", 400);
            lstMsg.Columns.Add("Id", 50);
            lstMsg.FullRowSelect = true;
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            ConnectToExchangeServer();

            TimeSpan ts = new TimeSpan(0, -24, 0, 0);
            DateTime date = DateTime.Now.Add(ts);
            SearchFilter.IsGreaterThanOrEqualTo filter = new SearchFilter.IsGreaterThanOrEqualTo(ItemSchema.DateTimeReceived, date);

            if (exchange != null)
            {
                FindItemsResults<Item> findResults = exchange.FindItems(WellKnownFolderName.Inbox, filter, new ItemView(50));

                if (findResults != null)
                {
                    foreach (Item item in findResults)
                    {

                        EmailMessage message = EmailMessage.Bind(exchange, item.Id);
                        ListViewItem listitem = new ListViewItem(new[]
                        {
                        message.DateTimeReceived.ToString(), message.From.Name.ToString() + "(" + message.From.Address.ToString() + ")", message.Subject, ((message.HasAttachments) ? "Var" : "Yok"),HtmlToPlainText(message.Body), message.Id.ToString()
                    });
                        lstMsg.Items.Add(listitem);

                    }

                    if (findResults.Items.Count <= 0)
                    {
                        lstMsg.Items.Add("Mail Bulunamadı!!");

                    }
                }
                else
                {
                    MessageBox.Show("Hatalı sunucu!");
                }
            }
        }

        static string HtmlToPlainText(string html)
        {
            string buf;
            string block = "address|article|aside|blockquote|canvas|dd|div|dl|dt|" +
              "fieldset|figcaption|figure|footer|form|h\\d|header|hr|li|main|nav|" +
              "noscript|ol|output|p|pre|section|table|tfoot|ul|video";

            string patNestedBlock = $"(\\s*?</?({block})[^>]*?>)+\\s*";
            buf = Regex.Replace(html, patNestedBlock, "\n", RegexOptions.IgnoreCase);

            // Replace br tag to newline.
            buf = Regex.Replace(buf, @"<(br)[^>]*>", "\n", RegexOptions.IgnoreCase);

            // (Optional) remove styles and scripts.
            buf = Regex.Replace(buf, @"<(script|style)[^>]*?>.*?</\1>", "", RegexOptions.Singleline);

            // Remove all tags.
            buf = Regex.Replace(buf, @"<[^>]*(>|$)", "", RegexOptions.Multiline);

            // Replace HTML entities.
            buf = WebUtility.HtmlDecode(buf);
            return buf;
        }

        public void ConnectToExchangeServer()
        {

            lblMsg.Text = "Sunucuya bağlanıyor..";
            lblMsg.Refresh();
            try
            {
                exchange = new ExchangeService(ExchangeVersion.Exchange2007_SP1);
                exchange.Credentials = new WebCredentials(txtEmail.Text, txtSifre.Text, "AutodiscoverUrl");
                exchange.AutodiscoverUrl(txtEmail.Text);

                lblMsg.Text = "Bağlanılan sunucu : " + exchange.Url.Host;
                lblMsg.Refresh();

            }
            catch (Exception ex)
            {
                lblMsg.Text = "Sunucuya bağlanırken haata oluştu !!" + ex.Message;
                lblMsg.Refresh();
            }

        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (exchange != null)
            {
                if (lstMsg.Items.Count > 0)
                {
                    ListViewItem item = lstMsg.SelectedItems[0];

                    if (item != null)
                    {
                        string msgid = item.SubItems[5].Text.ToString();
                        EmailMessage message = EmailMessage.Bind(exchange, new ItemId(msgid));
                        if (message.HasAttachments && message.Attachments[0] is FileAttachment)
                        {
                            FileAttachment fileAttachment = message.Attachments[0] as FileAttachment;
                            //Change the below Path   
                            fileAttachment.Load(
                            "C:\\Users\\Serdar\\source\repos\\EmailReceive\\ReadMailFromExchangeServer" + fileAttachment.Name);
                            lblAttach.Text = "Attachment Downloaded : " + fileAttachment.Name;
                        }
                        else
                        {
                            MessageBox.Show("Ekli dosya bulunamadı!!");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Lütfen bir maili seçiniz!!");
                    }
                }
                else
                {
                    MessageBox.Show("Mail yüklenemedi!!");
                }

            }
            else
            {
                MessageBox.Show("Mail sunucusuna bağlanamadı!!");
            }
        }

        private void lstMsg_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstMsg.SelectedItems.Count > 0)
            {
                ListViewItem item = lstMsg.SelectedItems[0];

                MessageBox.Show(item.SubItems[4].Text);
            }

        }
    }
}
