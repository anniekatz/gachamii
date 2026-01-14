using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InventorySceneController : MonoBehaviour
{
    [Header("Grid Setup")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private InventoryItemCell cellPrefab;
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private CanvasGroup scrollAreaCanvasGroup;

    [Header("Navigation")]
    [SerializeField] private Button backButton;

    [Header("Fade")]
    [SerializeField] private GameObject fadeInScreen;
    [SerializeField] private GameObject fadeOutScreen;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("Details")]
    [SerializeField] private CanvasGroup detailPopupCanvasGroup;
    [SerializeField] private GameObject detailPopup;
    [SerializeField] private Image detailImage;
    [SerializeField] private TMPro.TextMeshProUGUI detailName;
    [SerializeField] private AspectRatioFitter detailImageFitter;
    [SerializeField] private TMPro.TextMeshProUGUI detailDescription;
    [SerializeField] private TMPro.TextMeshProUGUI detailOwned;
    [SerializeField] private TMPro.TextMeshProUGUI detailGeneration;

    private bool _isClosing;

    private bool _canDismissPopup;

    private readonly List<InventoryItemCell> _spawnedCells = new List<InventoryItemCell>();
    private Animator _detailPopupAnimator;

    private void Start()
    {
        if (gridLayout == null && contentParent != null)
            gridLayout = contentParent.GetComponent<GridLayoutGroup>();

        if (detailPopupCanvasGroup != null)
            _detailPopupAnimator = detailPopupCanvasGroup.GetComponent<Animator>();

        StartCoroutine(InitializeScene());
    }

    private void Update()
    {
        if (!_canDismissPopup) return;
        if (_isClosing) return;
        if (detailPopup == null || !detailPopup.activeSelf) return;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            StartCoroutine(CloseDetailPopup());
            return;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartCoroutine(CloseDetailPopup());
            return;
        }
    }


    private IEnumerator InitializeScene()
    {
        if (fadeInScreen != null) fadeInScreen.SetActive(true);

        while (CollectionManager.Instance == null) yield return null;

        if (backButton != null) backButton.onClick.AddListener(OnBackPressed);
        if (detailPopup != null) detailPopup.SetActive(false);

        yield return StartCoroutine(PopulateGridRoutine());

        if (fadeInScreen != null)
        {
            yield return new WaitForSeconds(fadeInDuration);
            fadeInScreen.SetActive(false);
        }

        _canDismissPopup = false;
    }

    private IEnumerator PopulateGridRoutine()
    {
        foreach (var cell in _spawnedCells)
        {
            if (cell != null) Destroy(cell.gameObject);
        }
        _spawnedCells.Clear();

        if (gridLayout != null) gridLayout.enabled = false;

        int currentHour = System.DateTime.Now.Hour;
        bool isNsfwTime = currentHour >= 20 && currentHour <= 23;

        var allItems = CollectionManager.Instance.GetAllItemsWithState();
        int itemsProcessed = 0;
        int itemsPerFrame = 15;

        foreach (var (def, state) in allItems)
        {
            if (!def.IsSfw && state.seen && !isNsfwTime)
                continue;

            var cell = Instantiate(cellPrefab, contentParent);
            cell.Setup(def, state);

            if (cell.cellButton != null)
            {
                var capturedDef = def;
                var capturedState = state;
                cell.cellButton.onClick.AddListener(() => OnCellClicked(capturedDef, capturedState));
            }

            _spawnedCells.Add(cell);

            itemsProcessed++;

            if (itemsProcessed % itemsPerFrame == 0)
            {
                yield return null;
            }
        }

        if (gridLayout != null)
        {
            gridLayout.enabled = true;
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent as RectTransform);
        }

        Debug.Log($"[InventoryScene] Populated {_spawnedCells.Count} items");
    }

    private void OnCellClicked(ItemDefinition def, CollectionStorageService.State state)
    {
        if (!state.seen) return;
        ShowDetailPopup(def, state);
    }

    private void ShowDetailPopup(ItemDefinition def, CollectionStorageService.State state)
    {
        if (scrollAreaCanvasGroup != null)
            scrollAreaCanvasGroup.blocksRaycasts = false;

        if (_detailPopupAnimator != null)
            _detailPopupAnimator.enabled = true;

        if (detailPopupCanvasGroup != null)
            detailPopupCanvasGroup.alpha = 1f;


        if (detailPopup == null) return;
        if (detailImage != null)
        {
            detailImage.sprite = def.Normal;

            if (detailImageFitter != null && def.Normal != null)
            {
                float width = def.Normal.rect.width;
                float height = def.Normal.rect.height;
                detailImageFitter.aspectRatio = width / height;
            }
        }
        if (detailName != null) detailName.text = def.DisplayName;
        if (detailDescription != null) detailDescription.text = def.Description;
        if (detailOwned != null) detailOwned.text = $"Owned: {state.amount}";
        if (detailGeneration != null) detailGeneration.text = $"Gen {def.Generation}";
        detailPopup.SetActive(true);
        StartCoroutine(EnableDismissNextFrame());
    }

    private IEnumerator EnableDismissNextFrame()
    {
        yield return null;
        _canDismissPopup = true;
    }


    private IEnumerator CloseDetailPopup()
    {
        _isClosing = true;
        _canDismissPopup = false;

        if (_detailPopupAnimator != null)
            _detailPopupAnimator.enabled = false;

        if (detailPopupCanvasGroup != null)
        {
            float elapsed = 0f;
            float dur = 0.3f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                detailPopupCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }

            detailPopupCanvasGroup.alpha = 0f;
        }

        if (detailPopup != null) detailPopup.SetActive(false);

        if (scrollAreaCanvasGroup != null)
            scrollAreaCanvasGroup.blocksRaycasts = true;

        _isClosing = false;
    }

    private void OnBackPressed()
    {
        StartCoroutine(FadeOutAndLoadScene("MainScene"));
    }

    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        if (fadeOutScreen != null)
        {
            fadeOutScreen.SetActive(true);
            yield return new WaitForSeconds(fadeOutDuration);
        }
        SceneManager.LoadScene(sceneName);
    }

    private void OnDestroy()
    {
        if (backButton != null)
            backButton.onClick.RemoveAllListeners();
    }
}