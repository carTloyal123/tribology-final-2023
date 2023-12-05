using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    private static string POSITION_NAME = "CameraPosition";
    private static string RAMP_SCENE = "RampScene";
    private static string ARCHARD_SCENE = "ArchardScene";
    
    // Menu Buttons
    public GameObject mainMenuGO;
    
    public Button startRampButton;
    public Button startArchardButton;

    public Camera mainCamera;
    
    // Private
    private bool shoudUpdateCamera = false;
    private GameObject cameraDest;
    public float moveSpeed = 2.0f; // Speed of the camera movement
    
    // Start is called before the first frame update
    private void Start()
    {
        if (startRampButton != null)
        {
            startRampButton.onClick.AddListener(StartRamp);
        }
        
        if (startArchardButton)
            startArchardButton.onClick.AddListener(StartArchard);
    }

    private void StartArchard()
    {
        Debug.Log("Start Archard");
        if (mainCamera)
            mainCamera.backgroundColor = HexToColor("#0015FF");
        StartCoroutine(SetScene(ARCHARD_SCENE));
    }
    private void StartRamp()
    {
        Debug.Log("Start Ramp!");
        if (mainCamera)
            mainCamera.backgroundColor = HexToColor("#8A88CF");
        StartCoroutine(SetScene(RAMP_SCENE));
    }

    private IEnumerator SetScene(string sceneName) 
    {
        if (mainMenuGO)
            mainMenuGO.SetActive(false);
        if (!String.IsNullOrWhiteSpace(sceneName))
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            // Wait until the scene is fully loaded
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            Debug.Log($"Loaded scene: {sceneName}");

            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            Debug.Log($"Scene isLoaded: {loadedScene.isLoaded}");
            if (loadedScene.isLoaded)
            {
                // Iterate through objects in the loaded scene
                GameObject[] objectsInLoadedScene = loadedScene.GetRootGameObjects();

                // Find the target object by name
                foreach (GameObject obj in objectsInLoadedScene)
                {
                    if (obj.name == POSITION_NAME)
                    {
                        // Object found
                        cameraDest = obj;
                        Debug.Log($"Found target object in loaded scene: {cameraDest.name}");
                        break; // Exit the loop once the target object is found
                    }
                }
            }
        }

        // find CameraPosition object, take transform
        cameraDest = GameObject.FindWithTag("Player");
        if (cameraDest)
        {
            shoudUpdateCamera = true;
            Debug.Log("Found Destination Camera!");
        }
    }

    private void UpdateCamera()
    {
        if (!shoudUpdateCamera) return;
        if (!mainCamera) return;
        // Smoothly move the camera towards the target position
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, cameraDest.transform.position, moveSpeed * Time.deltaTime);

        // Smoothly rotate the camera towards the target rotation
        mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, cameraDest.transform.rotation, moveSpeed * Time.deltaTime);
    }
    
    Color HexToColor(string hex)
    {
        Color color = Color.black;

        if (ColorUtility.TryParseHtmlString(hex, out color))
        {
            return color;
        }
        else
        {
            Debug.LogError($"Invalid hex color value: {hex}");
            return Color.black; // Default to black in case of an error
        }
    }
    
    
    private void Update()
    {
        UpdateCamera();
    }
}
