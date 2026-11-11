using Priestess;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Equipment Ability/Blade Sharpener")]
    public class BladeSharpener : ItemAbility, IStatusGiver
    {
        public StatusEffectType GetStatus()
        {
            return StatusEffectType.Bleed;
        }
    }
}