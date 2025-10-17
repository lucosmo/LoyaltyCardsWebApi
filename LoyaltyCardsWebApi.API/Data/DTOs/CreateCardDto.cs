﻿using System.ComponentModel.DataAnnotations;
using LoyaltyCardsWebApi.API.Attributes;

namespace LoyaltyCardsWebApi.API.Data.DTOs;
public class CreateCardDto
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    [ValidateNotEmptyIfProvided]
    public required string Name { get; set; }
    [Url(ErrorMessage = "Image must be a valid URL.")]
    [ValidateNotEmptyIfProvided]
    public string Image { get; set; } = "https://..default-image.png"; // Default image URL
    [ValidateNotEmptyIfProvided]
    public required string Barcode { get; set; }
    public int UserId { get; set; }
}