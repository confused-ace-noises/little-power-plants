using UnityEngine;
using UnityEngine.InputSystem;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance { get; private set; }

    [SerializeField] private TileIsBeingSelected tileIsBeingSelected;
    private bool previousOverrideState;

    [Header("Overlays")]
    [SerializeField] private GameObject pauseOverlay;
    [SerializeField] private GameObject HUD;
    [SerializeField] private GameObject ProgressionOverlay;

    private GameObject currentOverlay;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        pauseOverlay.SetActive(false);
        HUD.SetActive(true);
        currentOverlay = HUD;
    }

    public void Pause()
    {
        var isPaused = pauseOverlay.activeSelf;
        if (!isPaused)
        {
            Debug.Log("pausing");
            ClearOverlay();
            pauseOverlay.SetActive(true);
            currentOverlay = pauseOverlay;
            SetPaused(true);    
        } else
        {
            Debug.Log("unpausing");
            ClearOverlay();
            pauseOverlay.SetActive(false);
            HUD.SetActive(true);
            currentOverlay = HUD;
            SetPaused(false);
        }
    }

    public void OpenProgression()
    {
        var isOpenProgression = pauseOverlay.activeSelf;

        if(!isOpenProgression)
        {
            ClearOverlay();
            ProgressionOverlay.SetActive(true);
            currentOverlay = ProgressionOverlay;
            SetOpenedProgression(true);
        } else
        {
            ClearOverlay();
            ProgressionOverlay.SetActive(false);
            HUD.SetActive(true);
            currentOverlay = HUD;
        }

    }

    public void Pause(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
            Pause();
    } 

    private void ClearOverlay()
    {
        if (currentOverlay != null)
        {
            currentOverlay.SetActive(false);
            currentOverlay = null;
        }
    }

    private void SetPaused(bool paused)
    {
        Time.timeScale = paused ? 0f : 1f;
        HandleTileIsBeingSelected(paused);
    }

    private void SetOpenedProgression(bool open)
    {
        HandleTileIsBeingSelected(open);
    }

    private void HandleTileIsBeingSelected(bool doHandle)
    {
        if (doHandle)
        {
            previousOverrideState = tileIsBeingSelected.DontPlaceOverride;
            tileIsBeingSelected.SetOverride(true);
        }
        else
        {
            tileIsBeingSelected.SetOverride(previousOverrideState);
        }
    }

    public void OnEscape(InputAction.CallbackContext callback)
    {
        if (callback.performed && currentOverlay != HUD)
        {
            ClearOverlay();
            HUD.SetActive(true);
            currentOverlay = HUD;
            HandleTileIsBeingSelected(false);
        }
    }
}