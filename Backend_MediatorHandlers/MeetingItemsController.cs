using MeetingItems.Core.AppServices.Authentication;
using MeetingItems.Core.AppServices.Features.MeetingItems;
using MeetingItems.Web.Abstractions;
using MeetingItems.Web.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MeetingItems.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MeetingItemsController(IMediator mediator) : ApiController(mediator)
{
    [HttpGet("{id}")]
    [Permission(Policy.CanViewMeetingItems)]
    [ProducesResponseType(typeof(GetMeetingItemById.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var query = new GetMeetingItemById.Query(id);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult<GetMeetingItemById.Response>(result);
    }

    [HttpPost]
    [Permission(Policy.CanCreateMeetingItems)]
    [ProducesResponseType(typeof(CreateMeetingItem.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMeetingItem.Command command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult<CreateMeetingItem.Response>(result);
    }

    [HttpPut("{id}")]
    [Permission(Policy.CanUpdateMeetingItems)]
    [ProducesResponseType(typeof(UpdateMeetingItem.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] UpdateMeetingItem.Command command,
        CancellationToken cancellationToken)
    {
        // Ensure route id matches command id
        if (id != command.MeetingItemId)
            return BadRequest("Meeting item ID mismatch");

        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult<UpdateMeetingItem.Response>(result);
    }

    [HttpPatch("{id}/status")]
    [Permission(Policy.CanUpdateMeetingItemStatus)]
    [ProducesResponseType(typeof(UpdateMeetingItemStatus.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateStatus(
        [FromRoute] string id,
        [FromBody] UpdateMeetingItemStatus.Command command,
        CancellationToken cancellationToken)
    {
        // Ensure route id matches command id
        if (id != command.MeetingItemId)
            return BadRequest("Meeting item ID mismatch");

        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult<UpdateMeetingItemStatus.Response>(result);
    }

    [HttpPost("{id}/documents")]
    [Permission(Policy.CanUploadMeetingItemDocuments)]
    [ProducesResponseType(typeof(UploadMeetingItemDocument.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadDocument(
        [FromRoute] string id,
        [FromBody] UploadMeetingItemDocument.Command command,
        CancellationToken cancellationToken)
    {
        // Ensure route id matches command id
        if (id != command.MeetingItemId)
            return BadRequest("Meeting item ID mismatch");

        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult<UploadMeetingItemDocument.Response>(result);
    }

    [HttpGet("template/{decisionBoardId}")]
    [Permission(Policy.CanViewTemplates)]
    [ProducesResponseType(typeof(GetTemplateByDecisionBoard.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTemplateByDecisionBoard(
        [FromRoute] string decisionBoardId,
        CancellationToken cancellationToken)
    {
        var query = new GetTemplateByDecisionBoard.Query(decisionBoardId);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult<GetTemplateByDecisionBoard.Response>(result);
    }
}
