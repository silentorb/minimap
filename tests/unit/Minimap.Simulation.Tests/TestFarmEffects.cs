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
        string? emergeActorId = null,
        float emergeAfterMatureSeconds = 0f)
    {
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
        new TestGrowEffect(
            DurationSeconds,
            MatureDepiction,
            YieldResourceTag,
            YieldAmount,
            EmergeActorId,
            EmergeAfterMatureSeconds);
}

internal sealed class TestHarvestEffect : AccessoryEffect, IInteractionEffect, IEffectUseCost
{
    public TestHarvestEffect(TagId? costResourceTag = null, int costAmount = 0)
    {
        CostResourceTag = costResourceTag;
        CostAmount = costAmount;
    }

    public TagId? CostResourceTag { get; }
    public int CostAmount { get; }

    public bool CanInteract(GameWorld world, Actor actor, Actor target)
    {
        if (!EffectUseCosts.CanAfford(actor, this))
            return false;

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

        if (!string.IsNullOrWhiteSpace(grow.EmergeActorId))
        {
            if (!grow.TryEmerge(world, target))
                return false;
            EffectUseCosts.TryConsume(actor, this);
            return true;
        }

        if (!world.TryRemoveActorAt(cell, out _))
            return false;
        if (grow.YieldResourceTag is { } tag && grow.YieldAmount > 0)
            actor.AddResource(tag, grow.YieldAmount);
        EffectUseCosts.TryConsume(actor, this);
        return true;
    }

    public override AccessoryEffect Clone() => new TestHarvestEffect(CostResourceTag, CostAmount);
}

internal sealed class TestPickupEffect : AccessoryEffect, IDefaultInteractionEffect, IEffectUseCost
{
    private readonly TagId _resourceTag;
    private readonly int _amount;

    public TestPickupEffect(
        TagId resourceTag,
        int amount,
        TagId? costResourceTag = null,
        int costAmount = 0)
    {
        _resourceTag = resourceTag;
        _amount = amount;
        CostResourceTag = costResourceTag;
        CostAmount = costAmount;
    }

    public TagId? CostResourceTag { get; }
    public int CostAmount { get; }

    public bool CanInteract(GameWorld world, Actor actor, Actor target) =>
        target.Cell is not null && EffectUseCosts.CanAfford(actor, this);

    public bool TryInteract(GameWorld world, Actor actor, Actor target)
    {
        if (!CanInteract(world, actor, target) || target.Cell is not { } cell)
            return false;
        if (!world.TryRemoveActorAt(cell, out _))
            return false;
        actor.AddResource(_resourceTag, _amount);
        EffectUseCosts.TryConsume(actor, this);
        return true;
    }

    public override AccessoryEffect Clone() =>
        new TestPickupEffect(_resourceTag, _amount, CostResourceTag, CostAmount);
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
                if (!world.TryGetActorDefinition(characterId, out var definition) || definition is null)
                    continue;

                world.TrySpawnNearbyHostile(
                    cell,
                    definition,
                    world.RivalFactionId,
                    AiTuning.DefaultAggression,
                    AiController.ActorSeeksCrops(definition));
            }
        }
    }

    public override AccessoryEffect Clone() =>
        new TestSpawnEffect(IntervalSeconds, Volume, _pool);
}

/// <summary>Test double for CompuQuest <c>spawn_nearby_ally</c>.</summary>
internal sealed class TestSpawnNearbyAllyEffect : AccessoryEffect, IWorldActorPassiveEffect
{
    private bool _spawned;

    public TestSpawnNearbyAllyEffect(string actorId, float aggression = AiTuning.DefaultAggression)
    {
        ActorId = actorId;
        Aggression = aggression;
    }

    public string ActorId { get; }

    public float Aggression { get; }

    public void Tick(GameWorld world, Actor actor, float dt)
    {
        if (_spawned || !actor.IsAlive)
            return;

        if (!world.TryGetActorDefinition(ActorId, out var definition) || definition is null)
        {
            throw new InvalidOperationException(
                $"spawn_nearby_ally actor definition '{ActorId}' is not registered.");
        }

        var origin = HexWorldLayout.WorldToAxial(actor.Position, world.HexSize);
        var ally = world.TrySpawnNearbyActor(
            origin,
            definition,
            actor.FactionId,
            Aggression,
            AiController.ActorSeeksCrops(definition));
        if (ally is not null)
            _spawned = true;
    }

    public override AccessoryEffect Clone() =>
        new TestSpawnNearbyAllyEffect(ActorId, Aggression);
}
