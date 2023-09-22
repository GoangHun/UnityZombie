﻿using System.Collections;
using UnityEngine;

// 총을 구현한다
public class Gun : MonoBehaviour {
    // 총의 상태를 표현하는데 사용할 타입을 선언한다
    public enum State {
        Ready, // 발사 준비됨
        Empty, // 탄창이 빔
        Reloading // 재장전 중
    }

    public State state { get; private set; } // 현재 총의 상태

    public Transform fireTransform; // 총알이 발사될 위치

    public ParticleSystem muzzleFlashEffect; // 총구 화염 효과
    public ParticleSystem shellEjectEffect; // 탄피 배출 효과

    private LineRenderer bulletLineRenderer; // 총알 궤적을 그리기 위한 렌더러

    private AudioSource gunAudioPlayer; // 총 소리 재생기
    public AudioClip shotClip; // 발사 소리
    public AudioClip reloadClip; // 재장전 소리

    public float damage = 25; // 공격력
    private float fireDistance = 50f; // 사정거리

    public int ammoRemain = 100; // 남은 전체 탄약
    public int magCapacity = 25; // 탄창 용량
    public int magAmmo; // 현재 탄창에 남아있는 탄약


    public float timeBetFire = 0.12f; // 총알 발사 간격
    public float reloadTime = 1.8f; // 재장전 소요 시간
    private float lastFireTime; // 총을 마지막으로 발사한 시점

    public event System.Action onCompleteReload;
    private void Awake() {
        // 사용할 컴포넌트들의 참조를 가져오기
        gunAudioPlayer = GetComponent<AudioSource>();
        bulletLineRenderer = GetComponent<LineRenderer>();

        bulletLineRenderer.positionCount = 2;   //직선만 그리기
        bulletLineRenderer.enabled = false;     //on/off
    }

    private void OnEnable() {
        // 총 상태 초기화
        state = State.Ready;
        magAmmo = magCapacity;
        lastFireTime = 0f;
    }


	// 발사 시도
	public void Fire() {
        if (state == State.Ready && Time.time > lastFireTime + timeBetFire)
        {
			lastFireTime = Time.time;
			Shot();
        }

    }

    // 실제 발사 처리
    private void Shot() {
        var hitPosition = fireTransform.position + fireTransform.forward * fireDistance;
		var ray = new Ray(fireTransform.position, fireTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, fireDistance))
        {
            var target = hit.collider.GetComponent<IDamageable>();   //IDamageable을 상속 받은 클래스를 반환함
            if (target != null)
            {
                target.OnDamage(damage, hit.point, hit.normal);
            }
            hitPosition = hit.point;
		}
        StartCoroutine(ShotEffect(hitPosition));

        --magAmmo;
        if (magAmmo <= 0)
        {
            state = State.Empty;
        }
    }

    // 발사 이펙트와 소리를 재생하고 총알 궤적을 그린다
    private IEnumerator ShotEffect(Vector3 hitPosition) {   //코루틴을 사용해서 동작

        muzzleFlashEffect.Play();
        shellEjectEffect.Play();
        gunAudioPlayer.PlayOneShot(shotClip);

		// 라인 렌더러를 활성화하여 총알 궤적을 그린다

		bulletLineRenderer.SetPosition(0, fireTransform.position);
		bulletLineRenderer.SetPosition(1, hitPosition);
		bulletLineRenderer.enabled = true;
        

        // 0.03초 동안 잠시 처리를 대기
        yield return new WaitForSeconds(0.03f);

        // 라인 렌더러를 비활성화하여 총알 궤적을 지운다
        bulletLineRenderer.enabled = false;
    }

    // 재장전 시도
    public bool Reload() {

        if (state == State.Reloading || ammoRemain <= 0 || magAmmo >= magCapacity)
			return false;

        StartCoroutine(ReloadRoutine());
        return true;
    }

    // 실제 재장전 처리를 진행
    private IEnumerator ReloadRoutine() {
        // 현재 상태를 재장전 중 상태로 전환
        state = State.Reloading;
        gunAudioPlayer.PlayOneShot(reloadClip);
        
        // 재장전 소요 시간 만큼 처리를 쉬기
        yield return new WaitForSeconds(reloadTime);

        var ammoToFill = magCapacity - magAmmo;
        if (ammoRemain < ammoToFill)
            ammoToFill = ammoRemain;
        ammoRemain -= ammoToFill;
        magAmmo += ammoToFill;

        if (onCompleteReload != null)
        {
            onCompleteReload();
        }

        // 총의 현재 상태를 발사 준비된 상태로 변경
        state = State.Ready;
    }
}