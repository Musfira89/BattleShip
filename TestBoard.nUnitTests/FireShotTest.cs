using NUnit.Framework;
using BattleShip.BLL.GameLogic;
using BattleShip.BLL.Requests;
using BattleShip.BLL.Responses;
using BattleShip.BLL.Ships;

namespace TestBoard.nUnitTests
{
    [TestFixture]
    public class FireShotTest
    {
        private Board _board;  // Declare a private variable to hold the Board instance

        [SetUp]
        public void Setup()
        {
            _board = new Board();  // Initialize the Board instance before each test
        }

        [Test]
        //valid shot hits a ship.
        public void ValidShot_HitsShip()
        {
          
            var ship = new Ship(ShipType.Destroyer);
            var placeShipRequest = new PlaceShipRequest
            {
                Coordinate = new Coordinate(1, 1),
                Direction = ShipDirection.Right,
                ShipType = ShipType.Destroyer
            };
            _board.PlaceShip(placeShipRequest);

            var coordinateToHit = new Coordinate(1, 1);
            var response = _board.FireShot(coordinateToHit);

            // Assert
            Assert.That(response.ShotStatus, Is.EqualTo(ShotStatus.Hit));
            Assert.That(response.ShipImpacted, Is.EqualTo("Destroyer"));
        }

        [Test]
        //Testing duplicate shots at the same coordinate.
        public void DuplicateShot_AtSameCoordinate()
        {
            // Arrange
            var ship = new Ship(ShipType.Destroyer);
            var placeShipRequest = new PlaceShipRequest
            {
                Coordinate = new Coordinate(2, 2),
                Direction = ShipDirection.Right,
                ShipType = ShipType.Destroyer
            };
            _board.PlaceShip(placeShipRequest);

            // Act: Fire the first shot
            var coordinateToHit = new Coordinate(2, 2);
            var firstShotResponse = _board.FireShot(coordinateToHit);

            // Assert: Check that the first shot is a Hit
            Assert.That(firstShotResponse.ShotStatus, Is.EqualTo(ShotStatus.Hit));
            Assert.That(firstShotResponse.ShipImpacted, Is.EqualTo("Destroyer"));

            // Act: Fire the second shot at the same coordinate
            var secondShotResponse = _board.FireShot(coordinateToHit);

            // Assert: Check that the second shot is a Duplicate
            Assert.That(secondShotResponse.ShotStatus, Is.EqualTo(ShotStatus.Duplicate));
        }



    }
}
