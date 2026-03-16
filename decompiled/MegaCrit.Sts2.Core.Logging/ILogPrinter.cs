namespace MegaCrit.Sts2.Core.Logging;

public interface ILogPrinter
{
	void Print(LogLevel logLevel, string text, int skipFrames);
}
