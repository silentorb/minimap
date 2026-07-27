using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Grows a planted food actor from seedling to mature/harvestable (or ambush emerge).</summary>
public sealed class GrowEffect : AccessoryEffect, IGrowEffect
{
    private float _elapsed;
    private float _postMatureElapsed;
    private bool _emerged;

    public GrowEffect(
        float durationSeconds,
        DepictionConfig matureDepiction,
        TagId? yieldResourceTag,
        int yieldAmount,
        string? emergeActorId = null,
        float emergeAfterMatureSeconds = 0f)
    {
        if (durationSeconds < 0f)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds));
        ArgumentNullException.ThrowIfNull(matureDepiction);
        if (yieldAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(yieldAmount));
        if (emergeAfterMatureSeconds < 0f)
            throw new ArgumentOutOfRangeException(nameof(emergeAfterMatureSeconds));
        if (emergeActorId is null && yieldResourceTag is null && yieldAmount > 0)
            throw new ArgumentException("Yield amount requires a yield resource tag.", nameof(yieldAmount));

        DurationSeconds = durationSeconds;
        MatureDepiction = matureDepiction;
        YieldResourceTag = yieldResourceTag;
        YieldAmount = yieldAmount;
        EmergeActorId = emergeActorId;
        EmergeAfterMatureSeconds = emergeAfterMatureSeconds;
    }

    public float DurationSeconds { get; }

    public DepictionConfig MatureDepiction { get; }

    public TagId? YieldResourceTag { get; }

    public int YieldAmount { get; }

    public string? EmergeActorId { get; }

    public float EmergeAfterMatureSeconds { get; }

    public bool IsMature => _elapsed >= DurationSeconds;

    public void Tick(GameWorld world, Actor actor, float dt)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(actor);
        if (dt <= 0f || _emerged)
            return;

        if (!IsMature)
        {
            _elapsed += dt;
            if (!IsMature)
                return;
            actor.DepictionOverride = MatureDepiction;
        }

        if (EmergeActorId is null)
            return;

        _postMatureElapsed += dt;
        if (_postMatureElapsed >= EmergeAfterMatureSeconds)
            TryEmerge(world, actor);
    }

    public bool TryEmerge(GameWorld world, Actor actor)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(actor);
        if (_emerged || string.IsNullOrWhiteSpace(EmergeActorId) || !IsMature)
            return false;
        if (actor.Cell is not { } cell)
            return false;
        if (!world.TryGetActorDefinition(EmergeActorId, out var characterDef) || characterDef is null)
            return false;

        if (!world.TryRemoveActorAt(cell, out _))
            return false;

        _emerged = true;
        world.SpawnChaseActor(
            characterDef,
            HexWorldLayout.ToWorld(cell, world.HexSize),
            world.RivalFactionId);
        return true;
    }

    public override AccessoryEffect Clone() =>
        new GrowEffect(
            DurationSeconds,
            MatureDepiction,
            YieldResourceTag,
            YieldAmount,
            EmergeActorId,
            EmergeAfterMatureSeconds);
}
