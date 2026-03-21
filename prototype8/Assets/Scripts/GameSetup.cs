using UnityEngine;

public class GameSetup : MonoBehaviour
{
    void Awake()
    {
        CreateSystems();
        SetupCamera();
    }

    void CreateSystems()
    {
        if (GameManager.Instance == null)
        {
            var obj = new GameObject("GameManager");
            obj.AddComponent<GameManager>();
        }

        if (LevelGenerator.Instance == null)
        {
            var obj = new GameObject("LevelGenerator");
            obj.AddComponent<LevelGenerator>();
        }

        if (UIManager.Instance == null)
        {
            var obj = new GameObject("UIManager");
            obj.AddComponent<UIManager>();
        }

        if (VisualFeedback.Instance == null)
        {
            var obj = new GameObject("VisualFeedback");
            obj.AddComponent<VisualFeedback>();
        }

        var bridge = new GameObject("Bridges");
        bridge.AddComponent<NoteInteractionBridge>();
        bridge.AddComponent<InteractPromptUpdater>();
        DontDestroyOnLoad(bridge);
    }

    void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            var obj = new GameObject("MainCamera");
            obj.tag = "MainCamera";
            cam = obj.AddComponent<Camera>();
            obj.AddComponent<AudioListener>();
        }

        cam.orthographic = true;
        cam.orthographicSize = 6.5f;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 1000f;
        cam.transform.position = new Vector3(1.5f, 1.5f, -10f);
        cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
        cam.clearFlags = CameraClearFlags.SolidColor;

        if (cam.GetComponent<CameraController>() == null)
            cam.gameObject.AddComponent<CameraController>();
    }
}
