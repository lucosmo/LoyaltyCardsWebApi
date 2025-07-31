using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;
using LoyaltyCardsWebApi.API.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LoyaltyCardsWebApi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardsController : ControllerBase
    {
        private readonly ICardService _cardService;
        public CardsController(ICardService cardService)
        {
            _cardService = cardService;
        }

        // GET api/<CardsController>/5

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var card = await _cardService.GetCardByIdAsync(id);
            if (card.Value == null)
            {
                return NotFound(card);
            }
            return Ok(card);
        }

        // POST api/<CardsController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateCardDto newCard)
        {
            var result = await _cardService.CreateCardAsync(newCard);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        // PATCH api/<CardsController>/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateCard(int id, [FromBody] UpdateCardDto updateCard)
        {
            var result = await _cardService.UpdateCardAsync(id, updateCard);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        // DELETE api/<CardsController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCardById(int id)
        {
            var card = await _cardService.DeleteCardAsync(id);
            if (!card.Success)
            {
                return NotFound(card);
            }
            return Ok(card);
        }
    }
}
