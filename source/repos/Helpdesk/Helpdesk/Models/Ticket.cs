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
		[Display(Name = "Tytuł")]
		public string Title { get; set; }

		[Required]
		[Display(Name = "Opis")]
		public string Description { get; set; }

		[ValidateNever]
		[Display(Name = "Data utworzenia")]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		[ValidateNever]
		public ICollection<Comment> Comments { get; set; } = new List<Comment>();

		[ValidateNever]
		public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

		[ValidateNever]
		public string UserId { get; set; }

		[ValidateNever]
		public ApplicationUser? CreatedBy { get; set; }

		[Display(Name = "Status")]
		public string? Status { get; set; } = "Nowy";

		[StringLength(20)]
		[Display(Name = "Priorytet")]
		public string Priority { get; set; } = "Normalny";

		public string? AssignedToId { get; set; }
		[ValidateNever]
		[Display(Name = "Przypisany do")]
		public ApplicationUser? AssignedTo { get; set; }

		[Display(Name = "Data oznaczenia jako Rozwiązane")]
		public DateTime? ResolvedAt { get; set; }

		[Display(Name = "Zamknięte automatycznie przez system")]
		public bool SystemClosed { get; set; }

		[Display(Name = "Data automatycznego zamknięcia")]
		public DateTime? SystemClosedAt { get; set; }
	}
}