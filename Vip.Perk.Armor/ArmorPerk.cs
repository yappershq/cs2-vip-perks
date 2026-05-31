using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.HookParams;
using Sharp.Shared.Managers;
using Vip.Perk.Armor.Configuration;
using Vip.Shared;
using Vip.Shared.Perks;

namespace Vip.Perk.Armor;

internal sealed class ArmorPerk : IVipPerk
{
    public string Id          => "armor";
    public string DisplayName => "Armor";
    public string Description => "Grants kevlar on spawn.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings { get; }

    private IVipShared?       _vip;
    private IVipPerkRegistry? _registry;

    private readonly IHookManager  _hookManager;
    private readonly ILogger       _logger;
    private readonly ArmorConfig   _config;

    private readonly Action<IPlayerSpawnForwardParams> _onSpawn;

    public ArmorPerk(ISharedSystem sharedSystem, ILogger logger, ArmorConfig config)
    {
        _hookManager = sharedSystem.GetHookManager();
        _logger      = logger;
        _config      = config;
        _onSpawn     = OnPlayerSpawn;

        Settings =
        [
            new VipPerkSetting("armorValue", "Armor Value", VipPerkSettingType.Int,
                _config.ArmorValue.ToString(), "1", "100"),
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

        prefs.Settings.TryGetValue("armorValue", out var raw);
        var armor = int.TryParse(raw, out var k) ? k : _config.ArmorValue;

        pawn.ArmorValue = armor;
        _logger.LogInformation("[Vip.Perk.Armor] armor={a} for {n}", armor, controller.PlayerName);
    }
}
