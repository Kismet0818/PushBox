using System.Collections;
using UnityEngine;

namespace PushBox
{
    public class Box:MonoBehaviour
    {
        public Vector2Int Cell{get;private set;}
        public bool OnTarget{get;private set;}
        Board Board;
        Grid Grid;

        static readonly Color NormalColor=Color.white;
        static readonly Color TargetColor=Color.yellow;

        public void Init(Board owner, Grid ownerGrid,Vector2Int cell)
        {
            Board=owner;
            Grid=ownerGrid;
            PlaceAt(cell);
        }

        public void PlaceAt(Vector2Int cell)
        {
            if(Board!=null && Board.BoxAt(Cell.x,Cell.y)==this)
                Board.ClearBox(Cell.x,Cell.y);
            Cell=cell;

            if(Board!=null)
                Board.SetBox(cell.x,cell.y,this);
            transform.position=GridUtil.CellCenter(Grid,cell);
            Refresh();
        }

        public IEnumerator MoveRoutine(Vector2Int to,float duration)
        {
            Vector3 start=transform.position;
            Vector3 end=GridUtil.CellCenter(Grid,to);
            float t=0f;
            while(t<1f)
            {
                t+=Time.deltaTime/Mathf.Max(0.0001f,duration);
                transform.position=Vector3.Lerp(start,end,Mathf.Clamp01(t));
                yield return null;
            }
            PlaceAt(to);
        }

        public void Refresh()
        {
            if(Board!=null)
                OnTarget=Board.IsTarget(Cell.x,Cell.y);
                
            if(OnTarget)
                GetComponent<SpriteRenderer>().color=TargetColor;
            else
                GetComponent<SpriteRenderer>().color=NormalColor;
        }
    }
}