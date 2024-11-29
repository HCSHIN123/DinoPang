using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectPlayer : MonoBehaviour
{
    public delegate void EndCallback();
    public EndCallback endCallback;

    private Animator anim;
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void PlayExplosionEffect()
    {
        anim.SetTrigger("NormalMatch");
    }

    public void SetActiveFalse()
    {
        // 현재 애니메이션이 끝났는지 확인하는 함수 끝나면 1이 됨
        while(anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {

        }
        endCallback();
    }
}
