using System;
using System.IO;

using Tommy;



namespace CutsceneMaker;

public class Settings
{
	private readonly string FILE_PATH = "./settings.toml";

	public string? VanillaGamePath { get; set; }

	private TomlTable? table;



	public Settings()
	{
		if (!File.Exists(FILE_PATH))
		{
			FileStream strm = File.Create(FILE_PATH);
			strm.Close();
		}

		using(StreamReader settFile = File.OpenText(FILE_PATH))
		{
            table = TOML.Parse(settFile);

			VanillaGamePath = table["vanilla_game_path"].HasValue ? table["vanilla_game_path"].AsString.Value : null;
			if (VanillaGamePath != null)
				Program.AutoCompletion.VanillaGamePath = VanillaGamePath;
		}
	}

	public void saveSettings()
	{
		table!["vanilla_game_path"] = VanillaGamePath;
		using(StreamWriter settFile = File.CreateText(FILE_PATH))
		{
			table.WriteTo(settFile);
		}
    }
}
