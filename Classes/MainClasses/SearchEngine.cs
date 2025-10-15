using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppLauncher.Classes.MainClasses
{
    public class SearchEngine
    {
        public int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s)) return t.Length;
            if (string.IsNullOrEmpty(t)) return s.Length;

            int[,] dp = new int[s.Length + 1, t.Length + 1];
            for (int i = 0; i <= s.Length; i++) dp[i, 0] = i;
            for (int j = 0; j <= t.Length; j++) dp[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                    dp[i, j] = Math.Min(
                        Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + cost);
                }
            }
            return dp[s.Length, t.Length];
        }

        // Damerau–Levenshtein Distance (simple version)
        public int DamerauLevenshtein(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] dp = new int[n + 1, m + 1];

            for (int i = 0; i <= n; i++) dp[i, 0] = i;
            for (int j = 0; j <= m; j++) dp[0, j] = j;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                    dp[i, j] = Math.Min(
                        Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + cost);

                    if (i > 1 && j > 1 && s[i - 1] == t[j - 2] && s[i - 2] == t[j - 1])
                    {
                        dp[i, j] = Math.Min(dp[i, j], dp[i - 2, j - 2] + cost);
                    }
                }
            }
            return dp[n, m];
        }

        // Jaro-Winkler Similarity
        public double JaroWinkler(string s1, string s2)
        {
            if (s1 == s2) return 1.0;

            int len1 = s1.Length;
            int len2 = s2.Length;

            int matchDistance = Math.Max(len1, len2) / 2 - 1;

            bool[] s1Matches = new bool[len1];
            bool[] s2Matches = new bool[len2];

            int matches = 0;
            for (int i = 0; i < len1; i++)
            {
                int start = Math.Max(0, i - matchDistance);
                int end = Math.Min(i + matchDistance + 1, len2);

                for (int j = start; j < end; j++)
                {
                    if (s2Matches[j]) continue;
                    if (s1[i] != s2[j]) continue;
                    s1Matches[i] = true;
                    s2Matches[j] = true;
                    matches++;
                    break;
                }
            }

            if (matches == 0) return 0.0;

            double t = 0;
            int k = 0;
            for (int i = 0; i < len1; i++)
            {
                if (!s1Matches[i]) continue;
                while (!s2Matches[k]) k++;
                if (s1[i] != s2[k]) t++;
                k++;
            }
            t /= 2.0;

            double m = matches;
            double jaro = ((m / len1) + (m / len2) + ((m - t) / m)) / 3.0;

            // Winkler boost
            int prefix = 0;
            for (int i = 0; i < Math.Min(4, Math.Min(len1, len2)); i++)
            {
                if (s1[i] == s2[i]) prefix++;
                else break;
            }

            return jaro + 0.1 * prefix * (1 - jaro);
        }

        // N-Gram Similarity (Trigram)
        public double NGramSimilarity(string s1, string s2, int n = 3)
        {
            var ngrams1 = Enumerable.Range(0, Math.Max(0, s1.Length - n + 1))
                                    .Select(i => s1.Substring(i, n)).ToList();
            var ngrams2 = Enumerable.Range(0, Math.Max(0, s2.Length - n + 1))
                                    .Select(i => s2.Substring(i, n)).ToList();

            if (ngrams1.Count == 0 || ngrams2.Count == 0) return 0.0;

            int intersect = ngrams1.Intersect(ngrams2).Count();
            int union = ngrams1.Count + ngrams2.Count - intersect;

            return (double)intersect / union;
        }

        // Combine points
        public double CombinedSimilarity(string query, string target)
        {
            query = query.ToLower();
            target = target.ToLower();

            if (target.StartsWith(query)) return 1.0;
            if (target.Contains(query)) return 0.95;

            double lev = 1.0 - (double)LevenshteinDistance(query, target) / Math.Max(query.Length, target.Length);
            double dLev = 1.0 - (double)DamerauLevenshtein(query, target) / Math.Max(query.Length, target.Length);
            double jw = JaroWinkler(query, target);
            double ngram = NGramSimilarity(query, target);

            // Weighting the algorithms
            return (lev * 0.25) + (dLev * 0.25) + (jw * 0.3) + (ngram * 0.2);
        }

        // Calculate the final score for an application
        public double ComputeScore(string query, string appName)
        {
            var qTokens = query.Split(' ', (char)StringSplitOptions.RemoveEmptyEntries);
            var aTokens = appName.Split(' ', (char)StringSplitOptions.RemoveEmptyEntries);

            double score = 0;

            foreach (var qt in qTokens)
            {
                double tokenScore = 0;

                // acronym
                string acronym = string.Concat(aTokens.Select(t => t[0])).ToLower();
                if (qt.Length <= acronym.Length && acronym.StartsWith(qt.ToLower()))
                    tokenScore = 0.9;

                // Compare with each token
                for (int i = 0; i < aTokens.Length; i++)
                {
                    double s = CombinedSimilarity(qt, aTokens[i]);
                    double weight = i == 0 ? 1.1 : 1.0; // boost for the first token
                    tokenScore = Math.Max(tokenScore, s * weight);
                }

                score += tokenScore;
            }

            return score / qTokens.Length;
        }
    }
}
