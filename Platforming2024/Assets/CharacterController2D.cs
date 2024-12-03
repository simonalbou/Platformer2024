using Unity.Collections;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    public float moveSpeed;
    public float gravity;
    public AnimationCurve gravityMultiplierCurve;

    public Transform self;
    public CharacterRaycaster2D raycaster;

    [System.NonSerialized] public bool isGrounded;
    [System.NonSerialized] public bool isJumping;
    [System.NonSerialized] public float jumpTimestamp;


    void Update()
    {
        MovementUpdate();
    }

    void MovementUpdate()
    {
        Vector2 movement = Vector2.zero;

        // check inputs
        movement.x = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;

        // check jump
        if (Input.GetKeyDown(KeyCode.Z)) TryJump();        
        float jumpMultiplier = 1;
        if (isJumping)
        {
            // lire la courbe pour mettre à jour le jumpMultiplier
            float timeSinceJumped = Time.time - jumpTimestamp;
            jumpMultiplier = gravityMultiplierCurve.Evaluate(timeSinceJumped);

            // si je découvre que je suis arrivé au bout de la courbe, on arrête le saut
            float xMax = gravityMultiplierCurve.keys[gravityMultiplierCurve.keys.Length-1].time;
            if (timeSinceJumped > xMax)
            {
                isJumping = false;
            }
        }

        // check gravity
        movement.y = gravity * jumpMultiplier * -1 * Time.deltaTime;

        // move accordingly
        Move(movement);
    }

    void TryJump()
    {
        // à moins d'un double-saut autorisé ou d'un coyote time actif, le saut doit échouer si on ne touche pas le sol
        if (!isGrounded) return;

        // le saut est autorisé : on initialise tout
        isGrounded = false;
        isJumping = true;
        jumpTimestamp = Time.time;

        // feedbacks divers ici : audio, VFX, etc etc
        // et évidemment, l'animation de saut
    }

    void Move(Vector2 movement)
    {
        HorizontalMovement(movement.x);
        VerticalMovement(movement.y);
    }

    void HorizontalMovement(float movement)
    {
        // détecter la collision éventuelle
        bool isThereCollision = raycaster.CalculateCollision(
            movement > 0 ? MovementDirection.Right : MovementDirection.Left,
            Mathf.Abs(movement)
        );

        // si collision, annule le mouvement
        if (isThereCollision) return;
        
        // else :
        self.Translate(Vector3.right * movement);
    }

    void VerticalMovement(float movement)
    {
        // détecter la collision éventuelle

        // structure ternaire : moins de lourdeur qu'un "if" pour simplement sélectionner entre deux valeurs
            // exemple :
            // float x = 4;
            // Debug.Log((x > 3) ? "hello" : "world");

        bool isThereCollision = raycaster.CalculateCollision(
            movement > 0 ? MovementDirection.Above : MovementDirection.Below,
            Mathf.Abs(movement)
        );
        
        // si collision, annule le mouvement et détecte la collision
        if (isThereCollision)
        {
            // unity event ou autre message visant à feedbacker la collision
            if (movement < 0) isGrounded = true;
            return;
        }
        
        // else : il n'y a pas collision

        // j'ai réussi à bouger vers le bas : c'est que je tombe, donc que je ne touche pas (plus) le sol
        if (movement < 0) isGrounded = false;
        
        // enfin, exécuter le mouvement
        self.Translate(Vector3.up * movement);
    }
}
