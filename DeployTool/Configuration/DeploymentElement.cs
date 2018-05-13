using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
namespace DeployTool
{
    class DeploymentElement: ConfigurationElementCollection
    {
        [ConfigurationProperty("enable",DefaultValue="1",IsDefaultCollection =false,IsKey =false,IsRequired =false)]
        public bool Enable
        {
            get => (bool)this["Enable"];
            set
            {
                this["Enable"] = value;
            }
        }

        [ConfigurationProperty("name", DefaultValue = "1", IsDefaultCollection = false, IsKey = false, IsRequired = true)]
        public string Name
        {
            get
            {
                return this["Name"] as string;
            }
            set
            {
                this["Name"] = value;
            }
        }
 

        protected override ConfigurationElement CreateNewElement()
        {
            return new DeploymentSection();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DeploymentSection)element).Key;
        }

        public DeploymentSection this[int i]
        {
            get
            {
                return (DeploymentSection)base.BaseGet(i);
            }
        }

    

    }
}
