using System.Collections.Generic;

namespace CatCatGo.Infrastructure
{
    public class SeededRandom
    {
        private int _state;

        public SeededRandom(int seed)
        {
            _state = seed;
        }

        public float Next()
        {
            unchecked
            {
                _state = _state + (int)0x6D2B79F5;
                int t = _state ^ (int)((uint)_state >> 15);
                t = t * (1 | _state);
                t = (t + (t ^ (int)((uint)t >> 7)) * (61 | t)) ^ t;
                return (uint)(t ^ (int)((uint)t >> 14)) / 4294967296f;
            }
        }

        public int NextInt(int min, int max)
        {
            return (int)(Next() * (max - min + 1)) + min;
        }

        public float NextFloat(float min, float max)
        {
            return Next() * (max - min) + min;
        }

        public T Pick<T>(IList<T> list)
        {
            return list[NextInt(0, list.Count - 1)];
        }

        public T WeightedPick<T>(IList<(T item, float weight)> entries)
        {
            float totalWeight = 0f;
            for (int i = 0; i < entries.Count; i++)
                totalWeight += entries[i].weight;

            float roll = Next() * totalWeight;
            for (int i = 0; i < entries.Count; i++)
            {
                roll -= entries[i].weight;
                if (roll <= 0f) return entries[i].item;
            }
            return entries[entries.Count - 1].item;
        }

        public List<T> Shuffle<T>(IList<T> list)
        {
            var result = new List<T>(list);
            for (int i = result.Count - 1; i > 0; i--)
            {
                int j = NextInt(0, i);
                (result[i], result[j]) = (result[j], result[i]);
            }
            return result;
        }

        public bool Chance(float probability)
        {
            return Next() < probability;
        }
    }
}
