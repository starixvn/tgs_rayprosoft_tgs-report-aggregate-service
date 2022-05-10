namespace Tgs.Report.Aggregate.Client.Models
{
	public record ManualFillPlayerBucketMinutelyModel(string Date, int Hour, int FromMinute, int ToMinute,
		bool FillPlayerBucket);
}
