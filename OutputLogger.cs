namespace ASP_DI;
public class OutputLogger : IOutputLogger
{
    public void Log(string message)
    {
#if DEBUG
        System.Diagnostics.Debug.WriteLine(message);
#endif
#if RELEASE
        System.Diagnostics.Trace.WriteLine(message);
#endif
    }
}

public interface IOutputLogger
{
    void Log(string message);
}
