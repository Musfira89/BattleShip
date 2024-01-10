using System;
using System.Collections.Generic;
using System.Linq;
using BattleShip.BLL.Requests;
using BattleShip.BLL.Responses;
using BattleShip.BLL.Ships;

namespace BattleShip.BLL.GameLogic
{
    public class Board
    {

        // Compile-Time Evaluation:
        public const int xCoordinator = 10; //size of the board
        public const int yCoordinator = 10;
        private const int NumberOfShips = 5; //Constant Propagation:

        private Dictionary<Coordinate, ShotHistory> ShotHistory;
        private int _currentShipIndex; //keep track of the index of the current ship being placed on the game board.
        public Ship[] Ships { get; private set; }  //array

        //constructor
        public Board()
        {
            ShotHistory = new Dictionary<Coordinate, ShotHistory>();

            Ships = new Ship[NumberOfShips];
            _currentShipIndex = 0;
        }



        //Method 1
        public FireShotResponse FireShot(Coordinate coordinate)
        {
            var response = new FireShotResponse();

            // is this coordinate on the board?
            if (!IsValidCoordinate(coordinate))
            {
                response.ShotStatus = ShotStatus.Invalid;
                return response;
            }

            // did they already try this position?
            if (ShotHistory.ContainsKey(coordinate))
            {
                response.ShotStatus = ShotStatus.Duplicate;
                return response;
            }

            CheckShipsForHit(coordinate, response);
            CheckForVictory(response);

            return response;            
        }

        //method 2
        public ShotHistory CheckCoordinate(Coordinate coordinate)
        {
            if(ShotHistory.ContainsKey(coordinate))
            {
                return ShotHistory[coordinate];
            }
            else
            {
                return Responses.ShotHistory.Unknown;
            }
        }

        // method 3 This method attempts to place a ship on the board based on the player's request
        // (coordinates and direction). It checks for available space and overlapping with
        // other ships.
        public ShipPlacement PlaceShip(PlaceShipRequest request)
        {
            if (_currentShipIndex > 4)
                throw new Exception("You can not add another ship, 5 is the limit!");

            if (!IsValidCoordinate(request.Coordinate))
                return ShipPlacement.NotEnoughSpace;

            Ship newShip = ShipCreator.CreateShip(request.ShipType);
            switch (request.Direction)
            {
                case ShipDirection.Down:
                    return PlaceShipDown(request.Coordinate, newShip);
                case ShipDirection.Up:
                    return PlaceShipUp(request.Coordinate, newShip);
                case ShipDirection.Left:
                    return PlaceShipLeft(request.Coordinate, newShip);
                default:
                    return PlaceShipRight(request.Coordinate, newShip);
            }
        }


        //Private Helper Methods:
        //1st method
        private void CheckForVictory(FireShotResponse response)
        {
            if (response.ShotStatus == ShotStatus.HitAndSunk)
            {
                // did they win?
                if (Ships.All(s => s.IsSunk))
                    response.ShotStatus = ShotStatus.Victory;
            }
        }

        //2nd method
        private void CheckShipsForHit(Coordinate coordinate, FireShotResponse response)
        {
            response.ShotStatus = ShotStatus.Miss;

            foreach (var ship in Ships)
            {
                // no need to check sunk ships
                if (ship.IsSunk)
                    continue;

                ShotStatus status = ship.FireAtShip(coordinate);

                // reduce data clumps
                if (status == ShotStatus.Hit || status == ShotStatus.HitAndSunk)
                {
                    response.ShotStatus = status;
                    response.ShipImpacted = ship.ShipName;
                    ShotHistory.Add(coordinate, Responses.ShotHistory.Hit);
                }
                else
                {
                    ShotHistory.Add(coordinate, Responses.ShotHistory.Miss);
                }


                // if they hit something, no need to continue looping
                if (status != ShotStatus.Miss)
                    break;
            }
        }



        //3rd method
        private bool IsValidCoordinate(Coordinate coordinate)
        {
            return coordinate.XCoordinate >= 1 && coordinate.XCoordinate <= xCoordinator &&
            coordinate.YCoordinate >= 1 && coordinate.YCoordinate <= yCoordinator;
        }

        //4th method (Refactoring)
        private ShipPlacement PlaceShip(Coordinate coordinate, Ship newShip, Func<int, int> calculateNextX, Func<int, int> calculateNextY)
        {
            int positionIndex = 0;
            int maxX = coordinate.XCoordinate + newShip.BoardPositions.Length;
            int maxY = coordinate.YCoordinate + newShip.BoardPositions.Length;

            for (int i = coordinate.XCoordinate, j = coordinate.YCoordinate; i < maxX && j < maxY; i = calculateNextX(i), j = calculateNextY(j))
            {
                var currentCoordinate = new Coordinate(i, j);

                if (!IsValidCoordinate(currentCoordinate))
                    return ShipPlacement.NotEnoughSpace;

                if (OverlapsAnotherShip(currentCoordinate))
                    return ShipPlacement.Overlap;

                newShip.BoardPositions[positionIndex] = currentCoordinate;
                positionIndex++;
            }

            AddShipToBoard(newShip);
            return ShipPlacement.Ok;
        }

        private ShipPlacement PlaceShipRight(Coordinate coordinate, Ship newShip)
        {
            return PlaceShip(coordinate, newShip, x => x + 1, y => y);
        }

        private ShipPlacement PlaceShipLeft(Coordinate coordinate, Ship newShip)
        {
            return PlaceShip(coordinate, newShip, x => x - 1, y => y);
        }

        private ShipPlacement PlaceShipUp(Coordinate coordinate, Ship newShip)
        {
            return PlaceShip(coordinate, newShip, x => x, y => y - 1);
        }

        private ShipPlacement PlaceShipDown(Coordinate coordinate, Ship newShip)
        {
            return PlaceShip(coordinate, newShip, x => x, y => y + 1);
        }


        //5th method
        private void AddShipToBoard(Ship newShip)
        {
            Ships[_currentShipIndex] = newShip;
            _currentShipIndex++;
        }

        private bool OverlapsAnotherShip(Coordinate coordinate)
        {
            foreach (var ship in Ships)
            {
                if (ship != null)
                {
                    if (ship.BoardPositions.Contains(coordinate))
                        return true;
                }
            }

            return false;
        }
    }
}
