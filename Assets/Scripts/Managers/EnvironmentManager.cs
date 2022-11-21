using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance;

    public bool Development;
    public bool Production;
    public bool Community;

    private void Awake()
    {
        Instance = this;
    }
}
