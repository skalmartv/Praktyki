using Helpdesk.Data;
using Helpdesk.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;

namespace Helpdesk.Models
{
	public class Attachment
	{
		public int Id { get; set; }

		[Required]
		public string FileName { get; set; }   // Oryginalna nazwa pliku

		[Required]
		public string FilePath { get; set; }   // Ścieżka pliku na serwerze (np. /uploads/xxx.pdf)
		[ValidateNever]
		public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

		
		public int TicketId { get; set; }
		[ValidateNever]
		public Ticket Ticket { get; set; }

		[ValidateNever]
		public string UserId { get; set; }
		[ValidateNever]
		public ApplicationUser User { get; set; }
	}
}
