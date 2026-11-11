using Priestess;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Equipment Ability/Poisoned Garland")]
    public class PoisonedGarland : ItemAbility, IStatusGiver
    {
        public StatusEffectType GetStatus()
        {
            return StatusEffectType.Poison;
        }
    }
}