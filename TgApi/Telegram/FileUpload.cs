using TdLib;

namespace TgApi.Telegram;

public class FileUpload
{
	private TdApi.File latestFile;
	private EventHandler<TdApi.Update> handler;

	/// <summary>
	/// A double between 0 and 1 of the upload progress
	/// </summary>
	public double Progress => (latestFile.Remote.UploadedSize + 0.0) / latestFile.ExpectedSize;
	/// <summary>
	/// Gets the local id of the file to be uploaded
	/// </summary>
	public int LocalId => latestFile.Id;
	/// <summary>
	/// Gets the remote id of the file to be uploaded
	/// </summary>
	public string RemoteId => latestFile.Remote.Id;
	/// <summary>
	/// Whether or not the upload is complete
	/// </summary>
	public bool IsComplete => latestFile.Remote is not null && latestFile.Remote.IsUploadingCompleted;

	/// <summary>
	/// Begins an upload
	/// </summary>
	/// <param name="client">An active TdClient</param>
	/// <param name="path">The path on the local system to upload</param>
	/// <param name="priority">The upload priority from 1 to 32</param>
	/// <param name="type">The file type on telegram (not the file format or extension)</param>
	/// <returns>A FileUpload Object</returns>
	public static async Task<FileUpload> StartUpload(TdClient client, string path, int priority = 1, TdApi.FileType? type = null)
	{
		type ??= new TdApi.FileType.FileTypeDocument();

		FileUpload r = new FileUpload
		{
			latestFile = await client.UploadFileAsync(new TdApi.InputFile.InputFileLocal { Path = path }, type, priority)
		};

		r.handler = (sender, update) =>
		{
			if (update is not TdApi.Update.UpdateFile fileUpdate || fileUpdate.File.Id != r.LocalId) return;
			r.latestFile = fileUpdate.File;
			if (r.latestFile.Remote.IsUploadingCompleted) client.UpdateReceived -= r.handler;
		};

		client.UpdateReceived += r.handler;
		return r;
	}

	/// <summary>
	/// Waits for the completion of the download
	/// </summary>
	/// <param name="delay">The delay between polling to check if the upload is completed</param>
	public async Task WaitForCompletion(int delay = 25)
	{
		while (latestFile.Remote is null || !latestFile.Local.IsDownloadingCompleted) await Task.Delay(delay);
	}
}
