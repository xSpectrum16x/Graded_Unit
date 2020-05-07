﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlimeJumping : MonoBehaviour
{
    
    public float jumpForce;
    private float RandomJump;
    public LayerMask GroundType;
    public Transform GroundDetection;
    Rigidbody2D rb;
    private CircleCollider2D CC2;
    private BoxCollider2D BC;
    private float direction;
    public float speed;
    public float aggrospeed;
    public float radius;
    public LayerMask PlayerID;
    private Coroutine SlimeIdle;
    private Coroutine SlimeAgro;
    private bool isgrounded;
    public Transform player;
    private Slider slider;
    // Start is called before the first frame update
    void Start()
    {
        RandomJump = Random.Range(2,4);
        rb = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
    {
        SlimeActions();
    }
    private void Update()
    {
        RaycastHit2D grounddetector = Physics2D.Raycast(GroundDetection.position, Vector2.down, 3, GroundType);
        {
            if (grounddetector.collider == true)
            {
                isgrounded = true;
            }
            else
            {
                isgrounded = false;
            }
        }
        print(isgrounded);

    }

    IEnumerator IdleStance()
    {
        print("Idling");
        //Calm slimy boy
        direction = 1;
        while (true)
        {
            RandomJump = Random.Range(3, 5);
            rb.velocity = (new Vector2(speed * direction, 0));
            direction *= -1;
            yield return new WaitForSeconds(RandomJump);
           
        }
    }
   private void SlimeActions()
    {
        Collider2D PlayerDetection = Physics2D.OverlapCircle(transform.position, radius, PlayerID);
        if(PlayerDetection != null)
        {
            StartCoroutine("Agro");
        }
        else
        {
            print("PlayerEscaped");
            StopCoroutine("Agro");
        }
       

    }
    IEnumerator Agro()
    {
        while (true)
        {
            print("I am aggressive");

            // 1 if player is to the right of the slime, If -1 if to the left
            int direction = (this.transform.position.x < player.transform.position.x) ? 1 : -1;
            print(direction);
           
            if(isgrounded==true)
            {
                    // Jump towards the player
                    rb.velocity = (new Vector2(aggrospeed * direction, jumpForce));
            }
            if (direction == 1)
            {
                transform.eulerAngles = new UnityEngine.Vector3(0, -180, 0);
            }
            else if (direction == -1)
            {
                transform.eulerAngles = new UnityEngine.Vector3(0, 0, 0);
            }
            yield return new WaitForSeconds(3);

        }
    }
    private void OnDrawGizmosSelected()
    {
        //Makes the wireframe red
        Gizmos.color = Color.red;
        //Creates the wireframe from the shooting point and uses the variable attackRange to represent how big or small it is.
        Gizmos.DrawWireSphere(transform.position, radius);
    }
   
}
   


    
        
    

