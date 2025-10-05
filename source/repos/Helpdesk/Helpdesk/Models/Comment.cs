using System;
using System.ComponentModel.DataAnnotations;
using Helpdesk.Models;
using Helpdesk.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

public class Comment
{
	public int Id { get; set; }

	[Required]
	public string Content { get; set; }

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	// Relacje
	public int TicketId { get; set; }
	public Ticket Ticket { get; set; }

	public string UserId { get; set; }

	[ValidateNever] // NOWE: nawigacja do użytkownika
	public ApplicationUser? User { get; set; }
}