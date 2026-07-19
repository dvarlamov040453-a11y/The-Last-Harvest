using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerDeath : MonoBehaviour
{
    public static int playerHealth;
    private static int savedHealthAtCheckpoint; // Храним здоровье с последнего чекпоинта

    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private GameObject heathBar;
    [SerializeField] private GameObject switchWeaponButton;
    [SerializeField] private GameObject RestoreCollider;
    [SerializeField] private GameObject joystick;
    [SerializeField] private GameObject fightBtn;
    [SerializeField] private GameObject jumpBtn;
    [SerializeField] private GameObject shieldBtn;
    [SerializeField] private GameObject specialHitButton;
    [SerializeField] private GameObject leftZone;
    [SerializeField] private GameObject rightZone;
    [SerializeField] private GameObject SwitchButtons;
    [SerializeField] private GameObject ArrowsAmountBtn;
    [SerializeField] private GameObject ArrowsShotBtn;
    [SerializeField] private GameObject loadingMenu;

    [SerializeField] private GameObject bloodFX;
    [SerializeField] private GameObject bloodPoint;

    [SerializeField] private Slider slider;

    public static bool damage1;
    public static bool damage2;
    public static bool damage3;
    public static bool damage4;

    public static bool takeDamage;
    public static bool punchReload;
    public static bool dead;

    private AudioSource audios;
    [SerializeField] private AudioClip[] hitSounds;

    private void Awake()
    {
        // Загружаем сохранённое здоровье с чекпоинта
        if (PlayerPrefs.HasKey("PlayerHealth"))
        {
            savedHealthAtCheckpoint = PlayerPrefs.GetInt("PlayerHealth");
            playerHealth = savedHealthAtCheckpoint;
        }
        else
        {
            savedHealthAtCheckpoint = 40;
            playerHealth = 40;
        }

        // Убеждаемся, что здоровье не ниже 1 (чтобы игрок не умер сразу после респавна)
        if (playerHealth <= 0)
        {
            playerHealth = 1;
        }

        // Обновляем слайдер
        slider.maxValue = 40;
        slider.value = playerHealth;

        dead = false;
        punchReload = false;

        RestoreCollider.SetActive(false);
        boxCollider.enabled = true;
        joystick.SetActive(true);
        specialHitButton.SetActive(true);
        fightBtn.SetActive(true);
        jumpBtn.SetActive(true);
        shieldBtn.SetActive(true);
    }

    private void Start()
    {
        audios = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (FireScript.damage)
        {
            FireScript.damage = false;
            StartCoroutine(TakeDamage());
        }

        if (damage1)
        {
            playerHealth -= 1;
            Instantiate(bloodFX, bloodPoint.transform.position, bloodPoint.transform.rotation);
            damage1 = false;
            UpdateSlider();
            StartCoroutine(TakeDamage());
            StartCoroutine(PunchReload());
        }

        if (damage2)
        {
            playerHealth -= 2;
            Instantiate(bloodFX, bloodPoint.transform.position, bloodPoint.transform.rotation);
            damage2 = false;
            UpdateSlider();
            StartCoroutine(TakeDamage());
            StartCoroutine(PunchReload());
        }

        if (damage3)
        {
            playerHealth -= 2;
            Instantiate(bloodFX, bloodPoint.transform.position, bloodPoint.transform.rotation);
            damage3 = false;
            UpdateSlider();
            StartCoroutine(TakeDamage());
            StartCoroutine(PunchReload());
        }

        if (damage4)
        {
            playerHealth -= 5;
            Instantiate(bloodFX, bloodPoint.transform.position, bloodPoint.transform.rotation);
            damage4 = false;
            UpdateSlider();
            StartCoroutine(TakeDamage());
            StartCoroutine(PunchReload());
        }

        // Ограничиваем здоровье
        if (playerHealth < 0)
        {
            playerHealth = 0;
        }

        if (playerHealth > 40)
        {
            playerHealth = 40;
        }

        UpdateSlider();

        if (playerHealth <= 0)
        {
            // НЕ СОХРАНЯЕМ здоровье при смерти!
            // Просто перезагружаем сцену, здоровье возьмется из savedHealthAtCheckpoint

            boxCollider.enabled = false;
            PlayerController.speed = 0f;
            PlayerController.jumpForce = 0f;
            FloatingJoystick.isRunning = false;

            dead = true;
            gameObject.tag = "Untagged";

            punchReload = true;
            switchWeaponButton.SetActive(false);
            SwitchButtons.SetActive(false);
            ArrowsAmountBtn.SetActive(false);
            ArrowsShotBtn.SetActive(false);
            leftZone.SetActive(false);
            joystick.SetActive(false);
            specialHitButton.SetActive(false);
            fightBtn.SetActive(false);
            jumpBtn.SetActive(false);
            shieldBtn.SetActive(false);

            StartCoroutine(SceneReload());
        }

        // Показываем коллайдер восстановления только если здоровье меньше 40
        if (playerHealth < 40)
        {
            RestoreCollider.SetActive(true);
        }
        else
        {
            RestoreCollider.SetActive(false);
        }
    }

    private void UpdateSlider()
    {
        slider.value = playerHealth;
    }

    private IEnumerator TakeDamage()
    {
        takeDamage = true;
        yield return new WaitForSeconds(0.3f);
        takeDamage = false;
    }

    private IEnumerator PunchReload()
    {
        punchReload = true;
        yield return new WaitForSeconds(0.3f);
        punchReload = false;
    }

    private IEnumerator SceneReload()
    {
        yield return new WaitForSeconds(5f);
        loadingMenu.SetActive(true);
        heathBar.SetActive(false);
        switchWeaponButton.SetActive(false);
        SwitchButtons.SetActive(false);
        ArrowsAmountBtn.SetActive(false);
        ArrowsShotBtn.SetActive(false);
        leftZone.SetActive(false);
        joystick.SetActive(false);
        specialHitButton.SetActive(false);
        fightBtn.SetActive(false);
        jumpBtn.SetActive(false);
        shieldBtn.SetActive(false);

        SceneManager.LoadScene(PlayerPrefs.GetInt("Level"));
    }

    private void PlayRandom()
    {
        if (audios.isPlaying) return;
        audios.clip = hitSounds[Random.Range(0, hitSounds.Length)];
        audios.Play();
    }

    // Публичный метод для обновления сохраненного здоровья с чекпоинта
    public static void SaveHealthToCheckpoint()
    {
        savedHealthAtCheckpoint = playerHealth;
        PlayerPrefs.SetInt("PlayerHealth", playerHealth);
        PlayerPrefs.Save();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "EnemyWeapon")
        {
            damage1 = true;
            PlayRandom();
        }

        if (collision.gameObject.tag == "EnemyWeapon2")
        {
            damage2 = true;
            PlayRandom();
        }

        if (collision.gameObject.tag == "EnemyWeapon3")
        {
            damage4 = true;
            PlayRandom();
        }

        if (collision.gameObject.tag == "EnemyArrow")
        {
            damage3 = true;
            PlayRandom();
        }
    }

    private void OnCollisionExit(Collision player)
    {
        damage1 = false;
        damage2 = false;
        damage3 = false;
        damage4 = false;
    }
}