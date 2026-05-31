using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.HookParams;
using Sharp.Shared.Managers;
using Sharp.Shared.Types;
using Vip.Shared;
using Vip.Shared.Perks;

namespace Vip.Perk.NoFallDamage;

internal sealed class NoFallDamagePerk : IVipPerk
{
    public string Id          => "no_fall_damage";
    public string DisplayName => "No Fall Damage";
    public string Description => "Prevents VIP players from taking fall damage.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings { get; } = [];

    private IVipShared?       _vip;
    private IVipPerkRegistry? _registry;

    private readonly IHookManager _hookManager;
    private readonly ILogger      _logger;

    private readonly Func<IPlayerDispatchTraceAttackHookParams, HookReturnValue<long>, HookReturnValue<long>> _onDamagePre;

    public NoFallDamagePerk(ISharedSystem sharedSystem, ILogger logger)
    {
        _hookManager = sharedSystem.GetHookManager();
        _logger      = logger;
        _onDamagePre = OnPlayerTakeDamagePre;
    }

    internal void SetDependencies(IVipShared vip, IVipPerkRegistry registry)
    {
        _vip      = vip;
        _registry = registry;
    }

    internal void Install()   => _hookManager.PlayerDispatchTraceAttack.InstallHookPre(_onDamagePre);
    internal void Uninstall() => _hookManager.PlayerDispatchTraceAttack.RemoveHookPre(_onDamagePre);

    public void OnPreferencesChanged(ulong steamId, VipPerkPreferences prefs) { }

    private HookReturnValue<long> OnPlayerTakeDamagePre(IPlayerDispatchTraceAttackHookParams parms, HookReturnValue<long> result)
    {
        if (parms.Controller?.IsValidEntity is not true) return default;
        if (parms.DamageType is not DamageFlagBits.Fall) return default;
        if (_vip?.IsVip(parms.Controller.SteamId) is not true) return default;

        var prefs = _registry?.GetPreferences(parms.Controller.SteamId, Id);
        if (prefs is null || !prefs.Enabled) return default;

        parms.Damage = 0;
        return default;
    }
}
