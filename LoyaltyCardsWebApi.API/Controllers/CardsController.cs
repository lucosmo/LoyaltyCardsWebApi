using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Extensions;
using LoyaltyCardsWebApi.API.Models;
using LoyaltyCardsWebApi.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LoyaltyCardsWebApi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardsController : ControllerBase
    {
        private readonly ICardService _cardService;
        private readonly ICurrentUserService _currentUserService;
        public CardsController(ICardService cardService, ICurrentUserService currentUserService)
        {
            _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        // GET api/<CardsController>/5
        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Get(int id)
        {
            var userId = _currentUserService.UserId;
            if (userId is null)
            {
                return Unauthorized("User ID not found.");
            }
            var cardResult = await _cardService.GetCardByIdAsync(id, userId);
            if (!cardResult.Success)
            {
                return NotFound(cardResult.Error);
            }
            return Ok(cardResult.Value);
        }

        // POST api/<CardsController>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateCardDto newCard)
        {
            var userId = _currentUserService.UserId;
            if (userId is null)
            {
                return Unauthorized("No permission to perform action.");
            }
            var cardResult = await _cardService.CreateCardAsync(newCard, userId);
            if (!cardResult.Success)
            {
                return BadRequest(cardResult.Error);
            }
            return CreatedAtAction(nameof(Get), new { id = cardResult.Value?.Id }, cardResult.Value);
        }

        // PATCH api/<CardsController>/5
        [Authorize]
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateCard(int id, [FromBody] UpdateCardDto updateCard)
        {
            var userId = _currentUserService.UserId;
            if (userId is null)
            {
                return Unauthorized("No permission to perform action.");
            }
            var cardResult = await _cardService.UpdateCardAsync(id, updateCard, userId);
            if (!cardResult.Success)
            {
                return NotFound(cardResult.Error);
            }
            return Ok(cardResult.Value);
        }

        // DELETE api/<CardsController>/5
        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteCardById(int id)
        {
            var userId = _currentUserService.UserId;
            if (userId is null)
            {
                return Unauthorized("No permission to perform action.");
            }

            var cardResult = await _cardService.DeleteCardAsync(id, userId);
            if (!cardResult.Success)
            {
                return NotFound(cardResult.Error);
            }
            return Ok(cardResult.Value);
        }
    }
}
