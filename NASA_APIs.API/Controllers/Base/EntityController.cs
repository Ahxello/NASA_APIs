﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NASA_APIs.DAL.Entities;
using NASA_APIs.DAL.Entities.Base;
using NASA_APIs.Interfaces.Base.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NASA_APIs.API.Controllers.Base
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class EntityController<T> : ControllerBase where T : Entity
    {
        private readonly IRepository<T> _Repository;

        protected EntityController(IRepository<T> Repository) => _Repository = Repository;

        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]

        public async Task<IActionResult> GetItemsCount() => Ok(await _Repository.GetCount());
        [HttpGet("exist/id/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(bool))]
        public async Task<IActionResult> ExistId(int id) =>
            await _Repository.ExistId(id) ? Ok(true) : NotFound(false);

        [HttpPost("exist")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(bool))]
        public async Task<IActionResult> Exist(T item) =>
            await _Repository.Exist(item) ? Ok(true) : NotFound(false);

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        public async Task<IActionResult> GetAll() => Ok(await _Repository.GetAll());

        [HttpGet("items[[{Skip:int}:{Count:int}]]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<T>>> Get(int Skip, int Count) =>
            Ok(await _Repository.Get(Skip, Count));


        [HttpGet("page/{PageIndex:int}/{PageSize:int}")]
        [HttpGet("page/[[{PageIndex:int}:{PageSize:int}]]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IPage<T>>> GetPage(int PageIndex, int PageSize)
        {
            var result = await _Repository.GetPage(PageIndex, PageSize);
            return result.Items.Any() ? Ok(result) : NotFound(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id) =>
            await _Repository.GetById(id) is { } item ? Ok(item) : NotFound();

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Add(T item)
        {
            var result = await _Repository.Add(item);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(T item)
        {
            if (await _Repository.Update(item) is not { } result)
                return NotFound(item);
            return AcceptedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpDelete("delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(T item)
        {
            if (await _Repository.Delete(item) is not { } result)
                return NotFound(item);
            return Ok(result);
        }

        [HttpDelete("delete/id/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteById(int Id)
        {
            if (await _Repository.DeleteById(Id) is not { } result)
                return NotFound(Id);
            return Ok(result);
        }
    }
    
}
