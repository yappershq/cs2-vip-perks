using Prefix.Poop.Shared;
using Sharp.Shared;
using Sharp.Shared.Managers;
using Vip.Shared;
using Vip.Shared.Perks;

namespace Vip.Perk.PoopGate;

internal sealed class InterfaceBridge
{
    internal ISharedSystem       SharedSystem       { get; }
    internal ISharpModuleManager SharpModuleManager { get; }

    internal IVipShared?    VipShared    { get; private set; }
    internal IVipPerkRegistry? PerkRegistry { get; private set; }
    internal IPoopShared?   PoopShared   { get; private set; }

    internal InterfaceBridge(ISharedSystem sharedSystem)
    {
        SharedSystem       = sharedSystem;
        SharpModuleManager = sharedSystem.GetSharpModuleManager();
    }

    internal bool Resolve()
    {
        VipShared    ??= SharpModuleManager.GetOptionalSharpModuleInterface<IVipShared>(IVipShared.Identity)?.Instance;
        PerkRegistry ??= SharpModuleManager.GetOptionalSharpModuleInterface<IVipPerkRegistry>(IVipPerkRegistry.Identity)?.Instance;
        PoopShared   ??= SharpModuleManager.GetOptionalSharpModuleInterface<IPoopShared>(IPoopShared.Identity)?.Instance;
        return VipShared is not null && PerkRegistry is not null && PoopShared is not null;
    }
}
