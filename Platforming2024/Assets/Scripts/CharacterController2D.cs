using UnityEngine;
using UnityEngine.Events;

[System.Flags]
public enum CollisionFlags2D
{
    Right, // 1 = 1 << 0
    Above, // 2 = 1 << 1
    Left, // 4 = 1 << 2 
    Below // 8 = 1 << 3
}

public class CharacterController2D : MonoBehaviour
{
    [Header("Component References")]
    public CharacterProfile characterProfile;
    public Transform self;
    public CharacterRaycaster2D raycaster;
    public Animator animator;
    public Transform graphicTransform;

    [Header("Events")]
    public UnityEvent onFell;
    public UnityEvent onGrounded, onHurt;
    public UnityEvent<int> onJumped;
    public UnityEvent<CollisionFlags2D> onCollisionStay;

    [System.NonSerialized] public bool isGrounded;
    [System.NonSerialized] public bool isJumping;
    [System.NonSerialized] public bool isUnderCoyoteTime;
    [System.NonSerialized] public float jumpTimestamp;
    [System.NonSerialized] public float coyoteTimestamp;
    [System.NonSerialized] public int remainingJumps;
    [System.NonSerialized] public CollisionFlags2D collisionFlags;

    void Start()
    {
        animator.SetFloat("WalkSpeed", characterProfile.moveSpeed);
    }

    void Update()
    {
        MovementUpdate();
    }

    void MovementUpdate()
    {
        Vector2 movement = Vector2.zero;

        // check inputs
        movement.x = Input.GetAxis("Horizontal") * characterProfile.moveSpeed * Time.deltaTime;
        
        // adapt graphics
        animator.SetBool("IsMovingHorizontally", movement.x != 0);
        if (movement.x < 0)
        {
            graphicTransform.localScale = new Vector3(
                -Mathf.Abs(graphicTransform.localScale.x),
                graphicTransform.localScale.y,
                graphicTransform.localScale.z
            );
        }
        // deux ifs distincts, et non un if/else, de manière à écarter le cas où x==0
        if (movement.x > 0)
        {
            graphicTransform.localScale = new Vector3(
                Mathf.Abs(graphicTransform.localScale.x),
                graphicTransform.localScale.y,
                graphicTransform.localScale.z
            );
        }

        // check jump
        if (Input.GetKeyDown(KeyCode.Z)) TryJump();        
        float jumpMultiplier = 1;
        if (isJumping)
        {
            float timeSinceJumped = Time.time - jumpTimestamp;

            // option 1 : lire la courbe pour mettre à jour le jumpMultiplier
            //jumpMultiplier = gravityMultiplierCurve.Evaluate(timeSinceJumped);

            // option 2 : appliquer le delta de position.y d'une frame à l'autre, en tant que mouvement vertical
            float yPositionCurrentFrame = characterProfile.gravityMultiplierCurve.Evaluate(timeSinceJumped);
            float yPositionPreviousFrame = characterProfile.gravityMultiplierCurve.Evaluate(timeSinceJumped - Time.deltaTime);
            movement.y = yPositionCurrentFrame - yPositionPreviousFrame;

            // si je découvre que je suis arrivé au bout de la courbe, on arrête le saut
            float xMax = characterProfile.gravityMultiplierCurve.keys[characterProfile.gravityMultiplierCurve.keys.Length-1].time;
            if (timeSinceJumped > xMax)
            {
                isJumping = false;
            }
        }

        // check/apply gravity
        // le "else" n'est à utiliser que pour l'option 2
        else movement.y = characterProfile.gravity * jumpMultiplier * -1 * Time.deltaTime;

        // coyote time (if applicable) : right after falling, allow one jump as if still on the ground
        if (isUnderCoyoteTime)
        {
            float timeSinceFell = Time.time - coyoteTimestamp;
            if (timeSinceFell > characterProfile.maxCoyoteTime)
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
        int jumpIndex = characterProfile.maxAllowedJumps - (remainingJumps+1);
        onJumped?.Invoke(jumpIndex);
    }

    void Move(Vector2 movement)
    {
        bool collH = HorizontalMovement(movement.x);
        bool collV = VerticalMovement(movement.y);
        if (collH || collV) onCollisionStay?.Invoke(collisionFlags);
    }

    bool HorizontalMovement(float movement)
    {
        // détecter la collision éventuelle
        bool isThereCollision = raycaster.CalculateCollision(
            movement > 0 ? MovementDirection.Right : MovementDirection.Left,
            Mathf.Abs(movement)
        );

        // si collision, annule le mouvement
        if (isThereCollision)
        {
            if (movement > 0) collisionFlags |= CollisionFlags2D.Right; // ajouter le marqueur "droite"
            else collisionFlags |= CollisionFlags2D.Left;
            return true;
        }
        
        // else :
        if (movement > 0) collisionFlags &= ~CollisionFlags2D.Right; // enlever le marqueur "droite"
        else collisionFlags &= ~CollisionFlags2D.Left;
        self.Translate(Vector3.right * movement);
        return false;
    }

    bool VerticalMovement(float movement)
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
                animator.SetBool("IsGrounded", true);
                isUnderCoyoteTime = false;
                remainingJumps = characterProfile.maxAllowedJumps;
                collisionFlags |= CollisionFlags2D.Below; // ajouter le marqueur "below"
                onGrounded?.Invoke();
            }
            collisionFlags |= CollisionFlags2D.Above;
            return true;
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
                onFell?.Invoke();
            }
            
            collisionFlags &= ~CollisionFlags2D.Below;
            isGrounded = false;
            animator.SetBool("IsGrounded", false);
        }

        collisionFlags &= ~CollisionFlags2D.Above; // enlever le marqueur "Above"
        
        // enfin, exécuter le mouvement
        self.Translate(Vector3.up * movement);

        return false;
    }
}
