using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ZivsKustiba : MonoBehaviour
{
    void Awake()
    {
        // Pārliecināties, ka Rigidbody2D ir uzstādīts PIRMS Start()
        // Izmanto Awake(), lai garantētu, ka tas notiek pirmais
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
}
