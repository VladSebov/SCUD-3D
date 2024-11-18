using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void LoadNextScene()
    {
        // Получаем текущую сцену
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Загружаем следующую сцену
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    public void Exit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
