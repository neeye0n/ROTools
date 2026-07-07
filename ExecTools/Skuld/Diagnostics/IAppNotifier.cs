namespace Skuld.Diagnostics
{
    public interface IAppNotifier
    {
        void Info(string message);
        void Success(string message);
        void Warn(string message);
        void Error(string message);
        Task<T> StatusAsync<T>(string initialStatus, Func<IStatusContext, Task<T>> action);
    }

    public interface IStatusContext
    {
        void SetStatus(string status);
    }
}