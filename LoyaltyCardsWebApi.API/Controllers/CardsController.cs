using Microsoft.AspNetCore.Mvc;
using LoyalityCardsWebApi.API.Services;
using LoyalityCardsWebApi.API.Data.DTOs;

namespace LoyalityCardsWebApi.API.Controllers;

[Route("api/[controller]")]
public class CardsController : ControllerBase
{
    private readonly ICardService _cardService;

    public CardsController(ICardService cardService)
    {
        _cardService = cardService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCaard([FromBody] CreateCardDto newCard)
    {
        var createdCard = await _cardService.CreateCardAsync(newCard);
        return Ok(createdCard);
    }
}

