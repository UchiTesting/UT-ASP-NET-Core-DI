namespace ASP_DI
{
    public interface IOperation
    {
        Guid OperationId { get; }
    }

    public interface IOperationTransient : IOperation { }
    public interface IOperationScoped : IOperation { }
    public interface IOperationSingleton : IOperation { }
    public interface IOperationSingletonInstance : IOperation { }

    public class Operation : IOperationScoped, IOperationSingleton, IOperationSingletonInstance, IOperationTransient
    {
        public Operation() : this(Guid.NewGuid()) { }

        public Operation(Guid id)
        {
            OperationId = id;
        }

        public Guid OperationId { get; private set; }
    }
}
