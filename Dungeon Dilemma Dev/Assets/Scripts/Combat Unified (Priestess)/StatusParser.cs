using Status;

namespace Priestess
{
    public class StatusParser
    {
        public StatusEffect getNewStatusFromEnum(StatusEffectType sty)
        {
            switch (sty)
            {
                case StatusEffectType.Bleed: return new Bleed();
                case StatusEffectType.Poison: return new Poison();
                case StatusEffectType.Cripple: return new Cripple();
                case StatusEffectType.Burn: return new Burn();
                case StatusEffectType.YarrowBuff: return new YarrowBuff();
                case StatusEffectType.LinayBuff: return new LinayBuff();
                case StatusEffectType.AmberBuff: return new AmberBuff();
                case StatusEffectType.RuruBuff: return new RuruBuff();
                case StatusEffectType.Webbed: return new Webbed();
                case StatusEffectType.Watched: return new Watched();
                case StatusEffectType.Withering: return new Withering();
                case StatusEffectType.Enraged: return new Enraged();
                case StatusEffectType.Regeneration: return new Regeneration();
                case StatusEffectType.TDRegeneration: return new TDRegeneration();
                case StatusEffectType.DeathVFX: return new DeathVFX();
                case StatusEffectType.ReviveVFX: return new ReviveVFX();
                case StatusEffectType.ReviveSuccessVFX: return new ReviveSuccessVFX();
                default: return null;
            }
        }
    }
}