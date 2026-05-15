using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public GameObject ExitPopup;
    private void Start()
    {
        if (ExitPopup != null)
        {
            ExitPopup.SetActive(false);
        }
    }

    public void OnClickGameStart()
    {
        SceneManager.LoadScene("Main");
    }
    public void OnClickExitButton()
    {
        if (ExitPopup != null)
        {
            ExitPopup.SetActive(true);
        }
    }
    public void OnClickExitConfirm()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnClickExitCancel()
    {
        if (ExitPopup != null)
        {
            ExitPopup.SetActive(false);
        }
    }
    
}
