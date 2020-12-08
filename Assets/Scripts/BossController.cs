using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public float attackFrequency = 3.0f;
    float timer;

    public int bossHealth = 5;
    bool defeated = false;

    Rigidbody2D rigidbody2d;
    Animator animator;
    SpriteRenderer spriteRenderer;

    AudioSource audioSource;
    public AudioClip bossDamage;
    public AudioClip bossDefeated;

    public ParticleSystem smokeEffect;
    public GameObject projectilePrefab;
    public GameObject rewardPrefab;

    RubyController rubyController;


    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        timer = attackFrequency;

        GameObject rubyControllerObject = GameObject.FindWithTag("RubyController");
        if (rubyControllerObject != null)
        {
            rubyController = rubyControllerObject.GetComponent<RubyController>();
        }
    }

    void Update()
    {
        if (defeated)
        {
            return;
        }

        timer -= Time.deltaTime;
        if (timer < 0)
        {
            Launch();
            timer = attackFrequency;
        }
    }

    void FixedUpdate()
    {
      
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        RubyController player = other.gameObject.GetComponent<RubyController>();

        if (player != null)
        {
            player.ChangeHealth(-2);
        }
    }

    void Launch()
    {
        int x;
        Vector2 left = new Vector2(-1, 0);
        for (x = 0; x < 6; x++)
        {
            GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * (0.5f * x) + Vector2.left, Quaternion.identity);
            EnemyProjectile projectile = projectileObject.GetComponent<EnemyProjectile>();
            projectile.Launch(left, 300);
        }
        animator.SetTrigger("Launch");
    }

    public void Damage()
    {
        bossHealth--;
        rubyController.UpdateScore();
        audioSource.PlayOneShot(bossDamage);
        
        if (bossHealth <= 0)
        {
            defeated = true;
            rigidbody2d.simulated = false;
            animator.SetBool("Defeated", true);
            smokeEffect.Stop(true);
            audioSource.PlayOneShot(bossDefeated);
            GameObject projectileObject = Instantiate(rewardPrefab, rigidbody2d.position + Vector2.left + (Vector2.down * 0.1f), Quaternion.identity);
        }
    }
}
