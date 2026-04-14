using UnityEngine;

/// <summary>
/// Marks a valid player arrival point in a scene.
/// 
/// The entranceId is used by scene transitions to decide where the player
/// should appear after loading this scene.
/// </summary>
public class SceneEntrance : MonoBehaviour
{
    public string entranceId;
}