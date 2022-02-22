using System.Text.Json;

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
		File.WriteAllText(EnsureFile(path), JsonSerializer.Serialize<T>(obj,
			new JsonSerializerOptions { WriteIndented = indent }));

	/// <summary>
	/// Deserializes an object from a json file
	/// </summary>
	/// <param name="path">The path of the json file to read the json from</param>
	/// <typeparam name="T">The type to deserialize to</typeparam>
	/// <returns>The object deserialized from the json</returns>
	public static T Deserialize<T>(string path) => JsonSerializer.Deserialize<T>(File.ReadAllText(EnsureFile(path)));


	/// <summary>
	/// Prompts the user for an input
	/// </summary>
	/// <param name="prompt">The text to prompt the user with</param>
	/// <returns> The User input </returns>
	public static string Prompt(string prompt)
	{
		Console.Write(prompt);
		return Console.ReadLine();
	}

	/// <summary>
	/// Gets the extension of a filename or path
	/// </summary>
	/// <param name="input">The input string to extract the extension from</param>
	/// <returns>The extension from the file</returns>
	public static string GetExtension(string input)
	{
		int i = input.LastIndexOf('.');
		if (i < input.Length - 1) return input.Substring(i + 1);
		return "";
	}

	/// <summary>
	/// Removes the file extension from a filename or path
	/// </summary>
	/// <param name="input">The input string to remove the extension from</param>
	/// <returns>The filename without the extension</returns>
	public static string RemoveExtension(string input)
	{
		int i = input.LastIndexOf('.');
		return input.Substring(0, i);
	}

	/// <summary>
	/// Gets the filename of a path
	/// </summary>
	/// <param name="path">The path in question</param>
	/// <returns>The name of the file in the path</returns>
	public static string GetPathFilename(string path)
	{
		int i = path.LastIndexOf(Path.DirectorySeparatorChar);
		return path.Substring(i + 1);
	}

	/// <summary>
	/// Gets the directory in a path
	/// </summary>
	/// <param name="path">The path in question</param>
	/// <returns>The directory in the path</returns>
	public static string GetPathDirectory(string path)
	{
		int i = path.LastIndexOf(Path.DirectorySeparatorChar);
		return path.Substring(0, i + 1);
	}

	/// <summary>
	/// Checks if a phone number is valid in any region
	/// </summary>
	/// <param name="phone">String containing the phone number in question</param>
	/// <returns>Whether or not the phone number is valid</returns>
	public static bool IsValidPhone(string phone)
	{
		var parser = PhoneNumbers.PhoneNumberUtil.GetInstance();
		try
		{
			var pn = parser.Parse(phone, "US");
			return parser.IsValidNumber(pn);
		}
		catch (PhoneNumbers.NumberParseException)
		{
			return false;
		}
		finally
		{
			parser = null;
		}
	}

	/// <summary>
	/// Formats a phone number to the International number format
	/// </summary>
	/// <param name="phone">The phone number in question</param>
	/// <returns>The formatted version of the number as a string</returns>
	public static string FormatPhone(string phone)
	{
		var parser = PhoneNumbers.PhoneNumberUtil.GetInstance();
		return parser.Format(parser.Parse(phone, "GB"), PhoneNumbers.PhoneNumberFormat.INTERNATIONAL);
	}

	/// <summary>
	/// Clears the temp directory of the program
	/// </summary>
	public static void ClearTemp()
	{
		if (!Directory.Exists(GlobalVars.TempDir)) return;
		foreach (FileInfo file in new DirectoryInfo(GlobalVars.TempDir).EnumerateFiles())
		{
			file.Delete();
		}
	}
}
