#nullable enable
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MeetingItemsApp.MeetingItems.Features;

namespace MeetingItemsApp.MeetingItems.Controllers;

[ApiController]
[Route("api/meeting-items")]
public class MeetingItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MeetingItemsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Get meeting items by decision board ID
    /// </summary>
    [HttpGet("decision-board/{decisionBoardId}")]
    [ProducesResponseType(typeof(GetMeetingItemsByDecisionBoard.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMeetingItemsByDecisionBoard(string decisionBoardId, CancellationToken cancellationToken)
    {
        var query = new GetMeetingItemsByDecisionBoard.Query(decisionBoardId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Data)
            : StatusCode(result.StatusCode, new { message = result.ErrorMessage });
    }

    /// <summary>
    /// Get a meeting item by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetMeetingItem.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMeetingItem(string id, CancellationToken cancellationToken)
    {
        var query = new GetMeetingItem.Query(id);
        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Data)
            : StatusCode(result.StatusCode, new { message = result.ErrorMessage });
    }

    /// <summary>
    /// Create a new meeting item
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateMeetingItem.Response), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMeetingItem(
        [FromBody] CreateMeetingItem.Command command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetMeetingItem), new { id = result.Data!.MeetingItemId }, result.Data)
            : StatusCode(result.StatusCode, new { message = result.ErrorMessage });
    }

    /// <summary>
    /// Update an existing meeting item (including document upload/deletion)
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UpdateMeetingItem.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMeetingItem(
        string id,
        [FromBody] UpdateMeetingItem.Command command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new { message = "ID mismatch" });
        }

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Data)
            : StatusCode(result.StatusCode, new { message = result.ErrorMessage });
    }
}
