using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    //[Header("Configuração do Projétil")]
    public float speed;
    public float duration;
    public int damage;
    public int ownerId;

    private Vector2 direction;

    Rigidbody2D rb2D;

    void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Destruir projétil após "duracao" segundos
        Destroy(gameObject, duration);
    }

    void FixedUpdate()
    {
        // Movimento do projétil
        //transform.position += (Vector3)(direction * speed * Time.deltaTime);
        rb2D.velocity = direction.normalized * speed;
    }

    /// <summary>
    /// Define a direção/rotação do projétil.
    /// </summary>
    public void SetTarget(Vector2 direction)
    {
        this.direction = direction.normalized;

        // Calcula ângulo e aplica rotação no eixo Z
        //float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        //transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        // Se seu sprite estiver "apontando para a esquerda", ative esta correção:
        transform.rotation *= Quaternion.Euler(0, 0, 90);
    }

    /// <summary>
    /// Detecta colisão com inimigos ou outros objetos.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy")) // Unity usa tags no lugar de groups
        {
            // Chama um sistema de dano global (precisa ser implementado)
            var enemy = other.GetComponent<IDamageable>();
            if (enemy != null)
            {
                DamageManager.Instance.ApplyDamage(other.gameObject, damage, ownerId);
            }
        }
        Destroy(gameObject); // destrói o projétil ao colidir
    }
}
