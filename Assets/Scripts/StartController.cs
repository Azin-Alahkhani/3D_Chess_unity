using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartController : MonoBehaviour
{
    public Button button;

   
    private void OnEnable()
    {
        button.onClick.AddListener(OnStartClicked);
    }

    private void OnDisable()
    {
        button.onClick.RemoveAllListeners();
    }
    public void OnStartClicked()
    {
        SceneManager.LoadScene(1);
    }
}
