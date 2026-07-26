namespace Minimap.Client.Achievements;

/// <summary>One achievement earned by a local player during the current session.</summary>
public readonly record struct SessionAchievementEarn(string AchievementId, bool FirstTime);
