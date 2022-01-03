using System.Text.Json;
using TdLib;

namespace TgApi;


/// <summary>
/// A static class containing miscellaneous utility methods
/// </summary>
public static class Utils
{

	/// <summary>
	/// Ensures that a file exists when accessing
	/// </summary>
	/// <param name="path">The string path of the file on your system</param>
	/// <returns>The path that the user input</returns>
	public static string EnsureFile(string path)
	{
		if (!File.Exists(path)) File.Create(path);
		return path;
	}

	/// <summary>
	/// Ensures that a path exists when accessing 
	/// </summary>
	/// <param name="path">The string path of the directory on your system</param>
	/// <returns>The path that the user input</returns>
	public static string EnsurePath(string path)
	{
		if (!Directory.Exists(path)) Directory.CreateDirectory(path);
		return path;
	}

	/// <summary>
	/// Serializes an object to a json file
	/// </summary>
	/// <param name="obj">The object to serialize</param>
	/// <param name="path">The path to save the json file to</param>
	/// <typeparam name="T">The type of the object to serialize</typeparam>
	public static void Serialize<T>(T obj, string path) => 
		File.WriteAllText(EnsureFile(path),JsonSerializer.Serialize<T>(obj));

	/// <summary>
	/// Deserializes an object from a json file
	/// </summary>
	/// <param name="path">The path of the json file to read the json from</param>
	/// <typeparam name="T">The type to deserialize to</typeparam>
	/// <returns>The object deserialized from the json</returns>
	public static T Deserialize<T>(string path) => JsonSerializer.Deserialize<T>(path);

	/// <summary>
	/// Checks if an object is a TdLib.TdApi.Ok object, which is returned upon completion of a successful request
	/// </summary>
	/// <param name="obj">The object to check</param>
	/// <returns>Whether the object is of type TdLib.TdApi.Ok</returns>
	public static bool IsOkTd(Object? obj) => obj != null && obj.GetType() == typeof(TdApi.Ok);
}
