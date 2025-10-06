using Helpdesk.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Helpdesk.Models
{
	public class Ticket
	{
		public int Id { get; set; }

		[Required, StringLength(200)]
		public string Title { get; set; }

		[Required]
		public string Description { get; set; }

		[ValidateNever]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		[ValidateNever]
		public ICollection<Comment> Comments { get; set; } = new List<Comment>();

		[ValidateNever]
		public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

		[ValidateNever]
		public string UserId { get; set; }

		[ValidateNever] // nawigacja do autora
		public ApplicationUser? CreatedBy { get; set; }

		public string? Status { get; set; } = "Nowy";

		// NOWE: Priorytet
		[StringLength(20)]
		public string Priority { get; set; } = "Normalny"; // wartości: Niski, Normalny, Wysoki, Krytyczny

		public string? AssignedToId { get; set; }
		[ValidateNever]
		public ApplicationUser? AssignedTo { get; set; }
	}
}