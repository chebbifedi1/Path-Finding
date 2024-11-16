using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // This method loads the "Pathfinding" scene
    public void LoadPathfindingScene()
    {
        SceneManager.LoadScene("Pathfinding");
    }
}
