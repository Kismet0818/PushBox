using System.Collections;
using UnityEngine;

namespace PushBox
{
    public class Box:MonoBehaviour
    {
        public Vector2Int Cell{get;private set;}
        public bool OnTarget{get;private set;}
        Board Board;

        static readonly Color NormalColor=Color.white;
        static readonly Color TargetColor=Color.yellow;

        public void Init(Board owner,Vector2Int cell)
        {
            Board=owner;
            PlaceAt(cell);
        }

        public void PlaceAt(Vector2Int cell)
        {
            if(Board!=null && Board.BoxAt(cell.x,cell.y)==this)
                Board.ClearBox(cell.x,cell.y);
            Cell=cell;

            if(Board!=null)
                Board.SetBox(cell.x,cell.y,this);
            transform.localPosition=new Vector3(cell.x,cell.y,0);
            Refresh();
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