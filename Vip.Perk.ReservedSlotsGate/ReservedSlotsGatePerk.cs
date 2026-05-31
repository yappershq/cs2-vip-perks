using System.Collections.Generic;
using Vip.Shared.Perks;

namespace Vip.Perk.ReservedSlotsGate;

internal sealed class ReservedSlotsGatePerk : IVipPerk
{
    public string Id             => "reservedSlotsGate";
    public string DisplayName    => "Reserved Slot VIP";
    public string Description    => "Tell ReservedSlots which players count as VIP.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings => [];

    public void OnPreferencesChanged(ulong steamId, VipPerkPreferences prefs) { }
}
