using Minimap.Simulation.Types;

namespace Minimap.Simulation.Tests;

/// <summary>Test doubles for grow / harvest / interaction.</summary>
internal sealed class TestGrowEffect : AccessoryEffect, IGrowEffect
{
    private float _elapsed;
    private float _postMatureElapsed;
    private bool _emerged;

    public TestGrowEffect(
        float durationSeconds,
        DepictionConfig matureDepiction,
        TagId? yieldResourceTag,
        int yieldAmount,
        string? emergeCharacterId = null,
        float emergeAfterMatureSeconds = 0f)
    {
        DurationSeconds = durationSeconds;
        MatureDepiction = matureDepiction;
        YieldResourceTag = yieldResourceTag;
        YieldAmount = yieldAmount;
        EmergeCharacterId = emergeCharacterId;
        EmergeAfterMatureSeconds = emergeAfterMatureSeconds;
    }

    public float DurationSeconds { get; }
    public DepictionConfig MatureDepiction { get; }
    public TagId? YieldResourceTag { get; }
    public int YieldAmount { get; }
    public string? EmergeCharacterId { get; }
    public float EmergeAfterMatureSeconds { get; }
    public bool IsMature => _elapsed >= DurationSeconds;

    public void Tick(GameWorld world, Actor actor, float dt)
    {
        if (dt <= 0f || _emerged)
            return;
        if (!IsMature)
        {
            _elapsed += dt;
            if (!IsMature)
                return;
            actor.DepictionOverride = MatureDepiction;
        }

        if (EmergeCharacterId is null)
            return;
        _postMatureElapsed += dt;
        if (_postMatureElapsed >= EmergeAfterMatureSeconds)
            TryEmerge(world, actor);
    }

    public bool TryEmerge(GameWorld world, Actor actor)
    {
        if (_emerged || string.IsNullOrWhiteSpace(EmergeCharacterId) || !IsMature)
            return false;
        if (actor.Cell is not { } cell)
            return false;
        if (!world.TryGetCharacterDefinition(EmergeCharacterId, out var characterDef) || characterDef is null)
            return false;
        if (!world.TryRemoveActorAt(cell, out _))
            return false;
        _emerged = true;
        world.SpawnChaseCharacter(
            characterDef,
            HexWorldLayout.ToWorld(cell, world.HexSize),
            world.RivalFactionId);
        return true;
    }

    public override AccessoryEffect Clone() =>
        new TestGrowEffect(
            DurationSeconds,
            MatureDepiction,
            YieldResourceTag,
            YieldAmount,
            EmergeCharacterId,
            EmergeAfterMatureSeconds);
}

internal sealed class TestHarvestEffect : AccessoryEffect, IInteractionEffect
{
    public bool CanInteract(GameWorld world, Actor actor, Actor target)
    {
        foreach (var effect in target.Effects)
        {
            if (effect is IGrowEffect grow && grow.IsMature)
                return true;
        }

        return false;
    }

    public bool TryInteract(GameWorld world, Actor actor, Actor target)
    {
        if (!CanInteract(world, actor, target))
            return false;

        IGrowEffect? grow = null;
        foreach (var effect in target.Effects)
        {
            if (effect is IGrowEffect g && g.IsMature)
            {
                grow = g;
                break;
            }
        }

        if (grow is null || target.Cell is not { } cell)
            return false;

        if (!string.IsNullOrWhiteSpace(grow.EmergeCharacterId))
            return grow.TryEmerge(world, target);

        if (!world.TryRemoveActorAt(cell, out _))
            return false;
        if (grow.YieldResourceTag is { } tag && grow.YieldAmount > 0)
            actor.AddResource(tag, grow.YieldAmount);
        return true;
    }

    public override AccessoryEffect Clone() => new TestHarvestEffect();
}

internal sealed class TestPickupEffect : AccessoryEffect, IDefaultInteractionEffect
{
    private readonly TagId _resourceTag;
    private readonly int _amount;

    public TestPickupEffect(TagId resourceTag, int amount)
    {
        _resourceTag = resourceTag;
        _amount = amount;
    }

    public bool CanInteract(GameWorld world, Actor actor, Actor target) =>
        target.Cell is not null;

    public bool TryInteract(GameWorld world, Actor actor, Actor target)
    {
        if (!CanInteract(world, actor, target) || target.Cell is not { } cell)
            return false;
        if (!world.TryRemoveActorAt(cell, out _))
            return false;
        actor.AddResource(_resourceTag, _amount);
        return true;
    }

    public override AccessoryEffect Clone() => new TestPickupEffect(_resourceTag, _amount);
}

internal sealed class TestDeathDropEffect : AccessoryEffect, IDeathDropEffect
{
    public TestDeathDropEffect(string actorDefinitionId) => ActorDefinitionId = actorDefinitionId;

    public string ActorDefinitionId { get; }

    public override AccessoryEffect Clone() => new TestDeathDropEffect(ActorDefinitionId);
}

internal sealed class TestSpawnEffect : AccessoryEffect, ISpawnEffect
{
    private readonly WeightedPool<string> _pool;
    private float _elapsed;

    public TestSpawnEffect(float intervalSeconds, int volume, WeightedPool<string> pool)
    {
        IntervalSeconds = intervalSeconds;
        Volume = volume;
        _pool = pool;
    }

    public float IntervalSeconds { get; }
    public int Volume { get; }

    public void Tick(GameWorld world, Actor actor, float dt)
    {
        if (dt <= 0f || !actor.IsAlive || actor.Cell is not HexAxial cell)
            return;

        _elapsed += dt;
        while (_elapsed >= IntervalSeconds)
        {
            _elapsed -= IntervalSeconds;
            for (var i = 0; i < Volume; i++)
            {
                if (!_pool.TryPick(world.Random, out var characterId) || string.IsNullOrWhiteSpace(characterId))
                    break;
                if (!world.TryGetCharacterDefinition(characterId, out var definition) || definition is null)
                    continue;

                world.TrySpawnNearbyHostile(
                    cell,
                    definition,
                    world.RivalFactionId,
                    AiTuning.DefaultAggression,
                    AiController.CharacterSeeksCrops(definition));
            }
        }
    }

    public override AccessoryEffect Clone() =>
        new TestSpawnEffect(IntervalSeconds, Volume, _pool);
}
