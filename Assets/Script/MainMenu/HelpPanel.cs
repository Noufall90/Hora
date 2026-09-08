using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpPanel : MonoBehaviour
{
    [Header("Content Viewport")]
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

    [Header("Current Page")]
    public int currentIndex = 0;

    [Header("Swipe Settings")]
    public float swipeThreshold = 50f;
    private Vector2 touchStartPos;

    [Header("Content Area")]
    public RectTransform contentArea;


    private void Start()
    {
        // Pastikan index tidak keluar batas
        if (contentPanels == null || contentPanels.Count == 0)
        {
            Debug.LogWarning("HelpPanel: Content Panels masih kosong.");
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, contentPanels.Count - 1);

        // Button listener
        if (nextButton != null)
            nextButton.onClick.AddListener(NextContent);

        if (prevButton != null)
            prevButton.onClick.AddListener(PreviousContent);

        // Initialize dots
        InitializeDots();

        // Display initial content
        ShowContent();

        // Timer
        if (useTimer)
        {
            timer = autoMoveTime;

            InvokeRepeating(nameof(AutoMoveContent), 1f, 1f);
        }
    }


    private void OnDestroy()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(NextContent);

        if (prevButton != null)
            prevButton.onClick.RemoveListener(PreviousContent);

        CancelInvoke(nameof(AutoMoveContent));
    }

    private void InitializeDots()
    {
        if (dotsContainer == null || dotPrefab == null)
            return;

        // Hapus dots lama jika ada
        foreach (Transform child in dotsContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Buat dot sesuai jumlah halaman
        for (int i = 0; i < contentPanels.Count; i++)
        {
            GameObject dot = Instantiate(dotPrefab, dotsContainer.transform);

            Image dotImage = dot.GetComponent<Image>();

            if (dotImage != null)
            {
                dotImage.color = (i == currentIndex)
                    ? Color.white
                    : Color.gray;

                dotImage.fillAmount = 0f;
            }
        }
    }

    private void ShowContent()
    {
        if (contentPanels == null || contentPanels.Count == 0)
            return;

        // Aktifkan hanya halaman yang sedang dipilih
        for (int i = 0; i < contentPanels.Count; i++)
        {
            if (contentPanels[i] != null)
            {
                contentPanels[i].SetActive(i == currentIndex);
            }
        }

        // Reset timer
        if (useTimer)
        {
            timer = autoMoveTime;
        }

        // Update dots
        UpdateDots();

        // Update button state
        UpdateButtons();
    }

    public void NextContent()
    {
        if (contentPanels == null || contentPanels.Count == 0)
            return;

        if (currentIndex >= contentPanels.Count - 1)
        {
            if (isLimitedSwipe)
            {
                return;
            }

            // Jika tidak limited, kembali ke halaman pertama
            currentIndex = 0;
        }
        else
        {
            currentIndex++;
        }

        ShowContent();
    }

    public void PreviousContent()
    {
        if (contentPanels == null || contentPanels.Count == 0)
            return;

        if (currentIndex <= 0)
        {
            if (isLimitedSwipe)
            {
                return;
            }

            // Jika tidak limited, kembali ke halaman terakhir
            currentIndex = contentPanels.Count - 1;
        }
        else
        {
            currentIndex--;
        }

        ShowContent();
    }

    private void UpdateDots()
    {
        if (dotsContainer == null)
            return;

        for (int i = 0; i < dotsContainer.transform.childCount; i++)
        {
            Image dotImage =
                dotsContainer.transform.GetChild(i).GetComponent<Image>();

            if (dotImage == null)
                continue;

            // Dot aktif
            dotImage.color = (i == currentIndex)
                ? Color.white
                : Color.gray;

            // Fill timer
            if (useTimer && i == currentIndex)
            {
                dotImage.fillAmount = 1f;
            }
            else
            {
                dotImage.fillAmount = 0f;
            }
        }
    }

    private void UpdateButtons()
    {
        if (isLimitedSwipe)
        {
            if (prevButton != null)
                prevButton.interactable = currentIndex > 0;

            if (nextButton != null)
                nextButton.interactable =
                    currentIndex < contentPanels.Count - 1;
        }
        else
        {
            if (prevButton != null)
                prevButton.interactable = true;

            if (nextButton != null)
                nextButton.interactable = true;
        }
    }

    private void AutoMoveContent()
    {
        if (!useTimer)
            return;

        timer -= 1f;

        if (timer <= 0f)
        {
            timer = autoMoveTime;

            NextContent();
        }

        UpdateTimerFill();
    }

    private void UpdateTimerFill()
    {
        if (!useTimer || dotsContainer == null)
            return;

        float fill = Mathf.Clamp01(1f - (timer / autoMoveTime));

        for (int i = 0; i < dotsContainer.transform.childCount; i++)
        {
            Image dotImage =
                dotsContainer.transform.GetChild(i).GetComponent<Image>();

            if (dotImage == null)
                continue;

            if (i == currentIndex)
            {
                dotImage.fillAmount = fill;
            }
            else
            {
                dotImage.fillAmount = 0f;
            }
        }
    }

    private void Update()
    {
        DetectSwipe();
    }

    private void DetectSwipe()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 touchEndPos = Input.mousePosition;

            float swipeDistance =
                touchEndPos.x - touchStartPos.x;

            // Pastikan swipe cukup jauh
            if (Mathf.Abs(swipeDistance) > swipeThreshold)
            {
                // Pastikan swipe terjadi di dalam content area
                if (!IsTouchInContentArea(touchStartPos))
                    return;

                // Swipe ke kanan
                if (swipeDistance > 0)
                {
                    PreviousContent();
                }
                // Swipe ke kiri
                else
                {
                    NextContent();
                }
            }
        }
    }

    private bool IsTouchInContentArea(Vector2 screenPosition)
    {
        if (contentArea == null)
            return true;

        return RectTransformUtility.RectangleContainsScreenPoint(
            contentArea,
            screenPosition,
            null
        );
    }
}