using gita_todo_app;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
var connectionString = "";

var app = builder.Build();

app.UseHttpsRedirection();

// Get all todos
app.MapGet("/todos", async () =>
{
    try
    {
        var todos = await GetTodosFromDatabase(null);
        return Results.Ok(todos);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Get a todo
app.MapGet("/todo/{id}", async (int id) =>
{
    try
    {
        var todos = await GetTodosFromDatabase(id);
        return todos.Count > 0 ? Results.Ok(todos.First()) : Results.NotFound("Todo not found.");
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Add a todo
app.MapPost("/todos", async (List<Todo> todos) =>
{
    try
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        var query = "INSERT INTO TodoList (Title, Description, IsCompleted) VALUES (@Title, @Description, @IsCompleted)";
        using var command = new SqlCommand(query, connection);

        foreach (var todo in todos)
        {
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@Title", todo.Title);
            command.Parameters.AddWithValue("@Description", todo.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsCompleted", todo.IsCompleted);

            await command.ExecuteNonQueryAsync();
        }

        return Results.Ok("Todos added successfully!");
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }

});

// Update a todo
app.MapPost("/todos/{id}", async (int id, Todo todo) =>
{
    try
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        var query = "UPDATE TodoList SET Title = @Title, Description = @Description, IsCompleted = @IsCompleted WHERE Id = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Title", todo.Title);
        command.Parameters.AddWithValue("@Description", todo.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@IsCompleted", todo.IsCompleted);

        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0 ? Results.Ok("Todo updated successfully!") : Results.NotFound("Todo not found.");

    }
    catch (Exception ex)
    {

        return Results.BadRequest(ex.Message);
    }
});

// Delete a todo
app.MapDelete("/todos/{id}", async (int id) =>
{
    try
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        var query = "DELETE FROM TodoList WHERE Id = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0 ? Results.Ok("Todo deleted successfully!") : Results.NotFound("Todo not found.");

    }
    catch (Exception ex)
    {

        return Results.BadRequest(ex.Message);
    }
});

// Delete all completed todos
app.MapDelete("/todos/completed", async () =>
{
    try
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        var query = "DELETE FROM TodoList WHERE IsCompleted = 1";
        using var command = new SqlCommand(query, connection);

        var rows = await command.ExecuteNonQueryAsync();
        return Results.Ok($"{rows} completed todos deleted successfully.");

    }
    catch (Exception ex)
    {

        return Results.BadRequest(ex.Message);
    }
});

#region Helper methods

async Task<List<Todo>> GetTodosFromDatabase(int? id)
{
    try
    {

        var todos = new List<Todo>();
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        var query = "SELECT Id, Title, Description, IsCompleted, CreatedAt FROM TodoList";
        if (id.HasValue)
            query += " WHERE Id = @Id";

        using var command = new SqlCommand(query, connection);
        if (id.HasValue)
            command.Parameters.AddWithValue("@Id", id.Value);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            todos.Add(new Todo
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                IsCompleted = reader.GetBoolean(3),
                CreatedAt = reader.GetDateTime(4)
            });
        }

        return todos;
    }
    catch (Exception)
    {

        throw;
    }
}
#endregion
app.Run();
