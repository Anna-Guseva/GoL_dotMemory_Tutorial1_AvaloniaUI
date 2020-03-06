using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace GameOfLife
{
    internal class GameGrid : Control
    {
        private readonly Random _rnd;
        private int _sizeX;
        private int _sizeY;
        private Cell[,] _cells;
        private Cell[,] _nextGenerationCells;
        private Geometry[] _cellsVisuals;

        static GameGrid()
        {
            ClipToBoundsProperty.OverrideDefaultValue<GameGrid>(true);
        }
        
        public GameGrid()
        {
            _rnd = new Random();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _sizeX = (int)(finalSize.Width / 5);
            _sizeY = (int)(finalSize.Height / 5);
            _cells = new Cell[_sizeX, _sizeY];
            _nextGenerationCells = new Cell[_sizeX, _sizeY];
            _cellsVisuals = new Geometry[_sizeX * _sizeY];

            for (var i = 0; i < _sizeX; i++)
                for (var j = 0; j < _sizeY; j++)
                {
                    _cells[i, j] = new Cell(i, j, 0, false);
                    _nextGenerationCells[i, j] = new Cell(i, j, 0, false);
                }

            SetRandomPattern();
            InitCellsVisuals();
            
            return base.ArrangeOverride(finalSize);
        }

        public override void Render(DrawingContext context)
        {
            var pen = new Pen();
            for (var i = 0; i < _sizeX; i++)
                for (var j = 0; j < _sizeY; j++)
                {
                    var fill = _cells[i, j].IsAlive
                        ? _cells[i, j].Age < 2 ? Brushes.White : Brushes.DarkGray
                        : Brushes.Gray;

                    context.DrawGeometry(fill, pen, _cellsVisuals[j * _sizeX + i]);
                }
        }


        public void Clear()
        {
            for (var i = 0; i < _sizeX; i++)
                for (var j = 0; j < _sizeY; j++)
                {
                    _cells[i, j] = new Cell(i, j, 0, false);
                    _nextGenerationCells[i, j] = new Cell(i, j, 0, false);
                }
            
            InvalidateVisual();
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            var point = e.GetCurrentPoint(this);
            var position = point.Position;
            var i = (int)position.X / 5;
            var j = (int)position.Y / 5;
            if (i < 0 || i >= _sizeX || j < 0 || j >= _sizeY)
                return;

            if (!point.Properties.IsLeftButtonPressed) 
                return;
            
            if (_cells[i, j].IsAlive) 
                return;
            
            _cells[i, j].IsAlive = true;
            _cells[i, j].Age = 0;
            
            InvalidateVisual();
        }

        private void InitCellsVisuals()
        {
            for (var i = 0; i < _sizeX; i++)
                for (var j = 0; j < _sizeY; j++)
                {
                    double left = _cells[i, j].PositionX;
                    double top = _cells[i, j].PositionY;
                    _cellsVisuals[j * _sizeX + i] = new EllipseGeometry(new Rect(left, top, 5, 5));
                }
        }

        private bool GetRandomBoolean()
        {
            return _rnd.NextDouble() > 0.8;
        }

        private void SetRandomPattern()
        {
            for (var i = 0; i < _sizeX; i++)
                for (var j = 0; j < _sizeY; j++)
                    _cells[i, j].IsAlive = GetRandomBoolean();
        }

        private void UpdateToNextGeneration()
        {
            for (var i = 0; i < _sizeX; i++)
                for (var j = 0; j < _sizeY; j++)
                {
                    _cells[i, j].IsAlive = _nextGenerationCells[i, j].IsAlive;
                    _cells[i, j].Age = _nextGenerationCells[i, j].Age;
                }
        }


        public void Update()
        {
            for (var i = 0; i < _sizeX; i++)
            {
                for (var j = 0; j < _sizeY; j++)
                {
                    CalculateNextGeneration(i, j, out var alive, out var age);
                    _nextGenerationCells[i, j].IsAlive = alive;
                    _nextGenerationCells[i, j].Age = age;
                }
            }

            UpdateToNextGeneration();
            InvalidateVisual();
        }

        private void CalculateNextGeneration(int row, int column, out bool isAlive, out int age)
        {
            isAlive = _cells[row, column].IsAlive;
            age = _cells[row, column].Age;

            var count = CountNeighbors(row, column);

            if (isAlive && count < 2)
            {
                isAlive = false;
                age = 0;
            }

            if (isAlive && (count == 2 || count == 3))
            {
                _cells[row, column].Age++;
                isAlive = true;
                age = _cells[row, column].Age;
            }

            if (isAlive && count > 3)
            {
                isAlive = false;
                age = 0;
            }

            if (!isAlive && count == 3)
            {
                isAlive = true;
                age = 0;
            }
        }

        private int CountNeighbors(int i, int j)
        {
            var count = 0;

            if (i != _sizeX - 1 && _cells[i + 1, j].IsAlive) count++;
            if (i != _sizeX - 1 && j != _sizeY - 1 && _cells[i + 1, j + 1].IsAlive) count++;
            if (j != _sizeY - 1 && _cells[i, j + 1].IsAlive) count++;
            if (i != 0 && j != _sizeY - 1 && _cells[i - 1, j + 1].IsAlive) count++;
            if (i != 0 && _cells[i - 1, j].IsAlive) count++;
            if (i != 0 && j != 0 && _cells[i - 1, j - 1].IsAlive) count++;
            if (j != 0 && _cells[i, j - 1].IsAlive) count++;
            if (i != _sizeX - 1 && j != 0 && _cells[i + 1, j - 1].IsAlive) count++;

            return count;
        }
    }
}