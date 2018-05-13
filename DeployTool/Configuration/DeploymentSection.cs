using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeployTool
{
    class DeploymentSection: ConfigurationElement
    {
        [ConfigurationProperty("key", IsRequired = true, IsKey = true)]
        public string Key
        {
            get
            {
                return (string)base["key"];
            }
            set
            {
                base["Key"]     = value;
            }
        }

        [ConfigurationProperty("value",     IsRequired = true)]
        public string Value
        {
            get
            {
                return (string)base["Value"];
            }
            set
            {
                base["Value"]     = value;
            }
        }
    }
}
