using System; 
using System.ServiceModel;
using System.ServiceModel.Description; 
using System.Configuration;

using Microsoft.Xrm.Sdk; 
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery; 

namespace DeployTool
{
    public abstract class CRMDataHandler
    {
     
        #region Fields
        private readonly static Lazy<OrganizationServiceProxy> _serviceProxy = new Lazy<OrganizationServiceProxy>(
            () =>
            {
                return null;
                //ServerConnection serverConnect = new ServerConnection();
                //ServerConnection.Configuration serverConfig = serverConnect.GetServerConfiguration();
                //return new OrganizationServiceProxy(serverConfig.OrganizationUri, serverConfig.HomeRealmUri, serverConfig.Credentials, serverConfig.DeviceCredentials);

                try
                {
                    string _discoveryServiceAddress = ConfigurationManager.AppSettings["DiscoveryService"];
                    string _organizationUniqueName = ConfigurationManager.AppSettings["OrganizationUniqueName"];

                    IServiceManagement<IDiscoveryService> serviceManagement =
                                ServiceConfigurationFactory.CreateManagement<IDiscoveryService>(
                                new Uri(_discoveryServiceAddress));
                    AuthenticationProviderType endpointType = serviceManagement.AuthenticationType;

                    // Set the credentials.
                    AuthenticationCredentials authCredentials = GetCredentials(serviceManagement, endpointType);


                    String organizationUri = String.Empty;
                    // Get the discovery service proxy.
                    using (DiscoveryServiceProxy discoveryProxy =
                        GetProxy<IDiscoveryService, DiscoveryServiceProxy>(serviceManagement, authCredentials))
                    {
                        // Obtain organization information from the Discovery service. 
                        if (discoveryProxy != null)
                        {
                            // Obtain information about the organizations that the system user belongs to.
                            OrganizationDetailCollection orgs = DiscoverOrganizations(discoveryProxy);
                            // Obtains the Web address (Uri) of the target organization.
                            organizationUri = FindOrganization(_organizationUniqueName,
                                orgs.ToArray()).Endpoints[EndpointType.OrganizationService];

                        }
                    }

                    if (!String.IsNullOrWhiteSpace(organizationUri))
                    {
                        IServiceManagement<IOrganizationService> orgServiceManagement =
                            ServiceConfigurationFactory.CreateManagement<IOrganizationService>(new Uri(organizationUri));

                        // Set the credentials.
                        //AuthenticationCredentials credentials = GetCredentials(orgServiceManagement, endpointType);

                        // Get the organization service proxy. 
                        return GetProxy<IOrganizationService, OrganizationServiceProxy>(orgServiceManagement, authCredentials);
                    }

                    //ClientCredentials cc = new ClientCredentials();
                    //cc.UserName.UserName = "s84065759";
                    //cc.UserName.Password = "520swh!!";
                    //return new OrganizationServiceProxy(new Uri("https://ccare.crm.huawei.com/XRMServices/2011/Organization.svc"),
                    //new Uri("https://ccare-sts.huawei.com/SecurityTokenService/Issue.svc?wsdl"), cc, null);

                }
                catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
                {
                    Console.WriteLine("The application terminated with an error.");
                    Console.WriteLine("Timestamp: {0}", ex.Detail.Timestamp);
                    Console.WriteLine("Code: {0}", ex.Detail.ErrorCode);
                    Console.WriteLine("Message: {0}", ex.Detail.Message);
                    Console.WriteLine("Trace: {0}", ex.Detail.TraceText);
                    Console.WriteLine("Inner Fault: {0}",
                        null == ex.Detail.InnerFault ? "No Inner Fault" : "Has Inner Fault");
                }
                catch (System.TimeoutException ex)
                {
                    Console.WriteLine("The application terminated with an error.");
                    Console.WriteLine("Message: {0}", ex.Message);
                    Console.WriteLine("Stack Trace: {0}", ex.StackTrace);
                    Console.WriteLine("Inner Fault: {0}",
                        null == ex.InnerException.Message ? "No Inner Fault" : ex.InnerException.Message);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("The application terminated with an error.");
                    Console.WriteLine(ex.Message);

                    // Display the details of the inner exception.
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine(ex.InnerException.Message);

                        FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> fe = ex.InnerException
                            as FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>;
                        if (fe != null)
                        {
                            Console.WriteLine("Timestamp: {0}", fe.Detail.Timestamp);
                            Console.WriteLine("Code: {0}", fe.Detail.ErrorCode);
                            Console.WriteLine("Message: {0}", fe.Detail.Message);
                            Console.WriteLine("Trace: {0}", fe.Detail.TraceText);
                            Console.WriteLine("Inner Fault: {0}",
                                null == fe.Detail.InnerFault ? "No Inner Fault" : "Has Inner Fault");
                        }
                    }
                }
                // Additional exceptions to catch: SecurityTokenValidationException, ExpiredSecurityTokenException,
                // SecurityAccessDeniedException, MessageSecurityException, and SecurityNegotiationException.

                finally
                {
              
                }
                return null;
            }, true);

        public static OrganizationServiceProxy ServiceProxy
        {
            get
            {
                return _serviceProxy.Value;

            }
        }

        #endregion

        #region Methods
        static AuthenticationCredentials GetCredentials<TService>(IServiceManagement<TService> service, AuthenticationProviderType endpointType)
        {
            string _userName = ConfigurationManager.AppSettings["CRMUser"];
            string _password = ConfigurationManager.AppSettings["CRMPWD"];
            String _domain = ConfigurationManager.AppSettings["Domain"];
            AuthenticationCredentials authCredentials = new AuthenticationCredentials();

            switch (endpointType)
            {
                case AuthenticationProviderType.ActiveDirectory:
                    authCredentials.ClientCredentials.Windows.ClientCredential =
                        new System.Net.NetworkCredential(_userName,
                            _password,
                            _domain);
                    break;

                default: // For Federated and OnlineFederated environments.                    
                    authCredentials.ClientCredentials.UserName.UserName = _userName;
                    authCredentials.ClientCredentials.UserName.Password = _password;
                    // For OnlineFederated single-sign on, you could just use current UserPrincipalName instead of passing user name and password.
                    // authCredentials.UserPrincipalName = UserPrincipal.Current.UserPrincipalName;  // Windows Kerberos


                    break;
            }

            return authCredentials;
        }
   
        static AuthenticationCredentials GetCredentials<TService>(string userName,string password,string domain, IServiceManagement<TService> service, AuthenticationProviderType endpointType)
        { 
            AuthenticationCredentials authCredentials = new AuthenticationCredentials();

            switch (endpointType)
            {
                case AuthenticationProviderType.ActiveDirectory:
                    authCredentials.ClientCredentials.Windows.ClientCredential =
                        new System.Net.NetworkCredential(userName, password,  domain);
                    break;

                default: // For Federated and OnlineFederated environments.                    
                    authCredentials.ClientCredentials.UserName.UserName = userName;
                    authCredentials.ClientCredentials.UserName.Password = password;
                    // For OnlineFederated single-sign on, you could just use current UserPrincipalName instead of passing user name and password.
                    // authCredentials.UserPrincipalName = UserPrincipal.Current.UserPrincipalName;  // Windows Kerberos
                     
                    break;
            }

            return authCredentials;
        }

        static TProxy GetProxy<TService, TProxy>(
            IServiceManagement<TService> serviceManagement,
            AuthenticationCredentials authCredentials)
            where TService : class
            where TProxy : ServiceProxy<TService>
        {
            Type classType = typeof(TProxy);

            if (serviceManagement.AuthenticationType !=
                AuthenticationProviderType.ActiveDirectory)
            {
                AuthenticationCredentials tokenCredentials =
                    serviceManagement.Authenticate(authCredentials);
                // Obtain discovery/organization service proxy for Federated, LiveId and OnlineFederated environments. 
                // Instantiate a new class of type using the 2 parameter constructor of type IServiceManagement and SecurityTokenResponse.
                return (TProxy)classType
                    .GetConstructor(new Type[] { typeof(IServiceManagement<TService>), typeof(SecurityTokenResponse) })
                    .Invoke(new object[] { serviceManagement, tokenCredentials.SecurityTokenResponse });
            }

            // Obtain discovery/organization service proxy for ActiveDirectory environment.
            // Instantiate a new class of type using the 2 parameter constructor of type IServiceManagement and ClientCredentials.
            return (TProxy)classType
                .GetConstructor(new Type[] { typeof(IServiceManagement<TService>), typeof(ClientCredentials) })
                .Invoke(new object[] { serviceManagement, authCredentials.ClientCredentials });
        }

        static OrganizationDetailCollection DiscoverOrganizations(
            IDiscoveryService service)
        {
            if (service == null) throw new ArgumentNullException("service");
            RetrieveOrganizationsRequest orgRequest = new RetrieveOrganizationsRequest();
            RetrieveOrganizationsResponse orgResponse =
                (RetrieveOrganizationsResponse)service.Execute(orgRequest);

            return orgResponse.Details;
        }

        static OrganizationDetail FindOrganization(string orgUniqueName,
            OrganizationDetail[] orgDetails)
        {
            if (String.IsNullOrWhiteSpace(orgUniqueName))
                throw new ArgumentNullException("orgUniqueName");
            if (orgDetails == null)
                throw new ArgumentNullException("orgDetails");
            OrganizationDetail orgDetail = null;

            foreach (OrganizationDetail detail in orgDetails)
            {
                if (String.Compare(detail.UrlName, orgUniqueName,
                    StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    orgDetail = detail;
                    break;
                }
            }
            return orgDetail;
        }


       public  static OrganizationServiceProxy GetOrganizationService(string username,string password,string domain,string organizationUniqueName,string _discoveryServiceAddress)
        {
            
            string _organizationUniqueName = organizationUniqueName;

            IServiceManagement<IDiscoveryService> serviceManagement =
                        ServiceConfigurationFactory.CreateManagement<IDiscoveryService>(
                        new Uri(_discoveryServiceAddress));
            AuthenticationProviderType endpointType = serviceManagement.AuthenticationType;

            // Set the credentials.
            AuthenticationCredentials authCredentials = GetCredentials(username,password,domain, serviceManagement, endpointType);


            String organizationUri = String.Empty;
            // Get the discovery service proxy.
            using (DiscoveryServiceProxy discoveryProxy =
                GetProxy<IDiscoveryService, DiscoveryServiceProxy>(serviceManagement, authCredentials))
            {
                // Obtain organization information from the Discovery service. 
                if (discoveryProxy != null)
                {
                    // Obtain information about the organizations that the system user belongs to.
                    OrganizationDetailCollection orgs = DiscoverOrganizations(discoveryProxy);
                    // Obtains the Web address (Uri) of the target organization.
                    organizationUri = FindOrganization(_organizationUniqueName,
                        orgs.ToArray()).Endpoints[EndpointType.OrganizationService];

                }
            }

            if (!String.IsNullOrWhiteSpace(organizationUri))
            {
                IServiceManagement<IOrganizationService> orgServiceManagement =
                    ServiceConfigurationFactory.CreateManagement<IOrganizationService>(new Uri(organizationUri));
                 
                // Get the organization service proxy. 
                return GetProxy<IOrganizationService, OrganizationServiceProxy>(orgServiceManagement, authCredentials);
            }
            return null;
        }
        #endregion 
    }

}
