using Minimap.Client.Profiles;

namespace Minimap.Client.Lobby;

/// <summary>Per-slot profile carousel (select-only). Confirmed id is exclusive across slots.</summary>
public sealed class LobbyProfileSelectionState
{
    public Guid? ConfirmedProfileId { get; private set; }

    public int CarouselIndex { get; private set; }

    public void SyncCarouselIndex(IReadOnlyList<PlayerProfileRecord> available)
    {
        ArgumentNullException.ThrowIfNull(available);
        if (available.Count == 0)
        {
            CarouselIndex = 0;
            return;
        }

        if (ConfirmedProfileId is Guid id)
        {
            for (var i = 0; i < available.Count; i++)
            {
                if (available[i].Id == id)
                {
                    CarouselIndex = i;
                    return;
                }
            }
        }

        CarouselIndex = Math.Clamp(CarouselIndex, 0, available.Count - 1);
    }

    public void Cycle(int delta, int optionCount)
    {
        if (optionCount <= 0)
            return;
        CarouselIndex = ((CarouselIndex + delta) % optionCount + optionCount) % optionCount;
    }

    public bool TryConfirm(IReadOnlyList<PlayerProfileRecord> available)
    {
        ArgumentNullException.ThrowIfNull(available);
        if (available.Count == 0)
            return false;
        SyncCarouselIndex(available);
        ConfirmedProfileId = available[CarouselIndex].Id;
        return true;
    }

    /// <summary>Restore a previously confirmed profile (lobby return-from-session).</summary>
    public void RestoreConfirmed(Guid profileId)
    {
        if (profileId == Guid.Empty)
            throw new ArgumentException("Profile id must be non-empty.", nameof(profileId));
        ConfirmedProfileId = profileId;
        CarouselIndex = 0;
    }

    public void Clear()
    {
        ConfirmedProfileId = null;
        CarouselIndex = 0;
    }
}
