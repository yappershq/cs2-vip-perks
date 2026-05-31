using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.HookParams;
using Sharp.Shared.Managers;
using Vip.Perk.DefuseKit.Configuration;
using Vip.Shared;
using Vip.Shared.Perks;

namespace Vip.Perk.DefuseKit;

internal sealed class DefuseKitPerk : IVipPerk
{
    public string Id          => "defuse_kit";
    public string DisplayName => "Defuse Kit";
    public string Description => "Gives CT-side VIP players a free defuse kit on spawn.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings { get; }

    private IVipShared?       _vip;
    private IVipPerkRegistry? _registry;

    private readonly IHookManager      _hookManager;
    private readonly ILogger           _logger;
    private readonly DefuseKitConfig   _config;

    private readonly Dictionary<ulong, int> _usageCounts = new();

    private readonly Action<IPlayerSpawnForwardParams> _onSpawn;

    public DefuseKitPerk(ISharedSystem sharedSystem, ILogger logger, DefuseKitConfig config)
    {
        _hookManager = sharedSystem.GetHookManager();
        _logger      = logger;
        _config      = config;
        _onSpawn     = OnPlayerSpawn;

        Settings =
        [
            new VipPerkSetting("usageLimit", "Usage Limit (-1 = unlimited)", VipPerkSettingType.Int,
                _config.UsageLimit.ToString(), "-1", "100"),
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
        if (controller.Team is CStrikeTeam.TE) return;
        if (_vip?.IsVip(controller.SteamId) is not true) return;

        var prefs = _registry?.GetPreferences(controller.SteamId, Id);
        if (prefs is null || !prefs.Enabled) return;

        prefs.Settings.TryGetValue("usageLimit", out var raw);
        var limit = int.TryParse(raw, out var l) ? l : _config.UsageLimit;

        if (limit >= 0)
        {
            _usageCounts.TryGetValue(controller.SteamId, out var count);
            if (count >= limit) return;
            _usageCounts[controller.SteamId] = count + 1;
        }

        var pawn = controller.GetPlayerPawn();
        if (pawn?.IsValidEntity is not true) return;

        var itemSvc = pawn.GetItemService();
        if (itemSvc is null) return;

        itemSvc.HasDefuser = true;
        _logger.LogInformation("[Vip.Perk.DefuseKit] gave kit to {n}", controller.PlayerName);
    }
}
