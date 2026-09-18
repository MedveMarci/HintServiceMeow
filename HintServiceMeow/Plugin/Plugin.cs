using System;
using HintServiceMeow.ApiFeatures;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.Core.Utilities.Patch;
using HintServiceMeow.Core.Utilities.Tools;
using HintServiceMeow.Core.Utilities.UnityAdaptors;
using HintServiceMeow.UI.Utilities;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Loader;
using LabApi.Loader.Features.Plugins.Enums;

namespace HintServiceMeow.Plugin;

internal class Plugin : LabApi.Loader.Features.Plugins.Plugin
{
    public static Plugin Instance { get; private set; } = null!;

    public override string Name => "HintServiceMeow";

    public override string Author => "MeowServer";

    public override Version Version => new(6, 0, 0);

    public override Version RequiredApiVersion => new(LabApiProperties.CompiledVersion);

    public override string Description => "A hint framework";

    public override LoadPriority Priority => LoadPriority.Highest;

    public PluginConfig Config { get; private set; } = null!;

    public override void LoadConfigs()
    {
        base.LoadConfigs();

        Config = this.LoadConfig<PluginConfig>("config.yml") ?? throw new NullReferenceException("Could not load plugin config!");
    }

    public override void Enable()
    {
        Instance = this;

        ServerEvents.WaitingForPlayers += OnWaitingForPlayers;
        PlayerEvents.Left += OnLeft;

        // Initialize Components
        _ = FontTool.Instance;
        _ = ConcurrentTaskDispatcher.Instance;

        NetworkTimeCache.Initialize(new UnityCoroutineRunner());
    }

    public override void Disable()
    {
        PlayerEvents.Left -= OnLeft;
        ServerEvents.WaitingForPlayers -= OnWaitingForPlayers;
    }

    private static void OnWaitingForPlayers()
    {
        Patcher.Patch();
        VersionManager.CheckForUpdates();
    }

    private static void OnLeft(PlayerLeftEventArgs ev)
    {
        PlayerUI.Destruct(ev.Player.ReferenceHub);
        PlayerDisplay.Dispose(ev.Player.ReferenceHub);
    }
}
