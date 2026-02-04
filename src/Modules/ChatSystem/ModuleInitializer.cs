using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenMir2;
using SystemModule;

namespace ChatSystem
{
    public class ModuleInitializer : IModuleInitializer
    {


        public void Configure(IHostEnvironment env)
        {
            // 聊天系统配置初始化
            LogService.Debug($"ChatSystem 配置初始化, 环境: {env.EnvironmentName}");
        }

        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IChatService, ChatService>();
            //serviceCollection.AddTransient<INotificationHandler<GameMessageEvent>, MessageEventHandler>();
        }

        public void Startup(CancellationToken cancellationToken = default)
        {
            LogService.Info("聊天系统插件启动...");
        }

        public void Stopping(CancellationToken cancellationToken = default)
        {
            LogService.Info("聊天系统插件停止...");
        }
    }
}