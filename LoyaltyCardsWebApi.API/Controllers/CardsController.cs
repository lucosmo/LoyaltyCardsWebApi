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

        /*
        // GET: api/<CardsController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
        */

        // GET api/<CardsController>/5

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var card = await _cardService.GetCardByIdAsync(id);
            if (card == null)
            {
                return NotFound();
            }
            return Ok(card);
        }

        // POST api/<CardsController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateCardDto newCard)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Wrong details");
            }
            var result = await _cardService.CreateCardAsync(newCard);
            if (!result.Success)
            {
                return BadRequest(result.Value);
            }
            return Ok(result.Value);
        }

        // PATCH api/<CardsController>/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateCard(int id, [FromBody] UpdatedCardDto updatedCard)
        {
            var isCardUpdated = await _cardService.UpdateCardAsync(id, updatedCard);
            if (!isCardUpdated.Success)
            {
                return NotFound();
            }
            return Ok(updatedCard);
        }

        // DELETE api/<CardsController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCardById(int id)
        {
            var card = await _cardService.DeleteCardAsync(id);
            if (!card.Success)
            {
                return NotFound();
            }
            return Ok(card);
        }
    }
}
