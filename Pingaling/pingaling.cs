using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using System.Collections.Specialized;
using System.Net.Configuration;
using System.Xml;
using System.IO;

namespace Pingaling
{
    class pingaling
    {
       

        static void Main(string[] args)
        {
            //Read in settings from app.config
            string emailFrom = ConfigurationManager.AppSettings["fromEmail"];
            string emailTo = ConfigurationManager.AppSettings["toEmail"];
            string emailUser = ConfigurationManager.AppSettings["emailUser"];
            string emailPassword = ConfigurationManager.AppSettings["emailPassword"];
            string smtpHost = ConfigurationManager.AppSettings["smtpHost"];

            //Read in data from XML file containing site data
            XmlDataDocument xmldoc = new XmlDataDocument();
            XmlNodeList xmlnode;
            int i = 0;
            string siteName = string.Empty;
            string siteUrl = string.Empty;
            FileStream fs = new FileStream("sites.xml", FileMode.Open, FileAccess.Read);
            xmldoc.Load(fs);
            xmlnode = xmldoc.GetElementsByTagName("Site");
            for (i = 0; i <= xmlnode.Count - 1; i++)
            {
                xmlnode[i].ChildNodes.Item(0).InnerText.Trim();
                siteName = xmlnode[i].ChildNodes.Item(0).InnerText.Trim();
                siteUrl = xmlnode[i].ChildNodes.Item(1).InnerText.Trim();

                HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(siteUrl);
                httpReq.AllowAutoRedirect = false;
                Console.WriteLine("Checking status of " + httpReq.RequestUri.ToString() + "...");
                try
                {
                    HttpWebResponse httpRes = (HttpWebResponse)httpReq.GetResponse();
                    if (httpRes == null || httpRes.StatusCode != HttpStatusCode.OK)
                    {

                        NetworkCredential auth = new NetworkCredential(emailUser, emailPassword);
                        SmtpClient smtp = new SmtpClient();
                        smtp.Host = smtpHost;
                        smtp.Port = 25;
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = auth;

                        MailAddress from = new MailAddress(emailFrom);
                        MailAddress to = new MailAddress(emailTo);
                        MailMessage msg = new MailMessage(from, to);

                        msg.Subject = siteName + " is Down!";
                        msg.Body = "My Pingaling says that " + httpReq.RequestUri.ToString() + " is down!" +
                            " The response is " + httpRes.StatusDescription + ", code " + httpRes.StatusCode.ToString();
                        smtp.Send(msg);

                    }
                    Console.WriteLine("The response status was: " + httpRes.StatusDescription + ", code " + httpRes.StatusCode.ToString());
                }
                catch (System.Net.WebException webEx)
                {
                    //If site server returns 403 forbidden, an exception will be thrown b/c the server denies the request,
                    //so we wil use the exception as the response
                    string httpRes = webEx.Message.ToString();
                    NetworkCredential auth = new NetworkCredential(emailUser, emailPassword);
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = smtpHost;
                    smtp.Port = 25;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = auth;

                    MailAddress from = new MailAddress(emailFrom);
                    MailAddress to = new MailAddress(emailTo);
                    MailMessage msg = new MailMessage(from, to);

                    msg.Subject = siteName + " is Down!";
                    msg.Body = "My Pingaling says that " + siteUrl + " is offline. The response from the server is " + httpRes + ".";
                    smtp.Send(msg);

                    Console.WriteLine(msg.Body);
                }
                
                
                   
            }
            
        }

    }

}
