namespace OverTheSky.Core
{
    public class GameManager : Singleton<GameManager>
    {
        
        void Start()
        {
            Logger.Instance.LogInfo("Starting Game Manager");
        }
    }
}
