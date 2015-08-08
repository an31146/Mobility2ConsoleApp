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

//using System.Web.Helpers;
using System.Xml;
using System.IO;

using Com.Interwoven.WorkSite.iManage;
using MobilityServiceLib.Types;
using MobilityServiceLib.Types.JSON;

using RestSharp;

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
            DMSDetails DMSDetails(MobilityServiceLib.ServerConnection sc);

            [OperationContract]
            [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
            Folder WorkSiteExplorer(BasicRequest details);

            [OperationContract]
            [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
            TopLevelObjects Top(TopRequest details);

            [OperationContract]
            [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
            ReturnedProfile GetProfile(GetProfileRequest profile);

            [OperationContract]
            [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
            SimpleContentsCollection SearchDocuments(SearchDocumentsRequest request);

            [OperationContract]
            [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method="POST")]
            SimpleContentsCollection Worklist(BasicRequest details);

            [OperationContract]
            [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
            Folder Folder(FolderRequest folderReq);
        }

        static void Main(string[] args)
        {
            RestClient rest_Ping = new RestClient("http://win2012svr:8010/Mobility2/MobilityService.svc/Ping");
            RestRequest rest_Req = new RestRequest();
            rest_Req.AddQueryParameter("userID", "wsadmin");
            rest_Req.AddQueryParameter("password", "mhdocs_");
            rest_Req.AddQueryParameter("appUuid", AppUuid);
            RestResponse resp1 = (RestResponse)rest_Ping.ExecuteAsGet(rest_Req, "GET");

                AuthenticationInfo auth1 = new AuthenticationInfo();
                auth1.appUuid = AppUuid;
                auth1.userID = "wsadmin";
                auth1.password = "mhdocs_";
                auth1.domain = "";

                BasicRequest req1 = new BasicRequest();
                req1.Authentication = auth1;
                req1.TimeZoneOffset = (double)TimeZoneInfo.Utc.GetUtcOffset(DateTime.UtcNow).Milliseconds;
                req1.Version = 3;
                
            RestClient rest_Worklist = new RestClient("http://win2012svr/Mobility2/MobilityService.svc/Worklist");
            RestRequest rest_Req2 = new RestRequest();
            rest_Req2.RequestFormat = DataFormat.Json;
            rest_Req2.Method = Method.POST;
            rest_Req2.AddJsonBody(req1);
            resp1 = (RestResponse)rest_Worklist.ExecuteAsPost(rest_Req2, "POST");

            if (resp1.ResponseStatus == ResponseStatus.Completed)
            {
                string strContent = resp1.Content;
                FileStream text = new FileStream("C:\\Temp\\[Content_Types].xml", FileMode.Open);
                XmlReader reader = XmlReader.Create(text);
                reader.MoveToElement();
                reader.Close();
                text.Close();
                //SimpleContentsCollection worklist2 = Json.Decode(strContent);
            }

            using (ChannelFactory<IService> cf = new ChannelFactory<IService>(new WebHttpBinding(), "http://win2012svr:8010/Mobility2/MobilityService.svc"))
            //using (ChannelFactory<IService> cf = new ChannelFactory<IService>(new WebHttpBinding(), "http://win2012svr/Mobility2/MobilityService.svc"))
            {
                cf.Endpoint.EndpointBehaviors.Add(new WebHttpBehavior());
                IService channel = cf.CreateChannel();

                //Ping res = channel.Ping("wsadmin", "mhdocs_", AppUuid);
                //string str1 = channel.TestConfiguration("wsadmin");

                //ServerConnection(string server, string domain, string user, string password, string app, Com.Interwoven.WorkSite.iManage.IManSession sess, System.DateTime dateTime)
                IManSession session1 = null;
                MobilityServiceLib.ServerConnection sc1 = new MobilityServiceLib.ServerConnection("win2012svr:8010", "BEANTOWN", "wsadmin", "mhdocs_", "Mobility2ConsoleApp", session1, DateTime.Now);                

                PingRequest pingReq1 = new PingRequest();
                pingReq1.Authentication = auth1;
                Ping pingRes = channel.Ping(pingReq1);

                DMSDetails details = channel.DMSDetails(req1);
                           //details = channel.DMSDetails(sc1);

                TopRequest topReq1 = new TopRequest();
                topReq1.Authentication = auth1;
                topReq1.Version = 3;
                topReq1.TimeZoneOffset = 0.0;
                TopLevelObjects topObjs = channel.Top(topReq1);

                Folder explorer = channel.WorkSiteExplorer(req1);
                
                FolderRequest folderReq1 = new FolderRequest();
                folderReq1.Authentication = auth1;
                folderReq1.DatabaseName = "Cheapside";
                folderReq1.GetSubfolders = true;
                folderReq1.FolderID = 1274;

                Folder folder1 = channel.Folder(folderReq1);

                GetProfileRequest prof1 = new GetProfileRequest();
                prof1.Authentication = auth1;
                prof1.DatabaseName = "Cheapside";
                prof1.DocNum = 2081;
                prof1.DocVer = 1;
                ReturnedProfile retProf = channel.GetProfile(prof1);

                List<Content> contents = new List<Content>();
                SimpleContentsCollection worklist1 = channel.Worklist(req1);
                contents = worklist1.Contents;

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
                search1.Version = 3;

                SimpleContentsCollection srchRes1 = channel.SearchDocuments(search1);
            };
        }
    }
}
