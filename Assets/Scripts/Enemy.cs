using System.Collections;
using UnityEngine;
using UnityEngine.AI; // AI, 내비게이션 시스템 관련 코드를 가져오기

// 적 AI를 구현한다
public class Enemy : LivingEntity {
    public LayerMask whatIsTarget; // 추적 대상 레이어

    private LivingEntity targetEntity; // 추적할 대상
    private NavMeshAgent pathFinder; // 경로계산 AI 에이전트

    public ParticleSystem hitEffect; // 피격시 재생할 파티클 효과
    public AudioClip deathSound; // 사망시 재생할 소리
    public AudioClip hitSound; // 피격시 재생할 소리

    private Animator enemyAnimator; // 애니메이터 컴포넌트
    private AudioSource enemyAudioPlayer; // 오디오 소스 컴포넌트
    private Renderer enemyRenderer; // 렌더러 컴포넌트

    public float damage = 20f; // 공격력
    public float timeBetAttack = 0.5f; // 공격 간격
    private float lastAttackTime; // 마지막 공격 시점

    // 추적할 대상이 존재하는지 알려주는 프로퍼티
    private bool hasTarget
    {
        get
        {
			// 추적할 대상이 존재하고, 대상이 사망하지 않았다면 true
			return targetEntity != null && !targetEntity.dead;
		}
    }

    private void Awake() {
		// 초기화
		pathFinder = GetComponent<NavMeshAgent>();
        enemyAnimator = GetComponent<Animator>();
		enemyAudioPlayer = GetComponent<AudioSource>();
        enemyRenderer = GetComponentInChildren<Renderer>();

	}

    // 적 AI의 초기 스펙을 결정하는 셋업 메서드
    public void Setup(float newHealth, float newDamage, float newSpeed, Color skinColor) {
        startingHealth = newHealth;
        health = newHealth;
        damage = newDamage;
        pathFinder.speed = newSpeed;
        enemyRenderer.material.color = skinColor;
    }

    private void Start() {
        // 게임 오브젝트 활성화와 동시에 AI의 추적 루틴 시작
        StartCoroutine(UpdatePath());
    }

    private void Update() {
        // 추적 대상의 존재 여부에 따라 다른 애니메이션을 재생
        enemyAnimator.SetBool("HasTarget", hasTarget);
    }

    // 주기적으로 추적할 대상의 위치를 찾아 경로를 갱신
    private IEnumerator UpdatePath() {
        // 살아있는 동안 무한 루프
        while (!dead)
        {
            if (hasTarget)
            {
				pathFinder.isStopped = false;
				pathFinder.SetDestination(targetEntity.transform.position); //목적지의 월드 포지션 셋
            }
            else
            {
                pathFinder.isStopped = true;
                //나를 중심으로 20f만큼의 반경 안에 있는 지정 레이어를 반환
                Collider[] collides =
                    Physics.OverlapSphere(transform.position, 20f, whatIsTarget);

                for (int i = 0; i < collides .Length; i++)
                {
                    var livingEntity = collides[i].GetComponent<LivingEntity>();
                    if (livingEntity != null && !livingEntity.dead)
                    {
                        targetEntity = livingEntity;
                        break;
					}
                }
			}

            // 0.25초 주기로 처리 반복
            yield return new WaitForSeconds(0.25f);
        }
    }

    // 데미지를 입었을때 실행할 처리
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal) {
        hitEffect.transform.position = hitPoint;
        hitEffect.transform.rotation = Quaternion.LookRotation(hitNormal);  //레이의 법선 방향으로 회전
        hitEffect.Play();
        enemyAudioPlayer.PlayOneShot(hitSound);
        
        // LivingEntity의 OnDamage()를 실행하여 데미지 적용
        base.OnDamage(damage, hitPoint, hitNormal);
    }

    // 사망 처리
    public override void Die() {
        // LivingEntity의 Die()를 실행하여 기본 사망 처리 실행
        base.Die();

        var colls = GetComponents<Collider>();
        foreach (var coll in colls)
        {
            coll.enabled = false;
        }

        pathFinder.isStopped = true;
        pathFinder.enabled = false;

        enemyAnimator.SetTrigger("Die");
        enemyAudioPlayer.PlayOneShot(deathSound);
    }

    private void OnTriggerStay(Collider other) {
		// 트리거 충돌한 상대방 게임 오브젝트가 추적 대상이라면 공격 실행
		if (lastAttackTime + timeBetAttack < Time.time && other.gameObject == targetEntity.gameObject && !targetEntity.dead)
        {
			lastAttackTime = Time.time;
            targetEntity.OnDamage(damage, Vector3.zero, Vector3.zero);
		}


		/*var palyer = other.GetComponent<IDamageable>();
        if (palyer != null)
        {
			if (lastAttackTime + timeBetAttack < Time.time)
			{
				lastAttackTime = Time.time;

				Vector3 crossPos = other.ClosestPoint(transform.position);
                Vector3 nomalVec = crossPos - transform.position;
                nomalVec.Normalize();
				palyer.OnDamage(damage, crossPos, nomalVec);

		palyer.OnDamage(damage, Vector3.zero, Vector3.zero);
			}
		}*/
	}
}