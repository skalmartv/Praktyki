using System;
using System.ComponentModel.DataAnnotations;
using Helpdesk.Models; 

public class Comment
{
	public int Id { get; set; }

	[Required]
	public string Content { get; set; }

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	// Relacje
	public int TicketId { get; set; }
	public Ticket Ticket { get; set; }
	public string UserId { get; set; } // Dodano pole UserId
}