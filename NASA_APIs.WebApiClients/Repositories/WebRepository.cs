﻿using NASA_APIs.Interfaces.Base.Entities;
using NASA_APIs.Interfaces.Base.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NASA_APIs.WebApiClients.Repositories
{
    public class WebRepository<T> : IRepository<T> where T : IEntity
    {
        private readonly HttpClient _Client;

        public WebRepository(HttpClient Client) => _Client = Client;
        public async Task<T> Add(T item, CancellationToken Cancel = default)
        {
            var response = await _Client.PostAsJsonAsync("", item, Cancel).ConfigureAwait(false);
            var result = await response
                .EnsureSuccessStatusCode()
                .Content
                .ReadFromJsonAsync<T>(cancellationToken: Cancel).ConfigureAwait(false);
            return result;
        }

        public async Task<T> Delete(T item, CancellationToken Cancel = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, "")
            {
                Content = JsonContent.Create(item)
            };
            var response = await _Client.SendAsync(request, Cancel).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return default;
            var result = await response
                .EnsureSuccessStatusCode()
                .Content
                .ReadFromJsonAsync<T>(cancellationToken: Cancel).ConfigureAwait(false);
            return result;
        }

        public async Task<T> DeleteById(int Id, CancellationToken Cancel = default)
        {
            var response = await _Client.DeleteAsync($"{Id}", Cancel).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return default;
            var result = await response
                .EnsureSuccessStatusCode()
                .Content
                .ReadFromJsonAsync<T>(cancellationToken: Cancel).ConfigureAwait(false);
            return result;
        }

        public async Task<bool> Exist(T item, CancellationToken Cancel = default)
        {
            var response = await _Client.PostAsJsonAsync("exist", Cancel).ConfigureAwait(false);
            return response.StatusCode != HttpStatusCode.NotFound && response.IsSuccessStatusCode;
        }

        public async Task<bool> ExistId(int Id, CancellationToken Cancel = default)
        {
            var response = await _Client.GetAsync($"exist/id/{Id}", Cancel).ConfigureAwait(false);
            return response.StatusCode != HttpStatusCode.NotFound && response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<T>> Get(int Skip, int Count, CancellationToken Cancel = default) =>
            await _Client.GetFromJsonAsync<IEnumerable<T>>($"items[{Skip}:{Count}]", Cancel).ConfigureAwait(false);

        public async Task<IEnumerable<T>> GetAll(CancellationToken Cancel = default) =>
            await _Client.GetFromJsonAsync<IEnumerable<T>>("", Cancel).ConfigureAwait(false);

        public async Task<T> GetById(int Id, CancellationToken Cancel = default) =>
            await _Client.GetFromJsonAsync<T>($"{Id}",Cancel).ConfigureAwait(false);

        public async Task<int> GetCount(CancellationToken Cancel = default)=>
            await _Client.GetFromJsonAsync<int>("count", Cancel).ConfigureAwait(false);


        public async Task<IPage<T>> GetPage(int PageIndex, int PageSize, CancellationToken Cancel = default)
        {
            var response = await _Client.GetAsync($"page/[{PageIndex}:{PageSize}]",Cancel).ConfigureAwait(false);
            if(response.StatusCode == HttpStatusCode.NotFound)
            {
                return new PageItems
                {
                    Items = Enumerable.Empty<T>(),
                    TotalCount = 0,
                    PageIndex = PageIndex,
                    PageSize = PageSize
                };
            }
            return await response
                .EnsureSuccessStatusCode()
                .Content
                .ReadFromJsonAsync<PageItems>(cancellationToken: Cancel).ConfigureAwait(false);

        }

        private class PageItems : IPage<T>
        {
            public IEnumerable<T> Items { get; init; }

            public int TotalCount { get; init; }

            public int PageIndex { get; init; }

            public int PageSize { get; init; }
            public int TotalPagesCount => (int)Math.Ceiling((double)TotalCount / PageSize);
        }
        public async Task<T> Update(T item, CancellationToken Cancel = default)
        {
            var response = await _Client.PutAsJsonAsync("", Cancel).ConfigureAwait(false);
            var result = await response
                .EnsureSuccessStatusCode()
                .Content
                .ReadFromJsonAsync<T>(cancellationToken: Cancel).ConfigureAwait(false);
            return result;
        }
    }
}
