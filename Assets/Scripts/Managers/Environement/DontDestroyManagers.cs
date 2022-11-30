public class DontDestroyManagers : MonoSingleton<DontDestroyManagers>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this.gameObject);
    }
}
