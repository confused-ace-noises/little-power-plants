using System;
using System.Text;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainManuManager : MonoBehaviour
{
    public TMP_InputField inputField;
    public string sceneLoad = "World";
    public string sceneUnload = "MainMenu";
    public WorldData worldData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Click()
    {
        string text = inputField.text;
        if (text.Equals("")) {
            worldData.seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        } else
        {
            uint hash = FNV1a(text);
            worldData.seed = unchecked((int)(hash - int.MaxValue));
        }

        SceneManager.LoadScene(sceneLoad);
        SceneManager.UnloadSceneAsync(sceneUnload);
    }

    uint FNV1a(string s)
    {
        uint hash = 2166136261u;
        foreach (char c in s)
        {
            hash ^= c;
            hash *= 16777619u;
        }
        return hash;
    }
}
