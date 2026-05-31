using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.HookParams;
using Sharp.Shared.Listeners;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using Vip.Perk.Jumps.Configuration;
using Vip.Shared;
using Vip.Shared.Perks;

namespace Vip.Perk.Jumps;

internal sealed class JumpsPerk : IVipPerk, IEventListener
{
    public string Id          => "jumps";
    public string DisplayName => "Extra Jumps";
    public string Description => "Allows VIP players to jump additional times mid-air.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings { get; }

    private IVipShared?       _vip;
    private IVipPerkRegistry? _registry;

    private readonly IHookManager  _hookManager;
    private readonly IEventManager _eventManager;
    private readonly ILogger       _logger;
    private readonly JumpsConfig   _config;

    private readonly Action<IPlayerSpawnForwardParams> _onSpawn;
    private readonly Func<IPlayerRunCommandHookParams, HookReturnValue<EmptyHookReturn>, HookReturnValue<EmptyHookReturn>> _onRunCommand;

    private readonly Dictionary<ulong, JumpState> _states = new();

    public JumpsPerk(ISharedSystem sharedSystem, ILogger logger, JumpsConfig config)
    {
        _hookManager  = sharedSystem.GetHookManager();
        _eventManager = sharedSystem.GetEventManager();
        _logger       = logger;
        _config       = config;
        _onSpawn      = OnPlayerSpawn;
        _onRunCommand = OnPlayerRunCommandPre;

        Settings =
        [
            new VipPerkSetting("extraJumpsCT", "Extra Jumps (CT)", VipPerkSettingType.Int,
                _config.TeamExtraJumps.TryGetValue("CT", out var ct) ? ct.ToString() : "1", "0", "5"),
            new VipPerkSetting("extraJumpsT", "Extra Jumps (T)", VipPerkSettingType.Int,
                _config.TeamExtraJumps.TryGetValue("T", out var t) ? t.ToString() : "1", "0", "5"),
        ];
    }

    internal void SetDependencies(IVipShared vip, IVipPerkRegistry registry)
    {
        _vip      = vip;
        _registry = registry;
    }

    internal void Install()
    {
        _hookManager.PlayerSpawnPost.InstallForward(_onSpawn);
        _hookManager.PlayerRunCommand.InstallHookPre(_onRunCommand);
        _eventManager.HookEvent("round_start");
        _eventManager.InstallEventListener(this);
    }

    internal void Uninstall()
    {
        _hookManager.PlayerSpawnPost.RemoveForward(_onSpawn);
        _hookManager.PlayerRunCommand.RemoveHookPre(_onRunCommand);
        _eventManager.RemoveEventListener(this);
        _states.Clear();
    }

    public void OnPreferencesChanged(ulong steamId, VipPerkPreferences prefs) { }

    public void FireGameEvent(IGameEvent e)
    {
        if (!e.Name.Equals("round_start", StringComparison.OrdinalIgnoreCase)) return;
        foreach (var s in _states.Values) s.JumpsCount = 0;
    }

    private void OnPlayerSpawn(IPlayerSpawnForwardParams parms)
    {
        var controller = parms.Controller;
        if (controller?.IsValidEntity is not true) return;
        if (_vip?.IsVip(controller.SteamId) is not true) return;

        var prefs = _registry?.GetPreferences(controller.SteamId, Id);
        if (prefs is null || !prefs.Enabled) return;

        var teamKey = controller.Team == CStrikeTeam.CT ? "CT" : "T";
        var settingKey = controller.Team == CStrikeTeam.CT ? "extraJumpsCT" : "extraJumpsT";

        prefs.Settings.TryGetValue(settingKey, out var raw);
        _config.TeamExtraJumps.TryGetValue(teamKey, out var configDefault);
        var extra = int.TryParse(raw, out var e) ? e : configDefault;

        if (_states.TryGetValue(controller.SteamId, out var s))
        {
            s.JumpsCount  = 0;
            s.ExtraJumps  = extra;
        }
        else
        {
            _states[controller.SteamId] = new JumpState { ExtraJumps = extra };
        }
    }

    private HookReturnValue<EmptyHookReturn> OnPlayerRunCommandPre(IPlayerRunCommandHookParams parms, HookReturnValue<EmptyHookReturn> ret)
    {
        var controller = parms.Controller;
        if (controller?.IsValidEntity is not true) return new();
        if (_vip?.IsVip(controller.SteamId) is not true) return new();

        var prefs = _registry?.GetPreferences(controller.SteamId, Id);
        if (prefs is null || !prefs.Enabled) return new();

        if (!_states.TryGetValue(controller.SteamId, out var jumpData)) return new();

        var pawn = controller.GetPlayerPawn();
        if (pawn?.IsValidEntity is not true) return new();

        var buttons = parms.KeyButtons;
        var flags   = pawn.Flags;

        if ((flags & EntityFlags.OnGround) != 0)
        {
            jumpData.JumpsCount = 0;
        }
        else if ((jumpData.PrevButtons & UserCommandButtons.Jump) == 0
                 && (buttons & UserCommandButtons.Jump) != 0
                 && jumpData.JumpsCount < jumpData.ExtraJumps)
        {
            jumpData.JumpsCount++;
            var boost = (buttons & UserCommandButtons.Duck) != 0 ? 320 : 300;
            pawn.ApplyAbsVelocityImpulse(new Vector(0, 0, boost));
        }

        jumpData.PrevButtons = buttons;
        jumpData.PrevFlags   = flags;

        return new();
    }

    int IEventListener.ListenerVersion  => IEventListener.ApiVersion;
    int IEventListener.ListenerPriority => 0;

    private sealed class JumpState
    {
        public int                ExtraJumps  { get; set; } = 1;
        public int                JumpsCount  { get; set; }
        public UserCommandButtons PrevButtons { get; set; }
        public EntityFlags        PrevFlags   { get; set; }
    }
}
