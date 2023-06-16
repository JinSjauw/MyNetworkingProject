using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagDollObject : MonoBehaviour
{
    [SerializeField] private Transform ragdollRootBone;

    public void Init(Transform _originalRootBone)
    {
        MathAllChildBones(_originalRootBone, ragdollRootBone);
        ApplyExplosion(ragdollRootBone, 300f, transform.position,10f);
    }

    private void MathAllChildBones(Transform _root, Transform _clone)
    {
        foreach (Transform child in _root)
        {
            Transform cloneChild = _clone.Find(child.name);
            if(cloneChild != null)
            {
                cloneChild.position = child.position;
                cloneChild.rotation = child.rotation;
                
                MathAllChildBones(child, cloneChild);
            }
        }
    }

    private void ApplyExplosion(Transform _root, float explosionForce, Vector3 explosionPosition, float explosionRange)
    {
        foreach (Transform child in _root)
        {
            if (child.TryGetComponent<Rigidbody>(out Rigidbody childRigidbody))
            {
                childRigidbody.AddExplosionForce(explosionForce, explosionPosition, explosionRange);
            }
            
            ApplyExplosion(child, explosionForce, explosionPosition, explosionRange);
        }
    }
}
