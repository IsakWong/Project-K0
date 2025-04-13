using K1.Gameplay;

namespace K3
{
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class K3CharacterUnit : GameUnit
{
    [Header("战斗属性")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("攻击点配置")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayers;

    [Header("动画参数")]
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string deathBool = "IsDead";

    private int currentHealth;
    private bool isDead;
    private bool canAttack = true;
    private float lastAttackTime;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // 朝向枚举
    private enum FacingDirection { Left, Right }
    private FacingDirection currentFacing = FacingDirection.Right;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        UpdateFacingDirection();
    }

    // 更新角色朝向
    private void UpdateFacingDirection()
    {
        currentFacing = spriteRenderer.flipX ? FacingDirection.Left : FacingDirection.Right;
    }

    // 攻击方法
    public void PerformAttack()
    {
        if (CanPerformAttack())
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private bool CanPerformAttack()
    {
        return !isDead && canAttack && Time.time >= lastAttackTime + attackCooldown;
    }

    private IEnumerator AttackRoutine()
    {
        canAttack = false;
        lastAttackTime = Time.time;

        // 触发攻击动画
        animator.SetTrigger(attackTrigger);

        // 等待动画前摇（根据实际动画调整）
        yield return new WaitForSeconds(0.2f);

        // 检测攻击区域
        DetectTargets();

        // 等待攻击冷却
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // 攻击检测逻辑
    private void DetectTargets()
    {
        Vector2 attackPosition = attackPoint.position;
        
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPosition,
            attackRange,
            enemyLayers
        );

        foreach (Collider2D enemy in hitEnemies)
        {
            K3CharacterUnit target = enemy.GetComponent<K3CharacterUnit>();
            if (target != null && target != this)
            {
                target.TakeDamage(attackDamage);
            }
        }
    }

    // 受击方法
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        
        // 触发受击动画
        animator.SetTrigger(hitTrigger);
        
        // 屏幕震动（可选）
        StartCoroutine(CameraShake(0.1f, 0.1f));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 死亡处理
    private void Die()
    {
        isDead = true;
        animator.SetBool(deathBool, true);
        
        // 禁用碰撞和移动
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().simulated = false;
        enabled = false;
    }

    // 屏幕震动协程
    private IEnumerator CameraShake(float duration, float magnitude)
    {
        Vector3 originalPos = Camera.main.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            Camera.main.transform.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.localPosition = originalPos;
    }

    // 调试绘制攻击范围
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    // 属性访问器
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;
}
}