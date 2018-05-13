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
        public static void Main(string[] args)
        { 
            try
            {
                Console.WriteLine("Confirm to deploy solutions(press y to deploy) ?");
                string key = Console.ReadLine();
                if (key == "y")
                {  
                    Deploy deploy = new Deploy();
                    deploy.RunAsync().Wait();
                    //deploy.GenerateSettings();
                } 
                Console.Read();

            }
            catch (Exception ex)
            {
                Console.WriteLine("error occured! {0}", ex.Message);
                Console.ReadKey();
            }
        }


    }
}
