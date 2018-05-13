# SolutionDeployerForDynamicsCRM
SolutionDeployer For Dynamics365/CRM

作者：Tencent Lee
日期：2018/5/12
联系方式:330124638@qq.com

说明：
由于开发Dynamics 365/CRM常常需要将自定义项（解决方案）从开发环境发布（部署）到测试环境或生产环境，而且测试环境通常不止一个，需要同时发布到多个环境，如果人工导出解决方案再导入到目标环境，则非常耗时，本程序的宗旨就是为了解决此问题而产生的。
虽然SDK里面有提供一个部署工具，在目录SDK\Tools\PackageDeployer下，但是此工具还需要先开发一个部署包才可用，做不同的部署任务时，需要开发不同的部署包，因此也不见得很方便，这里提供了另外一种部署方式，使用JSON配置来简化部署工作。

本程序支持通过配置的方式来完成部署工作
主要支持两种部署模式：
1.部署到生产环境
2.部署到测试环境

第一种部署模式：
程序工作方式：
1. 先自行准备需要部署的解决方案包，放置在指定的目录下；
2.程序连接生产环境，读取该路径下的解决方案，进行导入。

第二种部署模式：
程序工作方式：
1. 读取部署配置。
2.连接部署源环境，执行导出任务；
3.	连接目标环境，导入解决方案到目标环境；多个目标环境可同时进行。
 
需要特别注意的是：
1.	使用本程序前，先了解配置说明，请参考DeploySetting.cs文件注释，可以先修改Main主程序执行deploy.GenerateSettings();来生成配置
2. 部署到测试环境连接环境查找组织服务时使用的是CRMServices.FindOrganization方法，根据组织的UrlName和配置文件里的 orgUniqueName作比较，如果您使用该程序时无法连接环境，请做相应的修改，或把连接方式改为使用new OrganizationServiceProxy的方式。
3. 部署到生产环境时，配置文件项DestinationDeployments的Name属性必须是Production。
4.本程序使用visual studio 2017来开发的，.NET Framework 版本最低为4.5.2。
5. SolutionsToBeImport配置项的解决方案包将按顺序导入，请自行调整需要前置的解决方案。

欢迎分享、提出修改意见
