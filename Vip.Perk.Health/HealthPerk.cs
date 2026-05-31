using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.HookParams;
using Sharp.Shared.Managers;
using Vip.Perk.Health.Configuration;
using Vip.Shared;
using Vip.Shared.Perks;

namespace Vip.Perk.Health;

internal sealed class HealthPerk : IVipPerk
{
    public string Id          => "health";
    public string DisplayName => "Health";
    public string Description => "Grants extra HP on spawn.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings { get; }

    private IVipShared?       _vip;
    private IVipPerkRegistry? _registry;

    private readonly IHookManager  _hookManager;
    private readonly ILogger       _logger;
    private readonly HealthConfig  _config;

    private readonly Action<IPlayerSpawnForwardParams> _onSpawn;

    public HealthPerk(ISharedSystem sharedSystem, ILogger logger, HealthConfig config)
    {
        _hookManager = sharedSystem.GetHookManager();
        _logger      = logger;
        _config      = config;
        _onSpawn     = OnPlayerSpawn;

        Settings =
        [
            new VipPerkSetting("healthValue", "Health Value (total HP)", VipPerkSettingType.Int,
                _config.HealthValue.ToString(), "100", "500"),
        ];
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
        var hp = int.TryParse(raw, out var h) ? h : _config.HealthValue;

        pawn.Health    = hp;
        pawn.MaxHealth = hp;

        _logger.LogInformation("[Vip.Perk.Health] hp={h} for {n}", hp, controller.PlayerName);
    }
}
