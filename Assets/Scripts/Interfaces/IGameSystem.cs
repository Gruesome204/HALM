// IGameSystem.cs
public interface IGameSystem
{
    int InitializePriority { get; } // Lower = earlier initialization
    void Initialize();
    void PostInitialize();
}