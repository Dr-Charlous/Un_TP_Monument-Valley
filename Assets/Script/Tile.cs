using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Tile : MonoBehaviour
{
    public Vector3 TopOffset;
    public Tile PreviousTile;
    public Tile[] Neighbours;
    List<Tile> tilesDone = new List<Tile>();

    private void OnMouseDown()
    {
        transform.DOLocalMoveY(transform.localPosition.y - 0.1f, 0.1f).SetLoops(2, LoopType.Yoyo);
        tilesDone.Clear();

        if (FindTile(GameCore.Instance.CurrentTile, this))
        {
            GameCore.Instance.Animator.SetBool("walking", true);

            List<Tile> path = GetPath(this);

            Sequence sequence = DOTween.Sequence();

            Vector3 from = GameCore.Instance.CurrentTile.transform.position + GameCore.Instance.CurrentTile.TopOffset;

            for (int i = 0; i < path.Count; i++)
            {
                Tile tile = path[i];

                Vector3 destination = tile.transform.position + tile.TopOffset;
                sequence.Append(GameCore.Instance.Player.transform.DOMove(destination, 1.0f).SetEase(Ease.Linear));

                if (i > 0)
                {
                    from = path[i - 1].transform.position + path[i - 1].TopOffset;
                }

                Vector3 direction = destination - from;
                direction.Normalize();

                sequence.Join(GameCore.Instance.Player.transform.DORotateQuaternion(Quaternion.LookRotation(-direction, Vector3.up), 0.3f));
            }

            sequence.AppendCallback(MoveEnd);

            foreach (var tile in path)
            {
                tile.PreviousTile = null;
            }
        }
    }

    void MoveEnd()
    {
        GameCore.Instance.Animator.SetBool("walking", false);
        GameCore.Instance.CurrentTile = this;
    }

    bool FindTile(Tile startTile, Tile endTile)
    {
        if (startTile == endTile)
            return true;

        tilesDone.Add(startTile);

        foreach (var item in startTile.Neighbours)
        {
            if (tilesDone.Contains(item) == false && FindTile(item, endTile))
            {
                item.PreviousTile = startTile;

                return true;
            }
        }

        return false;
    }

    List<Tile> GetPath(Tile lastTile)
    {
        List<Tile> path = new List<Tile>();

        Tile current = lastTile;
        while (current != null)
        {
            path.Insert(0, current);
            current = current.PreviousTile;
        }

        path.RemoveAt(0);

        return path;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position + TopOffset, 0.2f);
    }

    private void OnDrawGizmosSelected()
    {
        foreach (var item in Neighbours)
        {
            Debug.DrawLine(transform.position + TopOffset, item.transform.position + item.TopOffset, Color.red);
        }
    }
}
