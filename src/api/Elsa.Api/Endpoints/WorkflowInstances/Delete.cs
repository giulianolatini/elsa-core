using System.Threading;
using System.Threading.Tasks;
using Elsa.AspNetCore;
using Elsa.Persistence.Services;
using Microsoft.AspNetCore.Mvc;

namespace Elsa.Api.Endpoints.WorkflowInstances;

[Area(AreaNames.Elsa)]
[ApiEndpoint(ControllerNames.WorkflowInstances, "Delete")]
public class Delete : Controller
{
    private readonly IWorkflowInstanceStore _store;
    public Delete(IWorkflowInstanceStore store) => _store = store;

    [HttpDelete]
    public async Task<IActionResult> HandleAsync(string id, CancellationToken cancellationToken)
    {
        var deleted = await _store.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}