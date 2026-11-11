namespace Status {
public class StatusEffect
{
    public short cooldown;
    public StatusEffect()
    {
        
    }
    public virtual bool CheckCooldown()
    {
        cooldown--;
        return cooldown==0;
    }
}
}