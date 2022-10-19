using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class PlayerController2 : MonoBehaviour
{
    [Header("Joystick")] [SerializeField] private Joystick joystick;
    private float horizontal;
    private float vertical;
    
    [Header("Movement")] [SerializeField] float speed = 10;
    private Vector3 direction;
    float currentTurnAngle;
    [SerializeField] float smoothTurnTime = 0.1f;
    Rigidbody rigidBody;
    
    [Header("Detect Collect Drop")]
    Collider[] colliders;
    [SerializeField] Transform detectTransform;
    [SerializeField] Transform dropTransform;

    [SerializeField] float detectionRange = 1;
    [SerializeField] LayerMask layer;
    [SerializeField] Transform holdTransform;
    [SerializeField] int itemCount = 0;
    [SerializeField] float endScale = 0.3f;

    [SerializeField] private int storageItems = 0;

    [SerializeField] private float distanceBetween = 1.25f;
    
    [Header("Bag")] [SerializeField] private int bagCapacity=6;
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(detectTransform.position, detectionRange);
    }
    
    void Update()
    {
        colliders = Physics.OverlapSphere(detectTransform.position, detectionRange, layer);
        foreach (var hit in colliders)
        {

            if (hit.CompareTag("Collectable") && itemCount <= bagCapacity)
            {
                hit.tag = "Collected";
                hit.transform.parent = holdTransform;

             
                var seq = DOTween.Sequence();
                seq.Append(hit.transform.DOLocalJump(new Vector3(0, itemCount * endScale), 2, 1, 0.3f))
                    .Join( hit.transform.DOScale(1.25f, 0.1f))
                    .Insert(0.1f, hit.transform.DOScale(endScale, 0.2f));
                seq.AppendCallback(() =>
                {
                    hit.transform.localRotation = Quaternion.Euler(0, 0, 0);
                });
                
                itemCount++;
                //speed--;
    
            }
        }
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Storage") && itemCount > 0)
        {
            GameObject[] collectedObjects = GameObject.FindGameObjectsWithTag("Collected");

            foreach (GameObject item in collectedObjects)
            {
                item.transform.parent = dropTransform;

                item.tag = "Finish";
                
                storageItems++;
                var sequence = DOTween.Sequence();
                sequence.Append(item.transform.DOLocalJump(new Vector3(0, storageItems * distanceBetween), 2, 1, 0.3f));
                Debug.Log("Storage item" + storageItems);
                //speed++;
            }
            itemCount = 0;
        }
    }

    private void FixedUpdate()
    {
        horizontal = joystick.Horizontal;
        vertical = joystick.Vertical;
        
        direction = new Vector3(horizontal, 0, vertical);

        if (direction.magnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentTurnAngle,
                smoothTurnTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);

            rigidBody.MovePosition(transform.position + (direction * speed * Time.deltaTime));
        }
    }
}
