using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BPV_app.Data;
using BPV_app.Models;
using BPV_tool.Server.DTOs.Process;
using BPV_tool.Server.Models;
using System.IO.Compression;

namespace BPV_tool.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StudentDashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public StudentDashboardController(ApplicationDbContext context) => _context = context;

        // GET: api/StudentDashboard/processes
        [HttpGet("processes")]
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

        // POST: api/StudentDashboard/processes
        [HttpPost("processes")]
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

        // GET: api/StudentDashboard/processes/{processId}/steps
        [HttpGet("processes/{processId}/steps")]
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

        // POST: api/StudentDashboard/processes/{processId}/steps/{stepId}/upload
        [HttpPost("processes/{processId}/steps/{stepId}/upload")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UploadStepFile(Guid processId, Guid stepId, [FromForm] UploadStepFileDTO dto)
        {
            var step = await _context.BPVSteps
                .Include(s => s.BPVProcess)
                    .ThenInclude(p => p.Student)
                .FirstOrDefaultAsync(s => s.Id == stepId && s.BPVProcessId == processId);

            if (step == null)
                return NotFound();

            var student = step.BPVProcess.Student;

            // Define a safe folder path like: BPVfiles/FirstNameLastName_GUID
            var studentFolderName = $"{student.FirstName}_{student.LastName}_{student.Id}".Replace(" ", "").ToLower();
            var projectRoot = Path.Combine(Directory.GetCurrentDirectory(), "..", "bpv_tool.client", "BPVfiles");
            var uploadFolder = Path.Combine(projectRoot, studentFolderName);
            Directory.CreateDirectory(uploadFolder);

            // Save file to folder
            var uniqueFileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
            var fullPath = Path.Combine(uploadFolder, uniqueFileName);
            await using (var fs = new FileStream(fullPath, FileMode.Create))
                await dto.File.CopyToAsync(fs);

            var fileEntity = new BPV_app.Models.File
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                FilePath = Path.Combine("BPVfiles", studentFolderName, uniqueFileName).Replace("\\", "/"),
                UploadedAt = DateTime.UtcNow
            };

            _context.Files.Add(fileEntity);
            await _context.SaveChangesAsync();

            step.FileId = fileEntity.Id;
            await _context.SaveChangesAsync();

            return Ok();
        }


        // POST: api/StudentDashboard/processes/{processId}/steps/{stepId}/approve
        [HttpPost("processes/{processId}/steps/{stepId}/approve")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> ApproveStep(Guid processId, Guid stepId, [FromBody] ApproveStepDTO dto)
        {
            var step = await _context.BPVSteps.FirstOrDefaultAsync(s => s.Id == stepId && s.BPVProcessId == processId);
            if (step == null) return NotFound();

            var approval = new BPVApproval
            {
                Id = Guid.NewGuid(),
                BPVStepId = stepId,
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

        [HttpGet("download-zip")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> DownloadZip()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);

            var folderName = $"{user.FirstName}_{user.LastName}_{user.Id}".Replace(" ", "").ToLower();
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "bpv_tool.client", "BPVfiles", folderName);

            if (!Directory.Exists(folderPath))
                return NotFound("Geen bestanden gevonden");

            var zipName = $"bpv-bestanden-{user.FirstName}-{user.LastName}.zip";
            var zipPath = Path.Combine(Path.GetTempPath(), zipName);
            if (System.IO.File.Exists(zipPath))
                System.IO.File.Delete(zipPath);

            ZipFile.CreateFromDirectory(folderPath, zipPath);
            var zipBytes = await System.IO.File.ReadAllBytesAsync(zipPath);
            return File(zipBytes, "application/zip", zipName);
        }


    }
}
