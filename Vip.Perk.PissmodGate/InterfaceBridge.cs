using PissMod.Shared;
using Sharp.Shared;
using Sharp.Shared.Managers;
using Vip.Shared;
using Vip.Shared.Perks;

namespace Vip.Perk.PissmodGate;

internal sealed class InterfaceBridge
{
    internal ISharedSystem       SharedSystem       { get; }
    internal ISharpModuleManager SharpModuleManager { get; }

    internal IVipShared?       VipShared     { get; private set; }
    internal IVipPerkRegistry? PerkRegistry  { get; private set; }
    internal IPissModShared?   PissModShared { get; private set; }

    internal InterfaceBridge(ISharedSystem sharedSystem)
    {
        SharedSystem       = sharedSystem;
        SharpModuleManager = sharedSystem.GetSharpModuleManager();
    }

    internal bool Resolve()
    {
        VipShared     ??= SharpModuleManager.GetOptionalSharpModuleInterface<IVipShared>(IVipShared.Identity)?.Instance;
        PerkRegistry  ??= SharpModuleManager.GetOptionalSharpModuleInterface<IVipPerkRegistry>(IVipPerkRegistry.Identity)?.Instance;
        PissModShared ??= SharpModuleManager.GetOptionalSharpModuleInterface<IPissModShared>(IPissModShared.Identity)?.Instance;
        return VipShared is not null && PerkRegistry is not null && PissModShared is not null;
    }
}
