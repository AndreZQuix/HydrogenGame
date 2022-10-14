using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

public class Scope : MonoBehaviour
{
    [SerializeField] private GameObject weaponCamera;
    [SerializeField] private Animator animator;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float zoomFOV;
    [SerializeField] private float scopeOverlayDelay = 0.15f;
    [SerializeField] private UnityEvent onScope;
    [SerializeField] private UnityEvent onUnscope;

    private PhotonView PV;
    private bool isScoped;
    private float normalFOV;

    private static readonly int ScopeKey = Animator.StringToHash("IsScoped");

    private void OnDisable()
    {
        isScoped = false;
        OnUnScoped();
    }

    private void Start()
    {
        PV = transform.root.GetComponent<PhotonView>();
        if (PV.IsMine) 
            normalFOV = mainCamera.fieldOfView;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isScoped = !isScoped;
            
            if(isScoped)
                StartCoroutine(OnScoped());
            else
                OnUnScoped();
        }
    }

    private IEnumerator OnScoped()
    {
        yield return new WaitForSeconds(scopeOverlayDelay);
        if (PV.IsMine)
        {
            mainCamera.fieldOfView = zoomFOV;
            weaponCamera.SetActive(false);
            animator.SetBool(ScopeKey, isScoped);
        }
        onScope.Invoke();
    }

    private void OnUnScoped()
    {
        if (PV.IsMine)
        {
            mainCamera.fieldOfView = normalFOV;
            weaponCamera.SetActive(true);
            animator.SetBool(ScopeKey, isScoped);
        }
        onUnscope.Invoke();
    }
}
