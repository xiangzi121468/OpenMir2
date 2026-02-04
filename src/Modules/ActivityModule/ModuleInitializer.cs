using Microsoft.Extensions.Hosting;
using NLog;
using SystemModule;

namespace ActivityModule
{
    /// <summary>
    /// 活动模块初始化器
    /// </summary>
    public class ModuleInitializer : IModuleInitializer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void ConfigureServices()
        {
            Logger.Debug("ActivityModule 服务配置初始化...");
        }

        public void Configure(IHostEnvironment env)
        {
            Logger.Debug($"ActivityModule 配置初始化, 环境: {env.EnvironmentName}");
        }
    }
}
