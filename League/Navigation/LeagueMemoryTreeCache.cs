﻿using System;
using System.Threading.Tasks;
using cloudscribe.Web.Navigation;
using cloudscribe.Web.Navigation.Caching;
using League.DI;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace League.Navigation
{
    public class LeagueMemoryTreeCache : ITreeCache
    {
        private IMemoryCache _cache;
        private TreeCacheOptions _options;
        private readonly SiteContext _siteContext;

        public LeagueMemoryTreeCache(IMemoryCache cache, SiteContext siteContext, IOptions<TreeCacheOptions> optionsAccessor = null)
        {
            _siteContext = siteContext;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _options = optionsAccessor?.Value;
            if (_options != null)
                return;
            _options = new TreeCacheOptions(); // TreeCacheOptions contains property CacheDurationInSeconds (defaults to 300)
        }

        /// <summary>
        /// Gets the whole navigation tree from cache. Cache is stored per culture.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public Task<TreeNode<NavigationNode>> GetTree(string cacheKey)
        {
            return Task.FromResult<TreeNode<NavigationNode>>((TreeNode<NavigationNode>)_cache.Get((object) string.Join('_', System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName, _siteContext.OrganizationKey, cacheKey)));
        }

        /// <summary>
        /// Adds the whole navigation tree to cache. Cache is stored per culture.
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="cacheKey"></param>
        public Task AddToCache(TreeNode<NavigationNode> tree, string cacheKey)
        {
            _cache.Set<TreeNode<NavigationNode>>((object)string.Join('_', System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName, _siteContext.OrganizationKey, cacheKey), tree, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds((double)this._options.CacheDurationInSeconds)));
            return Task.CompletedTask;
        }

        public Task ClearTreeCache(string cacheKey)
        {
            _cache.Remove(cacheKey);
            return Task.CompletedTask;
        }

        public Task ClearTreeCache()
        {
            // IMemoryCache has no "Clear" method, and "Dispose" would make the singleton completely unusable
            throw new NotImplementedException();
        }
    }
}
