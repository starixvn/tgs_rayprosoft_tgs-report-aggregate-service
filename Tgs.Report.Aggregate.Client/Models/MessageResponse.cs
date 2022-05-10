namespace Tgs.Report.Aggregate.Client.Models
{
	public record MessageResponse
	{
		public bool Valid { get; set; }

		public string Message { get; set; }

		public string[] Errors { get; set; }

		public MessageResponse()
		{
			Valid = false;
			Message = string.Empty;
			Errors = System.Array.Empty<string>();
		}
	}
}
