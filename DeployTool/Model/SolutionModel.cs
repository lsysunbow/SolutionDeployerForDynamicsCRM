using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
namespace DeployTool
{
    [DataContract]
    internal class SolutionModel
    {
        /// <summary>
        /// 解决方案名称
        /// </summary>
        [DataMember(IsRequired = true)]
        internal string Name { get; set; }

         
    }
}
