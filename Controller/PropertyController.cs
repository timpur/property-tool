using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyTool.DataBase;
using PropertyTool.Client;

namespace PropertyTool.Controller
{
    [ApiController, Route("[controller]")]
    public class PropertyController : ControllerBase
    {
        private readonly PropertyContext _context;

        public PropertyController(PropertyContext context)
        {
            _context = context;
        }

        [HttpGet("all")]
        public async Task<IEnumerable<Property>> GetProperties()
        {
            return await _context.Properties.ToListAsync();
        }

        [HttpGet("sync")]
        public async Task SyncProperties()
        {
            var client = new RealeState();
            var properties = await client.GetProperties();
            while (properties.Any())
            {
                var batch = properties.Take(10);
                properties = properties.Skip(10);
                var tasks = batch.Select(x => client.GetProperty(x));
                var results = await Task.WhenAll(tasks);
                _context.AddRange(results);
            }
            await _context.SaveChangesAsync();
        }
    }
}