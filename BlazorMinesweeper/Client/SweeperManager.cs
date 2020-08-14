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
        private string[] _markers = { "mine-hidden", "mine-marked", "mine-query" };

        public bool Mined { get; set; }
        public bool Shown { get; set; }
        public string CssClass
        {
            get
            {
                if (!Shown)
                    return _markers[(int)Marker];
                else if (Mined)
                    return "mine-blown";
                else 
                    return $"val-{Neighbours} mine-shown";
            }
        }
        public int Neighbours { get; set; }
        public int Column { get; set; }
        public int Row { get; set; }
        public Marker Marker { get; set; }
    }

    public class SweeperManager
    {
        private readonly Cell[,] _field = new Cell[40, 20];

        public SweeperManager()
        {
            Console.WriteLine("Value is: " + Environment.SystemDirectory); 

            Reset();
        }

        public void Reset()
        {
            Random rand = new Random();

            for (int col = 0; col < 40; ++col)
            {
                for (int row = 0; row < 20; ++row)
                {
                    _field[col, row] = new Cell { Mined = rand.Next(100) > 85, Column = col, Row = row };
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
                    if (x >= 0 && x < 40 && y >= 0 && y < 20 && !(x == col && y == row))
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

        public void DoubleClick(int col, int row)
        {
            var cell = _field[col, row];

            Console.WriteLine($"Dbl Clicked ({col},{row}).");

            if (cell.Shown)
            {
                int exposedNeighbours = 0;
                
                VisitNeighbours(col, row, neighbour =>
                {
                    if (neighbour.Marker == Marker.Mine && neighbour.Mined)
                        ++exposedNeighbours;
                });

                Console.WriteLine($"Exposed neightbours: {exposedNeighbours}.");
                Console.WriteLine($"Neightbours: {cell.Neighbours}.");

                if (exposedNeighbours == cell.Neighbours)
                    VisitNeighbours(col, row, neighbour =>
                    {
                        if (neighbour.Marker != Marker.Mine)
                        {
                            Console.WriteLine($"Clicking ({neighbour.Column},{neighbour.Row}).");
                            Click(neighbour.Column, neighbour.Row);
                        }
                    });
            }
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
    }
}
