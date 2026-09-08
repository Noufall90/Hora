using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Panel")]
    public GameObject PanelTutorial;

    [Header("Content Vieport")]
    public Image contentDisplay;
    public List<GameObject> contentPanels;

    [Header("Navigation Dots")]
    public GameObject dotsContainer;
    public GameObject dotPrefab;

    [Header("Pagination Buttons")]
    public Button nextButton;
    public Button prevButton;

    [Header("Page Settings")]
    public bool useTimer = false;
    public bool isLimitedSwipe = false;
    public float autoMoveTime = 5f;
    private float timer;
    public int currentIndex = 0;
    public float swipeThreshold = 50f;
    private Vector2 touchStartPos;

    public RectTransform contentArea;

    void Start()
    {
        if (SaveDataJson.Instance != null && SaveDataJson.Instance.IsTutorialCompleted())
        {
            PanelTutorial.SetActive(false);
            return;
        }
        
        nextButton.onClick.AddListener(NextContent);
        prevButton.onClick.AddListener(PreviousContent);

        InitializeDots();
        ShowContent();

        if (useTimer)
        {
            timer = autoMoveTime;
            InvokeRepeating("AutoMoveContent", 1f, 1f);
        }
    }

public void OpenPanel()
{
    Time.timeScale = 0f;
    PanelTutorial.SetActive(true);
    UIHandler.OpenWindow(gameObject, false);

    if (useTimer)
    {
        CancelInvoke("AutoMoveContent");
        timer = autoMoveTime;
        InvokeRepeating("AutoMoveContent", 1f, 1f);
    }
}

public void ClosedPanel()
{
    if (!PanelTutorial.activeSelf) return;

    CancelInvoke("AutoMoveContent");

    UIHandler.CloseWindow(gameObject);

    if (SaveDataJson.Instance != null)
    {
        SaveDataJson.Instance.MarkTutorialCompleted();
    }

    PanelTutorial.SetActive(false);
    Time.timeScale = 1f;
}

    void InitializeDots()
    {
        for (int i = 0; i < contentPanels.Count; i++)
        {
            GameObject dot = Instantiate(dotPrefab, dotsContainer.transform);
            Image dotImage = dot.GetComponent<Image>();
            dotImage.color = (i == currentIndex) ? Color.white : Color.gray;
            dotImage.fillAmount = 0f;
        }
    }

    void UpdateDots()
    {
        for (int i = 0; i < dotsContainer.transform.childCount; i++)
        {
            Image dotImage = dotsContainer.transform.GetChild(i).GetComponent<Image>();
            dotImage.color = (i == currentIndex) ? Color.white : Color.gray;

            float targetFillAmount = timer / autoMoveTime;
            StartCoroutine(SmoothFill(dotImage, targetFillAmount, 0.5f));
        }
    }

    IEnumerator SmoothFill(Image image, float targetFillAmount, float duration)
    {
        float startFillAmount = image.fillAmount;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            image.fillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        image.fillAmount = targetFillAmount;
    }

    void Update()
    {
        DetectSwipe();
    }

    void DetectSwipe()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 touchEndPos = Input.mousePosition;
            float swipeDistance = touchEndPos.x - touchStartPos.x;

            if (Mathf.Abs(swipeDistance) > swipeThreshold && IsTouchInContentArea(touchStartPos))
            {
                if (isLimitedSwipe && ((currentIndex == 0 && swipeDistance > 0) || (currentIndex == contentPanels.Count - 1 && swipeDistance < 0)))
                {
                    return;
                }

                if (swipeDistance > 0)
                {
                    PreviousContent();
                }
                else
                {
                    NextContent();
                }
            }
        }
    }

    bool IsTouchInContentArea(Vector2 touchPosition)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(contentArea, touchPosition);
    }

void AutoMoveContent()
{
    if (!gameObject.activeSelf) return;
    if (PanelTutorial != null && !PanelTutorial.activeSelf) return;

    timer -= 1f;

    if (timer <= 0)
    {
        timer = autoMoveTime;
        NextContent();
    }

    UpdateDots();
}

    void NextContent()
    {
        currentIndex = (currentIndex + 1) % contentPanels.Count;
        ShowContent();
        UpdateDots();
    }

    void PreviousContent()
    {
        currentIndex = (currentIndex - 1 + contentPanels.Count) % contentPanels.Count;
        ShowContent();
        UpdateDots();
    }

    void ShowContent()
    {
        for (int i = 0; i < contentPanels.Count; i++)
        {
            bool isActive = i == currentIndex;
            contentPanels[i].SetActive(isActive);

            Image dotImage = dotsContainer.transform.GetChild(i).GetComponent<Image>();
            dotImage.color = isActive ? Color.white : Color.gray;

            if (isActive)
            {
                timer = autoMoveTime;
                dotImage.fillAmount = 1f;
            }
            else
            {
                dotImage.fillAmount = 0f;
            }
        }
    }

    public void SetCurrentIndex(int newIndex)
    {
        if (newIndex >= 0 && newIndex < contentPanels.Count)
        {
            currentIndex = newIndex;
            ShowContent();
            UpdateDots();
        }
    }
}