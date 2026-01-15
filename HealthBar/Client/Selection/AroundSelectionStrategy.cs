using System;
using System.Collections.Generic;
using Vintagestory.API.Common.Entities;

namespace HealthBar.Client.Selection;

public sealed class AroundSelectionStrategy : ISelectionStrategy
{
    private readonly List<Candidate> _candidates = new(256);

    public void Select(in SelectionContext ctx, List<Entity> output)
    {
        output.Clear();

        var target = ctx.Target;
        var targetValid = SelectionUtil.IsEligible(target, ctx.Config);
        var excludeId = targetValid ? target!.EntityId : -1;

        var cap = Math.Max(0, ctx.Config.MaxBarsDisplayed);
        if (cap == 0)
            return;

        if (ctx.Config.AlwaysShowTargetInAround && targetValid)
            output.Add(target!);

        var remaining = cap - output.Count;
        if (remaining <= 0) return;

        CollectCandidates(in ctx, excludeId);
        TakeClosest(remaining, output);
    }

    private void CollectCandidates(in SelectionContext ctx, long excludeId)
    {
        _candidates.Clear();

        var range = ctx.Config.DisplayRange;
        var ents = ctx.Api.World.GetEntitiesAround(ctx.PlayerPos, range, range);
        var playerId = ctx.PlayerEntity.EntityId;

        foreach (var e in ents)
        {
            if (!SelectionUtil.IsEligible(e, ctx.Config)) continue;
            if (e.EntityId == playerId) continue;
            if (excludeId >= 0 && e.EntityId == excludeId) continue;

            if (!SelectionUtil.IsInFov(ctx.PlayerPos, e.Pos.XYZ, ctx.Forward, ctx.CosHalfFov))
                continue;

            var dx = e.Pos.X - ctx.PlayerPos.X;
            var dz = e.Pos.Z - ctx.PlayerPos.Z;
            var d2 = dx * dx + dz * dz;
            _candidates.Add(new Candidate(e, d2));
        }

        _candidates.Sort(CandidateComparer.Instance);
    }

    private void TakeClosest(int take, List<Entity> dest)
    {
        var limit = Math.Min(take, _candidates.Count);
        for (var i = 0; i < limit; i++) dest.Add(_candidates[i].Entity);
    }

    private readonly struct Candidate(Entity e, double d2)
    {
        public readonly Entity Entity = e;
        public readonly double Dist2 = d2;
    }

    private sealed class CandidateComparer : IComparer<Candidate>
    {
        public static readonly CandidateComparer Instance = new();
        public int Compare(Candidate a, Candidate b) => a.Dist2.CompareTo(b.Dist2);
    }
}
