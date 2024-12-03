using UnityEngine;

public enum MovementDirection
{
    Right,
    Above,
    Left,
    Below
}

public class CharacterRaycaster2D : MonoBehaviour
{
    [Range(1, 10)]
    public int accuracy = 4;
    [Range(0, 0.1f)]
    public float skinWidth = 0.02f;
    public LayerMask collidableElements;
    public LayerMask collidableFromAboveOnly;
    public Transform self;
    public BoxCollider2D selfBox;

    // input : normalized coordinates from -1 to 1 (in box collider space)
    // output : world coordinates of that point
    // cette flèche "=>" représente le mot-clé "return", qu'on utilise ici pour overloader la fonction
    Vector2 GetPointPositionInBox(float x, float y) => GetPointPositionInBox(new Vector2(x, y));
    Vector2 GetPointPositionInBox(Vector2 position)
    {
        // calculer les coordonnées du centre du collider: on commence par la position de l'objet
        Vector2 result = self.position;

        // on applique l'offset du collider pour avoir les coordonnées du centre du box collider
        result.x += selfBox.offset.x * self.lossyScale.x;
        result.y += selfBox.offset.y * self.lossyScale.y;

        // incorporer les dimensions du collider
        result.x += position.x * selfBox.size.x * 0.5f * self.lossyScale.x;
        result.y += position.y * selfBox.size.y * 0.5f * self.lossyScale.y;

        return result;
    }

    Vector2 DirectionToVector(MovementDirection dir)
    {
        if (dir == MovementDirection.Right) return Vector2.right;
        if (dir == MovementDirection.Above) return Vector2.up;
        if (dir == MovementDirection.Left) return Vector2.left;
        if (dir == MovementDirection.Below) return Vector2.down;

        // default
        return Vector2.zero;
    }

    // fonction de détection généraliste pour toutes directions.
    public bool CalculateCollision(MovementDirection dir, float dist)
    {
        Vector2 direction = DirectionToVector(dir);
        LayerMask usedLayerMask = collidableElements;
        if (dir == MovementDirection.Below) usedLayerMask |= collidableFromAboveOnly;
        
        // bitwise operators:
        /**
        usedLayerMask = collidableElements & collidableFromAboveOnly;
        usedLayerMask = collidableElements | collidableFromAboveOnly;
        usedLayerMask = collidableElements ^ collidableFromAboveOnly;
        usedLayerMask = ~collidableElements;
        /**/

        // cas particulier : un unique ray au milieu du collider
        if (accuracy == 1)
        {
            Vector2 origin = GetPointPositionInBox(direction);
            origin += direction * skinWidth;
            RaycastHit2D hitResult = Physics2D.Raycast(origin, direction, dist, usedLayerMask);
            return hitResult.collider != null;
        }

        Vector2 cornerA = Vector2.zero;
        Vector2 cornerB = Vector2.zero;

        if (dir == MovementDirection.Below)
        {
            cornerA = GetPointPositionInBox(-1, -1);
            cornerB = GetPointPositionInBox(1, -1);
            cornerA.x += skinWidth;
            cornerB.x -= skinWidth;
        }
        if (dir == MovementDirection.Above)
        {
            cornerA = GetPointPositionInBox(-1, 1);
            cornerB = GetPointPositionInBox(1, 1);
            cornerA.x += skinWidth;
            cornerB.x -= skinWidth;
        }
        if (dir == MovementDirection.Left)
        {
            cornerA = GetPointPositionInBox(-1, -1);
            cornerB = GetPointPositionInBox(-1, 1);
            cornerA.y += skinWidth;
            cornerB.y -= skinWidth;
        }
        if (dir == MovementDirection.Right)
        {
            cornerA = GetPointPositionInBox(1, -1);
            cornerB = GetPointPositionInBox(1, 1);
            cornerA.y += skinWidth;
            cornerB.y -= skinWidth;
        }

        // dans tous les cas autres cas : on obtient l'origine des rays par interpolation entre deux coins
        for (int i = 0; i < accuracy; i++)
        {
            float ratio = ((float)i) / (float)(accuracy-1);
            Vector2 origin = Vector2.Lerp(cornerA, cornerB, ratio);
            origin += direction * skinWidth;

            // on exécute un raycast
            RaycastHit2D hitResult = Physics2D.Raycast(origin, direction, dist, usedLayerMask);
            Debug.DrawRay(origin, direction, Color.blue);

            // examiner le résultat : si un collider a été touché, on renvoie true (il y a collision)
            if (hitResult.collider != null) return true;
        }

        return false;
    }
}
