using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using Com.Interwoven.WorkSite.iManage;
using MobilityServiceLib.Types;
using MobilityServiceLib.Types.JSON;

namespace Mobility2ConsoleApp
{
    class Program
    {
        const string AppUuid = "4662881B-B736-4353-ACFE-F743E03CD950";

        [ServiceContract]
        public interface IService
        {
            /*
            [OperationContract]
            [WebGet(BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
            Ping Ping(string userID, string password, string appUuid);
            */

            [OperationContract]
            [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
            Ping Ping(PingRequest request);

            [OperationContract]
            [WebGet(BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
            string TestConfiguration(string user);

            [OperationContract]
            [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
            DMSDetails DMSDetails(BasicRequest details);

            [OperationContract]
            [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
            Folder WorkSiteExplorer(BasicRequest details);

            [OperationContract]
            [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
            ReturnedProfile GetProfile(GetProfileRequest profile);

            [OperationContract]
            [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
            SimpleContentsCollection SearchDocuments(SearchDocumentsRequest request);
        }

        static void Main(string[] args)
        {
            RestSharp.RestClient rc = new RestSharp.RestClient("http://win2012svr:8010/Mobility2/MobilityService.svc");
            RestSharp.RestRequest rr = new RestSharp.RestRequest(RestSharp.Method.GET);

            using (ChannelFactory<IService> cf = new ChannelFactory<IService>(new WebHttpBinding(), "http://win2012svr:8010/Mobility2/MobilityService.svc"))
            //using (ChannelFactory<IService> cf = new ChannelFactory<IService>(new WebHttpBinding(), "http://win2012svr:4430/"))
            {
                cf.Endpoint.EndpointBehaviors.Add(new WebHttpBehavior());
                IService channel = cf.CreateChannel();

                //Ping res = channel.Ping("wsadmin", "mhdocs_", AppUuid);
                string str1 = channel.TestConfiguration("wsadmin");

                //ServerConnection(string server, string domain, string user, string password, string app, Com.Interwoven.WorkSite.iManage.IManSession sess, System.DateTime dateTime)
                IManSession session1 = null;
                MobilityServiceLib.ServerConnection sc1 = new MobilityServiceLib.ServerConnection("win2012svr", "BEANTOWN", "wsadmin", "mhdocs_", "Mobility2ConsoleApp", session1, DateTime.Now);
                
                AuthenticationInfo auth1 = new AuthenticationInfo();
                auth1.appUuid = AppUuid;
                auth1.userID = "homer";
                auth1.password = "homer$_";
                auth1.domain = "BEANTOWN";

                PingRequest pingReq1 = new PingRequest();
                pingReq1.Authentication = auth1;
                Ping res = channel.Ping(pingReq1);

                BasicRequest req1 = new BasicRequest();
                req1.Authentication = auth1;
                req1.TimeZoneOffset = (double)TimeZoneInfo.Utc.GetUtcOffset(DateTime.UtcNow).Milliseconds;
                req1.Version = 0;
                DMSDetails details = channel.DMSDetails(req1);
                Folder explorer = channel.WorkSiteExplorer(req1);

                GetProfileRequest prof1 = new GetProfileRequest();
                prof1.Authentication = auth1;
                prof1.DatabaseName = "Cheapside";
                prof1.DocNum = 2081;
                prof1.DocVer = 1;
                ReturnedProfile retProf = channel.GetProfile(prof1);

                SearchDocumentsRequest search1 = new SearchDocumentsRequest();
                search1.Authentication = auth1;
                search1.DatabaseName = "Cheapside";
                //search1.FolderID = 1274;                // 36 Capitalgroup, Llc == Fit N Fresh Acquisition
                search1.FolderID = 7138;                // Depositions !folder:ordinary,7138
                Dictionary<int,string> srchParams1 = new Dictionary<int,string>();
                //srchParams1.Add((int)imProfileAttributeID.imProfileDocNum, ">0");
                srchParams1.Add((int)imProfileAttributeID.imProfileCustom1, "1002310");
                search1.SearchType = (int)imSearchEmail.imSearchEmailOrDocuments;
                search1.ProfileSearchCriteria = srchParams1;

                SimpleContentsCollection srchRes1 = channel.SearchDocuments(search1);
            };
        }
    }
}
