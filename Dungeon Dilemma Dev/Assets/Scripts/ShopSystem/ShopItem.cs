using Unity.VisualScripting;

namespace Shop
{
    public class ShopItem
    {
        public string name;
        public int cost;
        public string flavortext;
        public string ability;
        public UnityEngine.Sprite image;
        public float HP;
        public float Regen;
        public float Speed;
        public float Power;
        public float Resistance;
        public float Scale;
        public float Cooldown;
        public float MinionCap;
        public float TrapCap;
        public float PurchaseAmount;
        public ShopItem(string _name, int _cost, string _flv, string _abl, UnityEngine.Sprite _img, float hp, float regen, float spd, float pwr, float resist, float scl, float cd, float mincap, float trapcap, float purchamt)
        {
            name = _name;
            cost = _cost;
            flavortext = _flv;
            ability = _abl;
            image = _img;
            HP = hp;
            Regen = regen;
            Speed = spd;
            Power = pwr;
            Resistance = resist;
            Scale = scl;
            Cooldown = cd;
            MinionCap = mincap;
            TrapCap = trapcap;
            PurchaseAmount = purchamt;
        }

    }
}