using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class ParentVerifyPanel : BaseUI
{
    [Header("显示")]
    [SerializeField] private Text questionText;
    [SerializeField] private Text inputText;
    [SerializeField] private Text tipText;

    [Header("数字按钮 0-9（按索引对应）")]
    [SerializeField] private Button[] numberButtons = new Button[10];

    [Header("功能按钮")]
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button closeButton; // 可选

    [Header("规则")]
    [SerializeField] private int maxNumber = 99;
    [SerializeField] private int maxInputLength = 3; // 999+999=1998
    [SerializeField] private bool allowSubtraction = true;
    [SerializeField] private bool allowAddition = true;
    [SerializeField] private int maxFailBeforeRefresh = 3; // 连错几次换题

    [Header("反馈颜色")]
    [SerializeField] private Color normalTipColor = Color.white;
    [SerializeField] private Color successTipColor = Color.green;
    [SerializeField] private Color errorTipColor = Color.red;

    [Header("动画与节奏")]
    [SerializeField] private RectTransform inputShakeTarget; // 不填则用 inputText 的 RectTransform
    [SerializeField] private float shakeDuration = 0.18f;
    [SerializeField] private float shakeStrength = 12f;
    [SerializeField] private float successCloseDelay = 0.25f;
    [SerializeField] private float confirmCooldown = 0.2f;

    private string currentInput = "";
    private int correctAnswer = 0;
    private string currentQuestion = "";

    private int failCount = 0;
    private bool isConfirmCooling = false;
    private Coroutine shakeCoroutine;
    private Coroutine closeCoroutine;

    private Vector2 shakeOriginPos;

    public event Action OnVerifyPassed;
    public event Action OnVerifyFailed;
    public event Action OnPanelClosed;

    private void Awake()
    {
        BindButtons();

        if (inputShakeTarget == null && inputText != null)
        {
            inputShakeTarget = inputText.rectTransform;
        }

        if (inputShakeTarget != null)
        {
            shakeOriginPos = inputShakeTarget.anchoredPosition;
        }

        gameObject.SetActive(false);
    }

    private void BindButtons()
    {
        for (int i = 0; i < numberButtons.Length; i++)
        {
            int digit = i;
            if (numberButtons[i] != null)
            {
                numberButtons[i].onClick.RemoveAllListeners();
                numberButtons[i].onClick.AddListener(() =>
                {
                    PlayClickSfx();
                    OnClickNumber(digit);
                });
            }
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() =>
            {
                PlayClickSfx();
                OnClickDelete();
            });
        }

        if (clearButton != null)
        {
            clearButton.onClick.RemoveAllListeners();
            clearButton.onClick.AddListener(() =>
            {
                PlayClickSfx();
                OnClickClear();
            });
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                PlayClickSfx();
                OnClickConfirm();
            });
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() =>
            {
                PlayClickSfx();
                Hide();
            });
        }
    }


    public override void Show()
    {
        base.Show();
        gameObject.SetActive(true);

        failCount = 0;
        isConfirmCooling = false;

        GenerateQuestion();
        ResetInput();
        SetTip("", normalTipColor);

        StopAllLocalCoroutines();

        if (inputShakeTarget != null)
        {
            inputShakeTarget.anchoredPosition = shakeOriginPos;
        }

        SetButtonsInteractable(true);
    }

    public override void Hide()
    {
        base.Hide();
        StopAllLocalCoroutines();
        gameObject.SetActive(false);
        OnPanelClosed?.Invoke();

    }


    private void GenerateQuestion()
    {
        bool useAdd;
        if (allowAddition && allowSubtraction)
        {
            useAdd = UnityEngine.Random.value > 0.5f;
        }
        else if (allowAddition)
        {
            useAdd = true;
        }
        else
        {
            useAdd = false;
        }

        int a = UnityEngine.Random.Range(0, maxNumber + 1);
        int b = UnityEngine.Random.Range(0, maxNumber + 1);

        if (useAdd)
        {
            correctAnswer = a + b;
            currentQuestion = $"{a} + {b}";
        }
        else
        {
            if (a < b)
            {
                int temp = a;
                a = b;
                b = temp;
            }

            correctAnswer = a - b;
            currentQuestion = $"{a} - {b}";
        }

        if (questionText != null)
        {
            questionText.text = currentQuestion;
        }
    }

    private void ResetInput()
    {
        currentInput = "";
        RefreshInputText();
    }

    private void RefreshInputText()
    {
        if (inputText != null)
        {
            inputText.text = string.IsNullOrEmpty(currentInput) ? " " : currentInput;
        }
    }

    private void SetTip(string msg, Color color)
    {
        if (tipText != null)
        {
            tipText.text = msg;
            tipText.color = color;
        }
    }

    private void OnClickNumber(int digit)
    {
        if (currentInput.Length >= maxInputLength) return;

        if (currentInput == "0")
        {
            currentInput = digit.ToString();
        }
        else
        {
            currentInput += digit.ToString();
        }

        RefreshInputText();
        SetTip("", normalTipColor);
    }

    private void OnClickDelete()
    {
        if (string.IsNullOrEmpty(currentInput)) return;

        currentInput = currentInput.Substring(0, currentInput.Length - 1);
        RefreshInputText();
    }

    private void OnClickClear()
    {
        ResetInput();
        SetTip("", normalTipColor);
    }

    private void OnClickConfirm()
    {
        if (isConfirmCooling) return;

        if (string.IsNullOrEmpty(currentInput))
        {
            ShowErrorFeedback("请输入答案");
            return;
        }

        int userAnswer;
        if (!int.TryParse(currentInput, out userAnswer))
        {
            ShowErrorFeedback("输入无效");
            return;
        }

        StartCoroutine(ConfirmCooldownRoutine());

        if (userAnswer == correctAnswer)
        {
            ShowSuccessFeedback();
        }
        else
        {
            failCount++;
            OnVerifyFailed?.Invoke();

            if (failCount >= maxFailBeforeRefresh)
            {
                failCount = 0;
                ShowErrorFeedback("连续错误，已刷新题目");
                GenerateQuestion();
            }
            else
            {
                ShowErrorFeedback("答案错误，请重试");
            }

            ResetInput();
        }
    }

    private void ShowSuccessFeedback()
    {
        SetTip("验证通过", successTipColor);

        SetButtonsInteractable(false);

        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
        }

        closeCoroutine = StartCoroutine(CloseAfterDelayRoutine(successCloseDelay));
    }

    private void ShowErrorFeedback(string msg)
    {
        SetTip(msg, errorTipColor);
        StartInputShake();
    }

    private void StartInputShake()
    {
        if (inputShakeTarget == null) return;

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            inputShakeTarget.anchoredPosition = shakeOriginPos;
        }

        shakeCoroutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        float timer = 0f;

        while (timer < shakeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(timer / shakeDuration);
            float damping = 1f - progress;

            float offsetX = UnityEngine.Random.Range(-1f, 1f) * shakeStrength * damping;
            float offsetY = UnityEngine.Random.Range(-0.3f, 0.3f) * shakeStrength * 0.35f * damping;

            inputShakeTarget.anchoredPosition = shakeOriginPos + new Vector2(offsetX, offsetY);

            yield return null;
        }

        inputShakeTarget.anchoredPosition = shakeOriginPos;
        shakeCoroutine = null;
    }

    private IEnumerator CloseAfterDelayRoutine(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        OnVerifyPassed?.Invoke();
        Hide();

        closeCoroutine = null;
    }

    private IEnumerator ConfirmCooldownRoutine()
    {
        isConfirmCooling = true;
        if (confirmButton != null) confirmButton.interactable = false;

        yield return new WaitForSecondsRealtime(confirmCooldown);

        if (confirmButton != null) confirmButton.interactable = true;
        isConfirmCooling = false;
    }

    private void SetButtonsInteractable(bool value)
    {
        for (int i = 0; i < numberButtons.Length; i++)
        {
            if (numberButtons[i] != null) numberButtons[i].interactable = value;
        }

        if (deleteButton != null) deleteButton.interactable = value;
        if (clearButton != null) clearButton.interactable = value;
        if (confirmButton != null) confirmButton.interactable = value;
    }

    private void StopAllLocalCoroutines()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }
    }

    // 你可替换为自己的音频系统
    private void PlayClickSfx()
    {
        // AudioManager.Instance?.PlayUI("click");
    }
}
