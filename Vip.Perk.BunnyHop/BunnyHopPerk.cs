using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.HookParams;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using Vip.Shared;
using Vip.Shared.Perks;

namespace Vip.Perk.BunnyHop;

internal sealed class BunnyHopPerk : IVipPerk
{
    public string Id          => "bunny_hop";
    public string DisplayName => "BunnyHop";
    public string Description => "Enables auto-bunny-hopping for VIP players.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings { get; } =
    [
        new VipPerkSetting("autoHopBoost", "Auto-Hop Boost", VipPerkSettingType.Float, "1.1", "1.0", "1.5"),
    ];

    private IVipShared?       _vip;
    private IVipPerkRegistry? _registry;

    private readonly IHookManager   _hookManager;
    private readonly IConVarManager _conVarManager;
    private readonly ILogger        _logger;

    private readonly Func<IPlayerRunCommandHookParams, HookReturnValue<EmptyHookReturn>, HookReturnValue<EmptyHookReturn>> _onRunCommand;
    private IConVar? _autoBhopCvar;

    public BunnyHopPerk(ISharedSystem sharedSystem, ILogger logger)
    {
        _hookManager   = sharedSystem.GetHookManager();
        _conVarManager = sharedSystem.GetConVarManager();
        _logger        = logger;
        _onRunCommand  = OnPlayerRunCommandPre;
    }

    internal void SetDependencies(IVipShared vip, IVipPerkRegistry registry)
    {
        _vip      = vip;
        _registry = registry;
    }

    internal void Install()
    {
        _autoBhopCvar = _conVarManager.FindConVar("sv_autobunnyhopping");
        if (_autoBhopCvar is not null)
            _autoBhopCvar.Flags &= ~ConVarFlags.Replicated;
        _hookManager.PlayerRunCommand.InstallHookPre(_onRunCommand);
    }

    internal void Uninstall() => _hookManager.PlayerRunCommand.RemoveHookPre(_onRunCommand);

    public void OnPreferencesChanged(ulong steamId, VipPerkPreferences prefs) { }

    private HookReturnValue<EmptyHookReturn> OnPlayerRunCommandPre(IPlayerRunCommandHookParams parms, HookReturnValue<EmptyHookReturn> ret)
    {
        var controller = parms.Controller;
        var client     = parms.Client;
        if (controller?.IsValidEntity is not true || client.IsValid is not true) return new();
        if (_vip?.IsVip(controller.SteamId) is not true) return new();

        var prefs = _registry?.GetPreferences(controller.SteamId, Id);
        if (prefs is null || !prefs.Enabled) return new();

        _autoBhopCvar?.ReplicateToClient(client, "1");
        _autoBhopCvar?.Set(1);

        return new();
    }
}
