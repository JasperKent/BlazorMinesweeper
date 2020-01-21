using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorMinesweeper.Client
{
    public enum Marker { None, Mine, Query}

    public class Cell
    {
        private static readonly string[] _markers = { " ", "X", "?" };

        public bool Mined { get; set; }
        public bool Shown { get; set; }
        public string CssClass
        {
            get
            {
                if (!Shown)
                    return "mine-hidden";
                else if (Mined)
                    return "mine-blown";
                else 
                    return "mine-shown";
            }
        }
        public int Neighbours { get; set; }
        public string Content
        {
            get
            {
                if (Shown)
                    return !Mined && Neighbours > 0 ? Neighbours.ToString() : "";
                else
                    return _markers[(int)Marker];
            }
        }
        public int Column { get; set; }
        public int Row { get; set; }
        public Marker Marker { get; set; }
    }

    public class SweeperManager
    {
        private readonly Cell[,] _field = new Cell[40, 20];

        public SweeperManager()
        {
            Reset();
        }

        public void Reset()
        {
            Random rand = new Random();

            for (int col = 0; col < 40; ++col)
            {
                for (int row = 0; row < 20; ++row)
                {
                    _field[col, row] = new Cell { Mined = rand.Next(100) > 80, Column = col, Row = row };
                }
            }

            for (int col = 0; col < 40; ++col)
            {
                for (int row = 0; row < 20; ++row)
                {
                    VisitNeighbours(col, row, cell =>
                    {
                        if (cell.Mined)
                            ++_field[col, row].Neighbours;
                    });
                }
            }
        }

        private void VisitNeighbours(int col, int row, Action<Cell> action)
        {
            for (int x = col - 1; x <= col + 1; ++x)
            {
                for (int y = row - 1; y <= row + 1; ++y)
                {
                    if (x > 0 && x < 40 && y > 0 && y < 20)
                        action(_field[x, y]);
                }
            }
        }

        public void Click(int col, int row)
        {
            _field[col, row].Shown = true;

            if (_field[col, row].Mined)
                BlowAll();
            else if (_field[col, row].Neighbours == 0)
                VisitNeighbours(col, row, cell =>
                  {
                      if (!cell.Shown)
                          Click(cell.Column, cell.Row);
                  });
        }

        public void MouseDown(MouseEventArgs e, int col, int row)
        {
            if (e.Button == 2) // right-click
                _field[col, row].Marker = (Marker)((((int)_field[col, row].Marker) + 1) % 3);
        }

        private void BlowAll()
        {
            foreach (var cell in _field)
            {
                if (cell.Mined)
                    cell.Shown = true;
            }
        }

        public string Class(int col, int row)
        {
            return _field[col, row].CssClass;
        }

        public string Content(int col, int row)
        {
            return _field[col, row].Content;
        }
    }
}
