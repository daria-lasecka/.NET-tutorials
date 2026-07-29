using System.Net;

namespace GameStoreApi.Exceptions;

public sealed class BadRequestException(string message)
    : AppException(message, HttpStatusCode.BadRequest);