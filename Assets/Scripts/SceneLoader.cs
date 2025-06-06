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

}
