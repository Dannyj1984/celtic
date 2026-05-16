using Celtic.Api.Data;
using Celtic.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Celtic.Api.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class SettingsController : ControllerBase
{
    private readonly CelticDbContext _context;

    public SettingsController(CelticDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ClubSettings>> GetSettings()
    {
        var settings = await _context.ClubSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new ClubSettings(); // Return defaults if none exist
        }
        return Ok(settings);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] ClubSettings updatedSettings)
    {
        var settings = await _context.ClubSettings.FirstOrDefaultAsync();
        
        if (settings == null)
        {
            updatedSettings.Id = Guid.NewGuid();
            _context.ClubSettings.Add(updatedSettings);
        }
        else
        {
            settings.NextSubPaymentDate = updatedSettings.NextSubPaymentDate;
            settings.TrainingDay = updatedSettings.TrainingDay;
            settings.TrainingStartTime = updatedSettings.TrainingStartTime;
            settings.TrainingEndTime = updatedSettings.TrainingEndTime;
            settings.TrainingLocation = updatedSettings.TrainingLocation;
            settings.CoachWhatsAppNumber = updatedSettings.CoachWhatsAppNumber;
            settings.TrainingFocus = updatedSettings.TrainingFocus;
            settings.GoodToKnow = updatedSettings.GoodToKnow;
        }

        await _context.SaveChangesAsync();
        return Ok(settings ?? updatedSettings);
    }
}
