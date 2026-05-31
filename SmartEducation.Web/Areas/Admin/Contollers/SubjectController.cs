using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartEducation.Application.Features.Subjects.Queries;

namespace SmartEducation.Web.Areas.Admin.Contollers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class SubjectController : Controller
    {
        private readonly IMediator _mediator;

        public SubjectController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: /Admin/Subject/Index
        [HttpGet("/Admin/Subjects")]
        public async Task<IActionResult> Index()
        {
            // Sending the query through MediatR pipeline
            var subjects = await _mediator.Send(new GetSubjectsQuery());

            // Passing the clean DTO collection to the Sneat layout View
            return View(subjects);
        }
    }
}
