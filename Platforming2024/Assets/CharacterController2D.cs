using Unity.Collections;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    public float moveSpeed;
    public float gravity;
    public int maxAllowedJumps;
    public float maxCoyoteTime = 0.3f;
    public AnimationCurve gravityMultiplierCurve;

    public Transform self;
    public CharacterRaycaster2D raycaster;

    [System.NonSerialized] public bool isGrounded;
    [System.NonSerialized] public bool isJumping;
    [System.NonSerialized] public bool isUnderCoyoteTime;
    [System.NonSerialized] public float jumpTimestamp;
    [System.NonSerialized] public float coyoteTimestamp;
    [System.NonSerialized] public int remainingJumps;


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
            float timeSinceJumped = Time.time - jumpTimestamp;

            // option 1 : lire la courbe pour mettre à jour le jumpMultiplier
            //jumpMultiplier = gravityMultiplierCurve.Evaluate(timeSinceJumped);

            // option 2 : appliquer le delta de position.y d'une frame à l'autre, en tant que mouvement vertical
            float yPositionCurrentFrame = gravityMultiplierCurve.Evaluate(timeSinceJumped);
            float yPositionPreviousFrame = gravityMultiplierCurve.Evaluate(timeSinceJumped - Time.deltaTime);
            movement.y = yPositionCurrentFrame - yPositionPreviousFrame;

            // si je découvre que je suis arrivé au bout de la courbe, on arrête le saut
            float xMax = gravityMultiplierCurve.keys[gravityMultiplierCurve.keys.Length-1].time;
            if (timeSinceJumped > xMax)
            {
                isJumping = false;
            }
        }

        // check/apply gravity
        // le "else" n'est à utiliser que pour l'option 2
        else movement.y = gravity * jumpMultiplier * -1 * Time.deltaTime;

        // coyote time (if applicable) : right after falling, allow one jump as if still on the ground
        if (isUnderCoyoteTime)
        {
            float timeSinceFell = Time.time - coyoteTimestamp;
            if (timeSinceFell > maxCoyoteTime)
            {
                remainingJumps--;
                isUnderCoyoteTime = false;
            }
        }

        // move accordingly
        Move(movement);
    }

    void TryJump()
    {
        // à moins d'un double-saut autorisé ou d'un coyote time actif, le saut doit échouer si on ne touche pas le sol
        // ligne commentée : on a enfin implémenté le double-saut, donc plus besoin de ce check
        //if (!isGrounded) return;

        // vérifier le nombre de sauts autorisés
        if (remainingJumps < 1) return;

        // le saut est autorisé : on initialise tout
        isUnderCoyoteTime = false;
        isGrounded = false;
        isJumping = true;
        jumpTimestamp = Time.time;
        remainingJumps--;

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
            if (movement < 0)
            {
                isGrounded = true;
                isUnderCoyoteTime = false;
                remainingJumps = maxAllowedJumps;
            }
            return;
        }
        
        // else : il n'y a pas collision

        // j'ai réussi à bouger vers le bas : c'est que je tombe, donc que je ne touche pas (plus) le sol
        if (movement < 0)
        {
            // si j'étais précédemment au sol : c'est que je chute en tombat d'une plateforme, sans sauter
            if (isGrounded)
            {
                isUnderCoyoteTime = true;
                coyoteTimestamp = Time.time;
            }

            isGrounded = false;
        }
        
        // enfin, exécuter le mouvement
        self.Translate(Vector3.up * movement);
    }
}
