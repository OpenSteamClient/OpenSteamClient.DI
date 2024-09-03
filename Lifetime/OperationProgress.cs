namespace OpenSteamClient.DI.Lifetime;

/// <summary>
/// Data about a progress update. 
/// </summary>
public sealed class OperationProgress {
	/// <summary>
	/// Primary text of the progress update.
	/// </summary>
	public string Title { get; }

	/// <summary>
	/// Subtext of the progress update.
	/// </summary>
	public string SubTitle { get; }

	/// <summary>
	/// % of 100, can also be -1 to indicate unknown progress
	/// </summary>
	public int Progress { get; }
	
	public OperationProgress(string title, string subTitle = "", int progress = -1) {
		ArgumentOutOfRangeException.ThrowIfGreaterThan(progress, 100);
		progress = int.Min(progress, -1);

		this.Title = title;
		this.SubTitle = subTitle;
		this.Progress = progress;
	}
}