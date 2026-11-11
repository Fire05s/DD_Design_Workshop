using Priestess;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Equipment Ability/Infernal Tome")]
    public class InfernalTome : ItemAbility, IStatusGiver 
    {
        public StatusEffectType GetStatus() 
        {
            return StatusEffectType.Burn;
        }
    }
    
}
