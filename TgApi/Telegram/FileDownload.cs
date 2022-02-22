using TdLib;

namespace TgApi.Types;

public class FileDownload
{
	private TdApi.Client client;
	private TdApi.File latestFile;
	private EventHandler<TdApi.Update> handler;

	/// <summary>
	/// A double between 0 and 1 of the download progress
	/// </summary>
	public double Progress => (latestFile.Local.DownloadedSize + 0.0) / latestFile.ExpectedSize;
	/// <summary>
	/// Gets the local id of the file to be downloaded
	/// </summary>
	public int LocalId => latestFile.Id;
	/// <summary>
	/// Gets the remote id of the file to be downloaded
	/// </summary>
	public string RemoteId => latestFile.Remote.Id;
	/// <summary>
	/// Whether or not the download is complete
	/// </summary>
	public bool IsComplete => latestFile.Local is not null && latestFile.Local.IsDownloadingCompleted;
	/// <summary>
	/// Gets the local path of the download
	/// </summary>
	public string LocalPath => latestFile.Local.Path;

	/// <summary>
	/// Begins a download
	/// </summary>
	/// <param name="client">An active TdClient</param>
	/// <param name="id">The local id of the file to download</param>
	/// <param name="priority">The priority from 1 to 32</param>
	/// <returns>A FileDownload Object</returns>
	public static async Task<FileDownload> StartDownload(TdClient client, int id, int priority = 1)
	{
		FileDownload r = new FileDownload();
		r.client = client;

		r.latestFile = await client.DownloadFileAsync(fileId: id, priority: priority);

		r.handler = (object? sender, TdApi.Update update) =>
		{
			if (update is TdApi.Update.UpdateFile fileUpdate && fileUpdate.File.Id == r.LocalId)
			{
				r.latestFile = fileUpdate.File;
				if (r.latestFile.Local.IsDownloadingCompleted) client.UpdateReceived -= r.handler;
				r.client = null;
				r.handler = null;
			}
		};

		client.UpdateReceived += r.handler;
		return r;
	}

	/// <summary>
	/// Waits for the completion of the download
	/// </summary>
	/// <param name="delay">The delay between polling to check if the upload is completed</param>
	public async Task<FileDownload> WaitForCompletion(int delay = 25)
	{
		while (latestFile.Local is null || !latestFile.Local.IsDownloadingCompleted)
		{
			await Task.Delay(delay);
		}
		return this;
	}

}
