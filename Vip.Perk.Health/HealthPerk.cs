using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.HookParams;
using Sharp.Shared.Managers;
using Vip.Shared;
using Vip.Shared.Perks;

namespace Vip.Perk.Health;

internal sealed class HealthPerk : IVipPerk
{
    public string Id          => "health";
    public string DisplayName => "Health";
    public string Description => "Grants extra HP on spawn.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings { get; } =
    [
        new VipPerkSetting("healthValue", "Health Value (total HP)", VipPerkSettingType.Int, "150", "100", "500"),
    ];

    private IVipShared?       _vip;
    private IVipPerkRegistry? _registry;

    private readonly IHookManager _hookManager;
    private readonly ILogger      _logger;

    private readonly Action<IPlayerSpawnForwardParams> _onSpawn;

    public HealthPerk(ISharedSystem sharedSystem, ILogger logger)
    {
        _hookManager = sharedSystem.GetHookManager();
        _logger      = logger;
        _onSpawn     = OnPlayerSpawn;
    }

    internal void SetDependencies(IVipShared vip, IVipPerkRegistry registry)
    {
        _vip      = vip;
        _registry = registry;
    }

    internal void Install()   => _hookManager.PlayerSpawnPost.InstallForward(_onSpawn);
    internal void Uninstall() => _hookManager.PlayerSpawnPost.RemoveForward(_onSpawn);

    public void OnPreferencesChanged(ulong steamId, VipPerkPreferences prefs) { }

    private void OnPlayerSpawn(IPlayerSpawnForwardParams parms)
    {
        var controller = parms.Controller;
        if (controller?.IsValidEntity is not true) return;
        if (_vip?.IsVip(controller.SteamId) is not true) return;

        var prefs = _registry?.GetPreferences(controller.SteamId, Id);
        if (prefs is null || !prefs.Enabled) return;

        var pawn = controller.GetPlayerPawn();
        if (pawn?.IsValidEntity is not true) return;

        prefs.Settings.TryGetValue("healthValue", out var raw);
        var hp = int.TryParse(raw, out var h) ? h : 150;

        pawn.Health    = hp;
        pawn.MaxHealth = hp;

        _logger.LogInformation("[Vip.Perk.Health] hp={h} for {n}", hp, controller.PlayerName);
    }
}
