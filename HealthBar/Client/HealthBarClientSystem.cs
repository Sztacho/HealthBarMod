using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HealthBar.Client.Selection;
using HealthBar.Client.Visibility;
using HealthBar.Config;
using HealthBar.Rendering;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;

namespace HealthBar.Client;

public sealed class HealthBarClientSystem : IDisposable
{
    private const float ScanInterval = 0.12f;
    private const float LosBaseInterval = 0.32f;
    private const float LosJitter = 0.12f;

    private const int MaxRaycastsPerTick = 3;

    private readonly ICoreClientAPI _capi;
    private readonly IConfigProvider _config;
    private readonly ILineOfSightService _los;

    private readonly ISelectionStrategy _targetOnlyStrategy;
    private readonly ISelectionStrategy _aroundStrategy;

    private readonly Dictionary<long, BarState> _states = new(256);
    private readonly List<Entity> _selected = new(32);
    private readonly List<long> _toRemove = new(64);

    private readonly HealthBarBatchRenderer _renderer;

    private float _scanAcc;
    private float _now;

    public HealthBarClientSystem(ICoreClientAPI capi, IConfigProvider config)
    {
        _capi = capi;
        _config = config;
        _los = new RaycastLineOfSightService(capi);

        _targetOnlyStrategy = new TargetOnlySelectionStrategy();
        _aroundStrategy = new AroundSelectionStrategy();

        _renderer = new HealthBarBatchRenderer(capi, config, _states);
        capi.Event.RegisterRenderer(_renderer, EnumRenderStage.Ortho);

        capi.Event.RegisterGameTickListener(OnGameTick, 16);
        config.Changed += OnSettingsChanged;
    }

    private void OnSettingsChanged()
    {
        _scanAcc = ScanInterval;
        _renderer.InvalidateTheme();
    }

    private void OnGameTick(float dt)
    {
        _now += dt;

        var cfg = _config.Snapshot;
        if (!cfg.Enabled)
        {
            FadeEverythingOut();
            CleanupDead();
            return;
        }

        _scanAcc += dt;
        if (_scanAcc >= ScanInterval)
        {
            _scanAcc = 0f;
            SelectEntities(cfg);
        }

        var rayBudget = MaxRaycastsPerTick;

        foreach (var st in _states.Select(kv => kv.Value))
        {
            if (st.Entity is not { Alive: true })
            {
                st.DesiredVisible = false;
                st.VisibleThisFrame = false;
                st.MarkDead();
                continue;
            }

            if (cfg.TargetOnly)
            {
                st.HasLineOfSight = true;
            }
            else if (st.DesiredVisible && _now >= st.NextLosCheckAt && rayBudget > 0)
            {
                rayBudget--;
                st.HasLineOfSight = _los.HasLineOfSight(st.Entity);
                st.NextLosCheckAt = _now + LosBaseInterval + ComputeJitter(st.Entity.EntityId) * LosJitter;
            }

            st.VisibleThisFrame = st.DesiredVisible && st.HasLineOfSight;
        }

        CleanupDead();
    }

    private void FadeEverythingOut()
    {
        foreach (var kv in _states)
        {
            kv.Value.DesiredVisible = false;
            kv.Value.VisibleThisFrame = false;
            kv.Value.MarkDead();
        }
    }

    private void SelectEntities(in ModConfigSnapshot cfg)
    {
        var player = _capi.World.Player;
        if (player?.Entity == null) return;

        var ctx = new SelectionContext(_capi, cfg);
        if (cfg.TargetOnly)
            _targetOnlyStrategy.Select(in ctx, _selected);
        else
            _aroundStrategy.Select(in ctx, _selected);

        foreach (var kv in _states)
        {
            kv.Value.DesiredVisible = false;
            kv.Value.IsTarget = false;
        }

        var target = ctx.Target;
        var targetValid = SelectionUtil.IsEligible(target, cfg);

        foreach (var e in _selected.Where(e => e != null))
        {
            if (!_states.TryGetValue(e.EntityId, out var st))
            {
                st = new BarState(e);
                _states[e.EntityId] = st;
                st.NextLosCheckAt = _now + ComputeJitter(e.EntityId) * 0.1f;
            }
            else
            {
                st.Entity = e;
            }

            st.DesiredVisible = true;
            st.IsTarget = targetValid && target!.EntityId == e.EntityId;

            if (!cfg.TargetOnly && st.NextLosCheckAt <= 0)
                st.NextLosCheckAt = _now + ComputeJitter(e.EntityId) * 0.1f;
        }

        foreach (var st in _states.Select(kv => kv.Value))
        {
            if (!st.DesiredVisible) st.MarkDead();
            else st.IsDead = false;
        }
    }

    private void CleanupDead()
    {
        _toRemove.Clear();
        foreach (var kv in from kv in _states let st = kv.Value where st.IsDead && st.Opacity <= 0.001f select kv)
        {
            _toRemove.Add(kv.Key);
        }

        foreach (var t in _toRemove)
            _states.Remove(t);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float ComputeJitter(long id)
    {
        unchecked
        {
            var x = (uint)id;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            return (x & 0xFFFF) / 65536f;
        }
    }

    public void Dispose()
    {
        _config.Changed -= OnSettingsChanged;
        _capi.Event.UnregisterRenderer(_renderer, EnumRenderStage.Ortho);
        _states.Clear();
        _renderer.Dispose();
    }
}
