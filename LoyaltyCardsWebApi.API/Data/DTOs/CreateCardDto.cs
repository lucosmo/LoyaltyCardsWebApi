using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace LoyalityCardsWebApi.API.Data.DTOs;
public class CreateCardDto
{
    public required string Name { get; set; }
    public string? Image { get; set;}
    public required string Barcode { get; set;}
}