using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [Header("Preset Fields")] 
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject splashFx;
    
    [Header("Settings")]
    [SerializeField] private float attackRange;

    [Header("Navigation")]
    public Transform target;   
    private NavMeshAgent nmAgent;
    private Vector3 randomPosition;


    public enum State 
    {
        None,
        Idle,
        Attack
    }
    
    [Header("Debug")]
    public State state = State.None;
    public State nextState = State.None;

    private bool attackDone;

    private void Start()
    { 
        state = State.None;
        nextState = State.Idle;

        nmAgent = GetComponent<NavMeshAgent>();
        target = FindObjectOfType<MoveControl>().transform;
    }

    void FixedUpdate()
    {
        
    }

    private void Update()
    {
        if (target != null)
        {
            Vector3 direction = target.position - transform.position;
            float angle = Vector3.Angle(transform.forward, direction);
            float distance = Vector3.Distance(transform.position, target.position);

            // 가까운 정면 방향으로 플레이어 보이면 추적
            if (angle < 90f && distance < 10f)
            {
                nmAgent.SetDestination(target.position);
            }
            else
            {
                // 아니면 랜덤 위치로 이동
                if((nmAgent.remainingDistance <= nmAgent.stoppingDistance) || !nmAgent.hasPath)
                {
                    randomPosition = GetRandomPositionOnNavMesh();
                    nmAgent.SetDestination(randomPosition);
                }
            }
        }

        //1. 스테이트 전환 상황 판단
        if (nextState == State.None) 
        {
            switch (state) 
            {
                case State.Idle:
                    //1 << 6인 이유는 Player의 Layer가 6이기 때문
                    if (Physics.CheckSphere(transform.position, attackRange, 1 << 6, QueryTriggerInteraction.Ignore))
                    {
                        nextState = State.Attack;
                    }
                    break;
                case State.Attack:
                    if (attackDone)
                    {
                        nextState = State.Idle;
                        attackDone = false;
                    }
                    break;
                //insert code here...
            }
        }
        
        //2. 스테이트 초기화
        if (nextState != State.None) 
        {
            state = nextState;
            nextState = State.None;
            switch (state) 
            {
                case State.Idle:
                    break;
                case State.Attack:
                    Attack();
                    break;
                //insert code here...
            }
        }
        
        //3. 글로벌 & 스테이트 업데이트
        //insert code here...
    }

    Vector3 GetRandomPositionOnNavMesh()
    {
        // 랜덤한 방향 벡터 생성
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * 10f;
        randomDirection += transform.position;  // 현재 위치에 더하기

        NavMeshHit navHit;
        // randomDirection에서 가장 가까운 navMesh 위치 찾기
        if (NavMesh.SamplePosition(randomDirection, out navHit, 10f, -1))
        {
            return navHit.position;
        }
        return transform.position;
    }
    
    private void Attack() //현재 공격은 애니메이션만 작동합니다.
    {
        animator.SetTrigger("attack");
    }

    public void InstantiateFx() //Unity Animation Event 에서 실행됩니다.
    {
        Instantiate(splashFx, transform.position, Quaternion.identity);
    }
    
    public void WhenAnimationDone() //Unity Animation Event 에서 실행됩니다.
    {
        attackDone = true;
    }


    private void OnDrawGizmosSelected()
    {
        //Gizmos를 사용하여 공격 범위를 Scene View에서 확인할 수 있게 합니다. (인게임에서는 볼 수 없습니다.)
        //해당 함수는 없어도 기능 상의 문제는 없지만, 기능 체크 및 디버깅을 용이하게 합니다.
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawSphere(transform.position, attackRange);
    }
}
