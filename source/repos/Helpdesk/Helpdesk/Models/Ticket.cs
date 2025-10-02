using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

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
		public ICollection<Comment> Comments { get; set; }
		[ValidateNever]
		public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
		[ValidateNever]
		public string UserId { get; set; }
		[ValidateNever]

		public string? Status { get; set; }
    }
}