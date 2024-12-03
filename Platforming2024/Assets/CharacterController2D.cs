using Unity.Collections;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    public float moveSpeed;
    public float gravity;

    public Transform self;
    public CharacterRaycaster2D raycaster;

    void Update()
    {
        MovementUpdate();
    }

    void MovementUpdate()
    {
        Vector2 movement = Vector2.zero;

        // check inputs
        movement.x = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        // check gravity
        movement.y = gravity * -1 * Time.deltaTime;

        // move accordingly
        Move(movement);
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
            return;
        }
        
        // else :
        self.Translate(Vector3.up * movement);
    }
}
