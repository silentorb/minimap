using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>One ratio band: applies when source/max &lt;= <see cref="MaxRatio"/> (first match wins).</summary>
public readonly record struct ResourceRatioBand(float MaxRatio, int Amount);

/// <summary>
/// Periodically modifies a target resource based on source/max ratio bands.
/// Bands must be ordered by ascending <see cref="ResourceRatioBand.MaxRatio"/>.
/// </summary>
public sealed class ModifyResourceByRatioBandsEffect : AccessoryEffect, IPassiveEffect
{
    private readonly ResourceRatioBand[] _bands;
    private float _elapsed;

    public ModifyResourceByRatioBandsEffect(
        TagId sourceTag,
        TagId targetTag,
        float periodSeconds,
        IReadOnlyList<ResourceRatioBand> bands)
    {
        if (periodSeconds <= 0f)
            throw new ArgumentOutOfRangeException(nameof(periodSeconds));
        ArgumentNullException.ThrowIfNull(bands);
        if (bands.Count == 0)
            throw new ArgumentException("At least one band is required.", nameof(bands));

        SourceTag = sourceTag;
        TargetTag = targetTag;
        PeriodSeconds = periodSeconds;
        _bands = bands.ToArray();
        for (var i = 1; i < _bands.Length; i++)
        {
            if (_bands[i].MaxRatio < _bands[i - 1].MaxRatio)
                throw new ArgumentException("Bands must be ordered by ascending maxRatio.", nameof(bands));
        }
    }

    public TagId SourceTag { get; }

    public TagId TargetTag { get; }

    public float PeriodSeconds { get; }

    public IReadOnlyList<ResourceRatioBand> Bands => _bands;

    public void Tick(Actor actor, float dt)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (dt <= 0f)
            return;

        _elapsed += dt;
        while (_elapsed >= PeriodSeconds)
        {
            _elapsed -= PeriodSeconds;
            ApplyPulse(actor);
        }
    }

    public static int ResolveAmount(int source, int sourceMax, IReadOnlyList<ResourceRatioBand> bands)
    {
        ArgumentNullException.ThrowIfNull(bands);
        var ratio = sourceMax <= 0 ? 0f : source / (float)sourceMax;
        foreach (var band in bands)
        {
            if (ratio <= band.MaxRatio)
                return band.Amount;
        }

        return bands[^1].Amount;
    }

    private void ApplyPulse(Actor actor)
    {
        var sourceMax = 0;
        if (actor.ResourceContext.TryGet(SourceTag, out var sourceDef) &&
            sourceDef?.LimitTag is { } limitTag)
        {
            sourceMax = actor.GetResource(limitTag);
        }

        var amount = ResolveAmount(actor.GetResource(SourceTag), sourceMax, _bands);
        if (amount != 0)
            actor.AddResource(TargetTag, amount);
    }

    public override AccessoryEffect Clone() =>
        new ModifyResourceByRatioBandsEffect(SourceTag, TargetTag, PeriodSeconds, _bands);
}
