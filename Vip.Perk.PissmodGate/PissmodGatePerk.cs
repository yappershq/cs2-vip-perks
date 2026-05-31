using System.Collections.Generic;
using Vip.Shared.Perks;

namespace Vip.Perk.PissmodGate;

internal sealed class PissmodGatePerk : IVipPerk
{
    public string Id             => "pissmodGate";
    public string DisplayName    => "VIP-only PissMod";
    public string Description    => "Restrict !piss to VIPs only.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings => [];

    public void OnPreferencesChanged(ulong steamId, VipPerkPreferences prefs) { }
}
