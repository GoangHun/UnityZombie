using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public GameObject player;
	private Transform target;
	private Vector3 localPos;

	private void Awake()
	{
		target = player.transform;
		localPos = transform.position - target.position;
	}
	private void Update()
	{
		transform.position = player.transform.position + localPos;
	}
}
