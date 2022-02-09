
namespace NoZ.Zisle
{
    public struct IslandVisibility
    {
        public ulong Chunk1;
        public ulong Chunk2;
        public ulong Chunk3;
        public ulong Chunk4;

        public bool IsVisible(int index)
        {
            if (index < 64) return (Chunk1 & (1UL << index)) != 0;
            index -= 64;
            if (index < 64) return (Chunk2 & (1UL << index)) != 0;
            index -= 64;
            if (index < 64) return (Chunk3 & (1UL << index)) != 0;
            index -= 64;
            if (index < 64) return (Chunk4 & (1UL << index)) != 0;
            return false;
        }

        public IslandVisibility SetVisible(int index, bool visible)
        {
            if (visible)
            {
                if (index < 64) { Chunk1 |= (1UL << index); return this; }
                index -= 64;
                if (index < 64) { Chunk2 |= (1UL << index); return this; }
                index -= 64;
                if (index < 64) { Chunk3 |= (1UL << index); return this; }
                index -= 64;
                if (index < 64) { Chunk4 |= (1UL << index); return this; }
            }
            else
            {
                if (index < 64) { Chunk1 &= ~(1UL << index); return this; }
                index -= 64;
                if (index < 64) { Chunk2 &= ~(1UL << index); return this; }
                index -= 64;
                if (index < 64) { Chunk3 &= ~(1UL << index); return this; }
                index -= 64;
                if (index < 64) { Chunk4 &= ~(1UL << index); return this; }
            }

            return this;
        }
    }
}
