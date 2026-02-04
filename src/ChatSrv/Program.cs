using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using Spectre.Console;
using System;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LogLevel = NLog.LogLevel;

namespace GameGate
{
    internal class Program
    {
        private static Logger LogService;
        private static readonly CancellationTokenSource CancellationToken = new CancellationTokenSource();

        private static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;

            IConfigurationRoot config = new ConfigurationBuilder().Build();

            LogService = LogManager.Setup()
                .SetupExtensions(ext => ext.RegisterConfigSettings(config))
                .GetCurrentClassLogger();

            ThreadPool.SetMaxThreads(200, 200);
            ThreadPool.GetMinThreads(out int workThreads, out int completionPortThreads);
            LogService.Info(new StringBuilder()
                .Append($"ThreadPool.ThreadCount: {ThreadPool.ThreadCount}, ")
                .Append($"Minimum work threads: {workThreads}, ")
                .Append($"Minimum completion port threads: {completionPortThreads})").ToString());

            PrintUsage();

            IHostBuilder builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<AppService>();
                }).ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    logging.AddNLog(config);
                });
            await builder.StartAsync(CancellationToken.Token);
            await ProcessLoopAsync();
            Stop();
        }

        private static void Stop()
        {
            AnsiConsole.Status().Start("Disconnecting...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                LogManager.Shutdown();
            });
        }

        private static async Task ProcessLoopAsync()
        {
            string input = null;
            do
            {
                input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                if (input.StartsWith("/exit") && AnsiConsole.Confirm("Do you really want to exit?"))
                {
                    return;
                }

                string firstTwoCharacters = input[..2];

                if (firstTwoCharacters switch
                {
                    "/s" => ShowServerStatus(),
                    "/c" => ClearConsole(),
                    "/r" => ReLoadConfig(),
                    "/q" => Exit(),
                    _ => null
                } is Task task)
                {
                    await task;
                    continue;
                }

            } while (input is not "/exit");
        }

        private static Task Exit()
        {
            CancellationToken.CancelAfter(3000);
            Environment.Exit(Environment.ExitCode);
            return Task.CompletedTask;
        }

        private static Task ClearConsole()
        {
            Console.Clear();
            AnsiConsole.Clear();
            return Task.CompletedTask;
        }

        private static Task ReLoadConfig()
        {
            LogService.Info("重新读取配置文件完成...");
            return Task.CompletedTask;
        }

        private static void ChanggeLogLevel(LogLevel logLevel)
        {
            LogManager.Configuration.Variables["MirLevel"] = logLevel.ToString();
            LogManager.ReconfigExistingLoggers();
            LogManager.Configuration.Reload();
        }

        /// <summary>
        /// 显示聊天服务状态
        /// </summary>
        private static Task ShowServerStatus()
        {
            // 显示聊天服务状态信息
            var table = new Table().Expand().BorderColor(Color.Grey);
            table.AddColumn("[yellow]Item[/]");
            table.AddColumn("[yellow]Value[/]");

            // 添加服务状态信息
            table.AddRow("[bold]Service[/]", "[green]ChatSrv[/]");
            table.AddRow("[bold]Status[/]", "[green]Running[/]");
            table.AddRow("[bold]Thread Count[/]", $"[blue]{ThreadPool.ThreadCount}[/]");
            table.AddRow("[bold]Available Threads[/]", GetAvailableThreads());
            table.AddRow("[bold]Memory Usage[/]", $"[blue]{GC.GetTotalMemory(false) / 1024 / 1024} MB[/]");
            table.AddRow("[bold]Gen0 Collections[/]", $"[blue]{GC.CollectionCount(0)}[/]");
            table.AddRow("[bold]Gen1 Collections[/]", $"[blue]{GC.CollectionCount(1)}[/]");
            table.AddRow("[bold]Gen2 Collections[/]", $"[blue]{GC.CollectionCount(2)}[/]");
            table.AddRow("[bold]Uptime[/]", $"[blue]{GetUptime()}[/]");

            AnsiConsole.Write(table);
            return Task.CompletedTask;
        }

        private static string GetAvailableThreads()
        {
            ThreadPool.GetAvailableThreads(out int workerThreads, out int completionPortThreads);
            return $"[blue]Worker: {workerThreads}, IO: {completionPortThreads}[/]";
        }

        private static readonly DateTime _startTime = DateTime.Now;
        private static string GetUptime()
        {
            var uptime = DateTime.Now - _startTime;
            return $"{(int)uptime.TotalHours}h {uptime.Minutes}m {uptime.Seconds}s";
        }

        private static void PrintUsage()
        {
            AnsiConsole.WriteLine();

            Table table = new Table()
            {
                Border = TableBorder.None,
                Expand = true,
            }.HideHeaders();
            table.AddColumn(new TableColumn("One"));

            FigletText header = new FigletText("OpenMir2")
            {
                Color = Color.Fuchsia
            };
            FigletText header2 = new FigletText("Chat Server")
            {
                Color = Color.Aqua
            };

            StringBuilder sb = new StringBuilder();
            sb.Append("[bold fuchsia]/r[/] [aqua]重读[/] 配置文件\n");
            sb.Append("[bold fuchsia]/c[/] [aqua]清空[/] 清除屏幕\n");
            sb.Append("[bold fuchsia]/q[/] [aqua]退出[/] 退出程序\n");
            Markup markup = new Markup(sb.ToString());

            table.AddColumn(new TableColumn("Two"));

            Table rightTable = new Table()
                .HideHeaders()
                .Border(TableBorder.None)
                .AddColumn(new TableColumn("Content"));

            rightTable.AddRow(header)
                .AddRow(header2)
                .AddEmptyRow()
                .AddEmptyRow()
                .AddRow(markup);
            table.AddRow(rightTable);

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }

    }

}