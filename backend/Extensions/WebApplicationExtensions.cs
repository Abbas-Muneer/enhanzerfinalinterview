namespace Enhanzer.Api.Extensions;

public static class WebApplicationExtensions
{
     public static WebApplication UseApplicationPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseExceptionHandler(exceptionApp =>
        {
            exceptionApp.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "An unexpected server error occurred."
                });
            });
        });

        app.UseHttpsRedirection();
        app.UseCors(ServiceCollectionExtensions.GetCorsPolicyName());
        app.MapControllers();

        return app;
    }
}