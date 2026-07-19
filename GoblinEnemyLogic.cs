using UnityEngine;
using System.Collections;

public class GoblinEnemyLogic : MonoBehaviour
{
    [SerializeField] private float seeDistance = 5f;
    [SerializeField] private float attackDistance = 2f;
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private GameObject[] wayPoints;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject sword;
    [SerializeField] private GameObject pickaxe;
    [SerializeField] private SphereCollider coll;

    public bool abilityToPatroling;
    public bool abilityToMining;

    [Header("Настройки атаки по столу")]
    [SerializeField] private string tableTag = "Table";
    [SerializeField] private float tableAttackRange = 1.5f;
    [SerializeField] private float attackTableInterval = 0.5f; // Интервал между ударами

    private int current = 0;
    private float wRadius = 1;

    private Transform target;
    private Transform mTransform;

    private Animator anim;

    private bool onEnemyZone;

    private bool attack;
    private bool seePlayer;
    private bool hitByShield;
    private bool isAttackingTable;

    private AudioSource audios;
    private bool startPlay;

    [SerializeField] private AudioClip[] goblinSounds;
    [SerializeField] private float timeRemaining;
    [SerializeField] private float timeReload;
    private bool timerIsRunning = false;

    private GameObject currentTable;
    private Coroutine tableAttackCoroutine;

    private void Awake()
    {
        mTransform = GetComponent<Transform>();
        anim = GetComponent<Animator>();
        audios = GetComponent<AudioSource>();
        timerIsRunning = true;
        startPlay = false;

        StartCoroutine(Sound());
    }

    private IEnumerator Sound()
    {
        yield return new WaitForSeconds(Random.Range(0, 10f));
        startPlay = true;
    }

    private void GoblinSoundsPlaying()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                PlayRandom();
                timeRemaining = timeReload;
            }
        }
    }

    private void PlayRandom()
    {
        if (audios.isPlaying) return;
        audios.clip = goblinSounds[Random.Range(0, goblinSounds.Length)];
        audios.Play();
    }

    private void Start()
    {
        leavePlayerAfterDead = false;
        leavePlayerAfterDead2 = false;
        target = GameObject.FindWithTag("Player").transform;
    }

    private void FixedUpdate()
    {
        if (attack && !PlayerDeath.dead)
        {
            nearFightRun = false;

            anim.SetBool("Walk", false);
            anim.SetBool("PlayerShield", false);
            anim.SetBool("Punch", true);
            anim.SetBool("FightRun", false);
        }
        else
        {
            anim.SetBool("Punch", false);
            anim.SetBool("FightRun", false);
        }

        if (!onEnemyZone && !PlayerDeath.dead)
        {
            if (Vector3.Distance(mTransform.position, target.transform.position) < seeDistance)
            {
                seePlayer = true;

                anim.SetBool("SeePlayer", true);
                anim.SetBool("Walk", false);

                var targetPos = target.position;
                targetPos.y = mTransform.position.y;
                var targetDir = Quaternion.LookRotation(targetPos - mTransform.position);
                mTransform.rotation = Quaternion.Slerp(mTransform.rotation, targetDir, rotationSpeed * Time.deltaTime);
            }
            else
            {
                seePlayer = false;
                anim.SetBool("SeePlayer", false);
            }
        }

        if (onEnemyZone)
        {
            onEnemyZone = true;
            anim.SetBool("SeePlayer", false);
            Following();
        }
        else
        {
            anim.SetBool("Run", false);
        }

        if (!KnightAnimations.blockWalking)
        {
            anim.SetBool("FightRun", false);
        }

        if ((nearFightRun || isAttackingTable) && !attack && !PlayerDeath.dead)
        {
            anim.SetBool("Run", false);
            anim.SetBool("FightRun", true);
        }
        else
        {
            if (!isAttackingTable) anim.SetBool("FightRun", false);
        }

        // Patrolling
        if (abilityToPatroling && !PlayerDeath.dead && !onEnemyZone && !seePlayer && !anim.applyRootMotion == false)
        {
            if (!hitByShield)
            {
                anim.SetBool("SeePlayer", false);
                anim.SetBool("Walk", true);

                if (Vector3.Distance(wayPoints[current].transform.position, transform.position) < wRadius)
                {
                    current++;

                    if (current >= wayPoints.Length)
                    {
                        current = 0;
                    }
                }

                speed = 1f;
                transform.position = Vector3.MoveTowards(transform.position, wayPoints[current].transform.position, Time.deltaTime * speed);

                var targetPos = wayPoints[current].transform.position;
                targetPos.y = transform.position.y;
                var targetDir = Quaternion.LookRotation(targetPos - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetDir, rotationSpeed * Time.deltaTime);
            }
            else
            {
                speed = 0f;
            }
        }
        else
        {
            anim.SetBool("Walk", false);
        }

        //HitByShield
        if (hitByShield)
        {
            anim.SetBool("HitByShield", true);
            anim.SetBool("TakeDamage", false);
            anim.SetBool("Walk", false);
            anim.SetBool("Punch", false);
            anim.SetBool("Run", false);
            anim.SetBool("PlayerShield", false);
            anim.SetBool("FightRun", false);
            anim.SetBool("SeePlayer", false);

            StartCoroutine(HitShield());
        }
        else
        {
            anim.SetBool("HitByShield", false);
        }

        // Patrolling
        if (PlayerDeath.dead && abilityToPatroling && !anim.applyRootMotion == false && !hitByShield)
        {
            StartCoroutine(TimeBeforeWalk());

            if (leavePlayerAfterDead)
            {
                anim.SetBool("SeePlayer", false);
                anim.SetBool("Walk", true);

                if (Vector3.Distance(wayPoints[current].transform.position, transform.position) < wRadius)
                {
                    current++;

                    if (current >= wayPoints.Length)
                    {
                        current = 0;
                    }
                }

                speed = 1f;
                transform.position = Vector3.MoveTowards(transform.position, wayPoints[current].transform.position, Time.deltaTime * speed);

                var targetPos = wayPoints[current].transform.position;
                targetPos.y = transform.position.y;
                var targetDir = Quaternion.LookRotation(targetPos - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetDir, rotationSpeed * Time.deltaTime);
            }
        }

        //Patrolling
        if (PlayerDeath.dead && !abilityToPatroling && !anim.applyRootMotion == false && !hitByShield)
        {
            if (Vector3.Distance(wayPoints[current].transform.position, transform.position) < wRadius)
            {
                anim.SetBool("Walk", false);

                var targetPos = target.position;
                targetPos.y = transform.position.y;
                var targetDir = Quaternion.LookRotation(targetPos - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetDir, rotationSpeed * Time.deltaTime);
            }
            else
            {
                StartCoroutine(TimeBeforeWalk2());

                if (leavePlayerAfterDead2)
                {
                    anim.SetBool("SeePlayer", false);
                    anim.SetBool("Walk", true);

                    speed = 1f;
                    transform.position = Vector3.MoveTowards(transform.position, wayPoints[current].transform.position, Time.deltaTime * speed);

                    var targetPos = wayPoints[current].transform.position;
                    targetPos.y = transform.position.y;
                    var targetDir = Quaternion.LookRotation(targetPos - transform.position);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetDir, rotationSpeed * Time.deltaTime);
                }
            }
        }

        //Patrolling
        if (!PlayerDeath.dead && !abilityToPatroling && !anim.applyRootMotion == false && !hitByShield)
        {
            if (Vector3.Distance(wayPoints[current].transform.position, transform.position) < wRadius)
            {
                if (onEnemyZone)
                {
                    anim.SetBool("Walk", false);

                    var targetPos = target.position;
                    targetPos.y = transform.position.y;
                    var targetDir = Quaternion.LookRotation(targetPos - transform.position);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetDir, rotationSpeed * Time.deltaTime);
                }
            }
            else
            {
                if (!onEnemyZone && !seePlayer && !hitByShield)
                {
                    anim.SetBool("SeePlayer", false);
                    anim.SetBool("Walk", true);

                    speed = 1f;
                    transform.position = Vector3.MoveTowards(transform.position, wayPoints[current].transform.position, Time.deltaTime * speed);

                    var targetPos = wayPoints[current].transform.position;
                    targetPos.y = transform.position.y;
                    var targetDir = Quaternion.LookRotation(targetPos - transform.position);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetDir, rotationSpeed * Time.deltaTime);
                }
            }
        }

        if (KnightAnimations.punch && seePlayer || KnightAnimations.specialHit && seePlayer)
        {
            coll.radius = 12f;
        }

        if (KnightAnimations.bowFire && seePlayer)
        {
            coll.radius = 12f;
        }

        //Mining
        if (abilityToMining && !seePlayer && !onEnemyZone && anim.applyRootMotion)
        {
            sword.SetActive(false);
            pickaxe.SetActive(true);
            anim.SetBool("Mining", true);
        }
        else
        {
            sword.SetActive(true);
            pickaxe.SetActive(false);
            anim.SetBool("Mining", false);
            abilityToMining = false;
        }
    }

    private bool nearFightRun;
    private void Following()
    {
        if (!PlayerDeath.dead && !hitByShield && !anim.applyRootMotion == false)
        {
            if (Vector3.Distance(mTransform.position, target.transform.position) < seeDistance)
            {
                if (Vector3.Distance(mTransform.position, target.transform.position) <= 1.4f && FloatingJoystick.isRunning)
                {
                    if (!attack)
                    {
                        nearFightRun = true;
                        speed = 2.5f;

                        var targetPos = target.position;
                        targetPos.y = mTransform.position.y;
                        var targetDir = Quaternion.LookRotation(targetPos - mTransform.position);
                        mTransform.rotation = Quaternion.Slerp(mTransform.rotation, targetDir, rotationSpeed * Time.deltaTime);

                        mTransform.Translate(new Vector3(0, 0, speed * Time.deltaTime));
                    }
                }
                else
                {
                    nearFightRun = false;

                    anim.SetBool("Run", false);
                    anim.SetBool("FightRun", false);
                }

                if (!nearFightRun)
                {
                    if (Vector3.Distance(mTransform.position, target.transform.position) > attackDistance)
                    {
                        attack = false;
                        speed = 3.4f;

                        if (KnightAnimations.blockWalking && Vector3.Distance(mTransform.position, target.transform.position) < 2f)
                        {
                            speed = 2.5f;
                            anim.SetBool("Punch", false);
                            anim.SetBool("Run", false);
                            anim.SetBool("PlayerShield", false);
                            anim.SetBool("FightRun", true);
                        }
                        else
                        {
                            speed = 3.4f;
                            anim.SetBool("Punch", false);
                            anim.SetBool("Run", true);
                            anim.SetBool("PlayerShield", false);
                        }

                        var targetPos = target.position;
                        targetPos.y = mTransform.position.y;
                        var targetDir = Quaternion.LookRotation(targetPos - mTransform.position);
                        mTransform.rotation = Quaternion.Slerp(mTransform.rotation, targetDir, rotationSpeed * Time.deltaTime);

                        mTransform.Translate(new Vector3(0, 0, speed * Time.deltaTime));
                    }
                    else if (Vector3.Distance(mTransform.position, target.transform.position) <= attackDistance)
                    {
                        if (!KnightAnimations.blockWalking)
                        {
                            anim.SetBool("FightRun", false);
                            speed = 3.4f;

                            if (!KnightAnimations.block)
                            {
                                anim.SetBool("Run", false);
                                anim.SetBool("FightRun", false);

                                StartCoroutine(TimeBeforeAttack());
                            }
                            else
                            {
                                attack = false;
                                anim.SetBool("Run", false);
                                anim.SetBool("PlayerShield", true);
                            }
                        }
                        else
                        {
                            speed = 2f;
                            anim.SetBool("Run", false);
                            anim.SetBool("FightRun", true);
                        }

                        if (PlayerController.onGround)
                        {
                            var targetPos = target.position;
                            targetPos.y = mTransform.position.y;
                            var targetDir = Quaternion.LookRotation(targetPos - mTransform.position);
                            mTransform.rotation = Quaternion.Slerp(mTransform.rotation, targetDir, rotationSpeed * Time.deltaTime);
                        }
                    }
                }
            }
        }
        else
        {
            nearFightRun = false;
            onEnemyZone = false;
            attack = false;
            anim.SetBool("SeePlayer", false);
            anim.SetBool("PlayerShield", false);
        }
    }

    private void Update()
    {
        if (startPlay && !onEnemyZone)
        {
            GoblinSoundsPlaying();
        }

        if (anim.applyRootMotion == false)
        {
            rotationSpeed = 0f;
        }

        if (onEnemyZone && !anim.applyRootMotion == false)
        {
            healthBar.SetActive(true);
        }
        else
        {
            healthBar.SetActive(false);
        }
    }

    private bool leavePlayerAfterDead;
    private IEnumerator TimeBeforeWalk()
    {
        yield return new WaitForSeconds(Random.Range(2.5f, 8f));
        leavePlayerAfterDead = true;
    }

    private IEnumerator TimeBeforeAttack()
    {
        yield return new WaitForSeconds(0.2f);
        attack = true;
    }

    private IEnumerator HitShield()
    {
        yield return new WaitForSeconds(1.6f);
        hitByShield = false;
    }

    private bool leavePlayerAfterDead2;
    private IEnumerator TimeBeforeWalk2()
    {
        yield return new WaitForSeconds(Random.Range(1f, 8f));
        leavePlayerAfterDead2 = true;
    }

    // ============ ОБНАРУЖЕНИЕ СТОЛА ============
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("PlayerShield"))
        {
            hitByShield = true;
        }

        // Проверяем, столкнулся ли гоблин со столом
        if (collision.gameObject.CompareTag(tableTag) && seePlayer)
        {
            currentTable = collision.gameObject;
            isAttackingTable = true;

            // Останавливаем предыдущую корутину если есть
            if (tableAttackCoroutine != null)
                StopCoroutine(tableAttackCoroutine);

            tableAttackCoroutine = StartCoroutine(AttackTableRepeatedly());
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag(tableTag))
        {
            isAttackingTable = false;
            anim.SetBool("FightRun", false);

            if (tableAttackCoroutine != null)
                StopCoroutine(tableAttackCoroutine);
            tableAttackCoroutine = null;
            currentTable = null;
        }
    }

    private IEnumerator AttackTableRepeatedly()
    {
        while (currentTable != null && seePlayer)
        {
            BrakeTable table = currentTable.GetComponent<BrakeTable>();

            // Если стол сломан или его нет - выходим
            if (table == null || table.IsBroken)
            {
                isAttackingTable = false;
                anim.SetBool("FightRun", false);
                currentTable = null;
                tableAttackCoroutine = null;
                yield break;
            }

            // Включаем анимацию атаки
            anim.SetBool("FightRun", true);

            // Небольшая задержка для анимации
            yield return new WaitForSeconds(0.2f);

            // Наносим урон столу
            table.TakeDamage(1);

            // Пауза между ударами
            yield return new WaitForSeconds(attackTableInterval);
        }

        isAttackingTable = false;
        anim.SetBool("FightRun", false);
        tableAttackCoroutine = null;
    }

    private void OnTriggerStay(Collider goblin)
    {
        if (goblin.gameObject.CompareTag("Player"))
        {
            onEnemyZone = true;
        }
    }

    private void OnTriggerExit(Collider goblin)
    {
        onEnemyZone = false;
        healthBar.SetActive(false);
    }
}