using System;



namespace Abacus;

public class CutsceneArchiveReadWrapper
{

	public bool IsError { get; private set; } = false;
	public string? ErrorMessage { get; private set; } = null;
	public CutsceneArchive? Archive { get; private set; } = null;


	private CutsceneArchiveReadWrapper(bool isError, string? errorMessage, CutsceneArchive? archive)
	{
		IsError = isError;
		ErrorMessage = errorMessage;
		Archive = archive;
	}


	public static CutsceneArchiveReadWrapper Error(string message)
	{
		return new CutsceneArchiveReadWrapper(true, message, null);
	}

	public static CutsceneArchiveReadWrapper Ok(CutsceneArchive archive)
	{
		return new CutsceneArchiveReadWrapper(false, null, archive);
	}


	public string GetErrorMessage()
	{
		return ErrorMessage ?? "";
	}

	public CutsceneArchive GetResult()
	{
		if (Archive == null) throw new Exception("Cannot get archive.");
		return Archive;
	}
}
