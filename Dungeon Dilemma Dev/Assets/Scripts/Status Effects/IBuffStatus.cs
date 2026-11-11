namespace Status
{
    public interface IBuffStatus
    {
        public void ApplyStatusEffect(SyncedStats stats);
        public void UnapplyStatusEffect(SyncedStats stats);
    }
}