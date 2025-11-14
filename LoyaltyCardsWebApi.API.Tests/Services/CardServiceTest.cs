using System.Security.Cryptography;
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
        private Mock<IDateTimeProvider> _dateTimeProvider;
        private CardService _cardService;

        [SetUp]
        public void SetUp()
        {
            _cardRepository = new Mock<ICardRepository>();
            _dateTimeProvider = new Mock<IDateTimeProvider>();
            _cardService = new CardService(_cardRepository.Object, _dateTimeProvider.Object);
        }

        private static class TimeData
        {
            public static readonly DateTime FixedTime1 = new DateTime(2023, 10, 1, 12, 0, 0, DateTimeKind.Utc);
            public static readonly DateTime FixedTime2 = new DateTime(2024, 11, 4, 11, 15, 0, DateTimeKind.Utc);
            public static readonly DateTime FixedTime3 = new DateTime(2025, 1, 7, 14, 25, 12, DateTimeKind.Utc);
            public static readonly DateTime FixedTime4 = new DateTime(2025, 2, 2, 22, 12, 23, DateTimeKind.Utc);
        }

        private static CardDto NewCardDto(int id, string name, string image, string barcode, DateTime addedAt, int userId) => new CardDto
        {
            Id = id,
            Name = name,
            Image = image,
            Barcode = barcode,
            AddedAt = addedAt,
            UserId = userId
        };
        private static CreateCardDto NewCreateCardDto(string name, string image, string barcode) => new CreateCardDto
        {
            Name = name,
            Image = image,
            Barcode = barcode
        };

        private static Card NewCard(int id, string name, string image, string barcode, DateTime addedAt, int userId) => new Card
        {
            Id = id,
            Name = name,
            Image = image,
            Barcode = barcode,
            AddedAt = addedAt,
            UserId = userId
        };

        private static UpdateCardDto NewUpdateCardDto(string? name, string? image, string? barcode) => new UpdateCardDto
        {
            Name = name,
            Image = image,
            Barcode = barcode
        };

        public static IEnumerable<TestCaseData> CreateCardTestCases()
        {
            yield return new TestCaseData(
                NewCreateCardDto("Card1", "test.jpg", "1234567890"),
                NewCard(1, "Card1", "test.jpg", "1234567890", TimeData.FixedTime1, 1),
                1
            );

            yield return new TestCaseData(
                NewCreateCardDto("Card2", "test2.jpg", "1234567890_2"),
                NewCard(2, "Card2", "test2.jpg", "1234567890_2", TimeData.FixedTime2, 2),
                2
            );

            yield return new TestCaseData(
                NewCreateCardDto("Card3", "test3.jpg", "1234567890_3"),
                NewCard(3, "Card3", "test3.jpg", "1234567890_3", TimeData.FixedTime3, 3),
                3
            );

            yield return new TestCaseData(
                NewCreateCardDto("Card4", "test4.jpg", "1234567890_4"),
                NewCard(4, "Card4", "test4.jpg", "1234567890_4", TimeData.FixedTime4, 4),
                4
            );
        }

        [Test, TestCaseSource(nameof(CreateCardTestCases))]
        public async Task CreateCard_ValidInput_ReturnsSuccess(CreateCardDto createCardDto, Card expectedCard, int userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.CreateCardAsync(It.IsAny<Card>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedCard);
            _cardRepository.Setup(cr => cr.ExistsCardByBarcodeAsync(createCardDto.Barcode, userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _dateTimeProvider.Setup(dp => dp.UtcNow).Returns(expectedCard.AddedAt);

            // Act
            Result<CardDto> result = await _cardService.CreateCardAsync(createCardDto, userId);
            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Value);
            Assert.That(result.Value.Id, Is.EqualTo(expectedCard.Id));
            Assert.That(result.Value.Name, Is.EqualTo(expectedCard.Name));
            Assert.That(result.Value.Image, Is.EqualTo(expectedCard.Image));
            Assert.That(result.Value.Barcode, Is.EqualTo(expectedCard.Barcode));
            Assert.That(result.Value.AddedAt, Is.EqualTo(expectedCard.AddedAt));
            _cardRepository.Verify(cr => cr.CreateCardAsync(It.Is<Card>(c =>
                c.Name == expectedCard.Name &&
                c.Image == expectedCard.Image &&
                c.Barcode == expectedCard.Barcode &&
                c.AddedAt == expectedCard.AddedAt &&
                c.UserId == expectedCard.UserId), It.IsAny<CancellationToken>()), Times.Once);
            _cardRepository.Verify(cr => cr.ExistsCardByBarcodeAsync(createCardDto.Barcode, userId, It.IsAny<CancellationToken>()), Times.Once);

        }

        [Test, TestCaseSource(nameof(CreateCardTestCases))]
        public void CreateCard_CancellationToken_ThrowsOperationCanceledException(CreateCardDto createCardDto, Card expectedCard, int userId)
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            _cardRepository
                .Setup(cr => cr.ExistsCardByBarcodeAsync(createCardDto.Barcode, userId, It.Is<CancellationToken>(ct => ct == cts.Token)))
                .ThrowsAsync(new OperationCanceledException(cts.Token));
            

            // Act & Assert
            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await _cardService.CreateCardAsync(createCardDto, userId, cts.Token);
            });

            _cardRepository.Verify(cr => cr.CreateCardAsync(It.IsAny<Card>(),It.IsAny<CancellationToken>()), Times.Never);
            _cardRepository.Verify(cr => cr.ExistsCardByBarcodeAsync(createCardDto.Barcode, userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, TestCaseSource(nameof(CreateCardTestCases))]
        public async Task CreateCard_NoUserId_ReturnsFail(CreateCardDto createCardDto, Card card, int userId)
        {
            // Arrange

            // Act
            Result<CardDto> result = await _cardService.CreateCardAsync(createCardDto, null);
            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("User ID is required to create a card."));
            _cardRepository.Verify(cr => cr.CreateCardAsync(It.IsAny<Card>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test, TestCaseSource(nameof(CreateCardTestCases))]
        public async Task CreateCard_DuplicateBarcode_ReturnsFail(CreateCardDto createCardDto, Card card, int userId)
        {
            // Arrange

            _cardRepository.Setup(cr => cr.ExistsCardByBarcodeAsync(createCardDto.Barcode, userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            Result<CardDto> result = await _cardService.CreateCardAsync(createCardDto, userId);
            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("A card with this barcode already exists for the user."));
            _cardRepository.Verify(cr => cr.ExistsCardByBarcodeAsync(createCardDto.Barcode, userId, It.IsAny<CancellationToken>()), Times.Once);
            _cardRepository.Verify(cr => cr.CreateCardAsync(It.IsAny<Card>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test, TestCaseSource(nameof(CreateCardTestCases))]
        public async Task CreateCard_CreateFailed_ReturnsFail(CreateCardDto createCardDto, Card card, int userId)
        {
            // Arrange

            _cardRepository.Setup(cr => cr.ExistsCardByBarcodeAsync(createCardDto.Barcode, userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _cardRepository.Setup(cr => cr.CreateCardAsync(It.IsAny<Card>(), It.IsAny<CancellationToken>())).ReturnsAsync((Card?)null);
            _dateTimeProvider.Setup(dp => dp.UtcNow).Returns(card.AddedAt);

            // Act
            Result<CardDto> result = await _cardService.CreateCardAsync(createCardDto, userId);
            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("Card creation failed."));
            _cardRepository.Verify(cr => cr.ExistsCardByBarcodeAsync(createCardDto.Barcode, userId, It.IsAny<CancellationToken>()), Times.Once);
            _cardRepository.Verify(cr => cr.CreateCardAsync(It.IsAny<Card>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        public static IEnumerable<TestCaseData> DeleteAndGetCardByIdTestCases()
        {
            yield return new TestCaseData(
                NewCardDto(1, "Card1", "test.jpg", "1234567890", TimeData.FixedTime1, 1),
                NewCard(1, "Card1", "test.jpg", "1234567890", TimeData.FixedTime1, 1),
                1,
                1
            );

            yield return new TestCaseData(
                NewCardDto(2, "Card2", "test2.jpg", "1234567890_2", TimeData.FixedTime2, 2),
                NewCard(2, "Card2", "test2.jpg", "1234567890_2", TimeData.FixedTime2, 2),
                2,
                2
            );

            yield return new TestCaseData(
                NewCardDto(3, "Card3", "test3.jpg", "1234567890_3", TimeData.FixedTime3, 3),
                NewCard(3, "Card3", "test3.jpg", "1234567890_3", TimeData.FixedTime3, 3),
                3,
                3
            );

            yield return new TestCaseData(
                NewCardDto(4, "Card4", "test4.jpg", "1234567890_4", TimeData.FixedTime4, 4),
                NewCard(4, "Card4", "test4.jpg", "1234567890_4", TimeData.FixedTime4, 4),
                4,
                4
            );
        }

        [Test, TestCaseSource(nameof(DeleteAndGetCardByIdTestCases))]
        public async Task DeleteCard_ValidInput_ReturnsSuccess(CardDto cardDto, Card card, int cardId, int userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>())).ReturnsAsync(card);
            _cardRepository.Setup(cr => cr.Delete(cardId, userId, It.IsAny<CancellationToken>())).ReturnsAsync(card);

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

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>()), Times.Once);
            _cardRepository.Verify(cr => cr.Delete(cardId, userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, TestCaseSource(nameof(DeleteAndGetCardByIdTestCases))]
        public void DeleteCard_CancellationToken_ThrowsOperationCanceledException(CardDto cardDto, Card card, int cardId, int userId)
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            _cardRepository
                .Setup(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException(cts.Token));

            // Act
            var ex = Assert.CatchAsync(async () => {
                await _cardService.DeleteCardAsync(cardId, userId, cts.Token);
            });

            // Assert
            Assert.That(ex, Is.InstanceOf<OperationCanceledException>());
            
            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>()), Times.Once);
            _cardRepository.Verify(cr => cr.Delete(cardId, userId, It.IsAny<CancellationToken>()), Times.Never);
        }


        [Test, TestCaseSource(nameof(DeleteAndGetCardByIdTestCases))]
        public async Task DeleteCard_NonExistentCard_ReturnsFailure(CardDto cardDto, Card card, int cardId, int userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>())).ReturnsAsync((Card?)null);

            // Act
            Result<CardDto> result = await _cardService.DeleteCardAsync(cardId, userId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("Card not found."));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>()), Times.Once);
            _cardRepository.Verify(cr => cr.Delete(cardId, userId, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test, TestCaseSource(nameof(DeleteAndGetCardByIdTestCases))]
        public async Task DeleteCard_NotMatchingUserId_ReturnsFailure(CardDto cardDto, Card card, int cardId, int userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>())).ReturnsAsync(card);
            int otherUserId = userId + 1;

            // Act
            Result<CardDto> result = await _cardService.DeleteCardAsync(cardId, otherUserId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("Card not found."));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId, otherUserId, It.IsAny<CancellationToken>()), Times.Once);
            _cardRepository.Verify(cr => cr.Delete(cardId, otherUserId, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test, TestCaseSource(nameof(DeleteAndGetCardByIdTestCases))]
        public async Task DeleteCard_UserIdIsNull_ReturnsFailure(CardDto cardDto, Card card, int cardId, int userId)
        {
            // Arrange
            int? otherUserId = null;

            // Act
            Result<CardDto> result = await _cardService.DeleteCardAsync(cardId, otherUserId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("User ID is required to delete this card."));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>()), Times.Never);
            _cardRepository.Verify(cr => cr.Delete(cardId, userId, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test, TestCaseSource(nameof(DeleteAndGetCardByIdTestCases))]
        public async Task DeleteCard_NotDeletedInRepository_ReturnsFailure(CardDto cardDto, Card card, int cardId, int userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>())).ReturnsAsync(card);
            _cardRepository.Setup(cr => cr.Delete(cardId, userId, It.IsAny<CancellationToken>())).ReturnsAsync((Card?)null);

            // Act
            Result<CardDto> result = await _cardService.DeleteCardAsync(cardId, userId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("Deletion failed."));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>()), Times.Once);
            _cardRepository.Verify(cr => cr.Delete(cardId, userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, TestCaseSource(nameof(DeleteAndGetCardByIdTestCases))]
        public async Task GetCardById_ValidInput_ReturnsSuccess(CardDto cardDto, Card card, int cardId, int userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>())).ReturnsAsync(card);

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

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, TestCaseSource(nameof(DeleteAndGetCardByIdTestCases))]
        public void GetCardById_CancellationToken_ThrowsOperationCanceledException(CardDto cardDto, Card card, int cardId, int userId)
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            _cardRepository
                .Setup(cr => cr.GetCardByIdAsync(cardId, userId, It.Is<CancellationToken>(ct => ct == cts.Token)))
                .ThrowsAsync(new OperationCanceledException(cts.Token));

            // Act
            Exception ex = Assert.CatchAsync(async () =>
            {
                await _cardService.GetCardByIdAsync(cardId, userId, cts.Token);
            }); 

            // Assert
            Assert.That(ex, Is.InstanceOf<OperationCanceledException>());   
            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, TestCaseSource(nameof(DeleteAndGetCardByIdTestCases))]
        public async Task GetCardById_NullFromRepository_ReturnsFailure(CardDto cardDto, Card card, int cardId, int userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>())).ReturnsAsync((Card?)null);

            // Act
            Result<CardDto> result = await _cardService.GetCardByIdAsync(cardId, userId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("Card not found."));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, TestCaseSource(nameof(DeleteAndGetCardByIdTestCases))]
        public async Task GetCardById_UserIdIsNull_ReturnsFailure(CardDto cardDto, Card card, int cardId, int userId)
        {
            // Arrange
            int? otherUserId = null;

            // Act
            Result<CardDto> result = await _cardService.GetCardByIdAsync(cardId, otherUserId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("User ID is required to access this card."));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test, TestCaseSource(nameof(DeleteAndGetCardByIdTestCases))]
        public async Task GetCardById_NotMatchingUserIds_ReturnsFailure(CardDto cardDto, Card card, int cardId, int userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>())).ReturnsAsync(card);
            int? otherUserId = userId + 1;

            // Act
            Result<CardDto> result = await _cardService.GetCardByIdAsync(cardId, otherUserId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("Card not found."));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId, It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        public static IEnumerable<TestCaseData> GetCardsByUserIdTestCases()
        {
            yield return new TestCaseData(
                new List<Card>
                {
                    NewCard(1, "Card1", "test.jpg", "1234567890", TimeData.FixedTime1, 1),
                    NewCard(2, "Card2", "test2.jpg", "1234567890_2", TimeData.FixedTime2, 1),
                    NewCard(3, "Card3", "test3.jpg", "1234567890_3", TimeData.FixedTime3, 1)
                },
                1,
                (int?)1,
                3
            );

            yield return new TestCaseData(
                new List<Card>
                {
                    NewCard(4, "Card4", "test4.jpg", "1234567890_4", TimeData.FixedTime4, 2)
                },
                2,
                (int?)2,
                1
            );

            yield return new TestCaseData(
                new List<Card>(),
                3,
                (int?)3,
                0
            );
        }
        [Test, TestCaseSource(nameof(GetCardsByUserIdTestCases))]
        public async Task GetCardsByUserId_ValidInput_ReturnsSuccess(List<Card> cards, int userId, int? currentUserId, int expectedCount)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardsByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cards);

            // Act
            Result<IEnumerable<CardDto>> result = await _cardService.GetCardsByUserIdAsync(userId, currentUserId);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Value);
            Assert.That(result.Value.Count(), Is.EqualTo(expectedCount));

            _cardRepository.Verify(cr => cr.GetCardsByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, TestCaseSource(nameof(GetCardsByUserIdTestCases))]
        public void GetCardsByUserId_CancellationToken_ThrowsOperationCanceledException(List<Card> cards, int userId, int? currentUserId, int expectedCount)
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            _cardRepository
                .Setup(cr => cr.GetCardsByUserIdAsync(userId, It.Is<CancellationToken>(ct => ct == cts.Token)))
                .ThrowsAsync(new OperationCanceledException(cts.Token));

            // Act
            Exception ex = Assert.CatchAsync(async () =>
            {
                await _cardService.GetCardsByUserIdAsync(userId, currentUserId, cts.Token);
            });

            // Assert
            Assert.That(ex, Is.InstanceOf<OperationCanceledException>());       
            _cardRepository.Verify(cr => cr.GetCardsByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetCardsByUserId_UserIdIsNull_ReturnsFailure()
        {
            // Arrange
            var userId = (int?)null;
            var currentUserId = (int?)null;

            // Act
            Result<IEnumerable<CardDto>> result = await _cardService.GetCardsByUserIdAsync(userId, currentUserId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("User ID is required to access cards."));

            _cardRepository.Verify(cr => cr.GetCardsByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        public static IEnumerable<TestCaseData> UpdateCardTestCases()
        {
            yield return new TestCaseData(
                NewUpdateCardDto("Card1_update", "test11_update.jpg", "1234567890_update"),
                NewCard(1, "Card1", "test.jpg", "1234567890", TimeData.FixedTime1, 1),
                NewCard(1, "Card1_update", "test11_update.jpg", "1234567890_update", TimeData.FixedTime1, 1),
                1, 1
            );
            yield return new TestCaseData(
                NewUpdateCardDto("Card2_update", "test22_update.jpg", "1234567890_2_update"),
                NewCard(2, "Card2", "test2.jpg", "1234567890_2", TimeData.FixedTime2, 2),
                NewCard(2, "Card2_update", "test22_update.jpg", "1234567890_2_update", TimeData.FixedTime2, 2),
                2, 2
            );
            yield return new TestCaseData(
                NewUpdateCardDto("Card3_update", "test33_update.jpg", "1234567890_3_update"),
                NewCard(3, "Card3", "test3.jpg", "1234567890_3", TimeData.FixedTime3, 3),
                NewCard(3, "Card3_update", "test33_update.jpg", "1234567890_3_update", TimeData.FixedTime3, 3),
                3, 3
            );
            yield return new TestCaseData(
                NewUpdateCardDto("Card4_update", "test4_update.jpg", "1234567890_4_update"),
                NewCard(4, "Card4", "test4.jpg", "1234567890_4", TimeData.FixedTime4, 4),
                NewCard(4, "Card4_update", "test4_update.jpg", "1234567890_4_update", TimeData.FixedTime4, 4),
                4, 4
            );
        }

        [Test, TestCaseSource(nameof(UpdateCardTestCases))]
        public async Task UpdateCard_ValidInput_ReturnsSuccess(UpdateCardDto updateCardDto, Card card, Card updatedCard, int cardId, int userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>())).ReturnsAsync(card);
            _cardRepository.Setup(cr => cr.UpdateCardAsync(It.IsAny<Card>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(updatedCard);

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

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>()), Times.Once);
            _cardRepository.Verify(cr => cr.UpdateCardAsync(It.Is<Card>(c =>
                c.Id == card.Id &&
                c.Name == updatedCard.Name &&
                c.Image == updatedCard.Image &&
                c.Barcode == updatedCard.Barcode &&
                c.AddedAt == card.AddedAt &&
                c.UserId == card.UserId), userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, TestCaseSource(nameof(UpdateCardTestCases))]
        public async Task UpdateCard_InvalidCardId_ReturnsFailure(UpdateCardDto updateCardDto, Card card, Card updatedCard, int cardId, int userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>())).ReturnsAsync((Card?)null);

            // Act
            Result<CardDto> result = await _cardService.UpdateCardAsync(cardId, updateCardDto, userId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("Card not found."));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>()), Times.Once);
            _cardRepository.Verify(cr => cr.UpdateCardAsync(It.IsAny<Card>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test, TestCaseSource(nameof(UpdateCardTestCases))]
        public void UpdateCard_CancellationToken_ThrowsOperationCanceledException(UpdateCardDto updateCardDto, Card card, Card updatedCard, int cardId, int userId)
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            _cardRepository
                .Setup(cr => cr.GetCardByIdAsync(cardId, userId, It.Is<CancellationToken>(ct => ct == cts.Token)))
                .ThrowsAsync(new OperationCanceledException(cts.Token));

            // Act
            Exception ex = Assert.CatchAsync(async () =>
            {
                await _cardService.UpdateCardAsync(cardId, updateCardDto, userId, cts.Token);
            });

            // Assert
            Assert.That(ex, Is.InstanceOf<OperationCanceledException>());
            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>()), Times.Once);
            _cardRepository.Verify(cr => cr.UpdateCardAsync(It.IsAny<Card>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test, TestCaseSource(nameof(UpdateCardTestCases))]
        public async Task UpdateCard_NotMatchingUserId_ReturnsFailure(UpdateCardDto updateCardDto, Card card, Card updatedCard, int cardId, int userId)
        {
            // Arrange
            int otherUserId = userId + 1;
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId, otherUserId, It.IsAny<CancellationToken>())).ReturnsAsync((Card?)null);

            // Act
            Result<CardDto> result = await _cardService.UpdateCardAsync(cardId, updateCardDto, otherUserId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("Card not found."));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId, otherUserId, It.IsAny<CancellationToken>()), Times.Once);
            _cardRepository.Verify(cr => cr.UpdateCardAsync(It.IsAny<Card>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test, TestCaseSource(nameof(UpdateCardTestCases))]
        public async Task UpdateCard_UserIdIsNull_ReturnsFailure(UpdateCardDto updateCardDto, Card card, Card updatedCard, int cardId, int userId)
        {
            // Arrange
            int? otherUserId = null;

            // Act
            Result<CardDto> result = await _cardService.UpdateCardAsync(cardId, updateCardDto, otherUserId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("User ID is required to update this card."));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId, It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _cardRepository.Verify(cr => cr.UpdateCardAsync(It.IsAny<Card>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test, TestCaseSource(nameof(UpdateCardTestCases))]
        public async Task UpdateCard_NotUpdatedInRepository_ReturnsFailure(UpdateCardDto updateCardDto, Card card, Card updatedCard, int cardId, int userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>())).ReturnsAsync(card);
            _cardRepository.Setup(cr => cr.UpdateCardAsync(It.IsAny<Card>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((Card?)null);

            // Act
            Result<CardDto> result = await _cardService.UpdateCardAsync(cardId, updateCardDto, userId);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.That(result.Error, Is.EqualTo("Card not found or update failed."));

            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>()), Times.Once);
            _cardRepository.Verify(cr => cr.UpdateCardAsync(It.IsAny<Card>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        public static IEnumerable<TestCaseData> UpdatePartialCardTestCases()
        {
            yield return new TestCaseData(
                NewUpdateCardDto(name: "Card1_update", image: "test11_update.jpg", barcode: null),
                NewCard(1, "Card1", "test.jpg", "1234567890", TimeData.FixedTime1, 1),
                NewCard(1, "Card1_update", "test11_update.jpg", "1234567890", TimeData.FixedTime1, 1),
                1, 1
            );
            yield return new TestCaseData(
                NewUpdateCardDto("Card2_update", null, "1234567890_2_update"),
                NewCard(2, "Card2", "test2.jpg", "1234567890_2", TimeData.FixedTime2, 2),
                NewCard(2, "Card2_update", "test2.jpg", "1234567890_2_update", TimeData.FixedTime2, 2),
                2, 2
            );
            yield return new TestCaseData(
                NewUpdateCardDto(null, "test33_update.jpg", "1234567890_3_update"),
                NewCard(3, "Card3", "test3.jpg", "1234567890_3", TimeData.FixedTime3, 3),
                NewCard(3, "Card3", "test33_update.jpg", "1234567890_3_update", TimeData.FixedTime3, 3),
                3, 3
            );
            yield return new TestCaseData(
                NewUpdateCardDto(null, null, null),
                NewCard(4, "Card4", "test4.jpg", "1234567890_4", TimeData.FixedTime4, 4),
                NewCard(4, "Card4", "test4.jpg", "1234567890_4", TimeData.FixedTime4, 4),
                4, 4
            );
        }

        [Test, TestCaseSource(nameof(UpdatePartialCardTestCases))]
        public async Task UpdatePartialCard_ValidInput_ReturnsSuccess(UpdateCardDto updateCardDto, Card card, Card updatedCard, int cardId, int userId)
        {
            // Arrange
            _cardRepository.Setup(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>())).ReturnsAsync(card);
            _cardRepository.Setup(cr => cr.UpdateCardAsync(It.IsAny<Card>(), userId, It.IsAny<CancellationToken>())).ReturnsAsync(updatedCard);

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


            _cardRepository.Verify(cr => cr.GetCardByIdAsync(cardId, userId, It.IsAny<CancellationToken>()), Times.Once);
            _cardRepository.Verify(cr => cr.UpdateCardAsync(
                It.Is<Card>(c =>
                    c.Id == updatedCard.Id &&
                    c.Name == updatedCard.Name &&
                    c.Image == updatedCard.Image &&
                    c.Barcode == updatedCard.Barcode &&
                    c.UserId == updatedCard.UserId &&
                    c.AddedAt == updatedCard.AddedAt),
                userId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
