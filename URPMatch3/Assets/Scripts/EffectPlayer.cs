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
        // ���� �ִϸ��̼��� �������� Ȯ���ϴ� �Լ� ������ 1�� ��
        while(anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {

        }
        endCallback();
    }
}
