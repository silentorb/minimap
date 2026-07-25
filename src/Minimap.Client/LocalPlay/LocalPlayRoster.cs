namespace Minimap.Client.LocalPlay;

/// <summary>Ordered local players (1–4) and device bindings passed lobby → world.</summary>
public sealed class LocalPlayRoster
{
    private readonly List<LocalPlayerEntry> _players = new();

    public IReadOnlyList<LocalPlayerEntry> Players => _players;

    public int PlayerCount => _players.Count;

    public bool IsEmpty => _players.Count == 0;

    public void Clear() => _players.Clear();

    public void SetPlayerCount(int count)
    {
        count = Math.Clamp(count, 0, 4);
        while (_players.Count < count)
            _players.Add(new LocalPlayerEntry());
        while (_players.Count > count)
            _players.RemoveAt(_players.Count - 1);
    }

    /// <summary>
    /// Replace this roster with <paramref name="source"/>'s player count, devices,
    /// profile identity, and selected accessories (lobby → world handoff).
    /// </summary>
    public void CopyFrom(LocalPlayRoster source)
    {
        ArgumentNullException.ThrowIfNull(source);
        Clear();
        SetPlayerCount(source.PlayerCount);
        for (var i = 0; i < source.PlayerCount; i++)
        {
            foreach (var device in source.Players[i].Devices)
                _players[i].AddDevice(device);
            _players[i].SetSelectedAccessories(source.Players[i].SelectedAccessories);
            var src = source.Players[i];
            if (src.ProfileId is Guid profileId && !string.IsNullOrWhiteSpace(src.DisplayName))
                _players[i].SetProfile(profileId, src.DisplayName);
            else
                _players[i].ClearProfile();
        }
    }

    public void ApplyDefaultSoloKeyboard()
    {
        Clear();
        var solo = new LocalPlayerEntry();
        solo.AddDevice(InputDeviceId.Keyboard);
        _players.Add(solo);
    }

    public int? FindPlayerIndexForDevice(InputDeviceId device)
    {
        for (var i = 0; i < _players.Count; i++)
        {
            if (_players[i].HasDevice(device))
                return i;
        }

        return null;
    }

    public bool IsDeviceAssignedToAnyPlayer(InputDeviceId device) =>
        FindPlayerIndexForDevice(device) is not null;

    public void RemovePlayerAt(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            return;
        _players.RemoveAt(playerIndex);
    }

    public bool IsJoypadAssignedToOtherPlayer(int deviceIndex, int exceptPlayerIndex)
    {
        for (var i = 0; i < _players.Count; i++)
        {
            if (i == exceptPlayerIndex)
                continue;
            if (_players[i].HasJoypad(deviceIndex))
                return true;
        }

        return false;
    }
}
