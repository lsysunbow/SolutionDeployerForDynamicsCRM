using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DeployTool
{
    /// <summary>
    /// 部署环境配置，具体的使用环境
    /// </summary>
    [DataContract]
    internal class Deployment
    {
        /// <summary>
        /// 部署环境名称
        /// </summary>
        [DataMember(IsRequired = true)]
        internal string Name { get; set; }

        /// <summary>
        /// 登录用户名
        /// </summary>
        [DataMember(IsRequired = true)]
        internal  string UserName { get; set; }

        [DataMember(IsRequired =true)]
        internal  string Password { get; set; }

        [DataMember(IsRequired = true)]
        internal  string Domain { get; set; }

        /// <summary>
        /// 组织名
        /// </summary>
        [DataMember(IsRequired = true)]
        internal  string OrganizationUniqueName { get; set; }

        [DataMember(IsRequired = true)]
        internal  string DiscoveryServiceAddress { get; set; }

        [DataMember(IsRequired = true)]
        internal  string OrganizationServiceAddress { get; set; }

        [DataMember(EmitDefaultValue = true)]
        internal  bool IsEnable { get; set; }
    }
}
