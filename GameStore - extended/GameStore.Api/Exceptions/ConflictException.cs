using System.Net;

namespace GameStoreApi.Exceptions;

public sealed class ConflictException(string message)
    : AppException(message, HttpStatusCode.Conflict);