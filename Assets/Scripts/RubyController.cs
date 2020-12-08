using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RubyController : MonoBehaviour
{
    public float speed = 3.0f;

    public int maxHealth = 5;

    int cogAmmo = 4;
    
    public float timeInvincible = 2.0f;
    bool isInvincible;
    float invincibleTimer;

    int currentHealth;
    public int health {get {return currentHealth;}}

    public int robotScore = 0;
    public Text robotText;
    public Text gameOverText;
    public Text cogText;
    public Text launcherText;
    public Text bossText;

    Rigidbody2D rigidbody2d;
    float horizontal;
    float vertical;
    BoxCollider2D collider2d;

    Animator animator;
    Vector2 lookDirection = new Vector2(1,0);
    SpriteRenderer spriteRenderer;

    AudioSource audioSource;
    public AudioClip throwSound;
    public AudioClip hitSound;

    public GameObject hitEffect;
    public GameObject healthEffect;
    public GameObject projectilePrefab;

    bool gameOver = false;
    AudioController audioController;

    BossController bossController;

    public static int level = 1;

    bool alienTalked = false;
    static int launcherType = 0;

    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        collider2d = GetComponent<BoxCollider2D>();
        currentHealth = maxHealth;
        gameOverText.text = "";
        UpdateScore();

        GameObject audioControllerObject = GameObject.FindWithTag("AudioController");
        if (audioControllerObject != null)
        {
            audioController = audioControllerObject.GetComponent<AudioController>();
        }
    }

    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector2 move = new Vector2(horizontal, vertical);

        if(!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }

        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);

        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
                isInvincible = false;
        }

        if(Input.GetKeyDown(KeyCode.C) && cogAmmo > 0)
        {
            Launch();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("NPC"));
            if (hit.collider != null)
            {
                NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                if (character != null)
                {
                    if (character.tag == "Jambi")
                    {
                        if (robotScore == 4)
                        {
                            level += 1;
                            SceneManager.LoadScene("Scene2");
                        }
                        else
                        {
                            character.DisplayDialog();
                        }
                    }
                    else if (character.tag == "Alien")
                    {
                        if (alienTalked == true) {
                            SceneManager.LoadScene("SceneBoss");
                        }
                        else 
                        {
                            character.DisplayDialog();
                            alienTalked = true;
                        }
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (gameOver)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    void FixedUpdate()
    {
        Vector2 position = rigidbody2d.position;
        position.x = position.x + speed * horizontal * Time.deltaTime;
        position.y = position.y + speed * vertical * Time.deltaTime;
        
        rigidbody2d.MovePosition(position);
    }

    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        {
            if (isInvincible)
                return;
            isInvincible = true;
            invincibleTimer = timeInvincible;
            
            animator.SetTrigger("Hit");
            Instantiate(hitEffect, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
            PlaySound(hitSound);
        }
        else if (amount > 0)
        {
            Instantiate(healthEffect, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);
        if (currentHealth == 0)
        {
            gameOverText.text = "You lost! Press R to restart.";
            gameOver = true;
            speed = 0.0f;
            audioController.ChangeMusic(2);
            collider2d.enabled = false;
        }
    }
    void Launch()
    {
        if (launcherType == 0)
        {
            GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

            Projectile projectile = projectileObject.GetComponent<Projectile>();
            projectile.Launch(lookDirection, 300);
        } 
        else if (launcherType == 1)
        {
            GameObject projectileObject1 = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f + Vector2.left, Quaternion.identity);
            GameObject projectileObject2 = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
            GameObject projectileObject3 = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f + Vector2.right, Quaternion.identity);

            Projectile projectile1 = projectileObject1.GetComponent<Projectile>();
            projectile1.Launch(RotateVector(lookDirection, 20.0f), 300);
            Projectile projectile2 = projectileObject2.GetComponent<Projectile>();
            projectile2.Launch(lookDirection, 300);
            Projectile projectile3 = projectileObject3.GetComponent<Projectile>();
            projectile3.Launch(RotateVector(lookDirection, -20.0f), 300);
            
        }
        cogAmmo -= 1;
        UpdateScore();
        animator.SetTrigger("Launch");
        PlaySound(throwSound);
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    public void ChangeScore(){
        robotScore += 1;
        UpdateScore();
        if (level == 1 && robotScore == 4) 
        {
            gameOverText.text = "Talk to Jambi to visit stage two!";
        }
        else if (level == 2 && robotScore == 4)
        {
            gameOverText.text = "You Win! Game Created by Eric Osorio.\nPress R to restart.";
            gameOver = true;
            audioController.ChangeMusic(1);
        }
    }

    public void UpdateScore(){
        robotText.text = "Robots Fixed: " + robotScore;
        cogText.text = "Cog Ammo: " + cogAmmo;
        if (launcherType == 1)
        {
            launcherText.text = "Launcher Type: Spread";
        }
        else
        {
            launcherText.text = "Launcher Type: Normal";
        }
        GameObject bossObject = GameObject.FindWithTag("Boss");
        if (bossObject != null)
        {
            bossController = bossObject.GetComponent<BossController>();
            bossText.text = "Boss Health: " + bossController.bossHealth;
        }
        else
        {
            bossText.text = "Boss Health: N/A";
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "CogBag")
        {
            cogAmmo += 4;
            UpdateScore();
            Destroy(other.gameObject);
        }
        if(other.tag == "SpreadBonus")
        {
            launcherType = 1;
            UpdateScore();
            Destroy(other.gameObject);
            SceneManager.LoadScene("Scene1");
        }
    }

    Vector2 RotateVector(Vector2 originalVector, float angle)
    {
        if (originalVector.y < 0)
        {
            return Quaternion.Euler(0, 0, angle * -1.0f) * originalVector;
        }
        else 
        {
            return Quaternion.Euler(0, 0, angle) * originalVector;
        }
        //return new Vector2 (((originalVector.x * Mathf.Cos(angle)) - (originalVector.y * Mathf.Sin(angle))), ((originalVector.x * Mathf.Sin(angle)) + (originalVector.y * Mathf.Cos(angle))));
    }
}
