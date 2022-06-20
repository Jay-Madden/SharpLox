using System.Collections.Generic;

namespace SSEHub.Core.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Adds support for getting a keyvalue with an optional explicit fall back method, if the
        /// key doesnt exist and a default is not given then a default(T) is returned
        /// </summary>
        /// <param name="dict">The source collection</param>
        /// <param name="key">The collection key to get</param>
        /// <param name="defaultVal">The value to return if the key is not found in the collection</param>
        /// <typeparam name="TKey">The keys type</typeparam>
        /// <typeparam name="TVal">The mapped values type</typeparam>
        /// <returns></returns>
        public static TVal? GetOrDefault<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key, TVal? defaultVal=default) 
            where TKey: notnull
            => dict.TryGetValue(key, out var val) ? val : defaultVal;
    }
}