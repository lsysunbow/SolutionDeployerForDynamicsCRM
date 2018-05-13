using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel.Description; 
using System.IO;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages; 

namespace DeployTool
{
    public class Deploy
    {
        public DeploySetting Settings { get; set; }

        /// <summary>
        /// 初始化，获取配置
        /// </summary>
        public Deploy()
        {
            this.Settings = GetSettings();
        }

        /// <summary>
        /// 生成配置
        /// </summary>
         public void GenerateSettings()
        {

            DeploySetting deploy = new DeploySetting()
            {
                DeployMode = DeployMode.Test,
                IsChangeWorkFlowName = true,
                IsExportSolutions = true,
                IsImportSolutions = false,
                PublishAllCustomizationAfterImported = true,
                PublishAllCustomizationBeforeExported = true,
                ExportPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                ImportSourcePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            deploy.SolutionsToBeExport = new List<string>();
            deploy.SolutionsToBeExport.AddRange(new string[]{
                "WebResources","PureProcesses","Reports","SecurityRoles",
            });
            deploy.SolutionsToBeImport = new List<string>();
            deploy.SolutionsToBeImport.AddRange(new string[]{
                "WebResources_8_2_4_0512","PureProcesses_8_2_4_0512","Reports_8_2_4_0512","SecurityRoles_8_2_4_0512",
            });
       
            Deployment sourceDeployment = new Deployment()
            {
                UserName = "crmadminfortestenv@hwccp.onmicrosoft.com",
                Password = "!QAZxsw2",
                IsEnable = true,
                DiscoveryServiceAddress = "https://disco.crm5.dynamics.com/XRMServices/2011/Discovery.svc",
                OrganizationServiceAddress = "https://ccaredev1.api.crm5.dynamics.com/XRMServices/2011/Organization.svc",
                OrganizationUniqueName = "ccaredev1",
                Domain = "hwccp.com",
                Name = "Online_Dev1"
            };

            deploy.SourceDeployment = sourceDeployment; 

            deploy.DestinationDeployments = new List<Deployment>();
            Deployment productionDeployment = new Deployment()
            {
                UserName = "crmadmin@hwccp.onmicrosoft.com",
                Password = "",
                IsEnable = true,
                DiscoveryServiceAddress = "https://disco.crm.dynamics.com/XRMServices/2011/Discovery.svc",
                OrganizationServiceAddress = "https://ccare.api.crm.dynamics.com/XRMServices/2011/Organization.svc",
                OrganizationUniqueName = "ccare",
                Domain = "hwccp.com",
                Name = "Production",
            };
            deploy.DestinationDeployments.Add(productionDeployment);  
            Deployment sitDeployment = new Deployment()
            {
                UserName = "crmadminfortestenv@hwccp.onmicrosoft.com",
                Password = "!QAZxsw2",
                IsEnable = true,
                DiscoveryServiceAddress = "https://disco.crm5.dynamics.com/XRMServices/2011/Discovery.svc",
                OrganizationServiceAddress = "https://ccaresit.api.crm5.dynamics.com/XRMServices/2011/Organization.svc",
                OrganizationUniqueName = "ccaresit",
                Domain = "hwccp.com",
                Name = "Online_SIT"
            };
            deploy.DestinationDeployments.Add(sitDeployment);
            Deployment uatDeployment = new Deployment()
            {
                UserName = "crmadminfortestenv@hwccp.onmicrosoft.com",
                Password = "!QAZxsw2",
                IsEnable = true,
                DiscoveryServiceAddress = "https://disco.crm5.dynamics.com/XRMServices/2011/Discovery.svc",
                OrganizationServiceAddress = "https://ccareuat.api.crm5.dynamics.com/XRMServices/2011/Organization.svc",
                OrganizationUniqueName = "ccareuat",
                Domain = "hwccp.com",
                Name = "Online_UAT"
            };
            deploy.DestinationDeployments.Add(uatDeployment);
            
            //某些解决方案也需要导入到开发环境
            deploy.DestinationDeployments.Add(sourceDeployment);

            JSONTool jt = new JSONTool();
            jt.Serialize<DeploySetting>(deploy);
        }

          DeploySetting GetSettings()
        {
            JSONTool jt = new JSONTool();
            try
            {

                DeploySetting target = jt.Deserialize<DeploySetting>(File.ReadAllText(jt.configPath));
                return target ?? null;
            }
            catch(Exception ex)
            {
                Console.WriteLine("获取配置失败\n{0}", ex.Message);
                jt = null;
            }
            return null; 
        }

       public void ExportSolution(OrganizationServiceProxy _serviceProxy)
        {

            if (this.Settings.IsChangeWorkFlowName)
            {
                ChangeWorkflowName(_serviceProxy);
            }

            string hostName = _serviceProxy.ServiceManagement.CurrentServiceEndpoint.ListenUri.Host;

            _serviceProxy.Timeout = TimeSpan.MaxValue;
            _serviceProxy.EnableProxyTypes();

            if (Settings.PublishAllCustomizationBeforeExported)
            { 
                Console.WriteLine($"{hostName} Publishing all customizations at " + DateTime.Now.ToLongTimeString());
                PublishAllXmlRequest pubReq = new PublishAllXmlRequest();
                _serviceProxy.Execute(pubReq);
            }

            foreach (string solution in this.Settings.SolutionsToBeExport)
            { 
                QueryExpression qe = new QueryExpression("solution");
                qe.ColumnSet = new ColumnSet("version");
                qe.Criteria.AddCondition("uniquename", ConditionOperator.Equal, solution);
                qe.TopCount = 1;
                qe.NoLock = true;
                EntityCollection solutions = _serviceProxy.RetrieveMultiple(qe);
                if (solutions.Entities.Count > 0)
                {
                    Console.WriteLine($"Export {solution} from {hostName} at {DateTime.Now.ToLongTimeString()}");
                    string monthDay = DateTime.Today.ToString("MMdd");
                    solutions[0].Attributes["version"] = "8.2.4." + monthDay;
                    _serviceProxy.Update(solutions[0]);
                    ExportSolutionRequest exportSolutionRequest = new ExportSolutionRequest();
                    exportSolutionRequest.Managed = false;
                    exportSolutionRequest.SolutionName = solution;
                    //exportSolutionRequest.TargetVersion = "8.2";
                    ExportSolutionResponse exportSolutionResponse = (ExportSolutionResponse)_serviceProxy.Execute(exportSolutionRequest);
                    byte[] exportXml = exportSolutionResponse.ExportSolutionFile;
                    string solutionVersion = solutions.Entities[0].GetAttributeValue<string>("version");
                    string filename = string.Format("{0}_{1}.zip", solution, solutionVersion.Replace('.', '_'));

                    WriteSolutionFile(exportXml, filename);
                }
            }
            Console.WriteLine("solution has been exported at " + DateTime.Now.ToLongTimeString());
        }

        public void ImportSolution(OrganizationServiceProxy _serviceProxy)
        {
            string hostName = _serviceProxy.ServiceManagement.CurrentServiceEndpoint.ListenUri.Host; 
            _serviceProxy.Timeout = TimeSpan.MaxValue; 
            foreach(string solution in this.Settings.SolutionsToBeImport )
            { 
                Console.WriteLine($"importing {solution} into {hostName} at {DateTime.Now.ToLongTimeString()}"); 
                byte[] fileBytes = ReadSolutionFile(solution);
                ImportSolutionRequest impSolReq = new ImportSolutionRequest()
                {
                    CustomizationFile = fileBytes,
                    PublishWorkflows = true
                }; 
                _serviceProxy.Execute(impSolReq);
                Console.WriteLine($"{solution} has been imported into {hostName}  at  {DateTime.Now.ToLongTimeString()}");
            } 

            if (this.Settings.PublishAllCustomizationAfterImported)
            {
                Console.WriteLine("Publishing all customizations " + DateTime.Now.ToLongTimeString());
                PublishAllXmlRequest pubReq = new PublishAllXmlRequest();
                _serviceProxy.Execute(pubReq);
                _serviceProxy.Dispose();
                Console.WriteLine("All customizations have been published!");
            }

        }

        /// <summary>
        /// 读取解决方案
        /// </summary>
        /// <param name="solutionName">解决方案文件名</param>
        /// <returns></returns>
          byte[] ReadSolutionFile(string solutionName )
        {  
            string fileName = this.Settings.ImportSourcePath + "\\" + solutionName + ".zip";
            if (File.Exists(fileName) == false)
            {
                Console.WriteLine($"{fileName} does not exists!");
                Console.Read();
                Environment.Exit(-1);
            }
            return File.ReadAllBytes(fileName);
        }

        /// <summary>
        /// 下载解决方案
        /// </summary>
        /// <param name="fileBytes"></param>
        /// <param name="fileName"></param>
          void WriteSolutionFile(byte[] fileBytes, string fileName)
        { 
            string directPath = this.Settings.ExportPath + "\\" + DateTime.Today.ToString("yyyyMMdd");

            if (Directory.Exists(directPath) == false)
            {
                Directory.CreateDirectory(directPath);
            }
            File.WriteAllBytes(directPath + "\\" + fileName, fileBytes);
            Console.WriteLine($"solution {fileName} has been exported!");
        }


        /// <summary>
        /// 修改工作流名称
        /// </summary>
        /// <param name="_serviceProxy"></param>
       void ChangeWorkflowName(OrganizationServiceProxy _serviceProxy)
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

        /// <summary>
        /// 部署到测试环境
        /// </summary>
        public async Task DeployToTestEnvAsync()
        {
            Deployment source = this.Settings.SourceDeployment;

            Console.WriteLine("Connecting to {0}", source.Name);
            OrganizationServiceProxy sourceService = CRMServices.GetOrganizationService(source.UserName, source.Password, source.Domain, source.OrganizationUniqueName, source.DiscoveryServiceAddress);

            Console.WriteLine("{0} Connected.", source.Name);

            if (this.Settings.IsExportSolutions)
            {
                ExportSolution(sourceService);
            }

            if (Settings.IsImportSolutions)
            {
                TaskFactory tFactory = new TaskFactory();
                List<Task> taskList = new List<Task>();
                foreach (Deployment deployment in Settings.DestinationDeployments)
                {
                    if (deployment.IsEnable)
                    { 
                        OrganizationServiceProxy targetService = CRMServices.GetOrganizationService(deployment.UserName
                            , deployment.Password, deployment.Domain, deployment.OrganizationUniqueName, deployment.DiscoveryServiceAddress);

                        Task task = new Task(() =>
                        {
                            ImportSolution(targetService);
                        });
                        taskList.Add(task);
                    }
                }

                await tFactory.ContinueWhenAll(taskList.ToArray(), new Action<Task[]>((x) =>
                 {
                     Console.WriteLine("Done!");
                 }));
            }
        }

        /// <summary>
        /// 部署到生产环境
        /// </summary>
        public async  Task DeployToProductionAsync()
        {
            TaskFactory tFactory = new TaskFactory();
            await   tFactory.StartNew(() =>
            {
                Deployment prodDeployment = Settings.DestinationDeployments.Where(x => x.Name.Equals("Production")).FirstOrDefault();
                if (prodDeployment != null)
                {
                    ClientCredentials prodcred = new ClientCredentials();
                    prodcred.UserName.UserName = prodDeployment.UserName;
                    prodcred.UserName.Password = prodDeployment.Password;

                    //因生产环境可能组织的UrlName, orgUniqueName不一致，改为采用此种方式连接
                    OrganizationServiceProxy prodProxy = new OrganizationServiceProxy(new Uri(prodDeployment.OrganizationServiceAddress), null, prodcred, null);
                    if (prodProxy != null)
                    {
                        if (this.Settings.IsChangeWorkFlowName) ChangeWorkflowName(prodProxy);

                        ImportSolution(prodProxy);
                    }
                }
            }); 
        }

        /// <summary>
        /// 主方法，运行部署任务
        /// </summary>
        public async Task RunAsync()
        {
            if (this.Settings != null)
            { 
                if (this.Settings.DeployMode == DeployMode.Production)
                   await DeployToProductionAsync();
                else if (this.Settings.DeployMode == DeployMode.Test)
                    await  DeployToTestEnvAsync();
            }
        }
    }
}
