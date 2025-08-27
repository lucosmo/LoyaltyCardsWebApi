using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;
using LoyaltyCardsWebApi.API.Repositories;
using LoyaltyCardsWebApi.API.Services;
using Moq;


namespace LoyaltyCardsWebApi.API.Tests.Services
{
    [TestFixture]
    public class CardServiceTests
    {
        private Mock<ICardRepository> _cardRepository;
        private Mock<IUserService> _userService;
        private Mock<IDateTimeProvider> _dateTimeProvider;
        private CardService _cardService;

        [SetUp]
        public void SetUp()
        {
            _cardRepository = new Mock<ICardRepository>();
            _userService = new Mock<IUserService>();
            _dateTimeProvider = new Mock<IDateTimeProvider>();
            _cardService = new CardService(_cardRepository.Object, _userService.Object, _dateTimeProvider.Object);
        }

        public static IEnumerable<TestCaseData> CreateCardTestCases()
        {
            var fixedTime1 = new DateTime(2023, 10, 1, 12, 0, 0, DateTimeKind.Utc);
            var fixedTime2 = new DateTime(2024, 11, 4, 11, 15, 0, DateTimeKind.Utc);
            var fixedTime3 = new DateTime(2025, 1, 7, 14, 25, 12, DateTimeKind.Utc);
            var fixedTime4 = new DateTime(2025, 2, 2, 22, 12, 23, DateTimeKind.Utc);

            yield return new TestCaseData(
                new CreateCardDto
                {
                    Name = "Card1",
                    Image = "test.jpg",
                    Barcode = "1234567890"
                },
                new Card
                {
                    Name = "Card1",
                    Image = "test.jpg",
                    Barcode = "1234567890",
                    AddedAt = fixedTime1,
                    UserId = 1
                },
                1
            );

            yield return new TestCaseData(
                new CreateCardDto
                {
                    Name = "Card2",
                    Image = "test2.jpg",
                    Barcode = "1234567890_2"
                },
                new Card
                {
                    Name = "Card2",
                    Image = "test2.jpg",
                    Barcode = "1234567890_2",
                    AddedAt = fixedTime2,
                    UserId = 2
                },
                2
            );

            yield return new TestCaseData(
                new CreateCardDto
                {
                    Name = "Card3",
                    Image = "test3.jpg",
                    Barcode = "1234567890_3"
                },
                new Card
                {
                    Name = "Card3",
                    Image = "test3.jpg",
                    Barcode = "1234567890_3",
                    AddedAt = fixedTime3,
                    UserId = 3
                },
                3
            );

            yield return new TestCaseData(
                new CreateCardDto
                {
                    Name = "Card4",
                    Image = "test4.jpg",
                    Barcode = "1234567890_4"
                },
                new Card
                {
                    Name = "Card4",
                    Image = "test4.jpg",
                    Barcode = "1234567890_4",
                    AddedAt = fixedTime4,
                    UserId = 4
                },
                4
            );
        }

        [Test, TestCaseSource(nameof(CreateCardTestCases))]
        public async Task CreateCard_ValidInput_ReturnsSuccess(CreateCardDto createCardDto, Card card, int? userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.CreateCardAsync(It.IsAny<Card>())).Returns((Card card) => Task.FromResult<Card?>(card));
            _cardRepository.Setup(cr => cr.ExistsCardByBarcodeAsync(createCardDto.Barcode, userId)).ReturnsAsync(false);
            _dateTimeProvider.Setup(dp => dp.UtcNow).Returns(card.AddedAt);

            // Act
            Result<CardDto> result = await _cardService.CreateCardAsync(createCardDto, userId);
            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Value);
            Assert.That(result.Value.Id, Is.EqualTo(card.Id));
            Assert.That(result.Value.Name, Is.EqualTo(card.Name));
            Assert.That(result.Value.Image, Is.EqualTo(card.Image));
            Assert.That(result.Value.Barcode, Is.EqualTo(card.Barcode));
            Assert.That(result.Value.AddedAt, Is.EqualTo(card.AddedAt));
            _cardRepository.Verify(cr => cr.CreateCardAsync(It.IsAny<Card>()), Times.Once);
            _cardRepository.Verify(cr => cr.ExistsCardByBarcodeAsync(createCardDto.Barcode, userId), Times.Once);

        }

        [Test, TestCaseSource(nameof(CreateCardTestCases))]
        public async Task CreateCard_NoUserId_ReturnsFail(CreateCardDto createCardDto, Card card, int? userId)
        {
            // Arrange

            // Act
            Result<CardDto> result = await _cardService.CreateCardAsync(createCardDto, null);
            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("User ID is required to create a card."));
            _cardRepository.Verify(cr => cr.CreateCardAsync(It.IsAny<Card>()), Times.Never);
        }

        [Test, TestCaseSource(nameof(CreateCardTestCases))]
        public async Task CreateCard_DuplicateBarcode_ReturnsFail(CreateCardDto createCardDto, Card card, int? userId)
        {
            // Arrange

            _cardRepository.Setup(cr => cr.ExistsCardByBarcodeAsync(createCardDto.Barcode, userId)).ReturnsAsync(true);

            // Act
            Result<CardDto> result = await _cardService.CreateCardAsync(createCardDto, userId);
            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("A card with this barcode already exists for the user."));
            _cardRepository.Verify(cr => cr.ExistsCardByBarcodeAsync(createCardDto.Barcode, userId), Times.Once);
            _cardRepository.Verify(cr => cr.CreateCardAsync(It.IsAny<Card>()), Times.Never);
        }

        [Test, TestCaseSource(nameof(CreateCardTestCases))]
        public async Task CreateCard_CreateFailed_ReturnsFail(CreateCardDto createCardDto, Card card, int? userId)
        {
            // Arrange

            _cardRepository.Setup(cr => cr.ExistsCardByBarcodeAsync(createCardDto.Barcode, userId)).ReturnsAsync(false);
            _cardRepository.Setup(cr => cr.CreateCardAsync(It.IsAny<Card>())).ReturnsAsync((Card?)null);
            _dateTimeProvider.Setup(dp => dp.UtcNow).Returns(card.AddedAt);

            // Act
            Result<CardDto> result = await _cardService.CreateCardAsync(createCardDto, userId);
            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("Card creation failed."));
            _cardRepository.Verify(cr => cr.ExistsCardByBarcodeAsync(createCardDto.Barcode, userId), Times.Once);
            _cardRepository.Verify(cr => cr.CreateCardAsync(It.IsAny<Card>()), Times.Once);
        }

        public static IEnumerable<TestCaseData> DeleteAndGetCardByIdTestCases()
        {
            var fixedTime1 = new DateTime(2023, 10, 1, 12, 0, 0, DateTimeKind.Utc);
            var fixedTime2 = new DateTime(2024, 11, 4, 11, 15, 0, DateTimeKind.Utc);
            var fixedTime3 = new DateTime(2025, 1, 7, 14, 25, 12, DateTimeKind.Utc);
            var fixedTime4 = new DateTime(2025, 2, 2, 22, 12, 23, DateTimeKind.Utc);

            yield return new TestCaseData(
                new CardDto
                {
                    Id = 1,
                    Name = "Card1",
                    Image = "test.jpg",
                    Barcode = "1234567890",
                    AddedAt = fixedTime1,
                    UserId = 1
                },
                new Card
                {
                    Id = 1,
                    Name = "Card1",
                    Image = "test.jpg",
                    Barcode = "1234567890",
                    AddedAt = fixedTime1,
                    UserId = 1
                },
                1,
                1
            );

            yield return new TestCaseData(
                new CardDto
                {
                    Id = 2,
                    Name = "Card2",
                    Image = "test2.jpg",
                    Barcode = "1234567890_2",
                    AddedAt = fixedTime2,
                    UserId = 2
                },
                new Card
                {
                    Id = 2,
                    Name = "Card2",
                    Image = "test2.jpg",
                    Barcode = "1234567890_2",
                    AddedAt = fixedTime2,
                    UserId = 2
                },
                2,
                2
            );

            yield return new TestCaseData(
                new CardDto
                {
                    Id = 3,
                    Name = "Card3",
                    Image = "test3.jpg",
                    Barcode = "1234567890_3",
                    AddedAt = fixedTime3,
                    UserId = 3
                },
                new Card
                {
                    Id = 3,
                    Name = "Card3",
                    Image = "test3.jpg",
                    Barcode = "1234567890_3",
                    AddedAt = fixedTime3,
                    UserId = 3
                },
                3,
                3
            );

            yield return new TestCaseData(
                new CardDto
                {
                    Id = 4,
                    Name = "Card4",
                    Image = "test4.jpg",
                    Barcode = "1234567890_4",
                    AddedAt = fixedTime4,
                    UserId = 4
                },
                new Card
                {
                    Id = 4,
                    Name = "Card4",
                    Image = "test4.jpg",
                    Barcode = "1234567890_4",
                    AddedAt = fixedTime4,
                    UserId = 4
                },
                4,
                4
            );
        }

        [Test, TestCaseSource(nameof(DeleteAndGetCardByIdTestCases))]
        public async Task DeleteCard_ValidInput_ReturnsSuccess(CardDto cardDto, Card card, int cardId, int? userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId)).ReturnsAsync(card);
            _cardRepository.Setup(cr => cr.Delete(cardId)).ReturnsAsync(card);

            // Act
            Result<CardDto> result = await _cardService.DeleteCardAsync(cardId, userId);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Value);
            Assert.That(result.Value.Id, Is.EqualTo(cardDto.Id));
            Assert.That(result.Value.Name, Is.EqualTo(cardDto.Name));
            Assert.That(result.Value.Image, Is.EqualTo(cardDto.Image));
            Assert.That(result.Value.Barcode, Is.EqualTo(cardDto.Barcode));
            Assert.That(result.Value.AddedAt, Is.EqualTo(cardDto.AddedAt));
            Assert.That(result.Value.UserId, Is.EqualTo(cardDto.UserId));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId), Times.Once);
            _cardRepository.Verify(cr => cr.Delete(cardId), Times.Once);
        }

        [Test, TestCaseSource(nameof(DeleteAndGetCardByIdTestCases))]
        public async Task DeleteCard_NonExistentCard_ReturnsFailure(CardDto cardDto, Card card, int cardId, int? userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId)).ReturnsAsync((Card?)null);

            // Act
            Result<CardDto> result = await _cardService.DeleteCardAsync(cardId, userId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("Card not found."));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId), Times.Once);
            _cardRepository.Verify(cr => cr.Delete(cardId), Times.Never);
        }

        [Test, TestCaseSource(nameof(DeleteAndGetCardByIdTestCases))]
        public async Task DeleteCard_NotMatchingUserId_ReturnsFailure(CardDto cardDto, Card card, int cardId, int? userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId)).ReturnsAsync(card);
            userId = userId + 1;

            // Act
            Result<CardDto> result = await _cardService.DeleteCardAsync(cardId, userId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("You do not have permission to delete this card."));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId), Times.Once);
            _cardRepository.Verify(cr => cr.Delete(cardId), Times.Never);
        }

        [Test, TestCaseSource(nameof(DeleteAndGetCardByIdTestCases))]
        public async Task DeleteCard_NotDeletedInRepository_ReturnsFailure(CardDto cardDto, Card card, int cardId, int? userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId)).ReturnsAsync(card);
            _cardRepository.Setup(cr => cr.Delete(cardId)).ReturnsAsync((Card?)null);

            // Act
            Result<CardDto> result = await _cardService.DeleteCardAsync(cardId, userId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("Deletion failed."));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId), Times.Once);
            _cardRepository.Verify(cr => cr.Delete(cardId), Times.Once);
        }

        [Test, TestCaseSource(nameof(DeleteAndGetCardByIdTestCases))]
        public async Task GetCardById_ValidInput_ReturnsSuccess(CardDto cardDto, Card card, int cardId, int? userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId)).ReturnsAsync(card);

            // Act
            Result<CardDto> result = await _cardService.GetCardByIdAsync(cardId, userId);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Value);
            Assert.That(result.Value.Id, Is.EqualTo(cardDto.Id));
            Assert.That(result.Value.Name, Is.EqualTo(cardDto.Name));
            Assert.That(result.Value.Image, Is.EqualTo(cardDto.Image));
            Assert.That(result.Value.Barcode, Is.EqualTo(cardDto.Barcode));
            Assert.That(result.Value.AddedAt, Is.EqualTo(cardDto.AddedAt));
            Assert.That(result.Value.UserId, Is.EqualTo(cardDto.UserId));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId), Times.Once);
        }

        [Test, TestCaseSource(nameof(DeleteAndGetCardByIdTestCases))]
        public async Task GetCardById_NullFromRepository_ReturnsFailure(CardDto cardDto, Card card, int cardId, int? userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId)).ReturnsAsync((Card?)null);

            // Act
            Result<CardDto> result = await _cardService.GetCardByIdAsync(cardId, userId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("Card not found"));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId), Times.Once);
        }

        [Test, TestCaseSource(nameof(DeleteAndGetCardByIdTestCases))]
        public async Task GetCardById_UserIdIsNull_ReturnsFailure(CardDto cardDto, Card card, int cardId, int? userId)
        {
            // Arrange
            userId = null;

            // Act
            Result<CardDto> result = await _cardService.GetCardByIdAsync(cardId, userId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("User ID is required to access this card."));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId), Times.Never);
        }

        [Test, TestCaseSource(nameof(DeleteAndGetCardByIdTestCases))]
        public async Task GetCardById_NotMatchingUserIds_ReturnsFailure(CardDto cardDto, Card card, int cardId, int? userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId)).ReturnsAsync(card);
            userId = userId + 1;

            // Act
            Result<CardDto> result = await _cardService.GetCardByIdAsync(cardId, userId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("You do not have permission to access this card."));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId), Times.Once);
        }

        public static IEnumerable<TestCaseData> GetCardsByUserIdTestCases()
        {
            var fixedTime1 = new DateTime(2023, 10, 1, 12, 0, 0, DateTimeKind.Utc);
            var fixedTime2 = new DateTime(2024, 11, 4, 11, 15, 0, DateTimeKind.Utc);
            var fixedTime3 = new DateTime(2025, 1, 7, 14, 25, 12, DateTimeKind.Utc);
            var fixedTime4 = new DateTime(2025, 2, 2, 22, 12, 23, DateTimeKind.Utc);

            yield return new TestCaseData(
                new List<Card>
                {
                    new Card
                    {
                        Id = 1,
                        Name = "Card1",
                        Image = "test.jpg",
                        Barcode = "1234567890",
                        AddedAt = fixedTime1,
                        UserId = 1
                    },
                    new Card
                    {
                        Id = 2,
                        Name = "Card2",
                        Image = "test2.jpg",
                        Barcode = "1234567890_2",
                        AddedAt = fixedTime2,
                        UserId = 1
                    },
                    new Card
                    {
                        Id = 3,
                        Name = "Card3",
                        Image = "test3.jpg",
                        Barcode = "1234567890_3",
                        AddedAt = fixedTime3,
                        UserId = 1
                    }
                },
                1,
                3
            );

            yield return new TestCaseData(
                new List<Card>
                {
                    new Card
                    {
                        Id = 4,
                        Name = "Card4",
                        Image = "test4.jpg",
                        Barcode = "1234567890_4",
                        AddedAt = fixedTime4,
                        UserId = 2
                    }
                },
                2,
                1
            );

            yield return new TestCaseData(
                new List<Card>(),
                3,
                0
            );
        }
        [Test, TestCaseSource(nameof(GetCardsByUserIdTestCases))]
        public async Task GetCardsByUserId_ValidInput_ReturnsSuccess(List<Card> cards, int? userId, int expectedCount)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardsByUserIdAsync(userId)).ReturnsAsync(cards);

            // Act
            Result<IEnumerable<CardDto>> result = await _cardService.GetCardsByUserIdAsync(userId);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Value);
            Assert.That(result.Value.Count(), Is.EqualTo(expectedCount));

            _cardRepository.Verify(cr => cr.GetCardsByUserIdAsync(userId), Times.Once);
        }

        [Test]
        public async Task GetCardsByUserId_UserIsNull_ReturnsFailure()
        {
            // Arrange
            var userId = (int?)null;

            // Act
            Result<IEnumerable<CardDto>> result = await _cardService.GetCardsByUserIdAsync(userId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("User ID is required to access cards."));

            _cardRepository.Verify(cr => cr.GetCardsByUserIdAsync(userId), Times.Never);
        }

        public static IEnumerable<TestCaseData> UpdateCardTestCases()
        {
            var fixedTime1 = new DateTime(2023, 10, 1, 12, 0, 0, DateTimeKind.Utc);
            var fixedTime2 = new DateTime(2024, 11, 4, 11, 15, 0, DateTimeKind.Utc);
            var fixedTime3 = new DateTime(2025, 1, 7, 14, 25, 12, DateTimeKind.Utc);
            var fixedTime4 = new DateTime(2025, 2, 2, 22, 12, 23, DateTimeKind.Utc);

            yield return new TestCaseData(
                new UpdateCardDto
                {
                    Name = "Card1_update",
                    Image = "test11_update.jpg",
                    Barcode = "1234567890_update"
                },
                new Card
                {
                    Id = 1,
                    Name = "Card1",
                    Image = "test.jpg",
                    Barcode = "1234567890",
                    AddedAt = fixedTime1,
                    UserId = 1
                },
                new Card
                {
                    Id = 1,
                    Name = "Card1_update",
                    Image = "test11_update.jpg",
                    Barcode = "1234567890_update",
                    AddedAt = fixedTime1,
                    UserId = 1
                },
                1,
                1
            );

            yield return new TestCaseData(
                new UpdateCardDto
                {
                    Name = "Card2_update",
                    Image = "test22_update.jpg",
                    Barcode = "1234567890_2_update"
                },
                new Card
                {
                    Id = 2,
                    Name = "Card2",
                    Image = "test2.jpg",
                    Barcode = "1234567890_2",
                    AddedAt = fixedTime2,
                    UserId = 2
                },
                new Card
                {
                    Id = 2,
                    Name = "Card2_update",
                    Image = "test22_update.jpg",
                    Barcode = "1234567890_2_update",
                    AddedAt = fixedTime2,
                    UserId = 2
                },
                2,
                2
            );

            yield return new TestCaseData(
                new UpdateCardDto
                {
                    Name = "Card3_update",
                    Image = "test33_update.jpg",
                    Barcode = "1234567890_3_update"
                },
                new Card
                {
                    Id = 3,
                    Name = "Card3",
                    Image = "test3.jpg",
                    Barcode = "1234567890_3",
                    AddedAt = fixedTime3,
                    UserId = 3
                },
                new Card
                {
                    Id = 3,
                    Name = "Card3_update",
                    Image = "test33_update.jpg",
                    Barcode = "1234567890_3_update",
                    AddedAt = fixedTime3,
                    UserId = 3
                },
                3,
                3
            );

            yield return new TestCaseData(
                new UpdateCardDto
                {
                    Name = "Card4_update",
                    Image = "test4_update.jpg",
                    Barcode = "1234567890_4_update"
                },
                new Card
                {
                    Id = 4,
                    Name = "Card4",
                    Image = "test4.jpg",
                    Barcode = "1234567890_4",
                    AddedAt = fixedTime4,
                    UserId = 4
                },
                new Card
                {
                    Id = 4,
                    Name = "Card4_update",
                    Image = "test4_update.jpg",
                    Barcode = "1234567890_4_update",
                    AddedAt = fixedTime4,
                    UserId = 4
                },
                4,
                4
            );
        }

        [Test, TestCaseSource(nameof(UpdateCardTestCases))]
        public async Task UpdateCard_ValidInput_ReturnsSuccess(UpdateCardDto updateCardDto, Card card, Card updatedCard, int cardId, int? userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId)).ReturnsAsync(card);
            _cardRepository.Setup(cr => cr.UpdateCardAsync(It.IsAny<Card>())).ReturnsAsync(updatedCard);

            // Act
            Result<CardDto> result = await _cardService.UpdateCardAsync(cardId, updateCardDto, userId);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Value);
            Assert.That(result.Value.Id, Is.EqualTo(updatedCard.Id));
            Assert.That(result.Value.Name, Is.EqualTo(updatedCard.Name));
            Assert.That(result.Value.Image, Is.EqualTo(updatedCard.Image));
            Assert.That(result.Value.Barcode, Is.EqualTo(updatedCard.Barcode));
            Assert.That(result.Value.AddedAt, Is.EqualTo(updatedCard.AddedAt));
            Assert.That(result.Value.UserId, Is.EqualTo(updatedCard.UserId));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId), Times.Once);
            _cardRepository.Verify(cr => cr.UpdateCardAsync(It.IsAny<Card>()), Times.Once);
        }

        [Test, TestCaseSource(nameof(UpdateCardTestCases))]
        public async Task UpdateCard_InvalidCardId_ReturnsFailure(UpdateCardDto updateCardDto, Card card, Card updatedCard, int cardId, int? userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId)).ReturnsAsync((Card?)null);

            // Act
            Result<CardDto> result = await _cardService.UpdateCardAsync(cardId, updateCardDto, userId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("Card not found."));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId), Times.Once);
            _cardRepository.Verify(cr => cr.UpdateCardAsync(It.IsAny<Card>()), Times.Never);
        }

        [Test, TestCaseSource(nameof(UpdateCardTestCases))]
        public async Task UpdateCard_NotMatchingUserId_ReturnsFailure(UpdateCardDto updateCardDto, Card card, Card updatedCard, int cardId, int? userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId)).ReturnsAsync(card);
            userId = userId + 1;

            // Act
            Result<CardDto> result = await _cardService.UpdateCardAsync(cardId, updateCardDto, userId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("You do not have permission to access this card."));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId), Times.Once);
            _cardRepository.Verify(cr => cr.UpdateCardAsync(It.IsAny<Card>()), Times.Never);
        }
        
        [Test, TestCaseSource(nameof(UpdateCardTestCases))]
        public async Task UpdateCard_NotUpdatedInRepository_ReturnsFailure(UpdateCardDto updateCardDto, Card card, Card updatedCard, int cardId, int? userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId)).ReturnsAsync(card);
            _cardRepository.Setup(cr => cr.UpdateCardAsync(It.IsAny<Card>())).ReturnsAsync((Card?)null);

            // Act
            Result<CardDto> result = await _cardService.UpdateCardAsync(cardId, updateCardDto, userId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("Card not found or update failed."));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId), Times.Once);
            _cardRepository.Verify(cr => cr.UpdateCardAsync(It.IsAny<Card>()), Times.Once);
        }
    }
}
