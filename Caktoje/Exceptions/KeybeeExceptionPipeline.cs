using System.Net;

namespace Caktoje.Exceptions;
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Try to process the rest of the HTTP pipeline
            await _next(context);
        }
        catch (BadRequestException ex)
        {
            var issueId = Guid.NewGuid().ToString();
            
            using (Serilog.Context.LogContext.PushProperty("IssueId", issueId))
            using (Serilog.Context.LogContext.PushProperty("Type", "BadRequest"))
            {
                _logger.LogError(ex, "A bad request happened.");
            }
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;


            var response = new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                ex.Message,
                IssueId = issueId
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }
        catch(ForbiddenException ex)
        {
            var issueId = Guid.NewGuid().ToString();
            
            using (Serilog.Context.LogContext.PushProperty("IssueId", issueId))
            using (Serilog.Context.LogContext.PushProperty("Type", "Forbidden"))
            {
                _logger.LogError(ex, "A forbidden request happened.");
            }
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

            var response = new
            {
                StatusCode = (int)HttpStatusCode.Forbidden,
                ex.Message,
                IssueId = issueId
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }
        catch (NotFoundException ex)
        {
            var issueId = Guid.NewGuid().ToString();
            
            using (Serilog.Context.LogContext.PushProperty("IssueId", issueId))
            using (Serilog.Context.LogContext.PushProperty("Type", "NotFound"))
            {
                _logger.LogError(ex, "A not found request happened.");
            }
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;

            var response = new
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                ex.Message,
                IssueId = issueId
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }
        catch (CriticalConfigurationException ex)
        {
            var issueId = Guid.NewGuid().ToString();
            
            using (Serilog.Context.LogContext.PushProperty("IssueId", issueId))
            using (Serilog.Context.LogContext.PushProperty("Type", "Configuration"))
            {
                _logger.LogError(ex, "A critical configuration exception occurred due to a configuration issue by the user.");
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "An internal server error occurred.",
                IssueId = issueId
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }
        catch(ConfigurationException ex)
        {
            var issueId = Guid.NewGuid().ToString();

            using (Serilog.Context.LogContext.PushProperty("IssueId", issueId))
            using (Serilog.Context.LogContext.PushProperty("Type", "Configuration"))
            {
                _logger.LogInformation(ex, "An exception occurred due to a configuration issue by the user.");
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "An internal server error occurred.",
                IssueId = issueId
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }
        catch (CriticalDevelopmentException ex)
        {
            var issueId = Guid.NewGuid().ToString();

            using (Serilog.Context.LogContext.PushProperty("IssueId", issueId))
            using (Serilog.Context.LogContext.PushProperty("Type", "Development"))
            using (Serilog.Context.LogContext.PushProperty("Domain", ex.Domain))
            using (Serilog.Context.LogContext.PushProperty("ClassName", ex.ClassName))
            {
                _logger.LogError(ex, "An exception occurred due to a configuration issue by the user.");
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "An internal server error occurred.",
                IssueId = issueId   
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }
        catch (Exception ex)
        {
            var issueId = Guid.NewGuid().ToString();

            using (Serilog.Context.LogContext.PushProperty("IssueId", issueId))
            using (Serilog.Context.LogContext.PushProperty("Type", "Unhandled"))
            {
                _logger.LogError(ex, "An unhandled exception occurred");
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "An internal server error occurred.",
                IssueId = issueId
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }
    }
}
