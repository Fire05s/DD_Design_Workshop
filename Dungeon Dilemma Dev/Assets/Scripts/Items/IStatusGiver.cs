using Priestess;
using Status;

namespace Items
{
    public interface IStatusGiver
    {
        public StatusEffectType GetStatus();
    }
}