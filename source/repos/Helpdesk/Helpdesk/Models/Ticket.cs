using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

public class Ticket
{
	public int Id { get; set; }

	[Required, StringLength(200)]
	public string Title { get; set; }

	[Required]
	public string Description { get; set; }

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public ICollection<Comment> Comments { get; set; }
	public ICollection<Attachment> Attachments { get; set; }
}