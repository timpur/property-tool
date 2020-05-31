using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyTool.DataBase;
using PropertyTool.Model;

namespace PropertyTool.Controller
{
    [ApiController, Route("[controller]")]
    public class PropertyController : ControllerBase
    {
        private readonly PropertyContext _context;
        private readonly ISource _source;

        public PropertyController(PropertyContext context, ISource source)
        {
            _context = context;
            _source = source;
        }

        [HttpGet("all")]
        public async Task<IEnumerable<Property>> GetProperties()
        {
            return await _context.Properties.ToListAsync();
        }

        [HttpGet("sync")]
        public async Task SyncProperties()
        {
            await foreach (var batch in _source.GetProperties())
            {
                _context.AddRange(batch);
                await _context.SaveChangesAsync();
            }
        }
    }
}