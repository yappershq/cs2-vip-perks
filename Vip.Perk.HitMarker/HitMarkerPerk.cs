using System.Collections.Generic;
using Vip.Shared.Perks;

namespace Vip.Perk.HitMarker;

internal sealed class HitMarkerPerk : IVipPerk
{
    public string Id             => "hitMarker";
    public string DisplayName    => "VIP Hit Markers";
    public string Description    => "Restrict floating damage numbers / hitmarker particles to VIPs only.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings => [];

    public void OnPreferencesChanged(ulong steamId, VipPerkPreferences prefs) { }
}
