﻿using System;

public class Attachment
{
	public int Id { get; set; }
	public string FileName { get; set; }
	public string FilePath { get; set; }

	public int TicketId { get; set; }
	public Ticket Ticket { get; set; }
}