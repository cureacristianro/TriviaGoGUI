using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadProfileScene()
    {
        SceneManager.LoadScene("DashboardProfile"); 
    }

    public void LoadHistoryScene()
    {
        SceneManager.LoadScene("DashboardHistory");
    }

    public void LoadDashboardScene()
    {
        SceneManager.LoadScene("Dashboard");
    }

    public void LoadGame1Player()
    {
        SceneManager.LoadScene("Game1Player");
    }

    public void LoadGame2Players()
    {
        SceneManager.LoadScene("Game2Players");
    }

}
