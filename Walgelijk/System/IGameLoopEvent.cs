namespace Walgelijk;

/// <summary>
/// Receives an update for every game loop cycle
/// </summary>
public interface IGameLoopEvent
{
    void Update(Game game, float dt);
    void FixedUpdate(Game game, float interval);
}
