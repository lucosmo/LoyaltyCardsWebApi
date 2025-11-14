using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Controllers.Results;
using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            var cardResult = await _cardService.GetCardByIdAsync(id, userId, cancellationToken);
            return new ApiResult<CardDto>(cardResult);
        }

        // POST api/<CardsController>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateCardDto newCard, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            var cardResult = await _cardService.CreateCardAsync(newCard, userId, cancellationToken);
            var location = Url.Action(nameof(Get), new { id = cardResult.Value?.Id });

            if (cardResult.Success && cardResult.Value is not null && location is not null)
            {
                return new ApiResult<CardDto>(Result<CardDto>.Created(cardResult.Value, location));    
            }
            return new ApiResult<CardDto>(cardResult);
        }

        // PATCH api/<CardsController>/5
        [Authorize]
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateCard(int id, [FromBody] UpdateCardDto updateCard, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            var cardResult = await _cardService.UpdateCardAsync(id, updateCard, userId, cancellationToken);
            return new ApiResult<CardDto>(cardResult);
        }

        // DELETE api/<CardsController>/5
        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteCardById(int id, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            var cardResult = await _cardService.DeleteCardAsync(id, userId, cancellationToken);
            return new ApiResult<CardDto>(cardResult);
        }
    }
}
