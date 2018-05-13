using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
namespace DeployTool
{
    internal class DeploymentCollection : ConfigurationSection
    { 
        [ConfigurationProperty("deployment", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(DeploymentElement), CollectionType = ConfigurationElementCollectionType.BasicMap, RemoveItemName ="remove")]
        public DeploymentElement Deployment
        {
            get
            {
                return (DeploymentElement)base["deployment"];
            }
            set
            {
                base["deployment"]     = value;
            }
        }

    }
}
