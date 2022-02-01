using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remotely.Desktop.Core;
using Remotely.Desktop.Core.Interfaces;
using Remotely.Desktop.Core.Services;
using Remotely.Desktop.XPlat.Services;
using Remotely.Desktop.XPlat.Views;
using Remotely.Shared.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Remotely.Desktop.XPlat
{
    public class App : Application
    {
        private static IServiceProvider Services => ServiceContainer.Instance;
        public static int ProcessID { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            //if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            //{
            //    desktop.MainWindow = new MainWindow
            //    {
            //        DataContext = new MainWindowViewModel(),
            //    };
            //}

            base.OnFrameworkInitializationCompleted();

            _ = Task.Run(Startup);
        }

        private void BuildServices()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder =>
            {
                builder.AddConsole().AddDebug();
            });

            serviceCollection.AddSingleton<IScreenCaster, ScreenCaster>();
            serviceCollection.AddSingleton<ICasterSocket, CasterSocket>();
            serviceCollection.AddSingleton<IdleTimer>();
            serviceCollection.AddSingleton<Conductor>();
            serviceCollection.AddSingleton<IChatClientService, ChatHostService>();
            serviceCollection.AddTransient<Viewer>();
            serviceCollection.AddScoped<IWebRtcSessionFactory, WebRtcSessionFactory>();
            serviceCollection.AddScoped<IDtoMessageHandler, DtoMessageHandler>();
            serviceCollection.AddScoped<IDeviceInitService, DeviceInitService>();

            switch (EnvironmentHelper.Platform)
            {
                case Shared.Enums.Platform.Linux:
                    {
                        serviceCollection.AddSingleton<IKeyboardMouseInput, KeyboardMouseInputLinux>();
                        serviceCollection.AddSingleton<IClipboardService, ClipboardServiceLinux>();
                        serviceCollection.AddSingleton<IAudioCapturer, AudioCapturerLinux>();
                        serviceCollection.AddSingleton<IChatUiService, ChatUiServiceLinux>();
                        serviceCollection.AddTransient<IScreenCapturer, ScreenCapturerLinux>();
                        serviceCollection.AddScoped<IFileTransferService, FileTransferServiceLinux>();
                        serviceCollection.AddSingleton<ICursorIconWatcher, CursorIconWatcherLinux>();
                        serviceCollection.AddSingleton<ISessionIndicator, SessionIndicatorLinux>();
                        serviceCollection.AddSingleton<IShutdownService, ShutdownServiceLinux>();
                        serviceCollection.AddScoped<IRemoteControlAccessService, RemoteControlAccessServiceLinux>();
                        serviceCollection.AddScoped<IConfigService, ConfigServiceLinux>();
                    }
                    break;
                case Shared.Enums.Platform.MacOS:
                    {

                    }
                    break;
                default:
                    throw new PlatformNotSupportedException();
            }

            ServiceContainer.Instance = serviceCollection.BuildServiceProvider();
        }


        private async Task Startup()
        {

            BuildServices();

            var conductor = Services.GetRequiredService<Conductor>();

            var args = Environment.GetCommandLineArgs().SkipWhile(x => !x.StartsWith("-"));
            Logger.Write("Processing Args: " + string.Join(", ", args));
            conductor.ProcessArgs(args.ToArray());

            await Services.GetRequiredService<IDeviceInitService>().GetInitParams();

            if (conductor.Mode == Core.Enums.AppMode.Chat)
            {
                await Services.GetRequiredService<IChatClientService>().StartChat(conductor.RequesterID, conductor.OrganizationName);
            }
            else if (conductor.Mode == Core.Enums.AppMode.Unattended)
            {
                var casterSocket = Services.GetRequiredService<ICasterSocket>();
                await casterSocket.Connect(conductor.Host).ConfigureAwait(false);
                await casterSocket.SendDeviceInfo(conductor.ServiceID, Environment.MachineName, conductor.DeviceID, conductor.DeviceAlias, conductor.DeviceGroup).ConfigureAwait(false);
                await casterSocket.NotifyRequesterUnattendedReady(conductor.RequesterID).ConfigureAwait(false);
                Services.GetRequiredService<IdleTimer>().Start();
                Services.GetRequiredService<IClipboardService>().BeginWatching();
                Services.GetRequiredService<IKeyboardMouseInput>().Init();
            }
            else
            {
                AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
                await Dispatcher.UIThread.InvokeAsync(() => {
                    StartAgent();
                    this.RunWithMainWindow<MainWindow>();
                });
            }
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if (ProcessID > 0)
            {
                var process = Process.GetProcessById(ProcessID);
                process?.Kill();
            }
        }

        private static void StartAgent()
        {
            var conductor = Services.GetRequiredService<Conductor>();
            string agentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, EnvironmentHelper.AgentExecutableFileName);

            if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(agentPath)).Length > 0)
            {
                Logger.Write("Remotely_Agent alreading running", Shared.Enums.EventType.Info);
                return;
            }

            try
            {
                if (!File.Exists(agentPath))
                {
                    Logger.Write($"Remotely_Agent not found at: {agentPath}", Shared.Enums.EventType.Warning);
                    agentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Remotely_Agent.dll");
                    if (!File.Exists(agentPath))
                    {
                        Logger.Write($"Remotely_Agent not found at: {agentPath}", Shared.Enums.EventType.Warning);
                        return;
                    }
                }

                var cfgSrv = Services.GetService<IConfigService>();
                var config = cfgSrv.GetConfig();

                var host = conductor?.Host;
                if (string.IsNullOrWhiteSpace(host))
                {
                    host = config.Host;
                }

                var orgId = conductor?.OrganizationId;
                if (string.IsNullOrWhiteSpace(orgId))
                {
                    orgId = config.OrganizationId;
                }

                var args = $"{agentPath} " +
                    $"-device \"{conductor.DeviceID}\" " +
                    $"-host \"{host}\" " +
                    $"-organization \"{orgId}\"";
                ProcessID = StartLinuxApp(args);
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }

            if (ProcessID > 0)
            {
                Logger.Write($"Agent app started.  Process ID: {ProcessID}");
            }
            else
            {
                Logger.Write($"Agent app did not start successfully.");
                return;
            }
        }

        private static int StartLinuxApp(string args)
        {
            var xauthority = GetXorgAuth();

            var display = ":0";
            var whoString = EnvironmentHelper.StartProcessWithResults("who", "")?.Trim();
            var username = "";

            if (!string.IsNullOrWhiteSpace(whoString))
            {
                try
                {
                    var whoLine = whoString
                        .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                        .First();

                    var whoSplit = whoLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    username = whoSplit[0];
                    display = whoSplit.Last().TrimStart('(').TrimEnd(')');
                    xauthority = $"/home/{username}/.Xauthority";
                    args = $"-u {username} {args}";
                }
                catch (Exception ex)
                {
                    Logger.Write(ex);
                }
            }

            var psi = new ProcessStartInfo
            {
                FileName = "sudo",
                Arguments = args,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            psi.Environment.Add("DISPLAY", display);
            psi.Environment.Add("XAUTHORITY", xauthority);
            Logger.Write($"Attempting to launch Remotely_Agent with username {username}, xauthority {xauthority}, display {display}, and args {args}.");

            return Process.Start(psi).Id;
        }

        private static string GetXorgAuth()
        {
            try
            {
                var processes = EnvironmentHelper.StartProcessWithResults("ps", "-eaf")?.Split(Environment.NewLine);
                if ((processes?.Length) <= 0)
                {
                    return string.Empty;
                }

                var xorgLine = processes.FirstOrDefault(x => x.Contains("xorg", StringComparison.OrdinalIgnoreCase));
                if (string.IsNullOrWhiteSpace(xorgLine))
                {
                    return string.Empty;
                }

                var xorgSplit = xorgLine?.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                var authIndex = xorgSplit?.IndexOf("-auth");
                if (authIndex > -1 && xorgSplit?.Count >= authIndex + 1)
                {
                    var auth = xorgSplit[(int)authIndex + 1];

                    return string.IsNullOrWhiteSpace(auth) ? string.Empty : auth;
                }

                return string.Empty;
            }
            catch { }
            return string.Empty;
        }

    }
}
