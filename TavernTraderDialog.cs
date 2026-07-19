using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TavernTraderDialog : MonoBehaviour
{
    [SerializeField] private GameObject arrowsImg;
    [SerializeField] private GameObject poisonImg;
    [SerializeField] private GameObject coinsImg;

    [SerializeField] private Text coinsText;
    [SerializeField] private Text arrowsText;
    [SerializeField] private Text poisonText;

    [SerializeField] private Text npcText;
    [SerializeField] private Text answerVariant1;
    [SerializeField] private Text answerVariant2;
    [SerializeField] private Text answerVariant3;
    [SerializeField] private GameObject answerButton1;
    [SerializeField] private GameObject answerButton2;
    [SerializeField] private GameObject answerButton3;

    [SerializeField] private GameObject dialogMenu;
    [SerializeField] private GameObject leftZone;
    [SerializeField] private GameObject RightZone;
    [SerializeField] private GameObject joystick;
    [SerializeField] private GameObject healthBar;

    [SerializeField] private GameObject helpTrigger;
    [SerializeField] private GameObject NextLvMenuTrigger;

    [Header("Настройки печатания")]
    [SerializeField] private float typeDelay = 0.05f;

    [Header("Настройки звука")]
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioClip[] voiceClips; // Массив звуков для разных реплик

    private AudioSource audios;
    private int number;
    private bool buyed;
    private bool abilityToBuy;
    private bool isTyping;
    private bool waitingForInput;
    private Coroutine typingCoroutine;
    private string buyType;
    private int potionPrice = 40;
    private int arrowsPrice = 60;
    private int arrowsAmount = 10;
    private string currentFullText;
    private bool isFirstDialog = true;
    private bool hasThanked = false;
    private bool hasGivenFreePotion = false;

    void Start()
    {
        audios = GetComponent<AudioSource>();
        buyed = false;
        number = 0;
        isTyping = false;
        waitingForInput = false;
        isFirstDialog = true;
        hasThanked = false;
        hasGivenFreePotion = false;

        if (voiceSource == null)
            voiceSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        arrowsText.text = ArrowLogic.arrows.ToString();
        poisonText.text = PoisonButtonRestoreHealth.restorePoison.ToString();
        coinsText.text = CoinsCollector.currentCoins.ToString();

        if (LookAtEnemyLogic.onEnemyZone)
        {
            Exit();
        }

        if (!isTyping && !waitingForInput)
        {
            if (number == 0)
            {
                if (!hasThanked)
                {
                    // Первый диалог - благодарность
                    if (PlayerPrefs.HasKey("ru"))
                    {
                        StartTyping("Спасибо что спас нашу таверну от гоблинов. Я слышал как они говорили что то про старый замок, который находится неподалеку отсюда. Полагаю, кристалл будет там.", 0);
                        answerVariant1.text = "Спасибо за информацию.";
                    }
                    else
                    {
                        StartTyping("Thank you for saving our tavern from the goblins. I heard them talking about an old castle nearby. I believe the crystal will be there.", 0);
                        answerVariant1.text = "Thanks for the information.";
                    }
                    answerButton2.SetActive(false);
                    answerButton3.SetActive(false);
                    number = 5;
                }
                else
                {
                    // Обычное приветствие торговли
                    if (PlayerPrefs.HasKey("ru"))
                    {
                        StartTyping("Охотник! Хочешь купить зелье или стрелы?", 1);
                        answerVariant1.text = "Купить зелье (40 монет)";
                        answerVariant2.text = "Купить стрелы (60 монет)";
                        answerVariant3.text = "Уйти";
                    }
                    else
                    {
                        StartTyping("Hunter! Do you want to buy a potion or arrows?", 1);
                        answerVariant1.text = "Buy potion (40 coins)";
                        answerVariant2.text = "Buy arrows (60 coins)";
                        answerVariant3.text = "Leave";
                    }
                    answerButton2.SetActive(true);
                    answerButton3.SetActive(true);
                    number = 1;
                }
            }
            else if (number == 5)
            {
                // После благодарности переходим к следующему этапу
            }
            else if (number == 1)
            {
                // Ожидаем выбора игрока через кнопки
            }
            else if (number == 2 && abilityToBuy && !buyed)
            {
                if (buyType == "potion")
                {
                    if (PlayerPrefs.HasKey("ru"))
                    {
                        StartTyping($"Отлично! Ты купил зелье за {potionPrice} монет. Будь осторожен!", 2);
                    }
                    else
                    {
                        StartTyping($"Great! You bought a potion for {potionPrice} coins. Be careful!", 2);
                    }

                    CoinsCollector.currentCoins -= potionPrice;
                    if (coinsText != null) coinsText.text = CoinsCollector.currentCoins.ToString();
                    PoisonButtonRestoreHealth.restorePoison += 1;
                    if (poisonText != null) poisonText.text = PoisonButtonRestoreHealth.restorePoison.ToString();
                }
                else if (buyType == "arrows")
                {
                    if (PlayerPrefs.HasKey("ru"))
                    {
                        StartTyping($"Отлично! Ты купил {arrowsAmount} стрел за {arrowsPrice} монет. Будь осторожен!", 3);
                    }
                    else
                    {
                        StartTyping($"Great! You bought {arrowsAmount} arrows for {arrowsPrice} coins. Be careful!", 3);
                    }

                    CoinsCollector.currentCoins -= arrowsPrice;
                    if (coinsText != null) coinsText.text = CoinsCollector.currentCoins.ToString();
                    ArrowLogic.arrows += arrowsAmount;
                    if (arrowsText != null) arrowsText.text = ArrowLogic.arrows.ToString();
                }

                PlayerPrefs.SetInt("Coins", CoinsCollector.currentCoins);
                PlayerPrefs.SetInt("Arrows", ArrowLogic.arrows);
                PlayerPrefs.Save();

                buyed = true;
                abilityToBuy = false;
            }
            else if (number == 2 && !abilityToBuy && !buyed)
            {
                if (PlayerPrefs.HasKey("ru"))
                {
                    StartTyping($"У тебя недостаточно монет! Тебе нужно {GetCurrentPrice()}, а у тебя только {CoinsCollector.currentCoins}.", 4);
                }
                else
                {
                    StartTyping($"You don't have enough coins! You need {GetCurrentPrice()}, but you only have {CoinsCollector.currentCoins}.", 4);
                }

                answerVariant1.text = GetLocalizedText("need_something_else");
                answerVariant2.text = GetLocalizedText("goodbye");
                answerVariant3.text = GetLocalizedText("leave");
                number = 4;
            }
            else if (number == 3)
            {
                buyed = false;
                if (PlayerPrefs.HasKey("ru"))
                {
                    StartTyping("Чем могу ещё помочь?", 5);
                    answerVariant1.text = "Купить зелье (40 монет)";
                    answerVariant2.text = "Купить стрелы (60 монет)";
                    answerVariant3.text = "Уйти";
                }
                else
                {
                    StartTyping("How can I help?", 5);
                    answerVariant1.text = "Buy potion (40 coins)";
                    answerVariant2.text = "Buy arrows (60 coins)";
                    answerVariant3.text = "Leave";
                }
                number = 1;
            }
            else if (number == 4)
            {
                // Ожидаем выбора игрока
            }
        }
    }

    private int GetCurrentPrice()
    {
        if (buyType == "potion") return potionPrice;
        if (buyType == "arrows") return arrowsPrice;
        return 0;
    }

    private string GetLocalizedText(string key)
    {
        bool isRussian = PlayerPrefs.HasKey("ru");

        switch (key)
        {
            case "need_something_else":
                return isRussian ? "Мне нужно кое-что ещё" : "I need something else";
            case "goodbye":
                return isRussian ? "Спасибо, прощай" : "Thank you, goodbye";
            case "leave":
                return isRussian ? "Уйти" : "Leave";
            default:
                return "";
        }
    }

    private void PlayVoiceClip(int clipIndex)
    {
        if (voiceSource != null && voiceClips != null && voiceClips.Length > clipIndex && voiceClips[clipIndex] != null)
        {
            voiceSource.Stop();
            voiceSource.clip = voiceClips[clipIndex];
            voiceSource.Play();
        }
    }

    private void StartTyping(string text, int voiceClipIndex)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        currentFullText = text;
        isTyping = true;
        waitingForInput = false;

        // Воспроизводим звук речи
        PlayVoiceClip(voiceClipIndex);

        typingCoroutine = StartCoroutine(TypeText(text));
    }

    IEnumerator TypeText(string fullText)
    {
        if (npcText != null)
            npcText.text = "";

        foreach (char letter in fullText.ToCharArray())
        {
            if (npcText != null)
                npcText.text += letter;
            yield return new WaitForSeconds(typeDelay);
        }

        typingCoroutine = null;
        isTyping = false;
        waitingForInput = true;

        if (number == 2 && buyed && !abilityToBuy)
        {
            number = 3;
            waitingForInput = false;
        }
    }

    public void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            isTyping = false;
            waitingForInput = true;

            if (npcText != null && !string.IsNullOrEmpty(currentFullText))
            {
                npcText.text = currentFullText;
            }

            if (number == 2 && buyed && !abilityToBuy)
            {
                number = 3;
                waitingForInput = false;
            }
        }
    }

    // Кнопка 1
    public void Answer1()
    {
        if (isTyping) return;
        if (!waitingForInput) return;

        // Состояние после благодарственного диалога
        if (number == 5)
        {
            // Отмечаем, что благодарность уже была сказана
            hasThanked = true;
            isFirstDialog = false;

            // Выдаём бесплатное зелье, если ещё не выдавали
            if (!hasGivenFreePotion)
            {
                PoisonButtonRestoreHealth.restorePoison += 1;
                if (poisonText != null) poisonText.text = PoisonButtonRestoreHealth.restorePoison.ToString();
                hasGivenFreePotion = true;

                if (showDebug) Debug.Log("[TavernTrader] Выдано бесплатное зелье!");

                // Меняем текст кнопки
                if (PlayerPrefs.HasKey("ru"))
                {
                    answerVariant1.text = "Спасибо за зелье.";
                }
                else
                {
                    answerVariant1.text = "Thanks for the potion.";
                }

                // Дополнительное сообщение о бесплатном зелье
                if (PlayerPrefs.HasKey("ru"))
                {
                    StartTyping("И держи, это тебе за помощь. Бесплатное зелье.", 6);
                }
                else
                {
                    StartTyping("And here, this is for your help. A free potion.", 6);
                }
                number = 6;
                waitingForInput = false;
                return;
            }

            number = 0;
            waitingForInput = false;
            return;
        }

        // Состояние после сообщения о бесплатном зелье - нажатие "Спасибо за зелье"
        if (number == 6)
        {
            // Переходим к торговле
            if (PlayerPrefs.HasKey("ru"))
            {
                StartTyping("Охотник! Хочешь купить зелье или стрелы?", 1);
                answerVariant1.text = "Купить зелье (40 монет)";
                answerVariant2.text = "Купить стрелы (60 монет)";
                answerVariant3.text = "Уйти";
            }
            else
            {
                StartTyping("Hunter! Do you want to buy a potion or arrows?", 1);
                answerVariant1.text = "Buy potion (40 coins)";
                answerVariant2.text = "Buy arrows (60 coins)";
                answerVariant3.text = "Leave";
            }
            answerButton2.SetActive(true);
            answerButton3.SetActive(true);
            number = 1;
            waitingForInput = false;
            return;
        }

        // Состояние после неудачной покупки (недостаточно монет)
        if (number == 4)
        {
            number = 3;
            waitingForInput = false;
            return;
        }

        if (number == 1)
        {
            buyType = "potion";
            abilityToBuy = CoinsCollector.currentCoins >= potionPrice;
            number = 2;
            waitingForInput = false;
        }
        else if (number == 3)
        {
            buyType = "potion";
            abilityToBuy = CoinsCollector.currentCoins >= potionPrice;
            number = 2;
            waitingForInput = false;
        }
    }

    // Кнопка 2
    public void Answer2()
    {
        if (isTyping) return;
        if (!waitingForInput) return;

        // Состояние после неудачной покупки (недостаточно монет)
        if (number == 4)
        {
            Exit();
            return;
        }

        if (number == 1)
        {
            buyType = "arrows";
            abilityToBuy = CoinsCollector.currentCoins >= arrowsPrice;
            number = 2;
            waitingForInput = false;
        }
        else if (number == 3)
        {
            buyType = "arrows";
            abilityToBuy = CoinsCollector.currentCoins >= arrowsPrice;
            number = 2;
            waitingForInput = false;
        }
    }

    // Кнопка 3 - выход из диалога
    public void Answer3()
    {
        if (isTyping) return;
        if (!waitingForInput) return;

        Exit();
    }

    public void Exit()
    {
        helpTrigger.SetActive(false);
        NextLvMenuTrigger.SetActive(true);

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        number = 0;
        buyed = false;
        isTyping = false;
        waitingForInput = false;

        npcText.text = "";
        answerVariant1.text = "";
        answerVariant2.text = "";
        answerVariant3.text = "";

        TeleportScript.freezeRunning = true;

        leftZone.SetActive(true);
        RightZone.SetActive(true);
        joystick.SetActive(true);
        healthBar.SetActive(true);

        // Восстанавливаем изображения при закрытии диалога
        if (arrowsImg != null) arrowsImg.SetActive(true);
        if (poisonImg != null) poisonImg.SetActive(true);
        if (coinsImg != null) coinsImg.SetActive(true);

        dialogMenu.SetActive(false);

        // Останавливаем звук при выходе
        if (voiceSource != null)
            voiceSource.Stop();
    }

    public void OpenDialog()
    {
        dialogMenu.SetActive(true);
        number = 0;
        waitingForInput = false;

        // Убеждаемся, что изображения видимы при открытии диалога
        if (arrowsImg != null) arrowsImg.SetActive(true);
        if (poisonImg != null) poisonImg.SetActive(true);
        if (coinsImg != null) coinsImg.SetActive(true);
    }

    private bool showDebug = true;
}