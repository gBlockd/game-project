using UnityEngine;

/// <summary>
/// Ensures that a GameManager exists no matter which scene is loaded first.
/// 
/// This looks for an existing GameManager in the scene.
/// If one is not found, it loads and instantiates the GameManager prefab
/// from the Resources folder before the first scene starts.
/// </summary>
public static class GameManagerBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Object.FindAnyObjectByType<GameStateManager>() != null)
            return;

        GameObject prefab = Resources.Load<GameObject>("GameManager");
        if (prefab == null)
        {
            Debug.LogError("GameManager prefab not found in Resources/GameManager.");
            return;
        }

        Object.Instantiate(prefab);
    }
}