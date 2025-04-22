using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BPV_app.Data;
using BPV_app.Models;
using BPV_tool.Server.DTOs.Process;
using BPV_tool.Server.Models;

namespace BPV_tool.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // all actions require at least authentication
    public class BPVProcessesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public BPVProcessesController(ApplicationDbContext context) => _context = context;

        // 1) Students list their own processes
        // GET api/BPVProcesses/my
        [HttpGet("my")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyProcesses()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var list = await _context.BPVProcesses
                .Include(p => p.Supervisor)
                .Where(p => p.StudentId == userId)
                .Select(p => new ProcessSummaryDTO
                {
                    Id = p.Id,
                    CompanyName = p.CompanyName,
                    SupervisorId = p.SupervisorId,
                    SupervisorName = p.Supervisor.FirstName + " " + p.Supervisor.LastName,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();
            return Ok(list);
        }

        // 2) Student creates a new process (seeding 4 steps)
        // POST api/BPVProcesses
        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CreateProcess([FromBody] CreateProcessDTO dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var process = new BPVProcess
            {
                Id = Guid.NewGuid(),
                StudentId = userId,
                CompanyName = dto.CompanyName,
                SupervisorId = dto.SupervisorId,
                Status = "In behandeling",
                CreatedAt = DateTime.UtcNow
            };
            _context.BPVProcesses.Add(process);

            var stepNames = new[]
            {
                "BPV‑contract upload",
                "Eerste beoordeling",
                "Tweede beoordeling",
                "Eind beoordeling"
            };
            for (var i = 0; i < stepNames.Length; i++)
            {
                _context.BPVSteps.Add(new BPVStep
                {
                    Id = Guid.NewGuid(),
                    BPVProcessId = process.Id,
                    StepIndex = i + 1,
                    StepName = stepNames[i]
                });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // 3) Anyone on the process (student, teacher, admin) can view its steps
        // GET api/BPVProcesses/{processId}/steps
        [HttpGet("{processId}/steps")]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> GetSteps(Guid processId)
        {
            var steps = await _context.BPVSteps
                .Include(s => s.File)
                .Include(s => s.Approval)
                .Where(s => s.BPVProcessId == processId)
                .OrderBy(s => s.StepIndex)
                .Select(s => new
                {
                    s.Id,
                    s.StepIndex,
                    s.StepName,
                    // from File nav
                    FilePath = s.File != null ? s.File.FilePath : null,
                    UploadedAt = s.File != null ? s.File.UploadedAt : (DateTime?)null,
                    // from Approval nav
                    ApprovalStatus = s.Approval != null ? s.Approval.Status : null,
                    Feedback = s.Approval != null ? s.Approval.Feedback : null,
                    ReviewedAt = s.Approval != null ? s.Approval.ReviewedAt : (DateTime?)null
                })
                .ToListAsync();
            return Ok(steps);
        }

        // 4) Student uploads a file to a step
        // POST api/BPVProcesses/{processId}/steps/{stepId}/upload
        [HttpPost("{processId}/steps/{stepId}/upload")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UploadStepFile(
            Guid processId,
            Guid stepId,
            [FromForm] UploadStepFileDTO dto)
        {
            var step = await _context.BPVSteps.FindAsync(stepId);
            if (step == null || step.BPVProcessId != processId)
                return NotFound();

            // save to disk
            var fileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
            var folder = Path.Combine("Uploads", processId.ToString());
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, fileName);
            await using (var fs = new FileStream(path, FileMode.Create))
                await dto.File.CopyToAsync(fs);

            // create File entity
            var fileEnt = new BPV_app.Models.File
            {
                Id = Guid.NewGuid(),
                StudentId = step.BPVProcess.StudentId,
                FilePath = path,
                UploadedAt = DateTime.UtcNow
            };
            _context.Files.Add(fileEnt);
            await _context.SaveChangesAsync();

            // link to step
            step.FileId = fileEnt.Id;
            await _context.SaveChangesAsync();
            return Ok();
        }

        // 5) Teacher approves (or rejects) a step
        // POST api/BPVProcesses/{processId}/steps/{stepId}/approve
        [HttpPost("{processId}/steps/{stepId}/approve")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> ApproveStep(
            Guid processId,
            Guid stepId,
            [FromBody] ApproveStepDTO dto)
        {
            var step = await _context.BPVSteps.FindAsync(stepId);
            if (step == null || step.BPVProcessId != processId)
                return NotFound();

            var approval = new BPVApproval
            {
                Id = Guid.NewGuid(),
                BPVStepId = step.Id,
                ReviewerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                Status = dto.Status,
                Feedback = dto.Feedback,
                ReviewedAt = DateTime.UtcNow
            };
            _context.BPVApprovals.Add(approval);
            await _context.SaveChangesAsync();

            step.ApprovalId = approval.Id;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
