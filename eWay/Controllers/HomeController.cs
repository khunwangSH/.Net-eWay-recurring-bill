using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace eWay.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var _url_test = "https://www.eway.com.au/gateway/rebill/test/manageRebill_test.asmx";
            var _url = "https://www.eway.com.au/gateway/rebill/manageRebill.asmx";
            var req = CreateWebRequest(_url_test, "http://www.eway.com.au/gateway/rebill/manageRebill/CreateRebillCustomer");
           
            //1. Create Rebill Customer
            var cus = SetupCustomer();
            var eWayCCXml = CreateCustomerSoapEnv(cus);
            var XMLCCEnv = CreateSoapEnvelope(eWayCCXml);
            InsertSoapEnvelopeIntoWebRequest(XMLCCEnv, req);

            // begin async call to web request.
            IAsyncResult asyncResult = req.BeginGetResponse(null, null);

            // suspend this thread until call is complete. You might want to
            // do something usefull here like update your UI.
            asyncResult.AsyncWaitHandle.WaitOne();

            // get the response from the completed web request.
            string soapResult;
            string rebillCustomerID;
            using (WebResponse webResponse = req.EndGetResponse(asyncResult))
            {
                using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                {
                    soapResult = rd.ReadToEnd();
                    XmlDocument soapEnv = new XmlDocument();
                    soapEnv.LoadXml(soapResult);
                    var res = soapEnv.GetElementsByTagName("Result").Item(0).InnerText;
                    rebillCustomerID = soapEnv.GetElementsByTagName("RebillCustomerID").Item(0).InnerText;
                    //string jsonText = JsonConvert.SerializeXmlNode(soapEnv);
                }
                Console.Write(soapResult);
            }
            //#####################################################################
            //2. Create Rebill Event
            //#####################################################################
            var bill = SetupRebill();
            bill.RebillCustomerID = rebillCustomerID;
            var eWayBillXml = CreateRebillSoapEnv(bill);
            var XMLBillEnv = CreateSoapEnvelope(eWayBillXml);
            req = CreateWebRequest(_url_test, "http://www.eway.com.au/gateway/rebill/manageRebill/CreateRebillEvent");

            InsertSoapEnvelopeIntoWebRequest(XMLBillEnv, req);

            // begin async call to web request.
            var asyncResult2 = req.BeginGetResponse(null, null);

            // suspend this thread until call is complete. You might want to
            // do something usefull here like update your UI.
            asyncResult.AsyncWaitHandle.WaitOne();

            // get the response from the completed web request.
            string soapResult2;
            using (WebResponse webResponse = req.EndGetResponse(asyncResult))
            {
                using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                {
                    soapResult2 = rd.ReadToEnd();
                    XmlDocument soapEnv = new XmlDocument();
                    soapEnv.LoadXml(soapResult2);
                    //var res = soapEnv.GetElementsByTagName("Result").Item(0).InnerText;
                    rebillCustomerID = soapEnv.GetElementsByTagName("RebillCustomerID").Item(0).InnerText;
                    var RebillID = soapEnv.GetElementsByTagName("RebillID").Item(0).InnerText;
                    //string jsonText = JsonConvert.SerializeXmlNode(soapEnv);
                }
              //  Console.Write(soapResult);
            }

            return View();
        }

        private HttpWebRequest CreateWebRequest(string url, string action)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("SOAPAction", action);
            request.ContentType = "text/xml;charset=\"utf-8\"";
            request.Accept = "text/xml";
            request.Method = "POST";

            return request;
        }

        private XmlDocument CreateSoapEnvelope(string xmlStr)
        {
            XmlDocument soapEnv = new XmlDocument();
            soapEnv.LoadXml(xmlStr);
            return soapEnv;
        }

        private void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvXml, HttpWebRequest request)
        {
            using (Stream stream = request.GetRequestStream())
            {
                soapEnvXml.Save(stream);
            }
        }
       
        private RebillModel SetupRebill()
        {
            var obj = new RebillModel
            {
                eWAYCustomerID = "eWay sndbox customer ID",
                Username = "eWay sandbox username",
                Password = "eWay sandbox API password",
                RebillCustomerID = "123445",
                RebillCCName = "testuser cybera",
                RebillCCNumber = "4444333322221111",
                RebillCCExpMonth = "09",
                RebillCCExpYear = "2018",
                RebillInitAmt = "120",
                RebillInitDate = DateTime.Now.ToString("dd/MM/yyyy"),
                RebillRecurAmt = "130",
                RebillStartDate = DateTime.Now.AddYears(1).ToString("dd/MM/yyyy"),
                RebillInterval = "30",
                RebillIntervalType = "4",
                RebillEndDate = DateTime.Now.AddYears(20).ToString("dd/MM/yyyy")
            };

            return obj;
        }


        private eWayCustomer SetupCustomer()
        {
            var obj = new eWayCustomer
            {
                eWAYCustomerID = "eWay sndbox customer ID",
                Username = "eWay sandbox username",
                Password = "eWay sandbox API password",
                customerTitle = "Mr",
                customerFirstName = "testRecuring",
                customerLastName  = "cybera",
                customerAddress   = "Netstudio",
                customerSuburb  = "Berwick",
                customerState   = "VIC",
                customerCompany = "Cybera",
                customerPostCode = "3061",
                customerCountry = "Australia",
                customerEmail = "testshop12@yopmail.com",
                customerFax = "0298989898",
                customerPhone1 = "0297979797",
                customerRef = "cus123",
                customerComments = "Hello eWay!",
                customerURL = "https://www.cybera.com.au"

            };

            return obj;
        }

        private string CreateRebillSoapEnv(RebillModel cus)
        {
            string rebillStr = @"<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">";
                   rebillStr += @"<soap:Header>";
                   rebillStr +=   @"<eWAYHeader xmlns=""http://www.eway.com.au/gateway/rebill/manageRebill"">";
                   rebillStr +=        @"<eWAYCustomerID>"+cus.eWAYCustomerID+@"</eWAYCustomerID>";
                   rebillStr +=             @"<Username>"+cus.Username+"</Username>";
                   rebillStr +=             @"<Password>"+cus.Password+"</Password>";
                   rebillStr +=   @"</eWAYHeader>";
                   rebillStr += @"</soap:Header>";
                   rebillStr += @"<soap:Body>";
                   rebillStr +=   @"<CreateRebillEvent xmlns=""http://www.eway.com.au/gateway/rebill/manageRebill"">";
                   rebillStr +=        @"<RebillCustomerID>"+cus.RebillCustomerID+"</RebillCustomerID>";
                   rebillStr += cus.RebillInvRef != null ? @"<RebillInvRef>" +cus.RebillInvRef+"</RebillInvRef>" : @"<RebillInvRef/>";
                   rebillStr += cus.RebillInvRef != null ? @"<RebillInvDes>" +cus.RebillInvDes + "</RebillInvDes>" : @"<RebillInvDes/>";
                   rebillStr +=        @"<RebillCCName>"+cus.RebillCCName + "</RebillCCName>";
                   rebillStr +=        @"<RebillCCNumber>"+cus.RebillCCNumber + "</RebillCCNumber>";
                   rebillStr +=        @"<RebillCCExpMonth>"+cus.RebillCCExpMonth + "</RebillCCExpMonth>";
                   rebillStr +=        @"<RebillCCExpYear>"+cus.RebillCCExpYear+"</RebillCCExpYear>";
                   rebillStr +=        @"<RebillInitAmt>"+cus.RebillInitAmt+"</RebillInitAmt>";
                   rebillStr +=        @"<RebillInitDate>"+cus.RebillInitDate+"</RebillInitDate>";
                   rebillStr +=        @"<RebillRecurAmt>"+cus.RebillRecurAmt+"</RebillRecurAmt>";
                   rebillStr +=        @"<RebillStartDate>"+cus.RebillStartDate+"</RebillStartDate>";
                   rebillStr +=        @"<RebillInterval>"+cus.RebillInterval+"</RebillInterval>";
                   rebillStr +=        @"<RebillIntervalType>"+cus.RebillIntervalType+"</RebillIntervalType>";
                   rebillStr +=        @"<RebillEndDate>"+cus.RebillEndDate+"</RebillEndDate>";
                   rebillStr +=   @"</CreateRebillEvent>";
                   rebillStr += @"</soap:Body>";
                   rebillStr += @"</soap:Envelope>";

            return rebillStr;
        }

        private string CreateCustomerSoapEnv(eWayCustomer cus)
        {
            string rebillStr = @"<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">";
                   rebillStr += @"<soap:Header>";
                   rebillStr +=   @"<eWAYHeader xmlns=""http://www.eway.com.au/gateway/rebill/manageRebill"">";
                   rebillStr +=        @"<eWAYCustomerID>"+cus.eWAYCustomerID+@"</eWAYCustomerID>";
                   rebillStr +=             @"<Username>"+cus.Username+"</Username>";
                   rebillStr +=             @"<Password>"+cus.Password+"</Password>";
                   rebillStr +=   @"</eWAYHeader>";
                   rebillStr += @"</soap:Header>";
                   rebillStr += @"<soap:Body>";
                   rebillStr +=   @"<CreateRebillCustomer  xmlns=""http://www.eway.com.au/gateway/rebill/manageRebill"">";
                   rebillStr +=        @"<customerTitle>"+cus.customerTitle + "</customerTitle>";
                   rebillStr +=        @"<customerFirstName>"+cus.customerFirstName+ "</customerFirstName>";
                   rebillStr +=        @"<customerLastName>"+cus.customerLastName + "</customerLastName>";
                   rebillStr +=        @"<customerAddress>"+cus.customerAddress + "</customerAddress>";
                   rebillStr +=        @"<customerSuburb>"+cus.customerSuburb + "</customerSuburb>";
                   rebillStr +=        @"<customerState>"+cus.customerState + "</customerState>";
                   rebillStr +=        @"<customerCompany>"+cus.customerCompany + "</customerCompany>";
                   rebillStr +=        @"<customerPostCode>"+cus.customerPostCode+ "</customerPostCode>";
                   rebillStr +=        @"<customerCountry>"+cus.customerCountry + "</customerCountry>";
                   rebillStr +=        @"<customerEmail>"+cus.customerEmail + "</customerEmail>";
                   rebillStr +=        @"<customerFax>"+cus.customerFax + "</customerFax>";
                   rebillStr +=        @"<customerPhone1>"+cus.customerPhone1 + "</customerPhone1>";
                   rebillStr +=        @"<customerPhone2/>";
                   rebillStr +=        @"<customerRef>"+cus.customerRef+ "</customerRef>";
                   rebillStr +=        @"<customerJobDesc/>";
                   rebillStr +=        @"<customerComments>"+cus.customerComments+ "</customerComments>";
                   rebillStr +=        @"<customerURL>"+cus.customerURL+ "</customerURL>";
                   rebillStr +=   @"</CreateRebillCustomer>";
                   rebillStr += @"</soap:Body>";
                   rebillStr += @"</soap:Envelope>";

            return rebillStr;
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }

    public class RebillModel
    {
        public string eWAYCustomerID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string RebillCustomerID { get; set; }
        public string RebillInvRef { get; set; }
        public string RebillInvDes { get; set; }
        public string RebillCCName { get; set; }
        public string RebillCCNumber { get; set; }
        public string RebillCCExpMonth { get; set; }
        public string RebillCCExpYear { get; set; }
        public string RebillInitAmt { get; set; }
        public string RebillInitDate { get; set; }
        public string RebillRecurAmt { get; set; }
        public string RebillStartDate { get; set; }
        public string RebillInterval { get; set; }
        public string RebillIntervalType { get; set; }
        public string RebillEndDate { get; set; }
    }

    public class eWayCustomer
    {
        public string eWAYCustomerID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string customerTitle { get; set; }
        public string customerFirstName { get; set; }
        public string customerLastName { get; set; }
        public string customerAddress { get; set; }
        public string customerSuburb { get; set; }
        public string customerState { get; set; }
        public string customerCompany { get; set; }
        public string customerPostCode { get; set; }
        public string customerCountry { get; set; }
        public string customerEmail { get; set; }
        public string customerFax { get; set; }
        public string customerPhone1 { get; set; }
        public string customerRef { get; set; }
        public string customerComments { get; set; }
        public string customerURL { get; set; }
    }
}