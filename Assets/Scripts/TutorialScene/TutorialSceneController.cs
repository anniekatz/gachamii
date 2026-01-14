using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TutorialSceneController : MonoBehaviour
{
    public GameObject fadeInScreen;
    public GameObject fadeOutScreen;

    public GameObject nickSprite;
    public GameObject annieSprite;
    private AnnieExpressionController annieExpression;
    private NickExpressionController nickExpression;

    public GameObject dialogueText;
    private string dialogueToWrite;
    private int currentTextLength;
    private int textLength;
    [SerializeField] GameObject mainTextBox;

    [SerializeField] GameObject nextButton;
    [SerializeField] int eventNumber = 0;

    [SerializeField] GameObject pointerGachaMachine;
    [SerializeField] GameObject pointerYenDisplay;
    [SerializeField] GameObject pointerCoinToDrag;
    [SerializeField] GameObject pointerGachaCoinSlot;
    [SerializeField] GameObject pointerGachaItemSlot;
    [SerializeField] GameObject pointerDailyCode;
    [SerializeField] GameObject pointerInventory;

    private Vector2 _annieOriginalAnchoredPosition;
    private RectTransform _annieRectTransform;
    private Animator _annieAnimator;

    [SerializeField] private float annieSlideDistance = 200f;
    [SerializeField] private float anneSlideDuration = 0.3f;

    private const string TUTORIAL_COMPLETED_KEY = "TutorialCompleted";

    void Update()
    {
        textLength = DialogueCreator.charCount;
    }

    void Start()
    {
        annieExpression = annieSprite.GetComponent<AnnieExpressionController>();
        nickExpression = nickSprite.GetComponent<NickExpressionController>();
        dialogueText.GetComponent<TMPro.TMP_Text>().text = "";
        StartCoroutine(StartTutorial());

    }

    public void NextButton()
    {
        StartCoroutine("Event_" + eventNumber);
    }

    void StartTyping(string line)
    {
        dialogueToWrite = line;
        currentTextLength = dialogueToWrite.Length;

        DialogueCreator.transferText = dialogueToWrite;
        DialogueCreator.runTextPrint = true;
    }

    IEnumerator StartTutorial()
    {
        //event 0
        fadeInScreen.SetActive(true);
        yield return new WaitForSeconds(2f);
        fadeInScreen.SetActive(false);
        nickSprite.SetActive(true);
        annieSprite.SetActive(true);
        yield return new WaitForSeconds(0.25f);
        SoundEffectController.Instance.PlaySound("hi");
        yield return new WaitForSeconds(0.25f);
        mainTextBox.SetActive(true);
        StartTyping("Hi, Nick!");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        yield return new WaitForSeconds(0.25f);
        nextButton.SetActive(true);
        eventNumber++;
    }

    private IEnumerator SlideAnnie(Vector2 targetAnchoredPosition, float duration)
    {
        if (_annieRectTransform == null) yield break;

        if (_annieAnimator != null)
            _annieAnimator.enabled = false;

        Vector2 startPos = _annieRectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            _annieRectTransform.anchoredPosition = Vector2.Lerp(startPos, targetAnchoredPosition, eased);
            yield return null;
        }

        _annieRectTransform.anchoredPosition = targetAnchoredPosition;

    }

    IEnumerator Event_1()
    {
        //event 1
        nextButton.SetActive(false);
        SoundEffectController.Instance.PlaySound("welcome");
        StartTyping("Welcome to your virtual gacha machine!");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        yield return new WaitForSeconds(0.25f);
        nextButton.SetActive(true);
        eventNumber++;
    }

    IEnumerator Event_2()
    {
        //event 2
        nextButton.SetActive(false);
        annieExpression.SetAnniePoint();
        SoundEffectController.Instance.PlaySound("watchTutorial");
        StartTyping("Before you can play, you have to watch the tutorial.");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        yield return new WaitForSeconds(0.25f);
        nextButton.SetActive(true);
        eventNumber++;
    }

    IEnumerator Event_3()
    {
        //event 3
        nextButton.SetActive(false);
        annieExpression.SetAnnieDetermined();
        nickExpression.SetNickSurprised();
        SoundEffectController.Instance.PlaySound("payAttention");
        StartTyping("Pay attention! Or else you might run out of yen!");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        yield return new WaitForSeconds(0.25f);
        nextButton.SetActive(true);
        eventNumber++;
    }

    IEnumerator Event_4()
    {
        //event 4
        nextButton.SetActive(false);
        annieExpression.SetAnnieHappy();
        SoundEffectController.Instance.PlaySound("butShort");
        StartTyping("But it's a short tutorial, ok?");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        nickExpression.StartNickWiggle(3.0f);
        yield return new WaitForSeconds(1.5f);
        nextButton.SetActive(true);
        eventNumber++;
    }
    IEnumerator Event_5()
    {
        //event 5
        nextButton.SetActive(false);
        annieExpression.SetAnnieClap();
        SoundEffectController.Instance.PlaySound("getStarted");
        StartTyping("Let's get started!");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        yield return new WaitForSeconds(0.25f);
        nextButton.SetActive(true);
        eventNumber++;
    }
    IEnumerator Event_6()
    {
        //event 6
        nextButton.SetActive(false);
        pointerGachaMachine.SetActive(true);
        annieExpression.SetAnnieExplain();
        nickExpression.SetNickDefault();
        SoundEffectController.Instance.PlaySound("yourGacha");
        StartTyping("Your Gacha machine is full of collectibles. You need 100 yen to collect one.");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        yield return new WaitForSeconds(0.25f);
        nextButton.SetActive(true);
        eventNumber++;
    }

    IEnumerator Event_7()
    {
        //event 7
        pointerGachaMachine.SetActive(false);
        nextButton.SetActive(false);
        pointerYenDisplay.SetActive(true);
        SoundEffectController.Instance.PlaySound("thisBox");
        StartTyping("This box displays how much yen you have.");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        yield return new WaitForSeconds(0.25f);
        nextButton.SetActive(true);
        eventNumber++;
    }

    IEnumerator Event_8()
    {
        //event 8
        nextButton.SetActive(false);
        nickExpression.SetNickAngry();
        SoundEffectController.Instance.PlaySound("youOnly");
        StartTyping("You only get 200 yen per day when you log in.");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        yield return new WaitForSeconds(0.25f);
        nextButton.SetActive(true);
        eventNumber++;
    }

    IEnumerator Event_9()
    {
        //event 9
        pointerYenDisplay.SetActive(false);
        nextButton.SetActive(false);
        pointerDailyCode.SetActive(true);
        annieExpression.SetAnnieDetermined();
        SoundEffectController.Instance.PlaySound("relax");
        StartTyping("Relax! This button is a special way to add 100 more yen every day!");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        yield return new WaitForSeconds(0.25f);
        nickExpression.SetNickDefault();
        nextButton.SetActive(true);
        eventNumber++;
    }

    IEnumerator Event_10()
    {
        //event 10
        nextButton.SetActive(false);
        annieExpression.SetAnniePoint();
        SoundEffectController.Instance.PlaySound("secretCode");
        StartTyping("You just have to input the day's secret code.");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        yield return new WaitForSeconds(0.25f);
        nextButton.SetActive(true);
        eventNumber++;
    }

    IEnumerator Event_11()
    {
        //event 11
        nextButton.SetActive(false);
        annieExpression.SetAnnieHappy();
        SoundEffectController.Instance.PlaySound("askRealAnnie");
        StartTyping("You can ask Real Annie for the code each day...");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        yield return new WaitForSeconds(0.25f);
        nextButton.SetActive(true);
        eventNumber++;
    }

    IEnumerator Event_12()
    {
        //event 12
        nextButton.SetActive(false);
        nickExpression.SetNickLeft();
        SoundEffectController.Instance.PlaySound("sheProbably");
        StartTyping("...and she'll *probably* give it to you if you ask nicely.");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        yield return new WaitForSeconds(0.25f);
        nextButton.SetActive(true);
        eventNumber++;
    }

    IEnumerator Event_13()
    {
        //event 13
        pointerDailyCode.SetActive(false);
        nextButton.SetActive(false);
        pointerCoinToDrag.SetActive(true);
        annieExpression.SetAnnieExplain();
        nickExpression.SetNickDefault();
        SoundEffectController.Instance.PlaySound("clickThisCoin");
        StartTyping("When you have enough yen, you can click and drag this coin...");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        yield return new WaitForSeconds(0.25f);
        nextButton.SetActive(true);
        eventNumber++;
    }

    IEnumerator Event_14()
    {
        //event 14
        pointerCoinToDrag.SetActive(false);
        nextButton.SetActive(false);
        pointerGachaCoinSlot.SetActive(true);
        SoundEffectController.Instance.PlaySound("intoSlot");
        StartTyping("...into this slot, and the gacha machine will run!");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        yield return new WaitForSeconds(0.25f);
        nextButton.SetActive(true);
        eventNumber++;
    }
    IEnumerator Event_15()
    {
        _annieRectTransform = annieSprite.GetComponent<RectTransform>();
        _annieAnimator = annieSprite.GetComponent<Animator>();
        _annieOriginalAnchoredPosition = _annieRectTransform.anchoredPosition;

        Vector2 slideLeftPosition = _annieOriginalAnchoredPosition + Vector2.left * annieSlideDistance;
        StartCoroutine(SlideAnnie(slideLeftPosition, anneSlideDuration));

        pointerGachaCoinSlot.SetActive(false);
        nextButton.SetActive(false);
        pointerGachaItemSlot.SetActive(true);
        SoundEffectController.Instance.PlaySound("capsuleWillPop");
        StartTyping("A Gacha capsule will pop out here, and you'll have to tap it to open it up.");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        yield return new WaitForSeconds(0.25f);
        nextButton.SetActive(true);
        eventNumber++;
    }

    IEnumerator Event_16()
    {
        nextButton.SetActive(false);
        annieExpression.SetAnnieDetermined();
        nickExpression.SetNickSurprised();
        SoundEffectController.Instance.PlaySound("makeSureOpen");
        StartTyping("Make sure you open any capsule in here before you exit the app, or you could lose it!");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        yield return new WaitForSeconds(0.25f);
        nextButton.SetActive(true);
        eventNumber++;
    }

    IEnumerator Event_17()
    {
        StartCoroutine(SlideAnnie(_annieOriginalAnchoredPosition, anneSlideDuration));

        pointerGachaItemSlot.SetActive(false);
        nextButton.SetActive(false);
        pointerInventory.SetActive(true);
        annieExpression.SetAnnieDefault();
        nickExpression.SetNickDefault();
        SoundEffectController.Instance.PlaySound("yourCollectibles");
        StartTyping("Your collectibles are stored here.");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        yield return new WaitForSeconds(0.25f);
        nextButton.SetActive(true);
        eventNumber++;
    }
    IEnumerator Event_18()
    {
        nextButton.SetActive(false);
        SoundEffectController.Instance.PlaySound("viewCollectibles");
        StartTyping("You can view the ones you own and see how many you're still missing.");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        yield return new WaitForSeconds(0.25f);
        nextButton.SetActive(true);
        eventNumber++;
    }


    IEnumerator Event_19()
    {
        nextButton.SetActive(false);
        annieExpression.SetAnniePoint();
        nickExpression.SetNickBothClosed();
        SoundEffectController.Instance.PlaySound("realAnnieAdd");
        StartTyping("Real Annie will periodically add new Generations of items so you can keep collecting!");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        yield return new WaitForSeconds(0.25f);
        nextButton.SetActive(true);
        eventNumber++;
    }

    IEnumerator Event_20()
    {
        pointerInventory.SetActive(false);
        nextButton.SetActive(false);
        annieExpression.SetAnnieHappy();
        SoundEffectController.Instance.PlaySound("thatsIt");
        StartTyping("That's it from me!");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        yield return new WaitForSeconds(0.25f);
        nextButton.SetActive(true);
        eventNumber++;
    }

    IEnumerator Event_21()
    {
        nextButton.SetActive(false);
        nickExpression.SetNickConfused();
        SoundEffectController.Instance.PlaySound("stillConfused");
        StartTyping("Well... if you're still confused, ask Real Annie for help!");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        yield return new WaitForSeconds(0.25f);
        nextButton.SetActive(true);
        eventNumber++;
    }

    IEnumerator Event_22()
    {
        nextButton.SetActive(false);
        annieExpression.SetAnnieClap();
        nickExpression.StartNickWiggle(3.0f);
        SoundEffectController.Instance.PlaySound("haveFun");
        StartTyping("Have fun! <3");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => textLength == currentTextLength);
        //nextButton.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        fadeOutScreen.SetActive(true);
        yield return new WaitForSeconds(2);

        SetTutorialCompleted();
        SceneManager.LoadScene("MainScene");
    }



    #region Tutorial Status Management
    public static void SetTutorialCompleted()
    {
        SecureStore.SetInt(TUTORIAL_COMPLETED_KEY, 1);
    }

    public static void ResetTutorialStatus()
    {
        SecureStore.DeleteKey(TUTORIAL_COMPLETED_KEY);
    }

    public static bool HasCompletedTutorial()
    {
        return SecureStore.GetInt(TUTORIAL_COMPLETED_KEY, 0) == 1;
    }

    #endregion
}
