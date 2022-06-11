using System.Text;

namespace ASP_DI;

public class DependencyService1
{
    private readonly IOperationTransient transient;
    private readonly IOperationScoped scoped;
    private readonly IOperationSingleton singleton;
    private readonly IOperationSingletonInstance singletonInstance;
    private readonly IOutputLogger _outputLogger;

    public DependencyService1(IOperationScoped operationScoped,
            IOperationSingleton operationSingleton,
            IOperationSingletonInstance operationSingletonInstance,
            IOperationTransient operationTransient,
            IOutputLogger outputLogger)
    {
        transient = operationTransient;
        scoped = operationScoped;
        singleton = operationSingleton;
        singletonInstance = operationSingletonInstance;
        _outputLogger = outputLogger;
    }

    public void Write()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"{Environment.NewLine}From Dependency Service 1");
        sb.AppendLine($"Transient → {transient.OperationId}");
        sb.AppendLine($"Scoped → {scoped.OperationId}");
        sb.AppendLine($"Singleton → {singleton.OperationId}");
        sb.AppendLine($"SingletonInstance → {singletonInstance.OperationId}");

        _outputLogger.Log(sb.ToString());
    }
}
