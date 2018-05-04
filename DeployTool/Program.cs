using System;
using System.ServiceModel;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System.Data;
using System.ServiceModel.Description;
using System.Xml;
using System.Text;
using System.IO;
using Microsoft.Crm.Sdk.Messages;
using System.Configuration;

namespace DeployTool
{
    public class Program
    {
        //public static readonly string[] exportSolutionList = new string[] { "WebResources", "main", "PureProcesses", "Processes", "Reports", "SecurityRoles" };
        //public static readonly string[] exportSolutionList = new string[] { "main", "PureProcesses", "Processes", "Reports", "SecurityRoles" };
        public static readonly string[] importSolutionList = new string[] { "WebResources"};

        public static readonly string[] exportSolutionList = new string[] { "main" }; 

        public static void Main(string[] args)
        {
            string sourceCRMUserName = ConfigurationManager.AppSettings["sourceCRMUserName"];
            string sourceCRMUserPwd = ConfigurationManager.AppSettings["sourceCRMUserPwd"];
            string sourceCRMDomain = ConfigurationManager.AppSettings["sourceCRMDomain"];
            string sourceCRMOrganizationUniqueName = ConfigurationManager.AppSettings["sourceCRMOrganizationUniqueName"];
            string sourceCRMDiscoveryService = ConfigurationManager.AppSettings["sourceCRMDiscoveryService"];

            string destinationCRMUserName = ConfigurationManager.AppSettings["destinationCRMUserName"];
            string destinationCRMUserPwd = ConfigurationManager.AppSettings["destinationCRMUserPwd"];
            string destinationCRMDomain = ConfigurationManager.AppSettings["destinationCRMDomain"];
            string destinationCRMOrganizationUniqueName = ConfigurationManager.AppSettings["destinationCRMOrganizationUniqueName"];
            string destinationCRMDiscoveryService = ConfigurationManager.AppSettings["destinationCRMDiscoveryService"];

            try
            {

                Console.WriteLine("Confirm to deploy solutions(y/n) ?");
                string key = Console.ReadLine();
                var monthDay = DateTime.Today.ToString("MMdd");
                switch (key)
                {
                    case "y":
                    case "1":

                        OrganizationServiceProxy devService = CRMDataHandler.GetOrganizationService(sourceCRMUserName, sourceCRMUserPwd
                                , sourceCRMDomain, "ccaredev1", sourceCRMDiscoveryService);

                        OrganizationServiceProxy uatService = CRMDataHandler.GetOrganizationService(destinationCRMUserName, destinationCRMUserPwd
                                , destinationCRMDomain, "ccareuat", destinationCRMDiscoveryService);

                        OrganizationServiceProxy sitService = CRMDataHandler.GetOrganizationService(destinationCRMUserName, destinationCRMUserPwd
                                , destinationCRMDomain, "ccaresit", destinationCRMDiscoveryService);

                        //从online 开发环境导出解决方案
                        //ExportSolution(devService, monthDay, exportSolutionList, true);

                        System.Threading.Tasks.TaskFactory tf = new System.Threading.Tasks.TaskFactory();

                        tf.ContinueWhenAll(new System.Threading.Tasks.Task[]
                        {
                                tf.StartNew(() =>
                                {
                                ////导入到online 开发环境 ccaredev1
                                    ImportSolution(devService, monthDay, importSolutionList);
                                }),
                                tf.StartNew(() => {
                                ////导入到online 开发环境 ccaredev1
                                    ImportSolution(sitService, monthDay, importSolutionList);

                                }),
                                tf.StartNew(() => {
                                    ////导入到online 开发环境 ccaredev1
                                    ImportSolution(uatService, monthDay, importSolutionList);

                                })
                        }, new Action<System.Threading.Tasks.Task[]>((x) =>
                        {

                            Console.WriteLine("Done!");
                        }));



                        ////导入解决方案到online sit
                        //ImportSolution(sitService, monthDay, importSolutionList);

                        //////导入解决方案到online uat
                        //ImportSolution(uatService, monthDay, importSolutionList);

                        break;
                    case "e": return;
                    default: Console.WriteLine("Invalid Command!"); break;
                }
                Console.Read();

            }
            catch (Exception ex)
            {
                Console.WriteLine("error occured! {0}", ex.Message);
                Console.ReadKey();
            }
        }
 

        public static void ExportSolution(OrganizationServiceProxy _serviceProxy, string monthDay,  string[] SolutionList, bool isOnline)
        {

            if (isOnline)
            {
                //修改工作流名称 by yuzelong 2017.11.07
                //ChangeWorkflowName(_serviceProxy);
            }

            string hostName = _serviceProxy.ServiceManagement.CurrentServiceEndpoint.ListenUri.Host;
            
            _serviceProxy.Timeout = TimeSpan.MaxValue;
            _serviceProxy.EnableProxyTypes();
            Console.WriteLine($"{hostName} Publishing all customizations at " + DateTime.Now.ToLongTimeString());
            PublishAllXmlRequest pubReq = new PublishAllXmlRequest();
            _serviceProxy.Execute(pubReq);
            for (int i = 0; i < SolutionList.Length; i++)
            {
                var toExpSolutionUniqueName = SolutionList[i];
                QueryExpression qe = new QueryExpression("solution");
                qe.ColumnSet = new ColumnSet("version");
                qe.Criteria.AddCondition("uniquename", ConditionOperator.Equal, toExpSolutionUniqueName);
                qe.TopCount = 1;
                qe.NoLock = true;
                EntityCollection solutions = _serviceProxy.RetrieveMultiple(qe);
                if (solutions.Entities.Count > 0)
                {
                    Console.WriteLine($" export {SolutionList[i]} from {hostName} at {DateTime.Now.ToLongTimeString()}");
                    solutions[0].Attributes["version"] = "8.2.4." + monthDay;
                    _serviceProxy.Update(solutions[0]);
                    ExportSolutionRequest exportSolutionRequest = new ExportSolutionRequest();
                    exportSolutionRequest.Managed = false;
                    exportSolutionRequest.SolutionName = SolutionList[i];
                    //exportSolutionRequest.TargetVersion = "8.2";
                    ExportSolutionResponse exportSolutionResponse = (ExportSolutionResponse)_serviceProxy.Execute(exportSolutionRequest);
                    byte[] exportXml = exportSolutionResponse.ExportSolutionFile;
                    string filename = string.Format("{0}_{1}.zip", toExpSolutionUniqueName, solutions.Entities[0].GetAttributeValue<string>("version").Replace('.', '_'));
              
                    WriteSolutionFile(exportXml, filename);
                }
            } 
            Console.WriteLine("solution has been exported at " + DateTime.Now.ToLongTimeString());
        }

        public static void ImportSolution(OrganizationServiceProxy _serviceProxy, string monthDay, string[] SolutionList)
        {

            string hostName = _serviceProxy.ServiceManagement.CurrentServiceEndpoint.ListenUri.Host;

            _serviceProxy.Timeout = TimeSpan.MaxValue;
            for (int i = 0; i < SolutionList.Length; i++)
            {
                Console.WriteLine($"importing {SolutionList[i]} into {hostName} at {DateTime.Now.ToLongTimeString()}");
          
                byte[] fileBytes = ReadSolutionFile(SolutionList[i]);
                ImportSolutionRequest impSolReq = new ImportSolutionRequest()
                {
                    CustomizationFile = fileBytes,
                    PublishWorkflows = true
                };

                _serviceProxy.Execute(impSolReq);
                Console.WriteLine($"{SolutionList[i]} has been imported into {hostName}  at  {DateTime.Now.ToLongTimeString()}");
            }
            Console.WriteLine("Publishing all customizations " + DateTime.Now.ToLongTimeString());
            PublishAllXmlRequest pubReq = new PublishAllXmlRequest();
            _serviceProxy.Execute(pubReq);
            _serviceProxy.Dispose();
            Console.WriteLine("All customizations have been published!");
        }

        public static byte[] ReadSolutionFile(string solution)
        {
            string baseFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("Documents", "Downloads");
            string directPath = baseFilePath + "\\" + DateTime.Today.ToString("yyyyMMdd");

            string monthday = DateTime.Today.ToString("MMdd");
            if (Directory.Exists(directPath) == false)
            {
                Directory.CreateDirectory(directPath);
            }
            string fileName = directPath + "\\" + solution + "_8_2_4_" + monthday + ".zip";
            if (File.Exists(fileName) == false)
            {
                Console.WriteLine($"{fileName} does not exists!");
                Console.Read(); 
                Environment.Exit(-1);
            }
            return File.ReadAllBytes(directPath + "\\" + solution + "_8_2_4_" + monthday + ".zip");
        }


        public static void WriteSolutionFile(byte[] fileBytes ,string fileName)
        {
            string baseFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("Documents", "Downloads");
            string directPath = baseFilePath + "\\" + DateTime.Today.ToString("yyyyMMdd");
            if (Directory.Exists(directPath)== false){
                Directory.CreateDirectory(directPath);
            }
            File.WriteAllBytes(directPath + "\\" + fileName, fileBytes);
            Console.WriteLine($"solution {fileName} has been exported!");
        }
 

        /// <summary>
        /// 修改工作流名称
        /// </summary>
        /// <param name="_serviceProxy"></param>
        private static void ChangeWorkflowName(OrganizationServiceProxy _serviceProxy)
        {
            Console.WriteLine("Changing workflow name...");
            string fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' no-lock='true'>
              <entity name='workflow'>
                <attribute name='workflowid' />
                <attribute name='name' />
                <attribute name='statecode' />
                <attribute name='statuscode' />
                <filter type='and'>
                  <condition attribute='type' operator='eq' value='1' />
                  <filter type='or'>
                    <condition attribute='name' operator='eq' value='回寄单监控状态描述，物流下单' />
                    <condition attribute='name' operator='eq' value='工单与CSPM集成（关闭）' />
                    <condition attribute='name' operator='eq' value='寄送单监控状态描述，产生物流单号' />
                  </filter>
                </filter>
              </entity>
            </fetch>";

            EntityCollection workflows = _serviceProxy.RetrieveMultiple(new FetchExpression(fetch));
            if (workflows.Entities.Count > 0)
            {
                for (int i = 0; i < workflows.Entities.Count; i++)
                {
                    //把工作流置为草稿
                    SetStateRequest deactivateRequest = new SetStateRequest
                    {
                        EntityMoniker = new EntityReference(workflows[i].LogicalName, workflows[i].Id),
                        State = new OptionSetValue(0),
                        Status = new OptionSetValue(1)
                    };
                    _serviceProxy.Execute(deactivateRequest);

                    //修改名称
                    switch (workflows.Entities[i].GetAttributeValue<string>("name"))
                    {
                        case "回寄单监控状态描述，物流下单": workflows.Entities[i].Attributes["name"] = "Status Desc. for Return Ticket, Order Placed by Logistics"; break;
                        case "工单与CSPM集成（关闭）": workflows.Entities[i].Attributes["name"] = "RN Integrated with CSPM(Closed)"; break;
                        case "寄送单监控状态描述，产生物流单号": workflows.Entities[i].Attributes["name"] = "Desc. for Status Monitering of Delivery Ticket, Generate Logistics Tracking No."; break;
                    }
                    //保存并激活工作流
                    _serviceProxy.Update(workflows.Entities[i]);
                }
            }
        }
    }
}
