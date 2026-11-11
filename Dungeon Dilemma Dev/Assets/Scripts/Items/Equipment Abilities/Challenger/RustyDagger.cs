using Priestess;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Equipment Ability/Rusty Dagger")]
    public class RustyDagger : ItemAbility, IStatusGiver
    {
        public StatusEffectType GetStatus()
        {
            return StatusEffectType.Bleed;
        }
    }
}