using System.Collections.Generic;
using Vip.Shared.Perks;

namespace Vip.Perk.PoopGate;

internal sealed class PoopGatePerk : IVipPerk
{
    public string Id             => "poopGate";
    public string DisplayName    => "VIP-only Poop";
    public string Description    => "Restrict !poop to VIPs only.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings => [];

    public void OnPreferencesChanged(ulong steamId, VipPerkPreferences prefs) { }
}
