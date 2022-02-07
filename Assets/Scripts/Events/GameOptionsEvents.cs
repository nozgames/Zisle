namespace NoZ.Zisle
{
    public struct GameOptionsSpawned { public GameOptions Options; }
    public struct GameOptionsDespawned { public GameOptions Options; }

    public struct GameOptionStartingLanesChanged
    {
        public GameOptions Options;
        public int OldValue;
        public int NewValue;
    }
}
