namespace Samples.HelloBlazorServer.Services
{
    public class PlanningPokerService : IComputeService
    {
        private volatile ImmutableList<(DateTime Time, string Name, int Estimates)> _estimates =
            ImmutableList<(DateTime, string, int)>.Empty;

        private readonly object _lock = new();

        [CommandHandler]
        public virtual Task PostEstimate(Estimate_Post command, CancellationToken cancellationToken = default)
        {
            if (Computed.IsInvalidating()) {
                _ = GetEstimateCount();
                _ = PseudoGetAnyTail();
                return Task.CompletedTask;
            }

            var (name, estimate) = command;
            lock (_lock) {
                _estimates = _estimates.Add((DateTime.Now, name, estimate));
            }
            return Task.CompletedTask;
        }

        [ComputeMethod]
        public virtual Task<int> GetEstimateCount()
            => Task.FromResult(_estimates.Count);

        [ComputeMethod]
        public virtual async Task<(DateTime Time, string Name, int Estimates)[]> GetEstimates(
            int count, CancellationToken cancellationToken = default)
        {
            // Fake dependency used to invalidate all GetMessages(...) independently on count argument
            await PseudoGetAnyTail();
            return _estimates.TakeLast(count).ToArray();
        }

        [ComputeMethod]
        protected virtual Task<Unit> PseudoGetAnyTail() => TaskExt.UnitTask;
    }

    // ReSharper disable once InconsistentNaming
    public record Estimate_Post(string Name, int Estimate) : ICommand<Unit>;
}
