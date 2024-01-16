using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ToDoGrpc.Data;
using ToDoGrpc.Models;

namespace ToDoGrpc.Services;

public class ToDoService : ToDoIt.ToDoItBase
{

  private readonly AppDbContext _dbContext;

  public ToDoService(AppDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public override async Task<CreateToDoResponse> CreateToDo(CreateToDoRequest request, ServerCallContext context)
  {
    if (request.Title == string.Empty || request.Description == string.Empty)
      throw new RpcException(new Status(StatusCode.InvalidArgument, "Title and Description are required"));

    var toDoItem = new ToDoItem
    {
      Title = request.Title,
      Description = request.Description,
    };

    await _dbContext.ToDoItems.AddAsync(toDoItem);
    await _dbContext.SaveChangesAsync();

    return await Task.FromResult(new CreateToDoResponse
    {
      Id = toDoItem.Id
    });
  }

  public override async Task<ReadToDoResponse> ReadToDo(ReadToDoRequest request, ServerCallContext context)
  {
    if (request.Id == 0)
      throw new RpcException(new Status(StatusCode.InvalidArgument, "Id is Illegal"));

    var toDoItem = await _dbContext.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);

    if (toDoItem == null)
      throw new RpcException(new Status(StatusCode.NotFound, $"ToDo with Id {request.Id} not found"));

    return await Task.FromResult(new ReadToDoResponse
    {
      Id = toDoItem.Id,
      Title = toDoItem.Title,
      Description = toDoItem.Description,
      ToDoStatus = toDoItem.ToDoStatus
    });
  }

  public override async Task<GetAllResponse> ListToDo(GetAllRequest request, ServerCallContext context)
  {
    var response = new GetAllResponse();
    var toDoItems = await _dbContext.ToDoItems.ToListAsync();

    foreach (var toDoItem in toDoItems)
    {
      response.ToDo.Add(new ReadToDoResponse
      {
        Id = toDoItem.Id,
        Title = toDoItem.Title,
        Description = toDoItem.Description,
        ToDoStatus = toDoItem.ToDoStatus
      });
    }

    return await Task.FromResult(response);

  }

  public override async Task<UpdateToDoResponse> UpdateToDo(UpdateToDoRequest request, ServerCallContext context)
  {
    if (request.Id == 0)
      throw new RpcException(new Status(StatusCode.InvalidArgument, "Id is Illegal"));

    var toDoItem = await _dbContext.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);

    if (toDoItem == null)
      throw new RpcException(new Status(StatusCode.NotFound, $"ToDo with Id {request.Id} not found"));

    if (request.Title != string.Empty)
      toDoItem.Title = request.Title;
    if (request.Description != string.Empty)
      toDoItem.Description = request.Description;
    if (request.ToDoStatus != string.Empty)
      toDoItem.ToDoStatus = request.ToDoStatus;

    await _dbContext.SaveChangesAsync();

    return await Task.FromResult(new UpdateToDoResponse
    {
      Id = toDoItem.Id
    });
  }

  public override async Task<DeleteToDoResponse> DeleteToDo(DeleteToDoRequest request, ServerCallContext context)
  {
    if (request.Id == 0)
      throw new RpcException(new Status(StatusCode.InvalidArgument, "Id is Illegal"));

    var toDoItem = await _dbContext.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);
    if (toDoItem == null)
      throw new RpcException(new Status(StatusCode.NotFound, $"ToDo with Id {request.Id} not found"));

    _dbContext.ToDoItems.Remove(toDoItem);
    await _dbContext.SaveChangesAsync();

    return await Task.FromResult(new DeleteToDoResponse
    {
      Id = toDoItem.Id
    });
  }
}