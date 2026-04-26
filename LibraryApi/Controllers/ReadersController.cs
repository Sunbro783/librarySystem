using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReadersController : ControllerBase
{
    private readonly LibraryDbContext _context;

    public ReadersController(LibraryDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IEnumerable<Reader>> GetAll()
    {
        return await _context.Readers.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Reader>> Get(int id)
    {
        var reader = await _context.Readers.FindAsync(id);

        if (reader == null)
            return NotFound();

        return reader;
    }

    [HttpPost]
    public async Task<ActionResult<Reader>> Create(Reader reader)
    {
        reader.RegistrationDate = DateTime.UtcNow;

        _context.Readers.Add(reader);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = reader.Id }, reader);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Reader updated)
    {
        if (id != updated.Id)
            return BadRequest();

        _context.Entry(updated).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var reader = await _context.Readers.FindAsync(id);

        if (reader == null)
            return NotFound();

        _context.Readers.Remove(reader);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}