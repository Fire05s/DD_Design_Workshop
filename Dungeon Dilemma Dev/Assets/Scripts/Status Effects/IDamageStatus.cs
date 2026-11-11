using UnityEngine;

namespace Status
{
    public interface IDamageStatus
    {
        public void DealDamage(HealthHandler healthHandler, Transform source);
        public void DealDamage(TDHealthHandler healthHandler, Transform source);
    }
}