using Spectre.Console;

namespace Skuld.Diagnostics
{
    public class SpectreAppNotifier : IAppNotifier
    {
        public void Info(string message) => AnsiConsole.MarkupLine($"ℹ️ [blue]{Escape(message)}[/]");
        public void Success(string message) => AnsiConsole.MarkupLine($"✅ [cyan]{Escape(message)}[/]");
        public void Warn(string message) => AnsiConsole.MarkupLine($"⚠️ [yellow]{Escape(message)}[/]");
        public void Error(string message) => AnsiConsole.MarkupLine($"❌ [red]{Escape(message)}[/]");

        public async Task<T> StatusAsync<T>(string initialStatus, Func<IStatusContext, Task<T>> action)
        {
            T result = default!;
            await AnsiConsole.Status().Spinner(Spinner.Known.Dots).StartAsync(initialStatus, async ctx =>
            {
                result = await action(new SpectreStatusContext(ctx));
            });
            return result;
        }

        private static string Escape(string s) => s.Replace("[", "[[").Replace("]", "]]");

        private class SpectreStatusContext(StatusContext ctx) : IStatusContext
        {
            public void SetStatus(string status) => ctx.Status(status);
        }
    }
}