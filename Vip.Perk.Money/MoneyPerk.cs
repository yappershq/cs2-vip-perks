using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.HookParams;
using Sharp.Shared.Managers;
using Vip.Perk.Money.Configuration;
using Vip.Shared;
using Vip.Shared.Perks;

namespace Vip.Perk.Money;

internal sealed class MoneyPerk : IVipPerk
{
    public string Id          => "money";
    public string DisplayName => "Money";
    public string Description => "Gives VIP players a bonus cash amount on round start.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings { get; }

    private IVipShared?       _vip;
    private IVipPerkRegistry? _registry;

    private readonly IHookManager _hookManager;
    private readonly IModSharp    _modSharp;
    private readonly ILogger      _logger;
    private readonly MoneyConfig  _config;

    private readonly Action<IPlayerSpawnForwardParams> _onSpawn;

    public MoneyPerk(ISharedSystem sharedSystem, ILogger logger, MoneyConfig config)
    {
        _hookManager = sharedSystem.GetHookManager();
        _modSharp    = sharedSystem.GetModSharp();
        _logger      = logger;
        _config      = config;
        _onSpawn     = OnPlayerSpawn;

        Settings =
        [
            new VipPerkSetting("bonusAmount", "Bonus Cash", VipPerkSettingType.Int,
                _config.BonusAmount.ToString(), "0", "16000"),
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

        prefs.Settings.TryGetValue("bonusAmount", out var raw);
        var bonus = int.TryParse(raw, out var b) ? b : _config.BonusAmount;

        _modSharp.PushTimer(() =>
        {
            var ctrl = controller;
            if (ctrl?.IsValidEntity is not true) return;
            var moneySvc = ctrl.GetInGameMoneyService();
            if (moneySvc is null) return;
            moneySvc.Account += bonus;
            _logger.LogInformation("[Vip.Perk.Money] +{m} for {n}", bonus, ctrl.PlayerName);
        }, 0.1f);
    }
}
