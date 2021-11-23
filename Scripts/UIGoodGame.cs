using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class UIGoodGame : MonoBehaviour
{
    private Text conclusionText;
    
    void Awake()
    {
        conclusionText = GameObject.Find("Conclusion Text").GetComponent<Text>();
        string conclusion = Abilities.Name + "享年" + Abilities.Age + "岁。";
        conclusionText.text = conclusion;
    }

    public void Restart()
    {
        Abilities.Bear();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void Quit()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 2);
    }
}
