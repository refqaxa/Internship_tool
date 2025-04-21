using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BPV_app.Data;
using BPV_app.Models;
using BPV_tool.Server.DTOs.Process;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BPV_tool.Server.Models;

namespace BPV_tool.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BPVProcessesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        // public BPVProcessStepsController(ApplicationDbContext ctx) => _context = ctx;

        public BPVProcessesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/BPVProcesses/My
        [HttpGet("My")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<IEnumerable<ProcessSummaryDTO>>> GetBPVProcesses()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
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

        // POST: api/BPVProcesses/Create
        [HttpPost("Create")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Create([FromBody] CreateProcessDTO dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
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

            var stepNames = new[] {
                "BPV‑contract upload",
                "Eerste beoordeling",
                "Tweede beoordeling",
                "Eind beoordeling"
            };

            for (var i = 0; i < stepNames.Length; i++)
            {
                _context.BPVProcessSteps.Add(new BPVProcessStep
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

        // GET: /api/BPVProcessSteps/GetStepsForProcess/{procId}
        [HttpGet("ByProcess/{procId}")]
        [Authorize(Roles = "Student,Admin,Teacher")]
        public async Task<IActionResult> GetStepsForProcess(Guid procId)
        {
            var steps = await _context.BPVProcessSteps
                .Include(s => s.File)
                .Include(s => s.Approval)
                .Where(s => s.BPVProcessId == procId)
                .OrderBy(s => s.StepIndex)
                .Select(s => new {
                    s.Id,
                    s.StepIndex,
                    s.StepName,
                    FilePath = s.File.FilePath,
                    UploadedAt = s.File.UploadedAt,
                    ApprovalStatus = s.Approval != null ? s.Approval.Status : null,
                    ApprovalComment = s.Approval != null ? s.Approval.Comment : null,
                    ReviewedAt = s.Approval != null ? s.Approval.ReviewedAt : (DateTime?)null
                })
                .ToListAsync();
            return Ok(steps);
        }

        // POST: /api/BPVProcessSteps/UploadFile/{stepId}
        [HttpPost("UploadFile/{stepId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UploadFile(Guid stepId, [FromForm] UploadStepFileDTO dto)
        {
            var step = await _context.BPVProcessSteps.FindAsync(stepId);
            if (step == null) return NotFound();

            // save file to disk...
            var fileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
            var folder = Path.Combine("Uploads", step.BPVProcessId.ToString());
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, fileName);
            using var fs = new FileStream(path, FileMode.Create);
            await dto.File.CopyToAsync(fs);

            // create File entity
            var fileEnt = new BPV_app.Models.File
            {
                Id = Guid.NewGuid(),
                StudentId = step.BPVProcess.StudentId,
                FilePath = path,
                BpvStep = step.StepName,
                UploadedAt = DateTime.UtcNow
            };
            _context.Files.Add(fileEnt);
            await _context.SaveChangesAsync();

            // link to step
            step.FileId = fileEnt.Id;
            await _context.SaveChangesAsync();
            return Ok();
        }


        // POST: /api/BPVProcessSteps/Approve/{stepId}
        [HttpPost("Approve/{stepId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Approve(Guid stepId, [FromBody] ApproveStepDTO dto)
        {
            var step = await _context.BPVProcessSteps.FindAsync(stepId);
            if (step == null) return NotFound();

            var approval = new BPVApproval
            {
                Id = Guid.NewGuid(),
                BPVProcessId = step.BPVProcessId,
                ReviewerId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value),
                Status = dto.Status,
                Comment = dto.Comment,
                ReviewedAt = DateTime.UtcNow
            };
            _context.BPVApprovals.Add(approval);
            await _context.SaveChangesAsync();

            step.ApprovalId = approval.Id;
            await _context.SaveChangesAsync();
            return Ok();
        }

        private bool BPVProcessExists(Guid id)
        {
            return _context.BPVProcesses.Any(e => e.Id == id);
        }
    }
}
