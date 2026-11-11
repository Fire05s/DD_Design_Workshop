namespace Items {
/// <summary>
/// An interface for effects that take place after attack damage calculation
/// </summary>
public interface IOnHitEffect
    {
        /// <summary>
        /// Returns a new damage value based on stats and some requirement.
        /// </summary>
        /// <param name="damage">Post-calculation Damage</param>
        /// <param name="stats">Attacker's Stats</param>
        /// <param name="opponentStats">Attacked's Stats</param>
        /// <returns>New damage value</returns>
        float ApplyOnHitEffect(float damage, SyncedStats stats, SyncedStats opponentStats=null);
    }
}