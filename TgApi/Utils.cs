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
		if (!File.Exists(path)) File.Create(path).Close();
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
	public static void Serialize<T>(T obj, string path, bool indent = true) => 
		File.WriteAllText(EnsureFile(path),JsonSerializer.Serialize<T>(obj, 
			new JsonSerializerOptions{WriteIndented = indent}));

	/// <summary>
	/// Deserializes an object from a json file
	/// </summary>
	/// <param name="path">The path of the json file to read the json from</param>
	/// <typeparam name="T">The type to deserialize to</typeparam>
	/// <returns>The object deserialized from the json</returns>
	public static T Deserialize<T>(string path) => JsonSerializer.Deserialize<T>(File.ReadAllText(EnsureFile(path)));
	
	
	// TODO XML Docs
	public static string Prompt(string prompt)
	{
		Console.Write(prompt);
		return Console.ReadLine();
	}

	public static string GetPathFilename(string path)
	{
		int i = path.LastIndexOf(Path.DirectorySeparatorChar);
		return path.Substring(i + 1);
	}

	public static string GetPathDirectory(string path)
	{
		int i = path.LastIndexOf(Path.DirectorySeparatorChar);
		return path.Substring(0, i + 1);
	}
}
