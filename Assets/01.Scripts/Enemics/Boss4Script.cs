﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Boss4Script : MonoBehaviour {
    public GameObject players, loadMenu, orbPrefab, orbPrefabBoss;
    GameObject player;
    int nivellPlayer;
    public AudioSource efectesAS, musicAS;
    public AudioClip enemieHit, levelWin, magSpell;
    GameObject[] materials = new GameObject[3];
    public Transform orbs, multipleOrbs1, multipleOrbs2;
    Animator animEnemic;
    Vector2 enemicPos;
    Vector2 enemicScale;
    float diferencePositions, velocity, vida;
    string[] actions = new string[3];
    bool getingAttack, defending, isDead, hasAttacked1, hasAttacked2;
    [HideInInspector]
    public bool attackDreta;
    BoxCollider2D playerCollider;

    void Start()
    {
        loadMenu.SetActive(false);
        BuscarPlayerActive();
        for (int i = 0; i < 3; i++)
        {
            materials[i] = transform.GetChild(i).gameObject;
            materials[i].SetActive(false);
        }
        enemicScale = transform.localScale;
        animEnemic = GetComponent<Animator>();
        actions[0] = "doAttack1";
        actions[1] = "doAttack2";
        actions[2] = "doIdle";
        isDead = false;
        getingAttack = false;
        attackDreta = false;
        velocity = 2f;
        vida = 120;
    }

    void Update()
    {
        IsDead();
        if (!isDead)
        {
            EnimieHited();
            PlayerNearEnimie();
            IsAttackingMagic1();
            IsAttackingMagic2();
        }
    }

    void BuscarPlayerActive()
    {
        for (int i = 0; i < players.transform.childCount; i++)
        {
            if (players.transform.GetChild(i).gameObject.activeInHierarchy)
            {
                player = players.transform.GetChild(i).gameObject;
                nivellPlayer = i + 1;
            }
        }
    }
    void GoToMenu()
    {
        loadMenu.SetActive(true);
    }

    void IsDead()
    {
        if (vida <= 0 && !isDead)
        {
            musicAS.Stop();
            musicAS.PlayOneShot(levelWin);
            animEnemic.SetBool("doDead", true);
            GetComponent<BoxCollider2D>().isTrigger = true;
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            isDead = true;
            Invoke("ActivateMaterials", 1f);
        }

        if (animEnemic.GetCurrentAnimatorStateInfo(0).IsName("Boss4-Dead"))
        {
            animEnemic.SetBool("doDead", false);
            PlayerPrefs.SetString("level5FActive", "true");
            Invoke("GoToMenu", 6);

            Social.ReportProgress("CggIv9GCrUcQAhAD", 100.0f, (bool success) => {
                Debug.Log("Result: " + success);
            });
        }
    }
    void ActivateMaterials()
    {
        for (int i = 0; i < 3; i++)
        {
            if (materials[i] != null)
            {
                materials[i].SetActive(true);
                if (materials[i].tag == "Or") materials[i].GetComponent<Animator>().SetTrigger("activateOr");
                else if (materials[i].tag == "Ferro") materials[i].GetComponent<Animator>().SetTrigger("activateFerro");
                else if (materials[i].tag == "Fusta") materials[i].GetComponent<Animator>().SetTrigger("activateFusta");
            }
        }
    }
    void EnimieHited()
    {
        playerCollider = player.GetComponent<BoxCollider2D>();
        if (!player.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Main-Attack")) getingAttack = false;
        if (GetComponent<BoxCollider2D>().IsTouching(playerCollider))
        {
            if (!getingAttack && player.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Main-Attack"))
            {
                efectesAS.PlayOneShot(enemieHit);
                animEnemic.SetTrigger("doHit");
                vida -= 10 * nivellPlayer;
                getingAttack = true;
            }
        }
    }
    void PlayerNearEnimie()
    {
        enemicPos = transform.position;
        diferencePositions = player.transform.position.x - transform.position.x;
        if (diferencePositions >= -15 && diferencePositions <= -8f)
        {
            if (enemicScale.x < 0)
            {
                enemicScale.x = enemicScale.x * -1;
                transform.localScale = enemicScale;
            }
            animEnemic.SetFloat("speed", velocity);
            float posX = enemicPos.x - (velocity * Time.deltaTime);
            enemicPos = new Vector2(posX, enemicPos.y);
            transform.position = enemicPos;
        }
        else if (diferencePositions <= 15 && diferencePositions >= 8f)
        {
            if (enemicScale.x > 0)
            {
                enemicScale.x = -enemicScale.x;
                transform.localScale = enemicScale;
            }
            animEnemic.SetFloat("speed", velocity);
            float posX = enemicPos.x + (velocity * Time.deltaTime);
            enemicPos = new Vector2(posX, enemicPos.y);
            transform.position = enemicPos;
        }
        else if (diferencePositions >= -8f && diferencePositions <= 8f)
        {
            if (diferencePositions <= 0 && enemicScale.x < 0)
            {
                enemicScale.x = enemicScale.x * -1;
                transform.localScale = enemicScale;
                attackDreta = false;
            }
            else if (diferencePositions >= 0 && enemicScale.x > 0)
            {
                enemicScale.x = -enemicScale.x;
                transform.localScale = enemicScale;
                attackDreta = true;
            }
            animEnemic.SetFloat("speed", 0);
            if (animEnemic.GetCurrentAnimatorStateInfo(0).normalizedTime >= animEnemic.GetCurrentAnimatorStateInfo(0).length)
            {
                int rand = Random.Range(0, 3);
                animEnemic.SetTrigger(actions[rand]);
            }
        }
        else
        {
            animEnemic.SetFloat("speed", 0);
        }
    }
    void IsAttackingMagic1()
    {
        if (!hasAttacked1 && animEnemic.GetCurrentAnimatorStateInfo(0).IsName("Boss4-Attack1") && animEnemic.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f)
        {
            hasAttacked1 = true;
            Invoke("AttackOrb", 0f);
        }
        else if (!animEnemic.GetCurrentAnimatorStateInfo(0).IsName("Boss4-Attack1")) hasAttacked1 = false;
    }
    void AttackOrb()
    {
        efectesAS.PlayOneShot(magSpell);
        Instantiate(orbPrefab, orbs);
    }
    void IsAttackingMagic2()
    {
        if (!hasAttacked2 && animEnemic.GetCurrentAnimatorStateInfo(0).IsName("Boss4-Attack2") && animEnemic.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f)
        {
            hasAttacked2 = true;
            Invoke("AttackMultipleOrbs", 0f);
        }
        else if (!animEnemic.GetCurrentAnimatorStateInfo(0).IsName("Boss4-Attack2")) hasAttacked2 = false;
    }
    void AttackMultipleOrbs()
    {
        efectesAS.PlayOneShot(magSpell);
        Instantiate(orbPrefabBoss, multipleOrbs1);
        Instantiate(orbPrefabBoss, multipleOrbs2);
    }
}