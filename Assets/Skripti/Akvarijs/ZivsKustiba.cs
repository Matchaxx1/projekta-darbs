using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]// jamaina abi no siem
public class ZivsKustiba : MonoBehaviour
{
    void Start()
    {
        // Pārliecināties, ka Rigidbody2D ir uzstādīts
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }
}
