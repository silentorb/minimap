namespace Minimap.Client.PostSession;

/// <summary>Pure ready-gate for the post-session summary (all local players must ready).</summary>
public sealed class PostSessionReadyModel
{
    private readonly bool[] _ready;

    public PostSessionReadyModel(int playerCount)
    {
        if (playerCount < 1)
            throw new ArgumentOutOfRangeException(nameof(playerCount));
        _ready = new bool[playerCount];
    }

    public int PlayerCount => _ready.Length;

    public bool IsReady(int playerIndex) =>
        playerIndex >= 0 && playerIndex < _ready.Length && _ready[playerIndex];

    public bool TrySetReady(int playerIndex, bool ready)
    {
        if (playerIndex < 0 || playerIndex >= _ready.Length)
            return false;
        _ready[playerIndex] = ready;
        return true;
    }

    public bool TryToggleReady(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= _ready.Length)
            return false;
        _ready[playerIndex] = !_ready[playerIndex];
        return true;
    }

    public bool AllReady
    {
        get
        {
            foreach (var r in _ready)
            {
                if (!r)
                    return false;
            }

            return true;
        }
    }
}
