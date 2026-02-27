using UnityEngine;

namespace CatCatGo.Domain.ValueObjects
{
    [System.Serializable]
    public struct Stats
    {
        public int Hp;
        public int MaxHp;
        public int Atk;
        public int Def;
        public float Crit;

        public static readonly Stats Zero = new Stats(0, 0, 0, 0, 0f);

        public Stats(int hp, int maxHp, int atk, int def, float crit)
        {
            Hp = hp;
            MaxHp = maxHp;
            Atk = atk;
            Def = def;
            Crit = crit;
        }

        public static Stats Create(int hp = 0, int maxHp = 0, int atk = 0, int def = 0, float crit = 0f)
        {
            return new Stats(hp, maxHp, atk, def, crit);
        }

        public Stats Add(Stats other)
        {
            return new Stats(
                Hp + other.Hp,
                MaxHp + other.MaxHp,
                Atk + other.Atk,
                Def + other.Def,
                Crit + other.Crit
            );
        }

        public Stats Multiply(float factor)
        {
            return new Stats(
                Mathf.FloorToInt(Hp * factor),
                Mathf.FloorToInt(MaxHp * factor),
                Mathf.FloorToInt(Atk * factor),
                Mathf.FloorToInt(Def * factor),
                Crit * factor
            );
        }

        public Stats WithHp(int hp)
        {
            return new Stats(hp, MaxHp, Atk, Def, Crit);
        }

        public Stats WithMaxHp(int maxHp)
        {
            return new Stats(Hp, maxHp, Atk, Def, Crit);
        }
    }
}
