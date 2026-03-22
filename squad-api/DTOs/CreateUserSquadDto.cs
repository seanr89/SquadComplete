using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace squad_api.DTOs;

public class CreateUserSquadDto
{
    [Required]
    public string BrowserIdentifierId { get; set; } = string.Empty;

    public string? UserName { get; set; }

    [Required]
    public int GameRecordId { get; set; }

    [Required]
    public int FormationId { get; set; }

    [Required]
    [MinLength(11, ErrorMessage = "A squad must contain exactly 11 players.")]
    [MaxLength(11, ErrorMessage = "A squad must contain exactly 11 players.")]
    public List<CreateUserSquadPlayerDto> Players { get; set; } = new();
}

public class CreateUserSquadPlayerDto
{
    [Required]
    public int PlayerId { get; set; }

    public string? Position { get; set; }

    public bool IsCaptain { get; set; }

    public bool IsViceCaptain { get; set; }
}
