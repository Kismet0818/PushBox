using UnityEngine;

namespace PushBox
{
    public class MoveRecord
    {
        public Vector2Int PlayerFrom;
        public Vector2Int PlayerTo;

        public Vector2Int BoxFrom;
        public Vector2Int BoxTo;
    }

    public class Board
    {
        public int MinX {get;}
        public int MinY {get;}
        public int Width {get;}
        public int Height {get;}

        readonly bool[,] Walls;
        readonly bool[,] Targets;

        public Board(int minx,int miny,int width,int height)
        {
            MinX=minx;
            MinY=miny;
            Width=width;
            Height=height;
            Walls=new bool[Width,Height];
            Targets=new bool[Width,Height];
        }

        public bool InBounds(int x,int y)   //判断是否在边界内
        {
            return x>=MinX && x<MinX+Width && y>=MinY && y<MinY+Height;
        }

        public bool IsWall(int x,int y)     //判断是否为墙(墙外区域均为墙)
        {
            if(!InBounds(x,y))
                return true;
            else
                return Walls[x-MinX,y-MinY];
        }

        public bool IsTarget(int x,int y)   //判断是否为目标点
        {
            if(!InBounds(x,y))
                return false;
            else
                return Targets[x-MinX,y-MinY];
        }

        public void SetWall(int x,int y)    //设置墙
        {
            if(InBounds(x,y))
                Walls[x-MinX,y-MinY]=true;
        }

        public void SetTarget(int x,int y)  //设置目标点
        {
            if(InBounds(x,y))
                Targets[x-MinX,y-MinY]=true;
        }

        public Rect WorldRect()     //世界棋盘
        {
            return new Rect(MinX,MinY,Width,Height);
        }
    }

    
}