using System.Collections.Generic;
using Vip.Shared.Perks;

namespace Vip.Perk.WeaponMenuGate;

internal sealed class WeaponMenuGatePerk : IVipPerk
{
    public string Id             => "weaponMenuGate";
    public string DisplayName    => "VIP-only Weapon Menu";
    public string Description    => "Restrict !guns / !primary / !secondary / !<weapon> to VIPs only.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings => [];

    public void OnPreferencesChanged(ulong steamId, VipPerkPreferences prefs) { }
}
