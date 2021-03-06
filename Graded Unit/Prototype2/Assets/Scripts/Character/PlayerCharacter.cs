﻿using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
//Callum
//22/02/2020
//Player Controls
public class PlayerCharacter : MonoBehaviour
{//Speed at which the player moves
    private float moveSpeed = 8f;
    //Fall Multiplier
    private float fallMultipler = 5f;
    private Respawn RespawnObj;
    private float lowJumpMultiplier = 5f;
    //Changes the height at which the player jumps
    private float jumpForce = 8;
    //For checking what type of ground is below
    public LayerMask GroundType;
    public LayerMask EnemyIdentifier;
    public int MeleeDamage;
    public int BulletDamage;
    //Boolean that turns on each time the player touches the ground. To allow a player a double jump but no more
    private bool doublejump;
    //A variable to reference the rigid body of the character
    Rigidbody2D rb;
    //A variable to reference the BoxCollider of the character
    BoxCollider2D bc;
    //How long in seconds the dash takes to be usable again
    [SerializeField]
    private float dashCooldown = 2;
    public int Health = 1000;
    //Time until the player can reuse the dash
    private float nextdashtime;
    private float Attackcooldown;
    public float nextattacktime;
    //References the PlayerAudio Script. It handles all the audio 
    private PlayerAudio AudioObj;
    private EnemyHealth EnemyHealthObj;
    //Keeps count of coins
    public int coins;
    //A boolean made to determine what direction the player is looking in
    private bool facingRight;
    //A float made to check a change in the direction checking if the velocity is currently positive or negative. 
    float direction;
    //2 different booleans used for jumping requests. These are here to for optimization purposes
    bool jumpRequest;
    bool doubleJumpRequest;
    //Used to reference the ShootingPos on the character for creating bullets
    public Transform ShootPoint;
    //Used to reference the bullet prefab for creating bullets
    public GameObject BulletPrefeb;
    public Transform AttackPoint;
    //These 2 variables are used to control how large of an area the melee hitbox takes up
    public float AttackRangeX;
    public float AttackRangeY;
    public Animator Animations;
    // Start is called before the first frame update
    void Start()
    {

        //Calls on the component RigidBody
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        facingRight = true;
        nextdashtime = 0;
        AudioObj = GameObject.FindObjectOfType<PlayerAudio>();
        RespawnObj = GameObject.FindObjectOfType<Respawn>();
        EnemyHealthObj = GameObject.FindObjectOfType<EnemyHealth>();
    }
    private void Update()
    {
        if (Time.timeScale == 1)
        {
            Updates();
        }
    }
    private void Updates()
    {
        Animations.SetBool("OnGround", IsGrounded());
        //If the player is grounded and jumps (using space) the game gives permission to jump
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            //Accesses the PlayerAudio script and plays a jump sound when the player jumps
            AudioObj.Source.clip = AudioObj.JumpSound;
            AudioObj.Source.Play();
            jumpRequest = true;
        }
        //If the player still has a jump left and jumps (using space) the game gives permission to double jump
        else if (Input.GetKeyDown(KeyCode.Space) && doublejump == true)
        {
            //Accesses the PlayerAudio script and plays a different jump sound when the player double jumps
            AudioObj.Source.clip = AudioObj.DoubleJumpSound;
            AudioObj.Source.Play();
            doubleJumpRequest = true;
        }
        // If the left mouse button is pressed
        if (Input.GetKeyDown(KeyCode.Mouse1))

        {
            //Runs the function shoot
            Shoot();
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Dash();
        }
    }
    //This method allows for a visual wireframe of the 1 attacks range. This makes it alot easier to change by eye instead of kinda guessing how big or small it is.
    private void OnDrawGizmosSelected()
    {
        //Makes the wireframe red
        Gizmos.color = Color.red;
        //Creates the wireframe from the shooting point and uses the variable attackRange to represent how big or small it is.
        Gizmos.DrawWireCube(AttackPoint.position, new Vector3(AttackRangeX, AttackRangeY, 1));
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        //Calls all methods
        BasicMovement();
        Jump();
        Melee();
    }
    //Method for movement
    void BasicMovement()
    {
        //Gets input from the Unity Horizontal function for movement using A or D (or left arrow or right arrow)
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0f, 0f);
        //Moves at a fixed rate 
        float HorizontalMovement = movement.x;
        Animations.SetFloat("Movement", Mathf.Abs(HorizontalMovement));
        transform.position += movement * Time.deltaTime * moveSpeed;
        //Gives value to the direction as a float. This is used to compare which way the player is looking
        direction = (Input.GetAxis("Horizontal"));
        //If the players direction is positive and they're not looking to the right then the player is flipped
        if (direction > 0 && !facingRight)
        {
            //runs the function Flip
            Flip();
            //if the players direction is negative and they're not looking to the right then the player is flipped
        }
        else if (direction < 0 && facingRight)
        {
            //runs the function Flip
            Flip();
        }


    }
    //Method for jumping
    void Jump()
    {
        //Checks if the space bar is pressed and if the player is on the ground (if not then they wont jump, this is to avoid infinite jumping
        if (jumpRequest)
        {

            //if the space bar is pressed the player gets moved up by the set variable "jumpForce" 

            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            //The request to jump is no longer true and thus the player can no longer jump
            jumpRequest = false;

        }

        //If the player isnt touching the ground BUT does have a double jump still remaining then the if statement below will trigger
        if (doubleJumpRequest)
        {
            //Adds a double jump equal to the original jump (though this can be easily changed by changing the "jumpForce" to something else
            rb.velocity = (new Vector2(0f, jumpForce));

            //Turns the doublejump boolean to false so they cant jump again
            doublejump = false;
            doubleJumpRequest = false;
        }

        //If on the ground the ability to use a double jump is made true again
        if (IsGrounded())
        {
            doublejump = true;
        }


        //For quicker descent to the ground, if the player starts falling the below if statement will trigger
        if (rb.velocity.y < 0)
        {//the player starts falling quicker as they fall more (-1 being to make sure they dont fall fast and Unity by default has a multiplier at 1 thus without this it would be too fast)
            rb.gravityScale = fallMultipler;
        }
        else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            //Sets the gravity to the low jump multiplier 
            rb.gravityScale = lowJumpMultiplier;
        }
        else
        {
            //Sets the gravity back to 1 
            rb.gravityScale = 1;
        }
    }
    //A method for checking if they're on the ground
    private bool IsGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(bc.bounds.center, bc.bounds.size, 0f, Vector2.down, 1f, GroundType);
        return raycastHit.collider != null;
    }

    void Dash()
    {
        //Gives a cooldown period of 3 seconds until they can dash again
        if (Time.time > nextdashtime)
        {
            //States in the log the cooldown is starting again
            AudioObj.Source.clip = AudioObj.DashSound;
            AudioObj.Source.Play();
            //Gives 10 units of speed to the right
            rb.velocity = new Vector2(Input.GetAxis("Horizontal") * 10, 0f);
            //resets the cooldown timer
            nextdashtime = Time.time + dashCooldown;
        }
    }
    void Melee()
    {

        if (Attackcooldown <= 0)
        {
            //If the left mouse button is pressed
            if (Input.GetKey(KeyCode.Mouse0))
            {
                //References Player Audio and states if the enemy was hit
                AudioObj.Source.clip = AudioObj.MeleeSound;
                AudioObj.Source.Play();
                //Creates a circlular collider at the the shootingpoint transform with a set attack range and makes it only deal damage to enemies
                Collider2D[] Enemies = Physics2D.OverlapBoxAll(AttackPoint.position, new Vector2(AttackRangeX, AttackRangeY), 0, EnemyIdentifier);
                //Creates a for loop
                for (int i = 0; i < Enemies.Length; i++)
                {
                    Enemies[i].GetComponent<EnemyHealth>().MeleeDamage();
                }
            }
            Attackcooldown = nextattacktime;
        }

        else
        {
            Attackcooldown -= Time.deltaTime;
        }

    }
    void Shoot()
    {

        //Plays the shooting sound
        AudioObj.Source.clip = AudioObj.ShootingSound;
        AudioObj.Source.Play();
        //Spawns the bullet prebab at the shootingpos position on the player at the correct rotation of the player
        Instantiate(BulletPrefeb, ShootPoint.position, ShootPoint.rotation);
    }
    private void Flip()
    {
        //Inverts the direction the player 
        facingRight = !facingRight;
        //Rotates the player in the y axis to change for shooting
        transform.Rotate(0f, 180f, 0f);




    }
    //Checks for collisions
    private void OnTriggerEnter2D(Collider2D other)
    {
        //If the player is touching a coin the following if statement is executed
        if (other.gameObject.CompareTag("Coins"))
        {
            //Destroys the coin so it cant be continuously picked up
            Destroy(other.gameObject);
            //Plays the coin sound effect "Coin_Sound_effect.wav"
            AudioObj.Source.clip = AudioObj.CoinSound;
            AudioObj.Source.Play();
            coins += 1;
        }
        //If the player touches an enemy the player takes 5 damage
        if (other.gameObject.CompareTag(("Enemies")))
        {
            AudioObj.Source.clip = AudioObj.DamageSound;
            AudioObj.Source.Play();
            Health -= 5;
            {
                Health -= 5;
            }

        }
    }
}

