# cs2-vip-perks

Fifteen independent ModSharp external modules, one per VIP perk. Each perk compiles to its own `.dll` and integrates with `Vip.Core` via `IVipPerkRegistry`. Drop any perk `.dll` into your ModSharp modules directory; omit it to remove that perk entirely — no changes to `Vip.Core` required. All perks default to `enabled: false` in their per-perk JSON config and `DefaultEnabled = false` in code; flip both to activate.
