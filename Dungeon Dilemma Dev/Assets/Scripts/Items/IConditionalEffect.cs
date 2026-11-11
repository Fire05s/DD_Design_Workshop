using UnityEngine;
namespace Items {
/// <summary>
/// An interface for items that have an one time applied effect
/// </summary>
public interface IConditionalEffect
{
    bool CheckForCondition(SyncedStats stats);
    /// <summary>
    /// Applies an effect based on a condition.
    /// </summary>
    /// <param name="stats">Sync stats class</param>
    void ApplyConditionalEffect(SyncedStats stats);
}
}