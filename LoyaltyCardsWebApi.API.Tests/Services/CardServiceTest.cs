using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;
using LoyaltyCardsWebApi.API.Repositories;
using LoyaltyCardsWebApi.API.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoyaltyCardsWebApi.API.Tests.Services
{
    [TestFixture]
    public class CardServiceTest
    {
        private Mock<ICardRepository> _cardRepository;
        private Mock<IUserService> _userService;
        private Mock<IDateTimeProvider> _dateTimeProvider;
        private CardService _cardService;
        private int _idCounter = 1;

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

            var accountCreatedDate1 = new DateTime(2022, 10, 1, 12, 0, 0, DateTimeKind.Utc);
            var accountCreatedDate2 = new DateTime(2023, 11, 4, 11, 15, 0, DateTimeKind.Utc);
            var accountCreatedDate3 = new DateTime(2024, 1, 7, 14, 25, 12, DateTimeKind.Utc);
            var accountCreatedDate4 = new DateTime(2024, 2, 2, 22, 12, 23, DateTimeKind.Utc);

            yield return new TestCaseData(
                new CreateCardDto 
                { 
                    Name = "Card1",
                    Image = "test.jpg",
                    Barcode = "1234567890",
                    UserId = 1
                },
                new User 
                {
                    Id = 1,
                    UserName = "User1",
                    Password = "password1",
                    Email = "User1@user.lpl",
                    FirstName = null,
                    LastName = null,
                    AccountCreatedDate = accountCreatedDate1,
                    Cards = null,
                    Settings = null
                },
                new UserDto
                {
                    Id = 1,
                    UserName = "User1",
                    Email = "User1@user.lpl"
                },
                new Card
                {
                    Name = "Card1",
                    Image = "test.jpg",
                    Barcode = "1234567890",
                    AddedAt = fixedTime1,
                    UserId = 1
                },
                new CardDto
                {
                    Id = 0,
                    Name = "Card1",
                    Image = "test.jpg",
                    Barcode = "1234567890",
                    AddedAt = fixedTime1,
                    UserId = 1
                }
            ).SetName("CreateCard_Success_1");

            yield return new TestCaseData(
                new CreateCardDto
                {
                    Name = "Card2",
                    Image = "test2.jpg",
                    Barcode = "1234567890_2",
                    UserId = 2
                },
                new User
                {
                    Id = 2,
                    UserName = "User2",
                    Password = "password2",
                    Email = "User2@user.lpl",
                    FirstName = null,
                    LastName = null,
                    AccountCreatedDate = accountCreatedDate2,
                    Cards = null,
                    Settings = null
                },
                new UserDto
                {
                    Id = 2,
                    UserName = "User2",
                    Email = "User2@user.lpl"
                },
                new Card
                {
                    Name = "Card2",
                    Image = "test2.jpg",
                    Barcode = "1234567890_2",
                    AddedAt = fixedTime2,
                    UserId = 2
                },
                new CardDto
                {
                    Id = 0,
                    Name = "Card2",
                    Image = "test2.jpg",
                    Barcode = "1234567890_2",
                    AddedAt = fixedTime2,
                    UserId = 2
                }
            ).SetName("CreateCard_Success_2");

            yield return new TestCaseData(
                new CreateCardDto
                {
                    Name = "Card3",
                    Image = "test3.jpg",
                    Barcode = "1234567890_3",
                    UserId = 3
                },
                new User
                {
                    Id = 3,
                    UserName = "User3",
                    Password = "password3",
                    Email = "User3@user.lpl",
                    FirstName = null,
                    LastName = null,
                    AccountCreatedDate = accountCreatedDate3,
                    Cards = null,
                    Settings = null
                },
                new UserDto
                {
                    Id = 3,
                    UserName = "User3",
                    Email = "User3@user.lpl"
                },
                new Card
                {
                    Name = "Card3",
                    Image = "test3.jpg",
                    Barcode = "1234567890_3",
                    AddedAt = fixedTime3,
                    UserId = 3
                },
                new CardDto
                {
                    Id = 0,
                    Name = "Card3",
                    Image = "test3.jpg",
                    Barcode = "1234567890_3",
                    AddedAt = fixedTime3,
                    UserId = 3
                }
            ).SetName("CreateCard_Success_3");

            yield return new TestCaseData(
                new CreateCardDto
                {
                    Name = "Card4",
                    Image = "test4.jpg",
                    Barcode = "1234567890_4",
                    UserId = 4
                },
                new User
                {
                    Id = 4,
                    UserName = "User4",
                    Password = "password4",
                    Email = "User4@user.lpl",
                    FirstName = null,
                    LastName = null,
                    AccountCreatedDate = accountCreatedDate4,
                    Cards = null,
                    Settings = null
                },
                new UserDto
                {
                    Id = 4,
                    UserName = "User4",
                    Email = "User4@user.lpl"
                },
                new Card
                {
                    Name = "Card4",
                    Image = "test4.jpg",
                    Barcode = "1234567890_4",
                    AddedAt = fixedTime4,
                    UserId = 4
                },
                new CardDto
                {
                    Id = 0,
                    Name = "Card4",
                    Image = "test4.jpg",
                    Barcode = "1234567890_4",
                    AddedAt = fixedTime4,
                    UserId = 4
                }
            ).SetName("CreateCard_Success_4");
        }

        [Test, TestCaseSource(nameof(CreateCardTestCases))]
        public async Task CreateCard_UserFound_ReturnSuccsess(CreateCardDto createCardDto, User user, UserDto userDto, Card card, CardDto cardDto)
        {
            // Arrange
            _userService.Setup(us => us.GetCurrentUserAsync()).ReturnsAsync(Result<UserDto>.Ok(userDto));
            _cardRepository.Setup(cr => cr.CreateCardAsync(It.IsAny<Card>())).Returns((Card card) => Task.FromResult<Card?>(card));
            _dateTimeProvider.Setup(dp => dp.UtcNow).Returns(card.AddedAt);

            // Act
            Result<CardDto> result = await _cardService.CreateCardAsync(createCardDto);
            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Value);
            Assert.That(result.Value.Id, Is.EqualTo(cardDto.Id));
            Assert.That(result.Value.Name, Is.EqualTo(cardDto.Name));
            Assert.That(result.Value.Image, Is.EqualTo(cardDto.Image));
            Assert.That(result.Value.Barcode, Is.EqualTo(cardDto.Barcode));
            Assert.That(result.Value.AddedAt, Is.EqualTo(cardDto.AddedAt));

        }

    }
}
