using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GoblinEnemyDeath : MonoBehaviour
{
    [SerializeField] private int goblinHealth;
    [SerializeField] private SphereCollider coll;
    [SerializeField] private Slider slider;
    [SerializeField] private GameObject healthBar;

    [SerializeField] private GameObject bloodFX;
    [SerializeField] private GameObject bloodPoint;

    private bool showCoin;
    private bool hideCoin;
    [SerializeField] private GameObject CoinBagPrefab;
    [SerializeField] private GameObject coinPoint1;

    private bool hitByPlayer;
    private bool hitByStealthPlayer;
    private bool hitByCompanion;
    private bool dead;
    private bool hitByStealthArrow;

    private Animator anim;

    private AudioSource audios;
    [SerializeField] private AudioClip[] hurtSounds;

    private void Start()
    {
        slider.maxValue = goblinHealth;
        slider.value = goblinHealth;
        anim = GetComponent<Animator>();
        audios = GetComponent<AudioSource>();
        anim.applyRootMotion = true;
        dead = false;
    }

    private void Update()
    {
        if (hitByPlayer && KnightAnimations.punch)
        {
            if (!dead)
            {
                StartCoroutine(TakeDamage());
            }
        }

        if (hitByCompanion)
        {
            if (!dead)
            {
                StartCoroutine(TakeCompanionDamage());
            }
        }

        if (KnightAnimations.specialHit && hitByPlayer)
        {
            if (!dead)
            {
                StartCoroutine(TakeDamage());
            }
        }

        if (hitByArrow)
        {
            if (!dead)
            {
                StartCoroutine(TakeArrowDamage());
            }
        }

        if (hitByStealthArrow)
        {
            if (!dead)
            {
                StartCoroutine(TakeStealthArrowDamage());
            }
        }

        if (goblinHealth <= 0)
        {
            audios.volume = 0f;
            showCoin = true;
            dead = true;

            slider.value = goblinHealth;
            gameObject.tag = "Untagged";

            healthBar.SetActive(false);
            anim.SetBool("Mining", false);
            anim.SetBool("HitByShield", false);
            anim.SetBool("TakeDamage", false);
            anim.SetBool("Walk", false);
            anim.SetBool("Punch", false);
            anim.SetBool("Run", false);
            anim.SetBool("PlayerShield", false);
            anim.SetBool("FightRun", false);
            anim.SetBool("SeePlayer", false);

            coll.radius = 0f;

            StartCoroutine(AnimDeath());
        }

        if (showCoin && !hideCoin)
        {
            StartCoroutine(ShowCoin());
        }
    }

    private IEnumerator TakeDamage()
    {
        PlayRandom();
        Instantiate(bloodFX, bloodPoint.transform.position, bloodPoint.transform.rotation);
        slider.value = goblinHealth;
        goblinHealth -= 1;
        yield return new WaitForSeconds(0.20f);
        anim.SetBool("Mining", false);
        anim.SetBool("Punch", false);
        anim.SetBool("Walk", false);
        anim.SetBool("PlayerShield", false);
        anim.SetBool("HitByShield", false);
        anim.SetBool("FightRun", false);
        anim.SetBool("TakeDamage", true);
        yield return new WaitForSeconds(0.2f);
        anim.SetBool("TakeDamage", false);
    }

    private IEnumerator TakeCompanionDamage()
    {
        PlayRandom();
        Instantiate(bloodFX, bloodPoint.transform.position, bloodPoint.transform.rotation);
        hitByCompanion = false;
        slider.value = goblinHealth;
        goblinHealth -= 1;
        yield return new WaitForSeconds(0.20f);
        anim.SetBool("Mining", false);
        anim.SetBool("Punch", false);
        anim.SetBool("Walk", false);
        anim.SetBool("PlayerShield", false);
        anim.SetBool("HitByShield", false);
        anim.SetBool("FightRun", false);
        anim.SetBool("TakeDamage", true);
        yield return new WaitForSeconds(0.2f);
        anim.SetBool("TakeDamage", false);
    }

    private IEnumerator TakeArrowDamage()
    {
        Instantiate(bloodFX, bloodPoint.transform.position, bloodPoint.transform.rotation);
        slider.value = goblinHealth;
        hitByArrow = false;
        goblinHealth -= Random.Range(6, 15);
        anim.SetBool("Mining", false);
        anim.SetBool("Punch", false);
        anim.SetBool("Walk", false);
        anim.SetBool("PlayerShield", false);
        anim.SetBool("HitByShield", false);
        anim.SetBool("FightRun", false);
        anim.SetBool("TakeDamage", true);
        yield return new WaitForSeconds(0.2f);
        anim.SetBool("TakeDamage", false);
    }

    private IEnumerator TakeStealthArrowDamage()
    {
        Instantiate(bloodFX, bloodPoint.transform.position, bloodPoint.transform.rotation);
        slider.value = goblinHealth;
        hitByArrow = false;
        goblinHealth -= 100;
        anim.SetBool("Mining", false);
        anim.SetBool("Punch", false);
        anim.SetBool("Walk", false);
        anim.SetBool("PlayerShield", false);
        anim.SetBool("HitByShield", false);
        anim.SetBool("FightRun", false);
        anim.SetBool("TakeDamage", true);
        yield return new WaitForSeconds(0.2f);
        anim.SetBool("TakeDamage", false);
    }

    private IEnumerator AnimDeath()
    {
        yield return new WaitForSeconds(0.2f);
        anim.SetBool("Death", true);
        anim.applyRootMotion = false;
    }

    private IEnumerator ShowCoin()
    {
        hideCoin = true;
        yield return new WaitForSeconds(0.2f);
        Instantiate(CoinBagPrefab, coinPoint1.transform.position, coinPoint1.transform.rotation);
        StopCoroutine(ShowCoin());
    }

    private void PlayRandom()
    {
        if (audios.isPlaying) return;
        audios.clip = hurtSounds[Random.Range(0, hurtSounds.Length)];
        audios.Play();
    }

    private bool hitByArrow;
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("PlayerSword"))
        {
            hitByPlayer = true;
        }

        if (other.gameObject.CompareTag("PlayerArrow"))
        {
            hitByArrow = true;
            coll.radius = 20f;
            PlayRandom();
        }

        if (other.gameObject.CompareTag("CompanionSword"))
        {
            hitByCompanion = true;
        }

        if (other.gameObject.CompareTag("PlayerStealthArrow"))
        {
            hitByStealthArrow = true;
            coll.radius = 20f;
            PlayRandom();
        }
    }

    private void OnCollisionExit(Collision other)
    {
        hitByPlayer = false;
        hitByStealthPlayer = false;
        hitByCompanion = false;
        hitByArrow = false;
    }
}