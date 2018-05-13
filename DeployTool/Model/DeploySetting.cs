using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
namespace DeployTool
{
    /// <summary>
    /// 部署配置
    /// </summary>
    [DataContract]
    public class DeploySetting
    {
        /// <summary>
        /// 部署模式
        /// </summary>
        [DataMember(IsRequired = true)]
        internal  DeployMode DeployMode { get; set; }

        /// <summary>
        /// 部署来源
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        internal  Deployment SourceDeployment { get; set; }

        /// <summary>
        /// 目标部署
        /// </summary>
        [DataMember(IsRequired = true)]
        internal  List<Deployment> DestinationDeployments { get; set; }

        /// <summary>
        /// 将要被导入的解决方案
        /// </summary>
        [DataMember(IsRequired =true)]
        internal  List<string> SolutionsToBeImport { get; set; }

        /// <summary>
        /// 将要被导出的解决方案
        /// </summary>
        [DataMember(IsRequired = false)]
        internal  List<string> SolutionsToBeExport { get; set; }

        /// <summary>
        /// 导出解决方案的路径，默认为桌面
        /// </summary>
        [DataMember(IsRequired = false)]
        internal  string ExportPath { get; set; }

        /// <summary>
        /// 导入解决方案的路径，默认为桌面
        /// </summary>
        [DataMember(IsRequired = false)]
        internal  string ImportSourcePath { set; get; }

        /// <summary>
        /// 是否需要更改工作流名称,导出解决方案前更改工作流名称，由中文改为英文，因为Online环境同一使用英文
        /// </summary>
        [DataMember(IsRequired =false)]
        internal bool IsChangeWorkFlowName { get; set; }

        /// <summary>
        /// 是否要从源环境导出解决方案
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool IsExportSolutions { get; set; }

        /// <summary>
        /// 是否要导入解决方案到目标环境
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool IsImportSolutions { get; set; }

        /// <summary>
        /// 是否在导入后发布所有自定义
        /// </summary>
        [DataMember(IsRequired =false,EmitDefaultValue =true)]     
        public bool PublishAllCustomizationAfterImported { get; set; }

        /// <summary>
        /// 是否在导入后发布所有自定义
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool PublishAllCustomizationBeforeExported { get; set; }
    }

    /// <summary>
    /// 部署模式
    /// </summary>
    internal  enum DeployMode
    {
        /// <summary>
        /// 部署到生产环境
        /// </summary>
        Production = 1,
        /// <summary>
        /// 部署到测试环境
        /// </summary>
        Test = 2
    }
}
