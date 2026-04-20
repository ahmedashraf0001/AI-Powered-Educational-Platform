namespace AIEduPlatform.Application.Common.Services
{
    internal static class RecommendationRandomUtils
    {
        public static List<T> ShuffleDeterministically<T>(IEnumerable<T> source, Guid userId, int salt)
        {
            var list = source.ToList();
            if (list.Count <= 1)
                return list;

            var rng = new Random(CreateDeterministicSeed(userId, salt));

            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }

            return list;
        }

        /// <summary>
        /// Interleaves two lists element-by-element, taking one item from each in turn until both
        /// are exhausted.
        /// <para>
        /// Fix #11: Empty GUIDs are filtered out here rather than requiring every call site to add
        /// its own <c>.Where(id => id != Guid.Empty)</c> guard. Callers that previously added such a
        /// guard can safely remove it.
        /// </para>
        /// </summary>
        public static IEnumerable<Guid> InterleaveLists(IReadOnlyList<Guid> first, IReadOnlyList<Guid> second)
        {
            var maxCount = Math.Max(first.Count, second.Count);

            for (var i = 0; i < maxCount; i++)
            {
                if (i < first.Count && first[i] != Guid.Empty) yield return first[i];
                if (i < second.Count && second[i] != Guid.Empty) yield return second[i];
            }
        }

        /// <summary>
        /// Creates a deterministic seed that is stable for a given user within a calendar day (UTC)
        /// and changes each midnight.
        /// <para>
        /// Fix #10: The daily rotation is intentional — it provides a lightweight freshness mechanism
        /// so that the same user sees a modestly different ordering each day without requiring an
        /// explicit randomisation pass. A consequence is that a user hitting the API at 23:59:59 UTC
        /// will receive a different ordering from a request made one second later at 00:00:01 UTC, so
        /// client-side caches should not be held across UTC midnight if freshness matters.
        /// </para>
        /// </summary>
        private static int CreateDeterministicSeed(Guid userId, int salt)
        {
            var bytes = userId.ToByteArray();
            var utcDate = DateTime.UtcNow.Date;
            var daySeed = (utcDate.Year * 1000) + utcDate.DayOfYear;

            return
                BitConverter.ToInt32(bytes, 0) ^
                BitConverter.ToInt32(bytes, 4) ^
                BitConverter.ToInt32(bytes, 8) ^
                BitConverter.ToInt32(bytes, 12) ^
                salt ^
                daySeed;
        }
    }
}